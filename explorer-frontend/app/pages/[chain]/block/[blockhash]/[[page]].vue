<template>
  <div>
    <h1 class="font-semibold py-4">
      <div class="uppercase">
        {{ t("Block.BlockTitle") }}: #{{ blockHeight }}
      </div>
      <div
        class="
            text-xs text-gray-500
            dark:text-gray-400
            max-w-full
            overflow-hidden
            text-ellipsis
          "
      >
        {{ blockHash }}
      </div>
    </h1>

    <BlockView v-if="blockData != null && blockData.block != null" :block="blockData" />

    <h1 class="font-semibold pt-5 uppercase">
      {{ t("Block.Transactions") }}
    </h1>

    <BlockTransactionsView
      v-if="blockData != null && blockData.transactions != null"
      :txdata="blockData.transactions"
    />

    <SharedPagination
      v-if="blockData != null && blockData.transactions != null" :overall-entries="blockData.txnCount"
      :entries-per-page="config.public.txsPerPage" :current-page="currentPage" :link-template="buildRouteTemplate()"
      @page-selected="selectPage"
    />
  </div>
</template>

<script setup lang="ts">
import type { BlockResponse } from "@/models/API/BlockResponse";
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

const getFetchBlockUrl = () => `${getApiPath()}/block`;
const getFetchBlockBody = () => {
  return {
    hash: route.params.blockhash,
    offset: (currentPage.value - 1) * config.public.txsPerPage,
    count: config.public.txsPerPage,
  };
};

const blockData = ref((await useFetch<BlockResponse>(getFetchBlockUrl(), { method: "POST", body: getFetchBlockBody() })).data);
const blockHeight = computed(
  () => blockData.value?.block?.height ?? t("Core.NoData"),
);
const blockHash = computed(
  () => blockData.value?.block?.hash_hex ?? t("Core.NoData"),
);

const buildRouteTemplate = () =>
  decodeURI(chainPath(`/block/${route.params.blockhash as string}/{page}/`));

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
  const blockInfoLocal = await $fetch<BlockResponse>(getFetchBlockUrl(), { method: "POST", body: getFetchBlockBody() });
  blockData.value = blockInfoLocal;
};

const meta = computed(() => {
  return {
    title: t("Block.Meta.Title", { block: `#${blockHeight.value}` }),
    meta: [
      {
        name: "description",
        content: t("Block.Meta.Description", {
          block: `#${blockHeight.value}`,
        }),
      },
      {
        name: "og:title",
        content: t("Block.Meta.Title", { block: `#${blockHeight.value}` }),
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