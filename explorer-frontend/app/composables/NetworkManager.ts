import type { SimplifiedBlock } from "@/models/API/SimplifiedBlock";
import type { GetBlockchainInfoResult } from "@/models/Node/GetBlockchainInfoResult";
import type { GetChainalgoStatsResult } from "@/models/Node/GetChainalgoStatsResult";
import { useConfigs } from "@/composables/Configs";
import { useBackgroundInfo, useBlockchainInfo, useLatestBlockInfo } from "@/composables/States";
import * as signalR from "@microsoft/signalr";

export const useNetworkManager = () => {
  const { getApiPath } = useConfigs();
  const backgroundInfoDataState = useBackgroundInfo();
  const blockchainInfoDataState = useBlockchainInfo();
  const latestBlockState = useLatestBlockInfo();

  const connect = () => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${getApiPath()}/events`)
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          return 5000;
        },
      })
      .configureLogging(signalR.LogLevel.Error)
      .build();

    connection.on("blockchainInfoUpdated", (chainInfo: GetBlockchainInfoResult) => {
      blockchainInfoDataState.value = chainInfo;
    });
    connection.on("backgroundInfoUpdated", (currentSyncedBlock: number, algoStats: GetChainalgoStatsResult) => {
      backgroundInfoDataState.value = {
        currentSyncedBlock,
        algoStats,
      };
    });
    connection.on("blocksUpdated", (simplifiedBlock: SimplifiedBlock) => {
      latestBlockState.value = simplifiedBlock;
    });

    connection.start().catch(err => console.log(err));
  };

  return { connect };
};