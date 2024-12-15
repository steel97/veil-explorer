import type { GetBlockchainInfoResult } from "@/models/Node/GetBlockchainInfoResult";
import type { GetChainalgoStatsResult } from "@/models/Node/GetChainalgoStatsResult";

export interface BlockchainInfo {
  currentSyncedBlock: number;
  chainInfo: GetBlockchainInfoResult;
  algoStats: GetChainalgoStatsResult;
}