using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ObjectGrabber : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float pickupRadius = 1.2f;
    [SerializeField] private float pickupHeight = 1f;
    [SerializeField] private float pickupForwardOffset = 0.8f;

    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 20f;
    public float MaxThrowForce => maxThrowForce;
    [SerializeField] private float throwChargeInterval = 0.1f;

    [Header("UI Settings")]
    [SerializeField] private Image forceBar;
    [SerializeField] private Image forceBarFrame;

    [Header("Pickup")]
    [SerializeField] private LayerMask pickupLayer;

    [Header("Scale Settings")]
    [Tooltip("Cuanto se achica el objeto al agarrarlo (0.5 = a la mitad de su tamano).")]
    [SerializeField, Range(0.05f, 1f)] private float shrinkFactor = 0.5f;
    [Tooltip("Grosor maximo (en metros) de un objeto agarrado, medido en su lado intermedio. " +
             "Los objetos voluminosos, como las piedras, se achican mas para no tapar la camara.")]
    [SerializeField] private float maxHeldSize = 0.7f;
    [Tooltip("Largo maximo (en metros, su lado mas largo) de un objeto agarrado. Es mas permisivo " +
             "para que los objetos largos y finos, como la escalera, no queden en miniatura.")]
    [SerializeField] private float maxHeldLength = 1.4f;

    private Vector3 heldOriginalScale = Vector3.one;

    [Header("Trajectory Settings")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int lineSegments = 30;
    [SerializeField] private float timeStep = 0.05f;

    [Header("Rotation & Magic Float")]
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource absorbAudioSource;     // Sonido al absorber con E
    [SerializeField] private AudioSource chargeAudioSource;     // Sonido al cargar tiro con click izquierdo (se detiene al soltar)
    [SerializeField] private AudioSource throwAudioSource;      // Sonido al expulsar/disparar

    [Header("Release Safety")]
    [Tooltip("Capas contra las que se evita que el objeto quede incrustado al soltarlo/lanzarlo.")]
    [SerializeField] private LayerMask releaseObstacleMask = ~0;
    [Tooltip("Separacion extra que se deja entre el objeto y lo que tocaba al soltarlo.")]
    [SerializeField] private float releaseSkin = 0.02f;
    [SerializeField] private int depenetrationIterations = 8;

    public bool IsRotatingObject { get; private set; }

    private Collider[] playerColliders;
    private readonly Collider[] overlapBuffer = new Collider[32];

    private Rigidbody heldObject;
    private Collider[] heldColliders;
    private ThrowableObject heldThrowableComponent;

    [SerializeField] private float currentThrowForce;
    public float CurrentThrowForce => currentThrowForce;
    private float throwChargeTimer;
    private bool isChargingThrow;
    private float magicFloatTimer;

    private void Awake()
    {
        playerColliders = GetComponentsInChildren<Collider>(true);
    }

    private void Start()
    {
        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);
            forceBarFrame.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (heldObject == null)
            {
                TryPickup();
            }
            else if (!isChargingThrow)
            {
                DropObject();
            }
        }

        if (heldObject != null)
        {
            IsRotatingObject = Keyboard.current.rKey.isPressed;

            HandleFloatingEffect();

            if (IsRotatingObject)
            {
                HandleRotation();
            }

            if (!IsRotatingObject)
            {
                HandleThrowInput();
            }
            else if (isChargingThrow)
            {
                CancelThrowCharge();
            }

            DrawTrajectory();
        }
        else
        {
            IsRotatingObject = false;

            if (trajectoryLine != null && trajectoryLine.enabled)
            {
                trajectoryLine.enabled = false;
            }
        }

        if (heldObject != null && isChargingThrow && forceBar != null)
        {
            forceBar.fillAmount = currentThrowForce;
        }
    }

    private void HandleRotation()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        heldObject.transform.Rotate(cameraTransform.up, -mouseDelta.x * rotationSpeed * Time.deltaTime, Space.World);
        heldObject.transform.Rotate(cameraTransform.right, mouseDelta.y * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void HandleFloatingEffect()
    {
        magicFloatTimer += Time.deltaTime;
        float offsetY = Mathf.Sin(magicFloatTimer * floatSpeed) * floatAmplitude;
        heldObject.transform.position = holdPoint.position + new Vector3(0, offsetY, 0);
    }

    private void HandleThrowInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartThrowCharge();
        }

        if (isChargingThrow)
        {
            ChargeThrow();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isChargingThrow)
        {
            ThrowObject();
        }
    }

    private void StartThrowCharge()
    {
        isChargingThrow = true;
        currentThrowForce = minThrowForce;
        throwChargeTimer = 0f;

        // Reproduce el sonido de carga
        if (chargeAudioSource != null)
        {
            chargeAudioSource.Play();
        }

        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(true);
            forceBarFrame.gameObject.SetActive(true);
            forceBar.fillAmount = currentThrowForce;
        }
    }

    private void ChargeThrow()
    {
        throwChargeTimer += Time.deltaTime;

        if (throwChargeTimer < throwChargeInterval)
            return;

        throwChargeTimer -= throwChargeInterval;

        if (currentThrowForce < maxThrowForce)
        {
            currentThrowForce += 1f;

            if (currentThrowForce > maxThrowForce)
                currentThrowForce = maxThrowForce;

            if (forceBar != null)
            {
                forceBar.fillAmount = currentThrowForce;
            }
        }
    }

    private void CancelThrowCharge()
    {
        isChargingThrow = false;
        currentThrowForce = minThrowForce;

        // Detiene el sonido si se cancela la carga
        if (chargeAudioSource != null && chargeAudioSource.isPlaying)
        {
            chargeAudioSource.Stop();
        }

        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);
            forceBarFrame.gameObject.SetActive(false);
        }
    }

    private void DrawTrajectory()
    {
        if (heldObject == null) return;
        if (trajectoryLine == null) return;

        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = lineSegments;

        Vector3 startPosition = holdPoint.position;
        Vector3 startVelocity = (cameraTransform.forward * currentThrowForce) / heldObject.mass;

        Vector3 currentPosition = startPosition;
        trajectoryLine.SetPosition(0, currentPosition);

        for (int i = 1; i < lineSegments; i++)
        {
            float timeOffset = i * timeStep;

            Vector3 gravityOffset = 0.5f * Physics.gravity * Mathf.Pow(timeOffset, 2);
            Vector3 nextPosition = startPosition + startVelocity * timeOffset + gravityOffset;

            if (Physics.Linecast(currentPosition, nextPosition, out RaycastHit hit))
            {
                trajectoryLine.positionCount = i + 1;
                trajectoryLine.SetPosition(i, hit.point);
                break;
            }

            trajectoryLine.SetPosition(i, nextPosition);
            currentPosition = nextPosition;
        }
    }

    // La deteccion usa hacia donde mira la camara (no hacia donde mira el cuerpo del
    // personaje), porque el cuerpo solo gira al caminar y en primera persona eso hacia
    // que a veces no se pudiera agarrar el objeto que se estaba mirando.
    private Vector3 GetPickupDetectionPosition()
    {
        Vector3 forward = transform.forward;

        if (cameraTransform != null)
        {
            Vector3 flatCameraForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);

            if (flatCameraForward.sqrMagnitude > 0.0001f)
            {
                forward = flatCameraForward.normalized;
            }
        }

        return transform.position + Vector3.up * pickupHeight + forward * pickupForwardOffset;
    }

    private void TryPickup()
    {
        Vector3 detectionPosition = GetPickupDetectionPosition();

        Collider[] hits = Physics.OverlapSphere(
            detectionPosition,
            pickupRadius,
            pickupLayer
        );

        if (hits.Length == 0)
            return;

        Rigidbody closestObject = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            ThrowableObject throwable = hit.transform.GetComponentInParent(typeof(ThrowableObject)) as ThrowableObject;

            if (throwable != null && throwable.IsLocked())
                continue;

            Rigidbody rb = hit.transform.GetComponentInParent(typeof(Rigidbody)) as Rigidbody;

            if (rb == null)
                continue;

            float distance = Vector3.Distance(transform.position, rb.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = rb;
            }
        }

        if (closestObject == null)
            return;

        heldObject = closestObject;
        heldThrowableComponent = heldObject.gameObject.GetComponent(typeof(ThrowableObject)) as ThrowableObject;

        heldObject.linearVelocity = Vector3.zero;
        heldObject.angularVelocity = Vector3.zero;
        // Los cuerpos cinematicos no soportan ContinuousDynamic: se pasa a Speculative antes
        heldObject.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        heldObject.isKinematic = true;

        if (absorbAudioSource != null)
        {
            absorbAudioSource.Play();
        }

        // Se calcula antes de achicarlo, con el objeto a su tamano real
        heldOriginalScale = heldObject.transform.localScale;
        Vector3 heldScale = heldOriginalScale * GetHeldScaleFactor(heldObject);

        if (heldThrowableComponent != null)
        {
            heldThrowableComponent.ShrinkScale(heldScale);
        }
        else
        {
            heldObject.transform.localScale = heldScale;
        }

        Component[] rawColliders = heldObject.gameObject.GetComponentsInChildren(typeof(Collider));
        heldColliders = new Collider[rawColliders.Length];
        for (int i = 0; i < rawColliders.Length; i++)
        {
            heldColliders[i] = rawColliders[i] as Collider;
        }

        foreach (Collider col in heldColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        currentThrowForce = minThrowForce;
        isChargingThrow = false;
        magicFloatTimer = 0f;
    }

    // Factor de escala al agarrar: se achica con shrinkFactor, y si aun asi el objeto
    // quedaria muy voluminoso (lado intermedio > maxHeldSize) o muy largo (lado mayor >
    // maxHeldLength), se achica lo necesario para entrar en esos limites.
    // Se usa el lado intermedio como "grosor" porque es lo que realmente tapa la camara:
    // una piedra es gruesa en todos sus lados, una escalera es larga pero fina.
    private float GetHeldScaleFactor(Rigidbody rb)
    {
        float factor = shrinkFactor;
        Vector3 size = GetObjectSize(rb.gameObject);

        // Ordena los lados de menor a mayor
        float a = size.x, b = size.y, c = size.z;
        if (a > b) { float t = a; a = b; b = t; }
        if (b > c) { float t = b; b = c; c = t; }
        if (a > b) { float t = a; a = b; b = t; }

        float middleSide = b;
        float longestSide = c;

        if (middleSide > 0.0001f && middleSide * factor > maxHeldSize)
        {
            factor = maxHeldSize / middleSide;
        }

        if (longestSide > 0.0001f && longestSide * factor > maxHeldLength)
        {
            factor = maxHeldLength / longestSide;
        }

        return factor;
    }

    // Tamano del objeto en metros sobre sus propios ejes (no cambia si el objeto esta girado),
    // segun lo que se ve (renderers) o, si no tiene, segun sus colliders
    private static Vector3 GetObjectSize(GameObject target)
    {
        Vector3 size = Vector3.zero;
        bool found = false;

        foreach (Renderer rend in target.GetComponentsInChildren<Renderer>())
        {
            if (!rend.enabled || rend is ParticleSystemRenderer || rend is TrailRenderer || rend is LineRenderer)
                continue;

            Vector3 scale = rend.transform.lossyScale;
            Vector3 rendSize = Vector3.Scale(rend.localBounds.size,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));

            size = Vector3.Max(size, rendSize);
            found = true;
        }

        if (found)
            return size;

        bool hasBounds = false;
        Bounds bounds = new Bounds(target.transform.position, Vector3.zero);

        foreach (Collider col in target.GetComponentsInChildren<Collider>())
        {
            if (!col.enabled || col.isTrigger)
                continue;

            if (hasBounds)
                bounds.Encapsulate(col.bounds);
            else
                bounds = col.bounds;

            hasBounds = true;
        }

        return hasBounds ? bounds.size : Vector3.zero;
    }

    private void DropObject()
    {
        if (heldObject != null)
        {
            if (heldThrowableComponent != null)
            {
                heldThrowableComponent.RestoreScale();
            }
            else
            {
                heldObject.transform.localScale = heldOriginalScale;
            }

            ReleaseSafely(heldObject);
        }

        // Detener sonido de carga por seguridad si se suelta el objeto de golpe
        if (chargeAudioSource != null && chargeAudioSource.isPlaying)
        {
            chargeAudioSource.Stop();
        }

        heldColliders = null;
        heldObject = null;
        heldThrowableComponent = null;

        isChargingThrow = false;
        currentThrowForce = minThrowForce;

        if (forceBar != null)
        {
            forceBar.gameObject.SetActive(false);
            forceBarFrame.gameObject.SetActive(false);
        }
    }

    private void ThrowObject()
    {
        Rigidbody objectToThrow = heldObject;

        // Detiene el sonido de carga al lanzar
        if (chargeAudioSource != null && chargeAudioSource.isPlaying)
        {
            chargeAudioSource.Stop();
        }

        if (objectToThrow != null)
        {
            if (heldThrowableComponent != null)
            {
                heldThrowableComponent.SetThrown();
            }
            else
            {
                objectToThrow.transform.localScale = heldOriginalScale;
            }

            ReleaseSafely(objectToThrow);

            heldColliders = null;
            heldObject = null;
            heldThrowableComponent = null;

            // Reproduce el sonido de expulsión/lanzamiento
            if (throwAudioSource != null)
            {
                throwAudioSource.Play();
            }

            objectToThrow.AddForce(
                cameraTransform.forward * currentThrowForce,
                ForceMode.Impulse
            );
        }

        isChargingThrow = false;
        currentThrowForce = minThrowForce;

        // Oculta la barra de fuerza una vez lanzado el objeto
        HideForceBar();
    }

    private void HideForceBar()
    {
        if (forceBar != null)
            forceBar.gameObject.SetActive(false);

        if (forceBarFrame != null)
            forceBarFrame.gameObject.SetActive(false);
    }

    // Suelta el objeto (ya con su escala real) sin que quede incrustado en el suelo,
    // en una pared o en el propio jugador.
    // Antes el objeto volvia a su tamano completo dentro de la geometria y la fisica lo
    // expulsaba de golpe: por eso salia disparado, atravesaba el suelo cerca de los bordes
    // o empujaba al jugador a traves del piso (y aparecia la pantalla de derrota).
    private void ReleaseSafely(Rigidbody rb)
    {
        Collider[] objectColliders = heldColliders;

        if (objectColliders == null)
        {
            objectColliders = rb.GetComponentsInChildren<Collider>(true);
        }

        // 1) Si entre la camara y el punto donde flota el objeto hay una pared o el suelo,
        //    se acerca el objeto hasta ese punto (los colliders del objeto siguen apagados).
        if (cameraTransform != null)
        {
            Vector3 origin = cameraTransform.position;
            Vector3 toObject = rb.transform.position - origin;
            float distance = toObject.magnitude;

            if (distance > 0.001f &&
                Physics.Raycast(origin, toObject / distance, out RaycastHit hit, distance,
                    releaseObstacleMask, QueryTriggerInteraction.Ignore) &&
                !IsPlayerCollider(hit.collider))
            {
                rb.transform.position = hit.point - (toObject / distance) * 0.05f;
            }
        }

        // 2) Se prenden los colliders y se ignora el choque con el jugador hasta que se separen.
        foreach (Collider col in objectColliders)
        {
            if (col == null)
                continue;

            col.enabled = true;

            foreach (Collider playerCol in playerColliders)
            {
                if (playerCol != null)
                    Physics.IgnoreCollision(col, playerCol, true);
            }
        }

        Physics.SyncTransforms();

        // 3) Se saca el objeto de cualquier cosa en la que haya quedado metido, con suavidad.
        Depenetrate(rb, objectColliders);

        // 4) Recien ahora vuelve a ser fisico, sin velocidad residual y con deteccion continua
        //    para que no atraviese el suelo al caer o al ser lanzado.
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        StartCoroutine(RestorePlayerCollisionWhenSeparated(objectColliders));
    }

    private void Depenetrate(Rigidbody rb, Collider[] objectColliders)
    {
        for (int iteration = 0; iteration < depenetrationIterations; iteration++)
        {
            bool moved = false;

            foreach (Collider col in objectColliders)
            {
                if (col == null || !col.enabled || col.isTrigger)
                    continue;

                Bounds bounds = col.bounds;

                int count = Physics.OverlapBoxNonAlloc(
                    bounds.center,
                    bounds.extents + Vector3.one * releaseSkin,
                    overlapBuffer,
                    Quaternion.identity,
                    releaseObstacleMask,
                    QueryTriggerInteraction.Ignore
                );

                for (int i = 0; i < count; i++)
                {
                    Collider other = overlapBuffer[i];

                    if (other == null || other.isTrigger || IsPlayerCollider(other) ||
                        System.Array.IndexOf(objectColliders, other) >= 0)
                        continue;

                    if (Physics.ComputePenetration(
                            col, col.transform.position, col.transform.rotation,
                            other, other.transform.position, other.transform.rotation,
                            out Vector3 direction, out float penetration))
                    {
                        rb.transform.position += direction * (penetration + releaseSkin);
                        Physics.SyncTransforms();
                        moved = true;
                    }
                }
            }

            if (!moved)
                break;
        }
    }

    private System.Collections.IEnumerator RestorePlayerCollisionWhenSeparated(Collider[] objectColliders)
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (!IsOverlappingPlayer(objectColliders))
                break;
        }

        foreach (Collider col in objectColliders)
        {
            if (col == null)
                continue;

            foreach (Collider playerCol in playerColliders)
            {
                if (playerCol != null)
                    Physics.IgnoreCollision(col, playerCol, false);
            }
        }
    }

    private bool IsOverlappingPlayer(Collider[] objectColliders)
    {
        foreach (Collider playerCol in playerColliders)
        {
            if (playerCol == null || !playerCol.enabled || playerCol.isTrigger)
                continue;

            // Se usa un margen chico para no devolver el choque justo cuando se tocan
            int count;

            if (playerCol is CharacterController cc)
            {
                Transform t = cc.transform;
                float scale = Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);
                float radius = (cc.radius + cc.skinWidth) * scale + releaseSkin;
                float halfHeight = Mathf.Max(cc.height * scale * 0.5f, radius);
                Vector3 center = t.TransformPoint(cc.center);
                Vector3 offset = Vector3.up * (halfHeight - radius);

                count = Physics.OverlapCapsuleNonAlloc(
                    center + offset, center - offset, radius,
                    overlapBuffer, ~0, QueryTriggerInteraction.Ignore);
            }
            else
            {
                Bounds bounds = playerCol.bounds;

                count = Physics.OverlapBoxNonAlloc(
                    bounds.center, bounds.extents + Vector3.one * releaseSkin,
                    overlapBuffer, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
            }

            for (int i = 0; i < count; i++)
            {
                if (System.Array.IndexOf(objectColliders, overlapBuffer[i]) >= 0)
                    return true;
            }
        }

        return false;
    }

    private bool IsPlayerCollider(Collider col)
    {
        return col != null && System.Array.IndexOf(playerColliders, col) >= 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 detectionPosition = GetPickupDetectionPosition();

        Gizmos.DrawWireSphere(
            detectionPosition,
            pickupRadius
        );
    }
}