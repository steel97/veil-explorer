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

    <h1 class="font-semibold py-4 uppercase">
      {{ t("Block.Transactions") }}
    </h1>

    <TransactionsView
      v-if="blockData != null && blockData.transactions != null"
      :block="blockData"
    />
  </div>
</template>

<script setup lang="ts">
import BlockView from "@/components/block/BlockView";
import TransactionsView from "@/components/block/TransactionsView";
import { BlockResponse } from "@/models/API/BlockResponse";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const { getApiPath } = useConfigs();
const route = useRoute();
const config = useRuntimeConfig();

const page = (route.params.page > 0 ? route.params.page : 1) - 1;
const currentPage = ref(page + 1);

const fetchBlock = async () =>
  await useFetch<string, BlockResponse>(`${getApiPath()}/block`, {
    method: "POST",
    body: {
      height: route.params.blockheight,
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

const pageAddition = route.params.page != "" ? `/${route.params.page}` : "";

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
        content: `${config.BASE_URL}/block-height/${route.params.blockheight}${currentPage.value}`,
      },
    ],
  };
});
useMeta(meta);
</script>