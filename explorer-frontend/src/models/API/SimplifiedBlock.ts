export enum BlockType {
    UNKNOWN = 0,
    POW_X16RT = 1,
    POW_ProgPow = 2,
    POW_RandomX = 3,
    POW_Sha256D = 4,
    POS = 5
}

export interface SimplifiedBlock {
    height: number;
    size: number;
    weight: number;
    proofType: BlockType;
    time: number;
    medianTime: number;
    txCount: number;
}