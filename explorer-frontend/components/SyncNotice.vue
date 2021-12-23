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
import { useChainInfo } from "@/composables/States";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const chainInfoDataState = useChainInfo();
const syncState = computed(
  () =>
    (chainInfoDataState.value.currentSyncedBlock /
      chainInfoDataState.value.chainInfo.blocks) *
    100
);
</script>