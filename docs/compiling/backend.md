# Backend compiling
[Dotnet 6+ (SDK)](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) required to compile backend.

## Setup

Make sure to install the dependencies

```bash
dotnet restore
```

## Development
Copy settings template **appsettings.json.tpl** to **appsettings.json**

Change **postgresql** connection string and other settings in **appsettings.json** for local development

Start the development server on http://localhost:5000

```bash
dotnet run
```

## Production
Copy settings template **appsettings.json.tpl** to **appsettings.Production.json**

Change **postgresql** connection string and other settings in **appsettings.Production.json** for production environment

Build the application for production:

```bash
dotnet publish ./explorer-backend.csproj -c Release -o ./output/linux-x64 --self-contained -r linux-x64
```
Or use **windows** batch script:
```bash
./create-sc-build.bat
```