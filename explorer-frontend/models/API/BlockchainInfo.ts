import { GetBlockchainInfoResult } from "@/models/Node/GetBlockchainInfoResult";
import { GetChainalgoStatsResult } from "@/models/Node/GetChainalgoStatsResult";

export interface BlockchainInfo {
    currentSyncedBlock: number,
    chainInfo: GetBlockchainInfoResult;
    algoStats: GetChainalgoStatsResult;
}