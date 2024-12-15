<template>
  <div class="flex justify-center">
    <div
      class="
        my-6
        max-w-7xl
        w-full
        p-3
        text-center
        rounded
        uppercase
        font-semibold
        bg-yellow-300
        text-gray-600
        dark:bg-yellow-500 dark:text-gray-700
      "
    >
      {{ t("Core.Synchronizing", { progress: syncState.toFixed(2) }) }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { useBackgroundInfo, useBlockchainInfo } from "@/composables/States";

const { t } = useI18n();
const backgroundInfoDataState = useBackgroundInfo();
const blockchaininfoDataState = useBlockchainInfo();

const syncState = computed(() => {
  if (
    backgroundInfoDataState.value == null
    || blockchaininfoDataState.value == null
  ) {
    return 0;
  }
  return (
    (backgroundInfoDataState.value.currentSyncedBlock
      / blockchaininfoDataState.value.blocks)
    * 100
  );
});
</script>