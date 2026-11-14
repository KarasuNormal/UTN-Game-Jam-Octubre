using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Control de volumen para un panel de UI.
// - Si se le asigna un Slider (y opcionalmente un texto) desde el Inspector, usa esos.
// - Si no, crea solo un texto "Volumen" + un slider con el estilo del panel donde esta
//   (el PauseManager lo agrega automaticamente al panel de pausa).
public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label;
    [SerializeField] private string labelPrefix = "Volumen";

    [Header("Creacion automatica (solo si no hay Slider asignado)")]
    [SerializeField] private Vector2 anchoredPosition = new Vector2(0f, -400f);
    [SerializeField] private Vector2 sliderSize = new Vector2(520f, 34f);
    [SerializeField] private float labelFontSize = 70f;
    [SerializeField] private Color backgroundColor = new Color(0.22f, 0.05f, 0.06f, 1f);
    [SerializeField] private Color fillColor = new Color(1f, 0.745f, 0.763f, 1f);
    [SerializeField] private Color handleColor = Color.white;
    [Tooltip("Agrega un fondo oscuro detras del control (util sobre fondos con dibujo, como el menu principal).")]
    [SerializeField] private bool addBackdrop = false;
    [SerializeField] private Color backdropColor = new Color(0f, 0f, 0f, 0.6f);

    private bool initialized;

    // Crea un control de volumen como hijo de "parent" en la posicion indicada.
    public static VolumeSlider Create(Transform parent, Vector2 position, bool withBackdrop)
    {
        GameObject volumeObject = new GameObject("Volumen", typeof(RectTransform));
        volumeObject.layer = parent.gameObject.layer;
        volumeObject.transform.SetParent(parent, false);

        VolumeSlider volumeSlider = volumeObject.AddComponent<VolumeSlider>();
        volumeSlider.anchoredPosition = position;
        volumeSlider.addBackdrop = withBackdrop;
        volumeSlider.Initialize();

        return volumeSlider;
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (initialized)
        {
            // Por si el volumen cambio desde otro lado (por ejemplo desde otra escena)
            slider.SetValueWithoutNotify(VolumeSettings.Volume);
            UpdateLabel(VolumeSettings.Volume);
        }
    }

    private void OnDisable()
    {
        VolumeSettings.Save();
    }

    public void Initialize()
    {
        if (initialized)
            return;

        if (slider == null)
        {
            BuildUI();
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(VolumeSettings.Volume);
        slider.onValueChanged.AddListener(OnSliderChanged);

        UpdateLabel(VolumeSettings.Volume);
        initialized = true;
    }

    private void OnSliderChanged(float value)
    {
        VolumeSettings.SetVolume(value);
        UpdateLabel(value);
    }

    private void UpdateLabel(float value)
    {
        if (label != null)
        {
            label.text = $"{labelPrefix}: {Mathf.RoundToInt(value * 100f)}%";
        }
    }

    private void BuildUI()
    {
        RectTransform root = GetComponent<RectTransform>();

        if (root == null)
        {
            root = gameObject.AddComponent<RectTransform>();
        }

        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = anchoredPosition;
        root.sizeDelta = new Vector2(sliderSize.x, labelFontSize + sliderSize.y + 20f);
        root.localScale = Vector3.one;

        // ---- Fondo opcional ----
        if (addBackdrop)
        {
            GameObject backdropObject = new GameObject("Fondo Volumen", typeof(RectTransform));
            backdropObject.layer = gameObject.layer;
            RectTransform backdropRect = backdropObject.GetComponent<RectTransform>();
            backdropRect.SetParent(root, false);
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.sizeDelta = new Vector2(80f, 30f); // margen alrededor del texto y el slider

            Image backdrop = backdropObject.AddComponent<Image>();
            backdrop.color = backdropColor;
            backdrop.raycastTarget = false;
        }

        // ---- Texto ----
        GameObject labelObject = new GameObject("Texto Volumen", typeof(RectTransform));
        labelObject.layer = gameObject.layer;
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(root, false);
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(200f, labelFontSize + 10f); // ancho extra para que no corte el texto

        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        CopyTextStyleFromPanel(text);
        text.fontSize = labelFontSize;
        text.enableAutoSizing = false;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        label = text;

        // ---- Slider ----
        GameObject sliderObject = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sliderObject.name = "Slider Volumen";
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.SetParent(root, false);
        sliderRect.anchorMin = new Vector2(0.5f, 0f);
        sliderRect.anchorMax = new Vector2(0.5f, 0f);
        sliderRect.pivot = new Vector2(0.5f, 0f);
        sliderRect.anchoredPosition = Vector2.zero;
        sliderRect.sizeDelta = sliderSize;

        SetLayerRecursively(sliderObject.transform, gameObject.layer);

        slider = sliderObject.GetComponent<Slider>();

        // Colores (DefaultControls crea las imagenes sin sprite: rectangulos planos)
        Transform background = sliderObject.transform.Find("Background");
        if (background != null)
            background.GetComponent<Image>().color = backgroundColor;

        Image fill = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
        if (fill != null)
            fill.color = fillColor;

        if (slider.handleRect != null)
        {
            slider.handleRect.GetComponent<Image>().color = handleColor;
            // Agarradera un poco mas alta y ancha que la barra para que sea facil de usar
            slider.handleRect.sizeDelta = new Vector2(sliderSize.y * 0.8f, sliderSize.y * 0.6f);
        }

        // Colores de estado del slider: un poco mas oscuro al pasar el mouse / arrastrar
        ColorBlock colors = slider.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = Color.white;
        slider.colors = colors;
    }

    // Usa la fuente y el color del titulo del panel: el texto claro mas grande que no este
    // dentro de un boton (los titulos tienen una copia oscura detras como sombra, que se descarta)
    private void CopyTextStyleFromPanel(TextMeshProUGUI target)
    {
        Transform panel = transform.parent;
        TMP_Text best = null;

        if (panel != null)
        {
            foreach (TMP_Text candidate in panel.GetComponentsInChildren<TMP_Text>(true))
            {
                if (candidate == target || candidate.GetComponentInParent<Button>(true) != null)
                    continue;

                if (candidate.color.grayscale < 0.5f)
                    continue;

                if (best == null || candidate.fontSize > best.fontSize)
                {
                    best = candidate;
                }
            }
        }

        if (best != null)
        {
            target.font = best.font;
            target.fontSharedMaterial = best.fontSharedMaterial;
            target.color = best.color;
        }
        else
        {
            target.color = Color.white;
        }
    }

    private static void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;

        foreach (Transform child in t)
        {
            SetLayerRecursively(child, layer);
        }
    }
}
