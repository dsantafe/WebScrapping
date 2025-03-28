# Utiliza la imagen oficial de .NET SDK 8.0 para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Expone el puerto 80 para la comunicación de red
EXPOSE 80

# Define el directorio de trabajo dentro del contenedor
WORKDIR /source

# Copia el archivo del proyecto .csproj para restaurar las dependencias
COPY ./*.csproj ./

# Restaura las dependencias del proyecto
RUN dotnet restore

# Copia el código fuente restante al contenedor
COPY . .

# Publica la aplicación en modo release como un ejecutable independiente para Linux
RUN dotnet publish -c release -o /app --self-contained -r linux-x64

# Usa la imagen de Selenium con Chrome preinstalado como base para la ejecución
FROM selenium/standalone-chrome

# Define el directorio de trabajo para la ejecución de la aplicación
WORKDIR /app

# Copia los archivos compilados desde la imagen de compilación
COPY --from=build /app .
USER 0
# Establece el punto de entrada ejecutando la aplicación con permisos de superusuario
ENTRYPOINT ["./WebScrapping"]
