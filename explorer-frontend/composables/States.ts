import { BlockchainInfo } from "@/models/API/BlockchainInfo";
import { SimplifiedBlock } from "~~/models/API/SimplifiedBlock";

export const useChainInfo = () => useState<BlockchainInfo | null>("chainInfo", () => null);
export const useLatestBlockInfo = () => useState<SimplifiedBlock | null>("latestBlockInfo", () => null);