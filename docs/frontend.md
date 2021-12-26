NodeJS 16+ required to run frontend
Frontend installation tutorial (ubuntu 20.04+):

1. create user for frontend app
sudo adduser explorer-frontend
sudo usermod -aG sudo explorer-frontend

2. create app directory
sudo mkdir /home/explorer-frontend/server/

3. upload backend build to /home/explorer-frontend/server/

4. issue permissions for backend
sudo chmod 755 /home/explorer-frontend/server
sudo chown -R explorer-frontend /home/explorer-frontend/server/

4.1 now you can create startup script
su explorer-frontend
cd /home/explorer-frontend/server/
sudo nano start.sh
# write next config
#!/bin/bash
export HOST=0.0.0.0
export PORT=3000
export BASE_URL=http://<ip>:3000
export CHAIN_APIS="[{\"name\": \"main\", \"path\": \"http://<ip>:5000/api\"}]"
export RECENT_BLOCKS_COUNT=5
export BLOCKS_PER_PAGE=15
export MAX_BLOCK_WEIGHT=4000000
export SYNC_NOTICE_CASE=15
export COOKIE_SAVE_DAYS=90


4.2 change permissions and test frontend, if there are no errors, move to step 5.
chmod 777 start.sh
node server/index.mjs

5. create service
sudo nano /etc/systemd/system/explorer-frontend.service

template:
[Unit]
Description=Veil-explorer frontend service

[Service]
User=explorer-frontend
KillMode=control-group
WorkingDirectory=/home/explorer-frontend/server/
ExecStart=/home/explorer-frontend/server/start.sh
Restart=always
TimeoutSec=300
RestartSec=5

[Install]
WantedBy=multi-user.target

6. finilize service creation
sudo chmod 664 /etc/systemd/system/explorer-frontend.service
sudo systemctl daemon-reload
sudo systemctl enable explorer-frontend.service
sudo systemctl start explorer-frontend.service
sudo systemctl status explorer-frontend.service