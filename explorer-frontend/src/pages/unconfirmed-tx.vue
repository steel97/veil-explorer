<template>
  <div>
    <h1 class="font-semibold pt-5 uppercase">
      {{ t("UnconfirmedTx.Title") }}
    </h1>
    <div
      v-if="
        unconfirmedTxData == null ||
        unconfirmedTxData.transactions == null ||
        unconfirmedTxData.transactions.length == 0
      "
      class="
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
      v-if="unconfirmedTxData != null && unconfirmedTxData.transactions != null"
      :txdata="unconfirmedTxData.transactions"
    />

    <SharedPagination
      v-if="unconfirmedTxData != null && unconfirmedTxData.transactions != null"
      :overallEntries="unconfirmedTxData.txnCount"
      :entriesPerPage="config.TXS_PER_PAGE"
      :currentPage="currentPage"
      :linkTemplate="buildRouteTemplate()"
      @pageSelected="selectPage"
    />
  </div>
</template>

<script setup lang="ts">
import { UnconfirmedTxResponse } from "@/models/API/UnconfirmedTxResponse";
import { useUI } from "@/composables/UI";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const { getApiPath } = useConfigs();
const { scrollToAnimated } = useUI();
const route = useRoute();
const config = useRuntimeConfig();

const page =
  ((route.params.page as any as number) > 0
    ? (route.params.page as any as number)
    : 1) - 1;
const currentPage = ref(page + 1);

const fetchUnconfirmedTx = async () =>
  await useFetch<string, UnconfirmedTxResponse>(
    `${getApiPath()}/unconfirmedtransactions?offset=${(
      (currentPage.value - 1) *
      config.TXS_PER_PAGE
    ).toString()}&count=${config.TXS_PER_PAGE as any as string}`
  );

const unconfirmedTxData = ref((await fetchUnconfirmedTx()).data);
const buildRouteTemplate = () => `/unconfirmed-tx/{page}/`;

const selectPage = async (pg: number) => {
  if (pg == currentPage.value) return;

  scrollToAnimated(
    document.documentElement,
    document.documentElement.scrollTop,
    0,
    150,
    150
  );

  const link = buildRouteTemplate().replace("{page}", pg.toString());
  currentPage.value = pg;
  window.history.replaceState({}, "", link);
  const unconfirmedTxDataLocal = await fetchUnconfirmedTx();
  unconfirmedTxData.value = unconfirmedTxDataLocal.data.value;
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
        content: `${config.BASE_URL}${buildRouteTemplate().replace(
          "{page}",
          currentPage.value.toString()
        )}`,
      },
    ],
  };
});
useMeta(meta);
</script>