# Frontend setup guide
## Notes
*To save file and close nano editor* press **CTRL+X** than **SHIFT+Y** than **ENTER**

## Setup
Required OS: **ubuntu 22.04+**

Recommended OS: **ubuntu 22.04 LTS**

Required Software:
1. [NodeJS 20+](https://nodejs.org/en/)

## Update packges
```bash
sudo apt update
sudo apt upgrade
```

## Install NodeJS 20
```bash
# add nodesource PPA
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo bash -
# install nodejs
sudo apt install nodejs
```

## Create user for frontend app
```bash
sudo adduser explorer-frontend
sudo usermod -aG sudo explorer-frontend
```

## Create app directory
```bash
sudo mkdir /home/explorer-frontend/server/
```

## Download and unpack frontend build (change version in link and command to actual)
```bash
sudo wget https://github.com/steel97/veil-explorer/releases/download/latest/explorer-frontend.universal-1.1.0.tar.gz
sudo tar -xzf explorer-frontend.universal-1.1.0.tar.gz -C /home/explorer-frontend/server/
```

## Issue permissions for frontend
```bash
sudo chmod 755 /home/explorer-frontend/server
sudo chown -R explorer-frontend /home/explorer-frontend/server/
```

## Create startup script
*variables starting from CHAIN_DEFAULT are public, so dont use sensetive data here*
```bash
su explorer-frontend
cd /home/explorer-frontend/server/
sudo nano start.sh
```
Add this content to opened file, change variables if required:
```bash
#!/bin/bash
# listen address, highly recommended to change it to localhost and hide nuxt server behind nginx proxy for example
export HOST=0.0.0.0
# listen port
export PORT=3000
# url on which frontend available, used for SEO, meta tags etc.
export NUXT_PUBLIC_SITE_URL=http://<ip>:3000
# url on which frontend available, used for SEO, meta tags etc.
export NUXT_PUBLIC_I18N_BASE_URL=http://<ip>:3000
# url on which frontend available, used for SEO, meta tags etc.
export NUXT_PUBLIC_BASE_URL=http://<ip>:3000
# which chain should be selected from CHAIN_APIS by default
export NUXT_PUBLIC_CHAIN_DEFAULT=main
# JSON formatted configuration to connect frontend with backend
# for now you should only change value of "path", so just replace <ip> and <backend_port>
export NUXT_PUBLIC_CHAIN_APIS="[{\"name\": \"main\", \"path\": \"http://<ip>:<backend_port>/api\"}]"
export NUXT_PUBLIC_RECENT_BLOCKS_COUNT=5
export NUXT_PUBLIC_BLOCKS_PER_PAGE=15
export NUXT_PUBLIC_TXS_PER_PAGE=15
export NUXT_PUBLIC_MAX_BLOCK_WEIGHT=4000000
export NUXT_PUBLIC_SYNC_NOTICE_CASE=15
export NUXT_PUBLIC_COOKIE_SAVE_DAYS=90

node server/index.mjs
```
See: [/docs/frontend-configuration.md](/docs/frontend-configuration.md)

## Change permissions and test frontend
```bash
sudo chmod 777 start.sh
node server/index.mjs
```
If there are no errors, move to next step.

## Register frontend as systemd service
```bash
cd /home/explorer-frontend/server/
wget https://raw.githubusercontent.com/steel97/veil-explorer/master/docs/systemd/explorer-frontend.service
sudo systemctl link /home/explorer-frontend/server/explorer-frontend.service
```

## Finilize service creation
```bash
sudo systemctl daemon-reload
sudo systemctl enable explorer-frontend.service
sudo systemctl start explorer-frontend.service
sudo systemctl status explorer-frontend.service
```

Done, now explorer frontend is running as a background service.