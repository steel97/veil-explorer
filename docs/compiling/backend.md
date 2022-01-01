# Backend compiling

## Setup

Make sure to install the dependencies

```bash
dotnet restore
```

## Development
Copy settings template **appsettings.json.tpl** to **appsettings.json**

Change **postgresql** connection string in **appsettings.json**

Start the development server on http://localhost:5000

```bash
dotnet run
```

## Production

Build the application for production:

```bash
dotnet publish ./explorer-backend.csproj -c Release -o ./output/linux-x64 --self-contained -r linux-x64
```
Or use **windows** batch script:
```bash
./create-sc-build.bat
```