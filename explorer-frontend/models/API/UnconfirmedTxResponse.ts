import { TransactionSimpleDecoded } from "@/models/API/BlockResponse";

export interface UnconfirmedTxResponse {
    txnCount: number;
    transactions: Array<TransactionSimpleDecoded>
}