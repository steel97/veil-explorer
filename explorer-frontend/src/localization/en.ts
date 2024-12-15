import Address from "./en/address";
import Block from "./en/block";
import Blocks from "./en/blocks";
import Core from "./en/core";
import Errors from "./en/errors";
import Footer from "./en/footer";
import Header from "./en/header";
import Home from "./en/home";
import Meta from "./en/meta";
import NetworkSummary from "./en/network_summary";
import Search from "./en/search";
import Tx from "./en/tx";
import TxStats from "./en/txstats";
import UnconfirmedTx from "./en/unconfirmedtx";

export default defineI18nLocale(async (locale) => {
  return {
    nuxtSiteConfig: {
      name: "Veil Explorer",
      description: "View blockchain, addresses, search blocks and transactions",
    },
    Core: Core(),
    Meta: Meta(),
    Errors: Errors(),
    Header: Header(),
    Footer: Footer(),
    Home: Home(),
    NetworkSummary: NetworkSummary(),
    Blocks: Blocks(),
    Search: Search(),
    Address: Address(),
    Block: Block(),
    Tx: Tx(),
    UnconfirmedTx: UnconfirmedTx(),
    TxStats: TxStats(),
  };
});