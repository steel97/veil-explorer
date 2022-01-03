import * as signalR from "@microsoft/signalr";
import { useConfigs } from "@/composables/Configs";
import { useBackgroundInfo, useBlockchainInfo, useLatestBlockInfo } from "@/composables/States";
import { GetBlockchainInfoResult } from "@/models/Node/GetBlockchainInfoResult";
import { GetChainalgoStatsResult } from "@/models/Node/GetChainalgoStatsResult";
import { SimplifiedBlock } from "@/models/API/SimplifiedBlock";

export const useNetworkManager = () => {
    const { getApiPath } = useConfigs();
    const backgroundInfoDataState = useBackgroundInfo();
    const blockchainInfoDataState = useBlockchainInfo();
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

        connection.on("blockchainInfoUpdated", (chainInfo: GetBlockchainInfoResult) => blockchainInfoDataState.value = chainInfo);
        connection.on("backgroundInfoUpdated", (currentSyncedBlock: number, algoStats: GetChainalgoStatsResult) =>
            backgroundInfoDataState.value = {
                currentSyncedBlock: currentSyncedBlock,
                algoStats: algoStats
            }
        );
        connection.on("blocksUpdated", (simplifiedBlock: SimplifiedBlock) => latestBlockState.value = simplifiedBlock);

        connection.start().catch(err => console.log(err));
    }

    return { connect };
}