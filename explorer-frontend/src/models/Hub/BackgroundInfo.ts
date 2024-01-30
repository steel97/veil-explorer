import type { GetChainalgoStatsResult } from "@/models/Node/GetChainalgoStatsResult";

export interface BackgroundInfo {
    currentSyncedBlock: number;
    algoStats: GetChainalgoStatsResult;
}