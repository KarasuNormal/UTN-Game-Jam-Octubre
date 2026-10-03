# GDD — MVP: Magicus Pontis

## 1. Concepto

**Magicus Pontis** es un juego de plataformas y puzzle basado en física, ambientado en un mundo de **islas flotantes amenazadas por una lava que asciende constantemente**.

El jugador controla a un mago equipado con una **honda mágica**. La honda puede utilizarse para lanzar objetos encontrados en las islas. Cuando el jugador la apoya sobre una superficie, la honda se vuelve **gigante**, permitiendo lanzar objetos mucho más grandes o pesados.

Entre las islas existen **zonas mágicas flotantes**. El jugador debe lanzar objetos para que toquen esas zonas. Cuando un objeto entra en contacto con una zona mágica, adquiere la capacidad de **flotar en el aire**.

El objetivo es utilizar esos objetos flotantes como plataformas improvisadas para atravesar el vacío y alcanzar la siguiente isla antes de que la lava alcance al jugador.

---

# 2. Lanzamiento de objetos

### 2.1. Lanzar para crear plataformas

El jugador no recibe plataformas predeterminadas. Debe transformar objetos del escenario en plataformas mediante la honda y las zonas mágicas.

Mecánica básica:
**Lanzar → tocar magia → objeto flotante → saltar sobre él.**

### 2.2. Física y experimentación

Los objetos conservan sus características físicas básicas.

El jugador debe resolver teniendo en cuenta:

- peso;
- tamaño;
- forma;
- posición;
- trayectoria;
- distancia;
- cantidad de objetos disponibles.

### 2.3. Presión constante

La lava asciende continuamente.

El jugador tiene tiempo suficiente para pensar, pero no puede quedarse indefinidamente en una isla.

### 2.4. Progresión espacial

Cada isla funciona como un pequeño desafío.

El jugador debe descubrir cómo utilizar los elementos disponibles para construir un camino hacia la siguiente isla.

---

# 3. Plataforma objetivo del MVP

**PC**

Controles principales:

- teclado / gamepad

---

# 4. Cámara

Vista **3D en tercera persona**, con cámara orientada hacia el área de juego.

La cámara deberá permitir visualizar:

- al mago;
- la isla actual;
- la siguiente isla;
- las zonas mágicas;
- los objetos utilizables;
- la lava.

El MVP no requiere cámara libre completamente independiente.

---

# 5. Objetivo del jugador

El objetivo principal de cada nivel es:

**Llegar desde la primera isla hasta la última sin tocar la lava.**

Al alcanzar la última isla:

- el nivel termina;
- se muestra una transición;
- comienza el siguiente nivel.

En el **nivel final**, la última isla conduce finalmente a **tierra firme**.

Al llegar a tierra firme:

**el jugador escapa y termina el juego.**

---

# 6. Bucle principal de gameplay

El gameplay principal sigue este ciclo:

```text
Explorar isla
      ↓
Buscar objetos utilizables
      ↓
Identificar zona mágica
      ↓
Apuntar con la honda
      ↓
Lanzar objeto
      ↓
Objeto atraviesa zona mágica
      ↓
Objeto comienza a flotar
      ↓
Usar objeto como plataforma
      ↓
Saltar
      ↓
Alcanzar siguiente isla
      ↓
Repetir
```

La lava asciende durante todo el proceso.

---

# 7. El jugador

## Mago

El jugador controla un personaje humanoide mágico.

### Capacidades

El mago puede:

- caminar;
- correr;
- saltar;
- apuntar;
- utilizar la honda;
- recoger/seleccionar objetos;
- lanzar objetos;
- utilizar objetos flotantes como plataformas.

### Restricciones

El mago:

- no puede volar;
- no puede atravesar el vacío;
- no puede sobrevivir al contacto con la lava;
- no puede crear objetos;
- no puede mover libremente objetos sin utilizar la honda.

---

# 8. La honda mágica

La honda es la principal herramienta del jugador.

## Estado normal

Cuando el jugador sostiene la honda normalmente:

- puede lanzar objetos pequeños;
- tiene alcance limitado;
- tiene una potencia determinada.

## Estado gigante

Cuando la honda se encuentra **apoyada sobre una superficie**, aumenta de tamaño.

```text
        HONDONORMAL
            ↓
       apoyarla
            ↓
     ┌─────────────┐
     │    MAGIA    │
     │      ↓      │
     │  H O N D A  │
     │   GIGANTE   │
     └─────────────┘
```

En este estado puede lanzar objetos.


---

# 9. Apuntado

Al utilizar la honda se muestra el vector de lanzamiento y la potencia.

El jugador debe ajustar:

- dirección izquierda-derecha;
- ángulo de altura;
- potencia.

---

# 10. Objetos

Las islas contienen objetos físicos que pueden utilizarse como proyectiles.

Ejemplos para el MVP:

### Piedra

- pequeña;
- pesada;
- fácil de lanzar;
- puede convertirse en pequeña plataforma.

### Caja

- tamaño medio;
- peso medio;
- buena plataforma.

### Barril

- grande;
- relativamente pesado;
- puede rodar;
- difícil de controlar.

### Tronco

- largo;
- relativamente liviano;
- excelente como plataforma horizontal.

---

# 11. Objetos flotantes

Cuando un objeto atraviesa una zona mágica válida:

```text
OBJETO NORMAL
      ↓
zona mágica
      ↓
OBJETO ENCANTADO
      ↓
comienza a flotar
```

El objeto deja de estar sujeto normalmente a la gravedad.

Puede:

- permanecer suspendido;
- moverse ligeramente;
- funcionar como plataforma;
- ser utilizado por el jugador para saltar.

---

# 12. Estado mágico del objeto

Cada objeto puede tener dos estados:

### Normal

```text
Gravity = ON
Floating = false
```

### Encantado

```text
Gravity = OFF
Floating = true
```

Opcionalmente, el objeto puede recibir una animación visual:

- partículas;
- brillo;
- aura;
- cambio de color;
- pequeñas partículas ascendentes.

---

# 13. Zonas mágicas

Las zonas mágicas son áreas suspendidas en el espacio entre las islas.

Visualmente pueden representarse como nubes de partículas.

Su función es detectar objetos que las atraviesan.

## Regla principal

**Solo los objetos que atraviesen una zona mágica se vuelven flotantes.**

El jugador no puede utilizar directamente la zona como plataforma.

---

# 14. Plataformas creadas por el jugador

Una vez encantado, el objeto queda suspendido.

El jugador puede utilizarlo para:

- saltar desde la isla;
- ganar altura;
- cambiar de dirección;
- alcanzar otro objeto flotante;
- llegar a la siguiente isla.

Esto permite que una misma situación tenga diferentes soluciones.


---

# 15. Salto

El salto debe ser suficientemente controlable para que el jugador pueda:

- aterrizar sobre objetos pequeños;
- corregir ligeramente su trayectoria en el aire;
- saltar entre plataformas flotantes.

El desafío debe provenir principalmente de:

**crear y posicionar las plataformas.**

---

# 16. Lava

La lava es el principal elemento de presión.

## Comportamiento

La lava comienza debajo del escenario y asciende progresivamente.

Su velocidad puede variar por nivel.

---

# 17. Condición de derrota

El jugador pierde inmediatamente si:

- toca la lava;
- cae y entra en contacto con la lava.

Al perder:

1. se detiene el gameplay;
2. se muestra una animación breve;
3. aparece el mensaje **"Has caído"**;
4. se permite reiniciar el nivel.

Opcionalmente:

**Reintentar** / **Salir al menú**

---

# 18. Diseño de niveles

Cada nivel está formado por una secuencia de islas.

Ejemplo:

```text
[ISLA 1]
    ↓
    ✦
    ↓
[ISLA 2]
    ↓
    ✦
    ↓
[ISLA 3]
    ↓
    ✦
    ↓
[ISLA 4]
```


---

# 19. Nivel MVP

El MVP puede contener **3 niveles normales + 1 nivel final**.

## Nivel 1 — Aprendiz

Objetivo:

Enseñar la mecánica básica.

Características:

- pocas islas;
- grandes zonas mágicas;
- objetos fáciles de lanzar;
- lava lenta;
- trayectorias sencillas.

El jugador aprende:

1. utilizar la honda;
2. lanzar objetos;
3. activar objetos con magia;
4. saltar sobre objetos flotantes.

---

## Nivel 2 — Distancia

Características:

- zonas mágicas más alejadas;
- objetos más pesados;
- mayor precisión.

El jugador debe comprender:

> La honda gigante permite resolver situaciones que la honda normal no puede.

---

## Nivel 3 — Encadenamiento

Introduce varios objetos flotantes.

El jugador debe crear una cadena de plataformas.

La lava asciende más rápidamente.

---

## Nivel 4 — Escape

Nivel final.

La dificultad aumenta y el jugador debe utilizar todas las mecánicas aprendidas.


Cuando el jugador llega a tierra firme:

- la lava queda atrás;
- se detiene la cámara;
- aparece una breve animación;
- se muestra el mensaje:

**"¡Escapaste!"**

---

# 20. Progresión de dificultad

La dificultad debe aumentar mediante:

| Variable | Fácil | Difícil |
|---|---|---|
| Distancia entre islas | Corta | Larga |
| Tamaño de zonas mágicas | Grande | Pequeño |
| Cantidad de objetos | Muchos | Limitados |
| Peso de objetos | Bajo | Alto |
| Precisión requerida | Baja | Alta |
| Velocidad de lava | Baja | Alta |
| Cantidad de saltos | Pocos | Muchos |
| Plataformas necesarias | 1 | Varias |

---

# 21. HUD

El MVP requiere un HUD muy simple.

### Elementos

**Nivel**

```text
Nivel 2
```

**Altura de lava**

Puede mostrarse visualmente mediante una pequeña barra:

```text
LAVA
██████░░░░
```

---

# 22. Feedback visual

El jugador debe entender inmediatamente cuándo un objeto se transforma.

### Objeto normal

Aspecto normal.

### Entrada a zona mágica

Pequeña explosión de partículas.

### Objeto encantado

- brillo;
- partículas;
- aura;
- animación de suspensión.

### Lava acercándose

- sonido;
- vibración sutil;
- aumento de partículas;
- cambio progresivo de iluminación.

---

# 23. Audio

## Música

Música ambiental mágica y aventurera.

Debe aumentar ligeramente la tensión a medida que la lava asciende.

## Efectos

- lanzamiento de honda;
- impacto;
- activación de zona mágica;
- transformación de objeto;
- salto;
- aterrizaje;
- lava;
- muerte;
- completar nivel.

---

# 24. Sistema de objetos

Cada objeto interactuable debe mostrar claro cuando:

- puede ser lanzado;
- cambia su comportamiento físico al hacerse mágico

---

# 25. Condiciones de nivel

## Victoria

```text
Jugador entra en GoalZone
        ↓
Nivel completado
        ↓
Transición
        ↓
Siguiente nivel
```

## Derrota

```text
Jugador toca Lava
        ↓
PlayerDeath
        ↓
Game Over
        ↓
Reintentar
```

---

# 26. Arte

### Objetos

Diseño reconocible inmediatamente.

La legibilidad es más importante que el detalle.

### Lava

Debe ser visualmente muy clara.

Debe comunicar:

> "Si llegás hasta acá, perdés."

---

# 27. Requerimientos mínimos de contenido

Para considerar terminado el MVP:

### Personaje

- 1 mago.

### Herramienta

- 1 honda.

### Objetos

- 3 tipos.

### Mecánica

- lanzamiento;
- honda gigante;
- zonas mágicas;
- objetos flotantes;
- plataformas;
- salto.

### Peligro

- lava ascendente.

### Niveles

- 3 niveles de prueba;
- 1 nivel final.

### UI

- menú principal;
- HUD;
- victoria;
- derrota;
- reinicio.

---

# 28. Lo que NO entra en el MVP

Para evitar que el proyecto crezca demasiado, quedan fuera inicialmente:

- enemigos;
- combate;
- inventario;
- sistema de experiencia;
- habilidades desbloqueables;
- múltiples personajes;
- crafting;
- narrativa compleja;
- diálogos;
- multiplayer;
- procedurally generated levels;
- skins;
- tienda;
- monetización;
- ranking online;
- checkpoints complejos.

---

# 29. Criterio de éxito del MVP

El MVP funciona si un jugador que nunca vio el juego puede:

1. entender que debe escapar de la lava;
2. descubrir que puede lanzar objetos;
3. descubrir que las zonas mágicas convierten objetos en plataformas;
4. utilizar esas plataformas para cruzar el vacío;
5. descubrir la honda gigante;
6. completar al menos un nivel sin instrucciones externas;
7. morir al tocar la lava;
8. reiniciar el nivel;
9. llegar a la tierra firme en el nivel final.

---

# 30. Vertical Slice recomendado

Antes de construir los cuatro niveles, se recomienda crear una única escena que contenga:

```text
          ISLA B
       ┌───────────┐
       │           │
       └───────────┘

          [TRONCO]
             ↓
            ✦
             ↓

       [CAJA FLOTANTE]

             ↓

       ┌───────────┐
       │   MAGO    │
       │           │
       └───────────┘

~~~~~~~~~~~~~~~~~~~~~~~~
          LAVA
~~~~~~~~~~~~~~~~~~~~~~~~
```

Esta escena debe demostrar **todo el core loop**:

**apuntar → lanzar → activar magia → crear plataforma → saltar → llegar a otra isla → escapar de la lava.**