<template>
  <div class="rounded p-4 bg-gray-50 dark:bg-gray-800">
    <BlocksTable :data="recentBlocks" :reactivityFix="reactivityFix" />
  </div>
</template>

<script setup lang="ts">
import BlocksTable from "@/components/shared/BlocksTable";
import { reactive } from "@vue/reactivity";
import { useLatestBlockInfo } from "@/composables/States";
import { useConfigs } from "@/composables/Configs";
import { SimplifiedBlock } from "@/models/API/SimplifiedBlock";

const { getApiPath } = useConfigs();
const config = useRuntimeConfig();
const latestBlock = useLatestBlockInfo();

const recentBlocksInfo = await useFetch<string, Array<SimplifiedBlock>>(
  `${getApiPath()}/blocks?offset=0&count=${config.RECENT_BLOCKS_COUNT}&sort=1`
);
const recentBlocks = reactive<Array<SimplifiedBlock>>(
  recentBlocksInfo.data.value
);
const reactivityFix = ref(0);

watch(latestBlock, (nval) => {
  recentBlocks.pop();
  recentBlocks.unshift(nval);
  reactivityFix.value++;
});
</script>