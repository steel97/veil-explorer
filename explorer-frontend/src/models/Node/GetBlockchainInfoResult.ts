export interface GetBlockchainInfoResult {
    chain: string;
    blocks: number;
    moneysupply: number;
    zerocoinsupply: Array<ZerocoinSupply>;
    bestblockhash: string;
    difficulty_pow: number;
    difficulty_randomx: number;
    difficulty_progpow: number;
    difficulty_sha256d: number;
    difficulty_pos: number;
    mediantime: number;
    size_on_disk: number;
    next_super_block: number;
}

export interface ZerocoinSupply {
    denom: string;
    amount: number;
    percent: number;
}