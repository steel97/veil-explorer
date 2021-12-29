namespace ExplorerBackend.VeilStructs;

public class Budget
{
    const int nHeightSupplyCreationStop = 9816000;
    const int nBlocksPerPeriod = 43200;
    const string currentNetwork = "main";

    public static string NetworkIDString() => currentNetwork;
    public static int HeightSupplyCreationStop() => nHeightSupplyCreationStop;

    public static bool IsSuperBlock(int nBlockHeight)
    {
        return (
                (NetworkIDString() == "main" && nBlockHeight % nBlocksPerPeriod == 0) ||
                ((NetworkIDString() == "test" || NetworkIDString() == "dev") && (nBlockHeight % nBlocksPerPeriod == 20000 || nBlockHeight == 1))
                );
    }

    public static void GetBlockRewards(int nBlockHeight, out long nBlockReward, out long nFounderPayment, out long nFoundationPayment, out long nBudgetPayment)
    {

        if (nBlockHeight <= 0 || nBlockHeight > HeightSupplyCreationStop())
        { // 43830 is the average size of a month in minutes when including leap years
            nBlockReward = 0;
            nFounderPayment = 0;
            nFoundationPayment = 0;
            nBudgetPayment = 0;
        }
        else if (nBlockHeight >= 1 && nBlockHeight <= 518399)
        {

            nBlockReward = 50;
            if (IsSuperBlock(nBlockHeight))
            {
                nFounderPayment = 10 * nBlocksPerPeriod;
                nFoundationPayment = 10 * nBlocksPerPeriod;
                nBudgetPayment = 30 * nBlocksPerPeriod;
            }
            else
            {
                nFounderPayment = nFoundationPayment = nBudgetPayment = 0;
            }

        }
        else if (nBlockHeight >= 518400 && nBlockHeight <= 1036799)
        {

            nBlockReward = 40;
            if (IsSuperBlock(nBlockHeight))
            {
                nFounderPayment = 8 * nBlocksPerPeriod;
                if (nBlockHeight > 518401)
                    nFounderPayment = 0;
                nFoundationPayment = 8 * nBlocksPerPeriod;
                nBudgetPayment = 24 * nBlocksPerPeriod;
            }
            else
            {
                nFounderPayment = nFoundationPayment = nBudgetPayment = 0;
            }

        }
        else if (nBlockHeight >= 1036800 && nBlockHeight <= 1555199)
        {

            nBlockReward = 30;
            if (IsSuperBlock(nBlockHeight))
            {
                nFounderPayment = 0;
                nFoundationPayment = 6 * nBlocksPerPeriod;
                nBudgetPayment = 18 * nBlocksPerPeriod;
            }
            else
            {
                nFounderPayment = nFoundationPayment = nBudgetPayment = 0;
            }

        }
        else if (nBlockHeight >= 1555200 && nBlockHeight <= 2073599)
        {

            nBlockReward = 20;
            if (IsSuperBlock(nBlockHeight))
            {
                nFounderPayment = 0;
                nFoundationPayment = 4 * nBlocksPerPeriod;
                nBudgetPayment = 12 * nBlocksPerPeriod;
            }
            else
            {
                nFounderPayment = nFoundationPayment = nBudgetPayment = 0;
            }

        }
        else if (nBlockHeight >= 2073600 && nBlockHeight <= 2591999)
        {

            nBlockReward = 10;
            if (IsSuperBlock(nBlockHeight))
            {
                nFounderPayment = 0;
                nFoundationPayment = 2 * nBlocksPerPeriod;
                nBudgetPayment = 6 * nBlocksPerPeriod;
            }
            else
            {
                nFounderPayment = nFoundationPayment = nBudgetPayment = 0;
            }

        }
        else
        {

            nBlockReward = 10;
            if (IsSuperBlock(nBlockHeight))
            {
                nFounderPayment = 0 * nBlocksPerPeriod;
                nFoundationPayment = 2 * nBlocksPerPeriod;
                nBudgetPayment = 8 * nBlocksPerPeriod;
            }
            else
            {
                nFounderPayment = nFoundationPayment = nBudgetPayment = 0;
            }

        }

        if (nBlockHeight == 1 && NetworkIDString() != "main")
            nBlockReward += 15000000;

        nBlockReward *= (long)Constants.COIN;
        nFounderPayment *= (long)Constants.COIN;
        nFoundationPayment *= (long)Constants.COIN;
        nBudgetPayment *= (long)Constants.COIN;
    }
}