# WebScrapping

## Overview
WebScrapping is a .NET Core console application that automates the extraction of data from various websites. The application processes the collected data and stores it in a database for further analysis.

## Features
- Extracts relevant information from multiple websites.
- Processes and stores scraped data in a SQL database.
- Supports scheduled execution using Azure Container Apps or Logic Apps.
- Uses dependency injection for better scalability and maintainability.

## Requirements
- .NET 8 SDK
- Docker (optional, for containerized deployment)
- SQL Server or any supported database

## Installation
1. Clone the repository:
   ```sh
   git clone https://github.com/dsantafe/WebScrapping.git
   cd WebScrapping
   ```
2. Restore dependencies:
   ```sh
   dotnet restore
   ```
3. Build the project:
   ```sh
   dotnet build
   ```
4. Run the application:
   ```sh
   dotnet run
   ```

## Configuration
The application requires a configuration file (`appsettings.json`) in the root directory. Example:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=yourserver;Database=yourdb;User Id=youruser;Password=yourpassword;"
  }
}
```

## Deployment
### Running in Docker
Create a Docker image and run the container:
```sh
docker build -t webscrapping .
docker run --rm webscrapping
```

### Running in Azure Container Apps
Deploy the application using Azure Container Apps:
```sh
az containerapp job create \
  --name webscrapping-job \
  --resource-group my-resource-group \
  --trigger-type Scheduled \
  --cron-expression "0,30 * * * *" \
  --image yourdockerhub/webscrapping:latest
```

## License
This project is licensed under the MIT License.
