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

    <BlockView
      v-if="blockData != null && blockData.block != null"
      :block="blockData"
    />

    <h1 class="font-semibold pt-5 uppercase">
      {{ t("Block.Transactions") }}
    </h1>

    <TransactionsView
      v-if="blockData != null && blockData.transactions != null"
      :txdata="blockData.transactions"
    />

    <Pagination
      v-if="blockData != null && blockData.transactions != null"
      :overallEntries="blockData.txnCount"
      :entriesPerPage="config.TXS_PER_PAGE"
      :currentPage="currentPage"
      :linkTemplate="buildRouteTemplate()"
      @pageSelected="selectPage"
    />
  </div>
</template>

<script setup lang="ts">
import BlockView from "@/components/block/BlockView";
import TransactionsView from "@/components/block/TransactionsView";
import Pagination from "@/components/shared/Pagination";
import { BlockResponse } from "@/models/API/BlockResponse";
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

const fetchBlock = async () =>
  await useFetch<string, BlockResponse>(`${getApiPath()}/block`, {
    method: "POST",
    body: {
      hash: route.params.blockhash,
      offset: (currentPage.value - 1) * config.TXS_PER_PAGE,
      count: config.TXS_PER_PAGE,
    },
  });

const blockData = ref((await fetchBlock()).data);
const blockHeight = computed(
  () => blockData.value?.block?.height ?? t("Core.NoData")
);
const blockHash = computed(
  () => blockData.value?.block?.hash_hex ?? t("Core.NoData")
);

const buildRouteTemplate = () =>
  `/block/${route.params.blockhash as string}/{page}/`;

const selectPage = async (pg: number) => {
  if (pg == currentPage.value) return;

  scrollToAnimated(document.documentElement, 0, 150);

  const link = buildRouteTemplate().replace("{page}", pg.toString());
  currentPage.value = pg;
  window.history.replaceState({}, null, link);
  const blockInfoLocal = await fetchBlock();
  blockData.value = blockInfoLocal.data.value;
};

const meta = computed(() => {
  return {
    title: t("Block.Meta.Title", { block: "#" + blockHeight.value }),
    meta: [
      {
        name: "description",
        content: t("Block.Meta.Description", {
          block: "#" + blockHeight.value,
        }),
      },
      {
        name: "og:title",
        content: t("Block.Meta.Title", { block: "#" + blockHeight.value }),
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