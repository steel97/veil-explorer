 SELECT b.height, b."size", b.weight, b.proof_type, b."time", b.mediantime, (SELECT COUNT(t.txid) AS txn FROM transactions t WHERE t.block_height = b.height)
	FROM blocks b
	WHERE b.synced = true
	ORDER BY height ASC OFFSET 10 LIMIT 5;