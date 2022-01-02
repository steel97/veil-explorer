# Frontend configuration
## Configuring process overview
Frontend take configuration from environment variables.

Default way to run frontend with custom configuration is to create startup script.

On **linux** variables defined int script in this way:
```bash
export [key]=[value]
```
On **windows**:
```
set [key]=[value]
```

## Available parameters
```bash
# listen address, highly recommended to change it to localhost and hide nuxt server behind nginx proxy for example
HOST=0.0.0.0
# listen port
PORT=3000
# url on which frontend available, used for SEO, meta tags etc.
BASE_URL=http://<ip>:3000
# JSON formatted configuration to connect frontend with backend
# for now you should only change value of "path", so just replace <ip> and <backend_port>
CHAIN_APIS="[{\"name\": \"main\", \"path\": \"http://<ip>:<backend_port>/api\"}]"
# amount of blocks shown on main page in "Recent blocks" section
RECENT_BLOCKS_COUNT=5
# amount of blocks shown on blocks list page
export BLOCKS_PER_PAGE=15
# amount of transactions on block page
export TXS_PER_PAGE=15
# variable to calculate block fullness percent, taken from veil node source code
# https://github.com/Veil-Project/veil/search?q=4000000
export MAX_BLOCK_WEIGHT=4000000
# how much blocks should be database behind veil node to display "synchronizing" notice
export SYNC_NOTICE_CASE=15
# user preferences store time
export COOKIE_SAVE_DAYS=90
```