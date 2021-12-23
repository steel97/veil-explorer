Backend installation tutorial (ubuntu 20.04+):

1. create user for backend app
sudo adduser explorer-backend
sudo usermod -aG sudo explorer-backend

2. create app directory
sudo mkdir /home/explorer-backend/server/

3. upload backend build to /home/explorer-backend/server/
4. issue permissions for backend
sudo chmod 755 /home/explorer-backend/server
sudo chmod 777 /home/explorer-backend/server/explorer-backend
sudo chown -R explorer-backend /home/explorer-backend/server/

4.1 change backend configuration (you can use template json for reference, appsettings.json.tpl)
sudo nano /home/explorer-backend/server/appsettings.json

4.2 now you can test backend, if there are no errors, move to step 5.
su explorer-backend
cd /home/explorer-backend/server/
./explorer-backend

5. create service
sudo nano /etc/systemd/system/explorer-backend.service

template:
[Unit]
Description=Veil-explorer backend service

[Service]
User=explorer-backend
KillMode=process
WorkingDirectory=/home/explorer-backend/server/
ExecStart=/home/explorer-backend/server/explorer-backend
Restart=always
TimeoutSec=300
RestartSec=5

[Install]
WantedBy=multi-user.target

6. finilize service creation
sudo chmod 664 /etc/systemd/system/explorer-backend.service
sudo systemctl daemon-reload
sudo systemctl enable explorer-backend.service
sudo systemctl start explorer-backend.service
sudo systemctl status explorer-backend.service