import type { SimplifiedBlock } from "@/models/API/SimplifiedBlock";
import type { BackgroundInfo } from "@/models/Hub/BackgroundInfo";
import type { GetBlockchainInfoResult } from "@/models/Node/GetBlockchainInfoResult";

export const useThemeState = () => useState<string>("themestate", () => "");
export const useChainState = () => useState<string>("chainstate", () => "");
export const useBlockchainInfo = () => useState<GetBlockchainInfoResult | null>("blockchainInfo", () => null);
export const useBackgroundInfo = () => useState<BackgroundInfo | null>("backgroundInfo", () => null);
export const useLatestBlockInfo = () => useState<SimplifiedBlock | null>("latestBlockInfo", () => null);