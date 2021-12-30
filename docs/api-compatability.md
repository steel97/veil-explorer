Notes about compatability of this explorer with previous version:
1. /api/getblockchaininfo
added next_super_block field
numeric types are numeric, not string, exception is amount_formatted under zerocoinsupply and moneysupply_formatted


2. /api/getchainalgostats
Fully compatible

3. /api/getaddressbalance/<address>
New version indicate status with response status code
400 - bad request (address is invalid or it is a stealth address)
200 - success
202 - request added to queue, retry requeste until you get status 200

4. /api/getmoneysupply
New version uses double type for all variables except for budget_address and foundation_address