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
## VSCode
For development/publishing [vscode](https://code.visualstudio.com/) IDE can be used. There are convinient commands predefined. For backend, make sure to open **explorer-backend** folder in **vscode** (it should be project-root) **or** open **workspace** via:

*veil-explorer.code-workspace*

### Tasks

Use next hotkey:
```bash
CTRL+SHIFT+ P
#or
F1
# than press
backspace
# than type word
task
# than press
space
# below actions will open tasks list dropdown menu
```
This actions can also be opened via: *Terminal > Run task*

Available actions:
```bash
# run application
run
# run with hot-reload (useful for development)
watch
# publish application
publish
# publish self-contained build for linux-x86_64 platform
publish linux-x86_64 (self-contained)
```

### Debugging
Press **F5** to start debugging. Addition configs can be configured in *.vscode/launch.json*