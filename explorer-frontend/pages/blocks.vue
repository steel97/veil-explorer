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
      <BlocksTable :data="blocks" />
    </div>
  </div>
</template>

<script setup lang="ts">
import BlocksTable from "@/components/shared/BlocksTable";
import { useI18n } from "vue-i18n";
import { reactive } from "@vue/reactivity";
import { useLatestBlockInfo } from "@/composables/States";
import { useConfigs } from "@/composables/Configs";
import { SimplifiedBlock } from "@/models/API/SimplifiedBlock";

const { getApiPath } = useConfigs();
const config = useRuntimeConfig();
const latestBlock = useLatestBlockInfo();

const route = useRoute();

const page = (route.params.page > 0 ? route.params.page : 1) - 1;
const currentPage = ref(page + 1);

const sort: string = route.params.sort ?? "desc";
let targetSort = sort.toLowerCase() == "asc" ? 0 : 1;

const buildRouteSort = (target: string) =>
  "/blocks/" + currentPage.value + "/" + target;

const changeSort = async (target: string) => {
  targetSort = target == "asc" ? 0 : 1;
  window.history.replaceState({}, null, buildRouteSort(target));
  const blocksInfoLocal = await fetchBlocks();
  blocks.value = blocksInfoLocal.data.value;
};

const fetchBlocks = async () =>
  await useAsyncData("blocksinfo", () =>
    $fetch<Array<SimplifiedBlock>>(
      `${getApiPath()}/blocks?offset=${
        (currentPage.value - 1) * config.BLOCKS_PER_PAGE
      }&count=${config.BLOCKS_PER_PAGE}&sort=${targetSort}`
    )
  );

const blocksInfo = await fetchBlocks();
const blocks = ref<Array<SimplifiedBlock>>(blocksInfo.data.value);

watch(latestBlock, (nval) => {
  if (currentPage.value > 1 || targetSort == 0) return;
  blocks.value.pop();
  blocks.value.unshift(nval);
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