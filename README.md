## Miembros del equipo
David Santos Requena
Roberto Ortuño Morcillo
Nicolás Martínez López
Miguel Torregrosa López
Saúl Cánovas Navarro 

## Descripción del Proyecto
**123-Punch** Es un videojuego desarrollado en Unity. Se trata de un juego interactivo con mecánicas que involucran combates (vs Bot y modo Historia) y funcionalidades sociales como un chat global gestionado a través de bases de datos. El juego cuenta con sistemas de personajes, menús interactivos, chat en tiempo real, combates y estadísticas de jugadores.

## Tecnologías Utilizadas
* **Motor Gráfico:** Unity 6000.2.11f1
* **Lenguaje de Programación:** C#
* **Backend y Base de Datos:** Firebase (Firestore / Realtime Database)
* **Control de Versiones:** Git y GitHub

## Arquitectura General del Sistema
El sistema se basa en la arquitectura de **Componentes de Unity (MonoBehaviour)** y patrones de diseño orientados a servicios (Singleton para managers). La lógica del juego está separada en diferentes dominios:
* **UI/Views:** Gestión de la interfaz de usuario, menús y ventanas de chat.
* **Services:** Servicios que conectan el cliente con el backend (Firebase) para sincronización de datos y chat en tiempo real.
* **Data/ScriptableObjects:** Definición de estadísticas, personajes y configuraciones generales del juego.

## Requisitos Previos
* **Unity Hub** instalado en el sistema.
* **Unity Versión:** `6000.2.11f1` (Obligatorio para evitar conflictos de serialización o incompatibilidades).
* **Git** instalado.
* **IDE recomendado:** Visual Studio 2022 o Visual Studio Code con los plugins de C# y Unity configurados.

## Instalación
Sigue estos pasos para preparar el entorno de desarrollo:
1. Clona este repositorio en tu máquina local:
   ```bash
   git clone https://github.com/ROMOR12/123-Punch.git
   ```
2. Abre **Unity Hub** y selecciona la opción *Add* o *Open*.
3. Navega hasta la carpeta del proyecto clonado y selecciónala.
4. Asegúrate de que el proyecto se abre usando la versión de Unity `6000.2.11f1`. Si te solicita instalar la versión, hazlo desde el mismo Unity Hub.
5. Espera a que Unity importe todos los *assets* y resuelva las dependencias (Firebase, etc.).

## Ejecución
Para poner en marcha el proyecto dentro del editor de Unity:
1. Navega en la ventana de proyecto a la carpeta: `Assets/_Game/Scenes/`.
2. Abre la escena inicial, `LoginScene.unity`.
3. Pulsa el botón de **Play (▶)** en la parte superior del editor para ejecutar el juego.

## Estructura del Repositorio
* `Assets/_Game/`: Contiene el código fuente, arte, animaciones y configuraciones propias del juego.
  * `Art/`: Modelos, animaciones y materiales.
  * `Audio/`: Efectos de sonido y `SoundManager`.
  * `Data/`: ScriptableObjects y configuraciones estáticas de personajes.
  * `Scenes/`: Escenas de Unity (Menú, vsBot, etc).
  * `Scripts/`: Lógica de programación dividida por responsabilidades (UI, Services, Managers).
* `Packages/`: Dependencias gestionadas por el Unity Package Manager.
* `ProjectSettings/`: Configuraciones de construcción, físicas y de editor.
* `FirebaseFunctions/`: Funciones serverless de backend si aplican.

## Configuración
* **Firebase:** Asegúrate de contar con el archivo de configuración `google-services.json`(para Android/PC) proporcionado por el equipo e inclúyelo en la carpeta `Assets/` o la ruta indicada para que los servicios en la nube funcionen correctamente.
* **Resolución:** El juego está diseñado para ejecutarse preferentemente en formato móvil.

## Uso del Sistema
El flujo principal comienza en la pantalla de Menú:
1. **Login/Registro:** Autenticación a través de los menús.
2. **Navegación UI:** Uso del ratón para interactuar con botones de "Jugar", "Mejoras" y "Opciones".
3. **Chat:** Accede a la ventana de chat desde la UI para comunicarte.
4. **Combate:** Controla al personaje en las escenas de pelea con las entradas asignadas.

## Credenciales de Prueba
Actualmente no hay credenciales genéricas por defecto obligatorias en el repositorio público. Durante el desarrollo, puedes registrar tu propia cuenta de prueba en el menú de registro del juego y usarla para probar las mejoras y el chat.

## Estado del Proyecto
**En Desarrollo.** Trabajando activamente en la rama `develop` para la resolución de bugs, mejora de UI/UX, sincronización de la base de datos y nuevas escenas de combate.

## Despliegue y URL
*(Actualmente no hay una versión publicada del proyecto accesible vía URL web. El juego se ejecuta mediante builds locales o desde el propio editor de Unity).*
