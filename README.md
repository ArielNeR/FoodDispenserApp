# 🐾 FoodDispenserApp

**FoodDispenserApp** es una aplicación multiplataforma desarrollada con [.NET MAUI](https://learn.microsoft.com/en-us/dotnet/maui/) que permite monitorear y controlar un dispensador de alimentos para mascotas en tiempo real. La app se comunica con un backend (por ejemplo, una Raspberry Pi) mediante HTTP o MQTT, dependiendo de si el usuario está conectado a la red local o de forma remota.

---

## 📱 Funcionalidades

- ✅ **Monitoreo en tiempo real**:
  - Temperatura
  - Humedad
  - Nivel de comida

- 📊 **Visualización de datos**:
  - Gráficos dinámicos con [Microcharts](https://github.com/aloisdeniel/microcharts)
  - Historial de los últimos 10 valores

- ⚙️ **Control del motor**:
  - Activación manual del dispensador

- ⏰ **Gestión de horarios**:
  - Visualización de horarios de dispensación
  - Edición de horarios y duraciones

- 🌐 **Conectividad dual**:
  - Modo local: comunicación HTTP con la Raspberry Pi
  - Modo remoto: comunicación MQTT a través de broker público (ej. HiveMQ)

- 🔄 **Actualización automática**:
  - Refresco cada 3 minutos o en tiempo real vía MQTT

---

## 🧪 Tecnologías utilizadas

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [.NET MAUI](https://dotnet.microsoft.com/en-us/apps/maui)
- [MQTTnet](https://github.com/dotnet/MQTTnet) para comunicación MQTT
- [Microcharts.Maui](https://www.nuget.org/packages/Microcharts.Maui) para gráficos
- [SkiaSharp](https://github.com/mono/SkiaSharp) para renderizado de gráficos
- Arquitectura MVVM (Model-View-ViewModel)

---

## 🗂️ Estructura del proyecto

```
FoodDispenserApp/
├── Models/                    # Modelos de datos (SensorData, Horario, etc.)
├── Services/                  # Servicios de API, MQTT, conectividad
├── ViewModels/                # Lógica de presentación (MVVM)
├── Views/                     # Interfaces de usuario (.xaml)
├── Platforms/                 # Código específico por plataforma
├── Resources/                 # Estilos, colores, fuentes, etc.
├── App.xaml                   # Recursos globales
├── AppShell.xaml              # Navegación principal
├── MainPage.xaml              # Vista principal
├── MauiProgram.cs             # Configuración de inyección de dependencias
└── FoodDispenserApp.csproj    # Archivo del proyecto
```

---

## 🔧 Configuración y ejecución

### Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) con carga de trabajo de .NET MAUI
- Dispositivo o emulador Android, iOS, Windows o macOS

### Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/FoodDispenserApp.git
cd FoodDispenserApp
```

### Configurar la IP del backend

En `MauiProgram.cs`, actualiza la URL base del backend:

```csharp
client.BaseAddress = new Uri("http://192.168.100.82:8000/");
```

### Ejecutar la aplicación

```bash
dotnet build
dotnet run -t android
```

O usa Visual Studio para compilar y ejecutar en el dispositivo deseado.

---

## 🧠 Lógica de conectividad

La app detecta automáticamente si estás en la red local (WiFi) o no:

- **Modo local**: consulta HTTP cada 3 minutos al backend
- **Modo remoto**: se conecta a un broker MQTT y recibe datos en tiempo real

---

## 📡 Endpoints y tópicos

### API REST (modo local)

| Método | Endpoint            | Descripción                    |
|--------|---------------------|--------------------------------|
| GET    | `/temperature`       | Obtener temperatura            |
| GET    | `/humidity`          | Obtener humedad                |
| GET    | `/food_level`        | Obtener nivel de comida        |
| GET    | `/horarios`          | Obtener horarios configurados  |
| POST   | `/horarios`          | Actualizar horarios            |
| POST   | `/activate_motor`    | Activar el motor               |

### MQTT (modo remoto)

| Tópico               | Tipo     | Descripción                          |
|----------------------|----------|--------------------------------------|
| `sensor/updates`     | Suscripción | Datos de sensores en tiempo real |
| `commands/activate_motor` | Publicación | Activar el motor                |

---

## 📌 Notas importantes

- El broker MQTT actual es `broker.hivemq.com` (público). Puedes cambiarlo por uno privado.
- Asegúrate de que el backend esté corriendo y sea accesible desde el dispositivo.
- La app está diseñada para trabajar con una API REST y/o MQTT, no ambas al mismo tiempo.

---

## 📄 Licencia

Este proyecto es de código abierto y está disponible para uso educativo y personal.

---