explorer uses veil-node and postgresql to retrive required data.
PostgreSQL installation guide: https://www.postgresql.org/download/linux/ubuntu/
Minimum required version: Postgresql 13

Guide after installation (ubuntu 20.04+):
1. change password for postgres user
sudo passwd postgres

2. switch to postgres user
sudo -i -u postgres

3. create app user (not superuser)
createuser --interactive -e veilusr -D -P -R

4. create app database
createdb veilexplorer -O veilusr -E UTF8

5. back to administrator user
su <your user>

6. change postgres configuration
sudo nano /etc/postgresql/<version>/main/postgresql.conf

# you can tweak configuration by using pgtune: https://pgtune.leopard.in.ua/#/

7. change pg_hba.conf to allow unix domain socket connection from app:
sudo nano /etc/postgresql/<version>/main/pg_hba.conf
# add next entry:
local   veilexplorer    veilusr                                 md5

8. restart postgresql server
sudo service postgresql restart

9. import sql scripts to newly created database from explorer-backend/schemas/ folder