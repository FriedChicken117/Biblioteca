Biblioteca  Aplicación ASP.NET Core con XML y Razor Pages

 Funcionalidades principales:
 Listado de libros en la página de inicio, con búsqueda por:
   Título / Autor (campo de texto general)
   Autor
   Categoría
 Detalles de un libro:
   Título, autor, categoría y resumen.
   Listado de reseñas ordenadas de la más reciente a la más antigua.
 Reseñas de usuarios:
   Calificación de 1 a 5.
   Comentario de texto.
   Solo los usuarios autenticados pueden crear reseñas.
 Autenticación de usuarios (login/registro/cerrar sesión):
   Sistema de usuarios almacenados en Data/users.xml.
   Contraseñas hasheadas con SHA256.
 Usuario administrador:
   Es el único usuario marcado como administrador.
   Puede crear nuevos libros desde la página Admin > Nuevo libro.

 Tecnologías usadas:
 ASP.NET Core 8.0
 Razor Pages
 Autenticación por cookies (Microsoft.AspNetCore.Authentication.Cookies)
 LINQ to XML (System.Xml.Linq)
 Bootstrap 5 para el diseño visual.

 Ejecución del proyecto:
  1.Instalar Visual Studio 2022 o superior y el SDK de .NET 8.
  2.Clonar el proyecto con el siguiente enlace:
    https://github.com/FriedChicken117/Biblioteca.git
    o descargar en formato zip desde el repositorio de GitHub.
  3.Restaurar y compilar:
    dotnet build
  4.Ejecutar.

 Usuario administrador

 Usuario: admin
 Contraseña: admin

Este usuario:

 Está almacenado en Data/users.xml con IsAdmin=true.
 Es el único usuario al que se le asigna el rol Admin.
 Tiene acceso a la página Admin/Nuevo libro (/Admin/Books/Create) para crear nuevos libros.




