import type { Block } from "@/models/Data/Block";

export interface TxVinSimpleDecoded {
    prevOutTx: string | null;
    prevOutNum: number;
    prevOutAddresses: Array<string> | null;
    prevOutAmount: number;

    type: TxInType;
    zerocoinSpend: number;

    anonInputs: Array<RingCTInput> | null;
}

export interface TxVoutSimpleDecoded {
    addresses: Array<string> | null;
    isOpreturn: boolean;
    isCoinBase: boolean;
    amount: number;
    type: OutputTypes;
    scriptPubKeyType: txnouttype;
    cTFee: number | null;
}

export interface RingCTInput {
    txId: string | null;
    voutN: number;
}

export enum TxInType {
    DEFAULT = 0,
    ZEROCOIN_SPEND = 1,
    ANON = 2
}

export enum OutputTypes {
    OUTPUT_NULL = 0, // marker for CCoinsView (0.14)
    OUTPUT_STANDARD = 1,
    OUTPUT_CT = 2,
    OUTPUT_RINGCT = 3,
    OUTPUT_DATA = 4,
}

export enum txnouttype {
    TX_NONSTANDARD,
    // 'standard' transaction types:
    TX_PUBKEY,
    TX_PUBKEYHASH,
    TX_SCRIPTHASH,
    TX_MULTISIG,
    TX_NULL_DATA, //!< unspendable OP_RETURN script that carries data
    TX_WITNESS_V0_SCRIPTHASH,
    TX_WITNESS_V0_KEYHASH,
    TX_WITNESS_UNKNOWN, //!< Only for Witness versions not already defined above

    TX_SCRIPTHASH256,
    TX_PUBKEYHASH256,
    TX_TIMELOCKED_SCRIPTHASH,
    TX_TIMELOCKED_SCRIPTHASH256,
    TX_TIMELOCKED_PUBKEYHASH,
    TX_TIMELOCKED_PUBKEYHASH256,
    TX_TIMELOCKED_MULTISIG,
    TX_ZEROCOINMINT,
}

export interface TransactionSimpleDecoded {
    txId: string | null;
    inputs: Array<TxVinSimpleDecoded> | null;
    outputs: Array<TxVoutSimpleDecoded> | null;
    isBasecoin: boolean;
    isCoinStake: boolean;
    isZerocoinMint: boolean;
    isZerocoinSpend: boolean;
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
    txnCount: number;
    block: Block;
    transactions: Array<TransactionSimpleDecoded>;
}