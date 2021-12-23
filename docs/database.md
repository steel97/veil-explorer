explorer uses veil-node and postgresql to retrive required data.
PostgreSQL installation guide: https://www.postgresql.org/download/linux/ubuntu/
Minimum required version: Postgresql 13

After installation:
1. change password for postgres user
sudo passwd postgres

2. switch to postgres user
sudo -i -u postgres

3. create app user
createuser --interactive -e veilusr -D -P -R

4. create app database
createdb veilexplorer -O veilusr -E UTF8

5. back to administrator user
su <your user>

6. change postgres configuration
sudo nano /etc/postgresql/<version>/main/postgresql.conf

# you can tweak configuration by using pgtune: https://pgtune.leopard.in.ua/#/
