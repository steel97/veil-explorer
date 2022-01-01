# Database setup guide

Required OS: **ubuntu 20.04+**

Recommended OS: **ubuntu 20.04.3 LTS**

Required Software:
1. [PostgreSQL 13+](https://www.postgresql.org/download/)

## Update packges
```bash
sudo apt update
sudo apt upgrade
```

## Install PostgreSQL 13
```bash
# https://www.postgresql.org/download/linux/ubuntu/
# Create the file repository configuration:
sudo sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'

# Import the repository signing key:
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo apt-key add -

# Update the package lists:
sudo apt-get update

# Install the latest version of PostgreSQL.
# If you want a specific version, use 'postgresql-12' or similar instead of 'postgresql':
sudo apt-get -y install postgresql
```

## Change password for postgres user and switch to this user
```bash
sudo passwd postgres
sudo -i -u postgres
```

## Create postgres user and database
```bash
# not superuser
createuser --interactive -e veilusr -D -P -R
createdb veilexplorer -O veilusr -E UTF8
```

## Back to administrator user
```bash
su [your_user]
```

## Change postgres configuration
```bash
sudo nano /etc/postgresql/13/main/postgresql.conf
```
you can tweak configuration by using pgtune: https://pgtune.leopard.in.ua/#/

## Change pg_hba.conf to allow unix domain socket connection from app
```bash
sudo nano /etc/postgresql/13/main/pg_hba.conf
```
Add this content to opened file:
```
local   veilexplorer    veilusr                                 md5
```

## Restart postgresql server
```bash
sudo service postgresql restart
```

## Import sql scripts to newly created database
SQL scripts located at [explorer-backend/schemas](/explorer-backend/schemas) directory.

Done, now database is ready.