import Core from "./ru/core";
import Meta from "./ru/meta";
import Errors from "./ru/errors";
import Header from "./ru/header";
import Footer from "./ru/footer";
import Home from "./ru/home";
import NetworkSummary from "./ru/network_summary";
import Blocks from "./ru/blocks";
import Search from "./ru/search";
import Address from "./ru/address";
import Block from "./ru/block";
import Tx from "./ru/tx";
import UnconfirmedTx from "./ru/unconfirmedtx";
import TxStats from "./ru/txstats";

export default defineI18nLocale(async (locale) => {
  return {
    nuxtSiteConfig: {
      name: "Veil Explorer",
      description: "Обзор блокчейна, поиск блоков, транзакций, просмотр адресов",
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