export enum BlockType {
    UNKNOWN = 0,
    POW_X16RT = 1,
    POW_ProgPow = 2,
    POW_RandomX = 3,
    POW_Sha256D = 4,
    POS = 5
}

export interface Block {
    height: number;
    hash_hex: string | null;
    strippedsize: number;
    size: number;
    weight: number;
    proof_type: BlockType;
    proofofstakehash_hex: string | null;
    progproofofworkhash_hex: string | null;
    progpowmixhash_hex: string | null;
    randomxproofofworkhash_hex: string | null;
    sha256dproofofworkhash_hex: string | null;
    proofofworkhash_hex: string | null;
    version: number;
    merkleroot_hex: string | null;
    time: number;
    mediantime: number;
    nonce: number;
    nonce64: number;
    mixhash_hex: string | null;
    bits_hex: string | null;
    difficulty: number;
    chainwork_hex: string | null;
    anon_index: number;
    veil_data_hash_hex: string | null;
    prog_header_hash_hex: string | null;
    prog_header_hex: string | null;
    epoch_number: number;
    synced: boolean;
}