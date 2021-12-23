explain analyze SELECT blocks.height, blocks."size", blocks.weight, blocks.proof_type, blocks."time", blocks.mediantime, count(transactions.block_height)
	FROM blocks left join transactions on blocks.height = transactions.block_height
	where blocks.synced = true
	group by blocks.height
	ORDER BY height asc offset 10 limit 5;