<template>
  <div>
    <div class="flex pb-4 pt-6 justify-between">
      <h1 class="uppercase font-semibold">
        {{ t("Blocks.Title") }}
      </h1>
      <div class="text-right">
        <a
          :href="buildRouteSort('desc')"
          @click.prevent="changeSort('desc')"
          class="
            uppercase
            text-sky-700
            dark:text-sky-400
            hover:underline
            underline-offset-4
          "
        >
          {{ t("Blocks.SortDesc") }}
        </a>
        |
        <a
          :href="buildRouteSort('asc')"
          @click.prevent="changeSort('asc')"
          class="
            uppercase
            text-sky-700
            dark:text-sky-400
            hover:underline
            underline-offset-4
          "
        >
          {{ t("Blocks.SortAsc") }}
        </a>
      </div>
    </div>
    <div class="rounded p-4 bg-gray-50 dark:bg-gray-800">
      <BlocksTable :data="blocks" :reactivityFix="reactivityFix" />
    </div>
    <Pagination
      :overallEntries="
        chainInfoDataState != null &&
        chainInfoDataState.currentSyncedBlock != null
          ? chainInfoDataState.currentSyncedBlock
          : 0
      "
      :entriesPerPage="config.BLOCKS_PER_PAGE"
      :currentPage="currentPage"
      :linkTemplate="buildRouteTemplate()"
      @pageSelected="selectPage"
    />
  </div>
</template>

<script setup lang="ts">
import BlocksTable from "@/components/shared/BlocksTable";
import Pagination from "@/components/shared/Pagination";
import { useI18n } from "vue-i18n";
import { reactive } from "@vue/reactivity";
import { useLatestBlockInfo } from "@/composables/States";
import { useConfigs } from "@/composables/Configs";
import { useChainInfo } from "@/composables/States";
import { SimplifiedBlock } from "@/models/API/SimplifiedBlock";

const { getApiPath } = useConfigs();
const config = useRuntimeConfig();
const latestBlock = useLatestBlockInfo();
const chainInfoDataState = useChainInfo();

const route = useRoute();

const page = (route.params.page > 0 ? route.params.page : 1) - 1;
const currentPage = ref(page + 1);

const sort: string = route.params.sort ?? "desc";
const targetSort = ref(sort.toLowerCase() == "asc" ? 0 : 1);

const buildRouteSort = (target: string) =>
  "/blocks/" + currentPage.value + "/" + target;

const changeSort = async (target: string) => {
  targetSort.value = target == "asc" ? 0 : 1;
  window.history.replaceState({}, null, buildRouteSort(target));
  const blocksInfoLocal = await fetchBlocks();
  blocks.value = blocksInfoLocal.data.value;
  reactivityFix.value++;
};

const buildRouteTemplate = () =>
  "/blocks/{page}/" + (targetSort.value == 0 ? "asc" : "desc");

const selectPage = async (pg: number) => {
  if (pg == currentPage.value) return;
  const link = buildRouteTemplate().replace("{page}", pg);
  currentPage.value = pg;
  window.history.replaceState({}, null, link);
  const blocksInfoLocal = await fetchBlocks();
  blocks.value = blocksInfoLocal.data.value;
  reactivityFix.value++;
};

const fetchBlocks = async () =>
  await useFetch<string, Array<SimplifiedBlock>>(
    `${getApiPath()}/blocks?offset=${
      (currentPage.value - 1) * config.BLOCKS_PER_PAGE
    }&count=${config.BLOCKS_PER_PAGE}&sort=${targetSort.value}`
  );

const blocksInfo = await fetchBlocks();
const blocks = ref<Array<SimplifiedBlock>>(blocksInfo.data.value);
const reactivityFix = ref(0);

watch(latestBlock, (nval) => {
  if (currentPage.value > 1 || targetSort.value == 0) return;
  blocks.value.pop();
  blocks.value.unshift(nval);
  reactivityFix.value++;
});

const { t } = useI18n();

const meta = computed(() => {
  return {
    title: t("Blocks.Meta.Title"),
    meta: [
      {
        name: "description",
        content: t("Blocks.Meta.Description"),
      },
      {
        name: "og:title",
        content: t("Blocks.Meta.Title"),
      },
      {
        name: "og:url",
        content: `${config.BASE_URL}/blocks`,
      },
    ],
  };
});
useMeta(meta);
</script>