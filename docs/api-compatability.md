Notes about compatability of this explorer with previous version:
1. /api/getblockchaininfo
Returned in simplified format, missed some fields, new format example below:
{
   "chain":"main",
   "blocks":1538885,
   "moneysupply":12694963331738307,
   "zerocoinsupply":[
      {
         "denom":"10",
         "amount":463945000000000,
         "percent":3.654559590889913
      },
      {
         "denom":"100",
         "amount":373720000000000,
         "percent":2.943844658973323
      },
      {
         "denom":"1000",
         "amount":650700000000000,
         "percent":5.125654820705184
      },
      {
         "denom":"10000",
         "amount":6470000000000000,
         "percent":50.96509403713315
      },
      {
         "denom":"total",
         "amount":7958365000000000,
         "percent":62.68915310770156
      }
   ],
   "bestblockhash":"b909cdad408194cbf0e22e51f80b05b82a0c11f58e7f6a35b09abedfacde33bf",
   "difficulty_pow":-1,
   "difficulty_randomx":0.0294575734079493,
   "difficulty_progpow":92.5094566428091,
   "difficulty_sha256d":570.7016921193928,
   "difficulty_pos":100786063.4138775,
   "mediantime":1640383969,
   "size_on_disk":23545905119,
   "next_super_block":1555200
}


2. /api/getchainalgostats
New version doesn't send x16rt field which now is always zero

3. /api/getaddressbalance/<address>
New version indicate status with response status code
400 - bad request (address is invalid or it is a stealth address)
200 - success
202 - request added to queue, retry requeste until you get status 200

4. /api/getmoneysupply
New version uses double type for all variables except for budget_address and foundation_address