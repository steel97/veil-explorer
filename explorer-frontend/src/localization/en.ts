import Core from "./en/core";
import Meta from "./en/meta";
import Errors from "./en/errors";
import Header from "./en/header";
import Footer from "./en/footer";
import Home from "./en/home";
import NetworkSummary from "./en/network_summary";
import Blocks from "./en/blocks";
import Search from "./en/search";
import Address from "./en/address";
import Block from "./en/block";
import Tx from "./en/tx";
import UnconfirmedTx from "./en/unconfirmedtx";
import TxStats from "./en/txstats";

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
    TxStats: TxStats()
  }
});