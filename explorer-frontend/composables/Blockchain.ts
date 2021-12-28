import { BlockType } from "@/models/Data/Block";

export const useBlockchain = () => {
    const getPow = (proofType: BlockType) => {
        let high = "";
        let low = "&nbsp;";
        let lowI: string | null = null;

        switch (proofType) {
            case BlockType.UNKNOWN: {
                high = "Unknown";
                break;
            }
            case BlockType.POW_X16RT: {
                high = "Proof-of-work";
                low = "X16RT";
                break;
            }
            case BlockType.POW_ProgPow: {
                high = "Proof-of-work";
                low = "ProgPow";
                break;
            }
            case BlockType.POW_RandomX: {
                high = "Proof-of-work";
                low = "RandomX";
                break;
            }
            case BlockType.POW_Sha256D: {
                high = "Proof-of-work";
                low = "Sha256D";
                break;
            }
            case BlockType.POS: {
                high = "Proof-of-stake";
                break;
            }
        }

        if (low != "&nbsp;") lowI = low;

        return [high, low, lowI];
    };
    return { getPow };
}