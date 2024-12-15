import Address from "./ru/address";
import Block from "./ru/block";
import Blocks from "./ru/blocks";
import Core from "./ru/core";
import Errors from "./ru/errors";
import Footer from "./ru/footer";
import Header from "./ru/header";
import Home from "./ru/home";
import Meta from "./ru/meta";
import NetworkSummary from "./ru/network_summary";
import Search from "./ru/search";
import Tx from "./ru/tx";
import TxStats from "./ru/txstats";
import UnconfirmedTx from "./ru/unconfirmedtx";

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
    TxStats: TxStats(),
  };
});