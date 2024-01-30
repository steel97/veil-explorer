<template>
    <div>
        <div class="flex pb-4 pt-6 justify-between">
            <h1 class="uppercase font-semibold">
                {{ t("Blocks.Title") }}
            </h1>
            <div class="text-right">
                <a :href="buildRouteSort('desc')" @click.prevent="changeSort('desc')" class="
              uppercase
              text-sky-700
              dark:text-sky-400
              hover:underline
              underline-offset-4
            ">
                    {{ t("Blocks.SortDesc") }}
                </a>
                |
                <a :href="buildRouteSort('asc')" @click.prevent="changeSort('asc')" class="
              uppercase
              text-sky-700
              dark:text-sky-400
              hover:underline
              underline-offset-4
            ">
                    {{ t("Blocks.SortAsc") }}
                </a>
            </div>
        </div>
        <div class="rounded p-4 bg-gray-50 dark:bg-gray-800">
            <SharedBlocksTable :data="blocks" :reactivityFix="reactivityFix" />
        </div>
        <SharedPagination :overallEntries="backgroundInfoDataState != null &&
            backgroundInfoDataState.currentSyncedBlock != null
            ? backgroundInfoDataState.currentSyncedBlock
            : 0
            " :entriesPerPage="config.public.blocksPerPage" :currentPage="currentPage"
            :linkTemplate="buildRouteTemplate()" @pageSelected="selectPage" />
    </div>
</template>
  
<script setup lang="ts">
import { useLatestBlockInfo } from "@/composables/States";
import { useConfigs } from "@/composables/Configs";
import { useBackgroundInfo } from "@/composables/States";
import { useUI } from "@/composables/UI";
import type { SimplifiedBlock } from "@/models/API/SimplifiedBlock";

const { getApiPath } = useConfigs();
const { scrollToAnimated } = useUI();
const config = useRuntimeConfig();
const latestBlock = useLatestBlockInfo();
const backgroundInfoDataState = useBackgroundInfo();

const route = useRoute();

const page =
    ((route.params.page as any as number) > 0
        ? (route.params.page as any as number)
        : 1) - 1;
const currentPage = ref(page + 1);

const sort: string = (route.params.sort as string) ?? "desc";
const targetSort = ref(sort.toLowerCase() == "asc" ? 0 : 1);

const buildRouteSort = (target: string) =>
    "/blocks/" + currentPage.value + "/" + target;

const changeSort = async (target: string) => {
    targetSort.value = target == "asc" ? 0 : 1;
    window.history.replaceState({}, "", buildRouteSort(target));
    const blocksInfoLocal = await $fetch<Array<SimplifiedBlock> | null>(getFetchBlocksUrl());
    blocks.value = blocksInfoLocal ?? [];
    reactivityFix.value++;
};

const buildRouteTemplate = () =>
    "/blocks/{page}/" + (targetSort.value == 0 ? "asc" : "desc");

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
    const blocksInfoLocal = await $fetch<Array<SimplifiedBlock> | null>(getFetchBlocksUrl());
    blocks.value = blocksInfoLocal ?? [];
    reactivityFix.value++;
};

const getFetchBlocksUrl = () =>
    `${getApiPath()}/blocks?offset=${(currentPage.value - 1) * config.public.blocksPerPage
    }&count=${config.public.blocksPerPage}&sort=${targetSort.value}`;

const blocksInfo = await useFetch<Array<SimplifiedBlock>>(getFetchBlocksUrl());
const blocks = ref<Array<SimplifiedBlock>>(blocksInfo.data.value ?? []);
const reactivityFix = ref(0);

watch(latestBlock, (nval) => {
    if (nval == null) return;
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
                content: `${config.public.baseUrl}${buildRouteTemplate().replace(
                    "{page}",
                    currentPage.value.toString()
                )}`,
            },
        ],
    };
});
useHead(meta);
</script>