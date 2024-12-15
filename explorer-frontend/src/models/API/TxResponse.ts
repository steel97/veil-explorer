import type { TransactionSimpleDecoded } from "@/models/API/BlockResponse";

export interface TxResponse {
  txId: string | null;
  confirmed: boolean;
  blockHeight: number;
  timestamp: number;
  version: number;
  size: number;
  vSize: number;
  locktime: number;
  transaction: TransactionSimpleDecoded | null;
}