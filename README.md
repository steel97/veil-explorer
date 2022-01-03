# veil-explorer
[Veil Blockchain](https://github.com/Veil-Project/veil) explorer developed with asp.net core and nuxt3

Example: https://explorer.veil-project.com

# Features
- light/dark theme
- multilingual UI (see [docs/localization.md](/docs/localization.md))
- real-time updates of blockchain info, algo stats, recent blocks and first page of blocks browser
- blocks list
- block and transactions information
- search by transaction, address, block hash or height

# API compatibility
See [docs/api-compatibility.md](/docs/api-compatibility.md)

# Compiling from sources
## Backend
See [docs/compiling/backend.md](/docs/compiling/backend.md)
## Frontend
See [docs/compiling/frontend.md](/docs/compiling/frontend.md)

# Getting started
## Tested OSes
Both of frontend and backend tested on **Windows 11** and **Ubuntu 20.04**

On production environment it is recommended to use the latest version of the LTS **Ubuntu**

## Required software
- [Veil Node](https://github.com/Veil-Project/veil) - required for backend to pull blockchain information
- [PostgreSQL 13+](https://www.postgresql.org/download/) - required to run backend
- [Dotnet 6+ (runtime)](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) - required to run backend **in case** when using non self-contained build
- [NodeJS 16+](https://nodejs.org/en/) - required to run frontend

## Installation
There are basic setup guide that suitable for all supported environments and full setup tutorial wrote for **Ubuntu 20.04**
### Basic setup guide
#### Node installation
1. Unpack latest veil node release
2. Generate **rpcauth** value with:
```
python3 rpcauth.py [username] [noderpc_password]
```
rpcauth.py: https://github.com/Veil-Project/veil/blob/master/share/rpcauth/rpcauth.py

3. Create config **default.conf**
Add this content to opened file, change variables if required:
```bash
listen=1
server=1
rpcbind=127.0.0.1
rpcauth=[generated rpcauth string]
rpcport=5050
txindex=1
```
3. Run bin/veild -txindex
#### Database
1. Create new **postgresql** database
2. Restore SQL files from [explorer-backend/schemas](/explorer-backend/schemas)
#### Backend
1. Unpack **veil-explorer-backend-\[arch\]-\[os\].zip**
2. Copy [explorer-backend/appsettings.json.tpl](/explorer-backend/appsettings.json.tpl) to \[server directory\]/appsettings.json
3. Change **postgresql** connection string inside **appsettings.json**, change other parameters.

See: [docs/backend-configuration.md](/docs/backend-configuration.md)

4. Run server executable (explorer-backend)
5. It will take some time to synchronize backend database with node.
#### Frontend
1. Unpack **veil-explorer-frontend.zip**
2. Create start script which should export environment variables used as explorer-frontend config:
```bash
# listen address
HOST=127.0.0.1
# listen port
PORT=3000
# url on which frontend available, used for SEO, meta tags etc.
BASE_URL=http://<ip>:3000
# JSON formatted configuration to connect frontend with backend
# for now you should only change value of "path", so just replace <ip> and <backend_port>
CHAIN_APIS="[{\"name\": \"main\", \"path\": \"http://<ip>:5000/api\"}]"
RECENT_BLOCKS_COUNT=5
BLOCKS_PER_PAGE=15
TXS_PER_PAGE=15
MAX_BLOCK_WEIGHT=4000000
SYNC_NOTICE_CASE=15
COOKIE_SAVE_DAYS=90
```
See: [docs/frontend-configuration.md](/docs/frontend-configuration.md)

3. Run start script


### Full setup tutorial
#### Node
see [docs/setup/node.md](/docs/setup/node.md)
#### Database
see [docs/setup/database.md](/docs/setup/database.md)
#### Backend
see [docs/setup/backend.md](/docs/setup/backend.md)
#### Frontend
see [docs/setup/frontend.md](/docs/setup/frontend.md)
