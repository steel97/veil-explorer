import * as signalR from "@microsoft/signalr";
import { useConfigs } from "@/composables/Configs";
import { useChainInfo, useLatestBlockInfo } from "@/composables/States";
import { GetBlockchainInfoResult } from "@/models/Node/GetBlockchainInfoResult";
import { GetChainalgoStatsResult } from "@/models/Node/GetChainalgoStatsResult";
import { SimplifiedBlock } from "@/models/API/SimplifiedBlock";

export const useNetworkManager = () => {
    const { getApiPath } = useConfigs();
    const chainInfoDataState = useChainInfo();
    const latestBlockState = useLatestBlockInfo();

    const connect = () => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(`${getApiPath()}/events`)
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    return 5000;
                }
            })
            .configureLogging(signalR.LogLevel.Error)
            .build();

        connection.on("BlockchainInfoUpdated", (currentSyncedBlock: number, chainInfo: GetBlockchainInfoResult, algoStats: GetChainalgoStatsResult) => {
            chainInfoDataState.value = {
                currentSyncedBlock: currentSyncedBlock,
                chainInfo: chainInfo,
                algoStats: algoStats
            };
        });

        connection.on("BlocksUpdated", (simplifiedBlock: SimplifiedBlock) => {
            latestBlockState.value = simplifiedBlock;
        });

        connection.start().catch(err => console.log(err));
    }

    return { connect };
}