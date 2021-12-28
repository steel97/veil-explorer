import { Block } from "@/models/Data/Block";

export interface TransactionSimpleDecoded {

}

export interface BlockBasicData {
    hash: string | null;
    height: number;
}

export interface BlockResponse {
    found: boolean;
    nextBlock: BlockBasicData;
    prevBlock: BlockBasicData;
    versionHex: string | null;
    block: Block;
    transactions: TransactionSimpleDecoded;
}