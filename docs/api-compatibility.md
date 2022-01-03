# API Compatibility
Notes about compatibility of this explorer with previous version:

For better compatibility next APIs available on both frontend and backend endpoints:
1. /api/getblockchaininfo
* added next_super_block field
* numeric types are numeric, not a string, an exception is amount_formatted under zerocoinsupply and moneysupply_formatted


2. /api/getchainalgostats
* Fully compatible

3. /api/getaddressbalance/<address>
* New version indicates status with response status code (only when accessed through backend endpoint)
```
400 - bad request (address is invalid or it is a stealth address)
200 - success
202 - request added to queue, retry request until you get status 200
```

4. /api/getmoneysupply
* New version uses double type for all variables except for budget_address and foundation_address