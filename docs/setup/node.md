# Node setup guide

Required OS: **ubuntu 20.04+**

Recommended OS: **ubuntu 20.04.3 LTS**

Required Software:
1. [Veil Node](https://github.com/Veil-Project/veil)

## Update packages
```bash
sudo apt update
sudo apt upgrade
```
## Create user which will be used to run node
```bash
sudo adduser --disabled-password --gecos "" node
```
## Download and unpack veil node
```bash
sudo wget https://github.com/Veil-Project/veil/releases/download/v1.2.2.1/veil-1.2.2.1-x86_64-linux-gnu.tar.gz
```
## Create data directory
```bash
sudo mkdir /home/node/veil
sudo mkdir /home/node/veil/data
```
## Download and unpack blockchain data
```bash
# replace 20210121-veil-snapshot-1055961 with actual version
sudo wget https://mirror-eu2.veil.tools/20210121-veil-snapshot-1055961.zip
sudo unzip 20210121-veil-snapshot-1055961.zip -d /home/node/veil/data
sudo chown -R node:node /home/node/veil/data
```
## Create config default.conf
```bash
sudo nano /home/node/veil/default.conf
```
Add this content to opened file, change variables if required:
```bash
listen=1
server=1
rpcbind=127.0.0.1
# rpcauth generated by script share/rpcauth/rpcauth.py after providing a username:
#
# ./share/rpcauth/rpcauth.py alice
# String to be appended to veil.conf:
# rpcauth=alice:f7efda5c189b999524f151318c0c86$d5b51b3beffbc02b724e5d095828e0bc8b2456e9ac8757ae3211a5d9b16a22ae
rpcauth=
rpcport=5050
txindex=1
```

## Register veil node as systemd service
```bash
sudo nano /etc/systemd/system/veil.service
```
Add this content to opened file, change variables if required:
```bash
[Unit]
Description=Veil Node
After=network.target

[Service]
User=node
Group=node
Type=forking
PIDFile=/home/node/veil/data/veil.pid
ExecStart=/home/node/veil/bin/veild -disablewallet -daemon -pid=/home/node/veil/data/veil.pid -conf=/home/node/veil/default.conf -datadir=/home/node/veil/data -txindex
Restart=always
RestartSec=5
PrivateTmp=true
TimeoutStopSec=60s
TimeoutStartSec=5s
StartLimitInterval=120s
StartLimitBurst=15

[Install]
WantedBy=multi-user.target
```

## Finilize service creation
```bash
sudo sudo chmod 664 /etc/systemd/system/veil.service
sudo systemctl daemon-reload
sudo systemctl start veil
sudo systemctl status veil
```

Done, now veil node is running as a background service.