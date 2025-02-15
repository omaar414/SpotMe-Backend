SpotMe API - Real-Time Location Tracker

SpotMe API es una API REST construida con ASP.NET Core Web API que permite a los usuarios registrarse, autenticarse y compartir su ubicación en tiempo real con amigos. Además, permite gestionar solicitudes de amistad y obtener la ubicación de los amigos en un mapa interactivo.

📌 Tecnologías Usadas
	•	Backend: ASP.NET Core Web API
	•	Base de Datos: PostgreSQL con Entity Framework Core
	•	Autenticación: JWT (JSON Web Tokens)
	•	Geolocalización: API del navegador y almacenamiento en BD
	•	Mapas: Leaflet.js con OpenStreetMap

🚀 Instalación y Configuración

1️⃣ Clonar el repositorio

cd spotme-api

2️⃣ Configurar la Base de Datos

Asegúrate de tener PostgreSQL instalado y crea una base de datos llamada SpotMeDB. Luego, edita el archivo appsettings.json y configura tu conexión:

"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=SpotMeDB;Username=tu_usuario;Password=tu_contraseña"
}

3️⃣ Aplicar Migraciones con Entity Framework

Ejecuta los siguientes comandos para aplicar las migraciones y actualizar la base de datos:

dotnet ef migrations add InitialCreate
dotnet ef database update

4️⃣ Ejecutar la API

dotnet watch run

La API estará disponible en http://localhost:5000 o https://localhost:5001.

🔑 Autenticación y Seguridad
	•	Registro de Usuario: Se usa BCrypt para encriptar contraseñas.
	•	Inicio de Sesión: Se genera un JWT Token que el usuario debe enviar en las solicitudes protegidas.
	•	Middleware de Autorización: Protege endpoints con [Authorize] en los controladores.

📍 Endpoints de la API

🔹 Autenticación

📝 Registro de Usuario

POST /api/auth/register

Body (JSON)

{
  "username": "johndoe",
  "password": "123456"
}

Respuesta:

{
  "message": "User created successfully!"
}

🔑 Iniciar Sesión

POST /api/auth/login

Body (JSON)

{
  "username": "johndoe",
  "password": "123456"
}

Respuesta (JWT Token):

{
  "token": "eyJhbGciOiJIUzI1..."
}

🔹 Amigos y Ubicación

📥 Enviar Solicitud de Amistad

POST /api/friendship/send-request
Authorization: Bearer {token}

Body (JSON)

{
  "receiverUsername": "friend123"
}

Respuesta:

{
  "message": "Friend request sent successfully!"
}

✅ Aceptar Solicitud de Amistad

POST /api/friendship/accept-request
Authorization: Bearer {token}

Body (JSON)

{
  "senderUsername": "friend123"
}

Respuesta:

{
  "message": "Friend request accepted!"
}

❌ Rechazar Solicitud de Amistad

POST /api/friendship/reject-request
Authorization: Bearer {token}

Body (JSON)

{
  "senderUsername": "friend123"
}

Respuesta:

{
  "message": "Friend request rejected!"
}

👥 Obtener Lista de Amigos y Ubicación

GET /api/friendship/friends
Authorization: Bearer {token}

Respuesta:

[
  {
    "username": "friend123",
    "latitude": 18.2208,
    "longitude": -66.5901,
    "updatedAt": "2025-02-15T14:30:00Z"
  }
]

🌍 Integración con Frontend
	•	El frontend en React.js consume los endpoints de esta API.
	•	Se usa fetch para obtener y actualizar ubicaciones en tiempo real cada 30 segundos.
	•	Se renderizan los amigos en un mapa interactivo con Leaflet.js.

🛡️ Seguridad Implementada
	•	Protección contra SQL Injection con Entity Framework.
	•	Autenticación JWT para evitar accesos no autorizados.
	•	Cifrado de contraseñas con BCrypt.
	•	Validación de datos para evitar inputs maliciosos.

🏗️ Futuras Mejoras

✔️ Notificaciones en tiempo real con WebSockets
✔️ Creación de Zonas Seguras para alertas de ubicación
✔️ Modo invisible para ocultar ubicación temporalmente

