<template>
  <div>
    <h1 class="font-semibold pt-5 uppercase">
      {{ t("UnconfirmedTx.Title") }}
    </h1>
    <div
      v-if="unconfirmedTxData === null
        || unconfirmedTxData.transactions === null
        || unconfirmedTxData.transactions.length === 0
      " class="
        rounded
        p-8
        bg-gray-50
        dark:bg-gray-800
        flex
        justify-center
        items-center
        mb-4
        mt-4
      "
    >
      {{ t("UnconfirmedTx.NoTxs") }}
    </div>
    <BlockTransactionsView
      v-if="unconfirmedTxData !== null && unconfirmedTxData.transactions !== null"
      :txdata="unconfirmedTxData.transactions"
    />

    <SharedPagination
      v-if="unconfirmedTxData !== null && unconfirmedTxData.transactions !== null"
      :overall-entries="unconfirmedTxData.txnCount" :entries-per-page="config.public.txsPerPage" :current-page="currentPage"
      :link-template="buildRouteTemplate()" @page-selected="selectPage"
    />
  </div>
</template>

<script setup lang="ts">
import type { UnconfirmedTxResponse } from "@/models/API/UnconfirmedTxResponse";
import { useUI } from "@/composables/UI";

const { t } = useI18n();
const { getApiPath } = useConfigs();
const { scrollToAnimated } = useUI();
const { chainPath } = useRoutingHelper();
const route = useRoute();
const config = useRuntimeConfig();

const page
  = ((route.params.page as any as number) > 0
    ? (route.params.page as any as number)
    : 1) - 1;
const currentPage = ref(page + 1);

const getFetchUnconfirmedTxUrl = () =>
  `${getApiPath()}/unconfirmedtransactions?offset=${(
    (currentPage.value - 1)
    * config.public.txsPerPage
  ).toString()}&count=${config.public.txsPerPage as any as string}`;

const unconfirmedTxData = ref((await useFetch<UnconfirmedTxResponse>(getFetchUnconfirmedTxUrl())).data);
const buildRouteTemplate = () => decodeURI(chainPath("/unconfirmed-tx/{page}/"));

const selectPage = async (pg: number) => {
  if (pg === currentPage.value)
    return;

  scrollToAnimated(
    document.documentElement,
    document.documentElement.scrollTop,
    0,
    150,
    150,
  );

  const link = buildRouteTemplate().replace("{page}", pg.toString());
  currentPage.value = pg;
  window.history.replaceState({}, "", link);
  const unconfirmedTxDataLocal = await $fetch<UnconfirmedTxResponse>(getFetchUnconfirmedTxUrl());
  unconfirmedTxData.value = unconfirmedTxDataLocal;
};

const meta = computed(() => {
  return {
    title: t("UnconfirmedTx.Meta.Title"),
    meta: [
      {
        name: "description",
        content: t("UnconfirmedTx.Meta.Description"),
      },
      {
        name: "og:title",
        content: t("UnconfirmedTx.Meta.Title"),
      },
      {
        name: "og:url",
        content: `${config.public.baseUrl}${buildRouteTemplate().replace(
          "{page}",
          currentPage.value.toString(),
        )}`,
      },
    ],
  };
});
useHead(meta);
</script>