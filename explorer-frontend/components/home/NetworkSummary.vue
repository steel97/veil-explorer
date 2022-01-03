<template>
  <div class="grid md:grid-cols-2 lg:grid-cols-4 gap-3">
    <div class="rounded p-4 text-xs bg-gray-50 dark:bg-gray-800">
      <div class="font-semibold mb-2 text-sm">
        {{ t("NetworkSummary.Blockchain") }}
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <CubeIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
          <span>{{ t("NetworkSummary.LastIndexedBlock") }}</span>
        </div>
        <div class="text-right">
          {{ blockchainData?.blocks ?? t("Core.NoData") }}
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <HashtagIcon
            class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
          /><span>{{ t("NetworkSummary.BestBlockHash") }}</span>
        </div>
        <div class="text-right overflow-hidden text-ellipsis max-right-width">
          {{ blockchainData?.bestblockhash ?? t("Core.NoData") }}
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <RefreshIcon
            class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
          /><span>{{ t("NetworkSummary.LastBlockUpdate") }}</span>
        </div>
        <div class="text-right overflow-hidden text-ellipsis max-right-width">
          <span v-if="blockchainData != null">{{
            formatDateTimeLocal(blockchainData?.mediantime)
          }}</span>
          <span v-else>{{ t("Core.NoData") }}</span>
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <DatabaseIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
          <span>{{ t("NetworkSummary.DataSize") }}</span>
        </div>
        <div class="text-right max-right-width">{{ getSize() }}</div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <PuzzleIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
          <span>{{ t("NetworkSummary.NextSuperblock") }}</span>
        </div>
        <div class="text-right max-right-width">
          {{ blockchainData?.next_super_block ?? t("Core.NoData") }}
        </div>
      </div>
    </div>
    <div class="rounded p-4 text-xs bg-gray-50 dark:bg-gray-800">
      <div class="font-semibold mb-2 text-sm">
        {{ t("NetworkSummary.Difficulty") }}
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <CreditCardIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
          <span>PoS</span>
        </div>
        <div class="text-right">
          {{ blockchainData?.difficulty_pos ?? t("Core.NoData") }}
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <ChipIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" /><span
            >ProgPow</span
          >
        </div>
        <div class="text-right">
          {{ blockchainData?.difficulty_progpow ?? t("Core.NoData") }}
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <LightningBoltIcon
            class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
          /><span>RandomX</span>
        </div>
        <div class="text-right">
          {{ blockchainData?.difficulty_randomx ?? t("Core.NoData") }}
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <ServerIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
          <span>SHA256D</span>
        </div>
        <div class="text-right">
          {{ blockchainData?.difficulty_sha256d ?? t("Core.NoData") }}
        </div>
      </div>
    </div>
    <div class="rounded p-4 text-xs bg-gray-50 dark:bg-gray-800">
      <div class="font-semibold mb-2 text-sm">
        {{ t("NetworkSummary.BlockSplit") }}
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <CreditCardIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
          <span>PoS</span>
        </div>
        <div class="text-right">
          <span
            v-if="
              backgroundData != null &&
              backgroundData.algoStats != null &&
              backgroundData.algoStats.pos
            "
            >{{ backgroundData?.algoStats?.pos }} ({{
              calculateBlocksplit(backgroundData?.algoStats?.pos)
            }}%)</span
          >
          <span v-else>{{ t("Core.NoData") }}</span>
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <ChipIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" /><span
            >ProgPow</span
          >
        </div>
        <div class="text-right">
          <span
            v-if="
              backgroundData != null &&
              backgroundData.algoStats != null &&
              backgroundData.algoStats.progpow
            "
            >{{ backgroundData?.algoStats?.progpow }} ({{
              calculateBlocksplit(backgroundData?.algoStats?.progpow)
            }}%)</span
          >
          <span v-else>{{ t("Core.NoData") }}</span>
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <LightningBoltIcon
            class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
          /><span>RandomX</span>
        </div>
        <div class="text-right">
          <span
            v-if="
              backgroundData != null &&
              backgroundData.algoStats != null &&
              backgroundData.algoStats.randomx
            "
            >{{ backgroundData?.algoStats?.randomx }} ({{
              calculateBlocksplit(backgroundData?.algoStats?.randomx)
            }}%)</span
          >
          <span v-else>{{ t("Core.NoData") }}</span>
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <ServerIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
          <span>SHA256D</span>
        </div>
        <div class="text-right">
          <span
            v-if="backgroundData != null && backgroundData.algoStats != null"
            >{{ backgroundData?.algoStats?.sha256d }} ({{
              calculateBlocksplit(backgroundData?.algoStats?.sha256d)
            }}%)</span
          >
          <span v-else>{{ t("Core.NoData") }}</span>
        </div>
      </div>
    </div>
    <div class="rounded p-4 text-xs bg-gray-50 dark:bg-gray-800">
      <div class="font-semibold mb-2 text-sm">
        {{ t("NetworkSummary.Supply") }}
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <CashIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
          <span>{{ t("NetworkSummary.Total") }}</span>
        </div>
        <div class="text-right">
          <span v-if="blockchainData != null">
            {{ blockchainData?.moneysupply / supplyDelimiter }}
          </span>
          <span v-else>{{ t("Core.NoData") }}</span>
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <ChartBarIcon
            class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
          /><span>{{ t("NetworkSummary.ZerocoinTotal") }}</span>
        </div>
        <div class="text-right">
          <span
            v-if="
              blockchainData != null && blockchainData.zerocoinsupply != null
            "
            >{{ calculateZerocoinTotal() }} ({{
              (
                (parseInt(calculateZerocoinTotal()) /
                  (blockchainData.moneysupply / supplyDelimiter)) *
                100
              ).toFixed(2)
            }}%)</span
          >
          <span v-else>{{ t("Core.NoData") }}</span>
        </div>
      </div>
      <div
        v-if="blockchainData != null && blockchainData.zerocoinsupply != null"
      >
        <div
          class="flex justify-between mb-2"
          v-for="(val, index) in blockchainData.zerocoinsupply.slice(
            0,
            blockchainData.zerocoinsupply.length - 1
          )"
          :key="'denom-' + index"
        >
          <div class="flex items-center">
            <ChartPieIcon
              class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
            /><span>{{ val.denom }}-denom</span>
          </div>
          <div class="text-right">
            {{ (val.amount / supplyDelimiter).toFixed(2) }} ({{
              (
                (val.amount /
                  supplyDelimiter /
                  (blockchainData?.moneysupply / supplyDelimiter)) *
                100
              ).toFixed(2)
            }}%)
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {
  CubeIcon,
  HashtagIcon,
  RefreshIcon,
  DatabaseIcon,
  PuzzleIcon,
  //
  CreditCardIcon,
  ChipIcon,
  LightningBoltIcon,
  ServerIcon,
  //,
  CashIcon,
  ChartBarIcon,
  ChartPieIcon,
} from "@heroicons/vue/solid";
import { BlockchainInfo } from "@/models/API/BlockchainInfo";
import { useFormatting } from "@/composables/Formatting";
import { useBackgroundInfo, useBlockchainInfo } from "@/composables/States";
import { useI18n } from "vue-i18n";
import { COIN } from "@/core/Constants";

const { t } = useI18n();
const { formatDateTimeLocal } = useFormatting();
const blockchainData = useBlockchainInfo();
const backgroundData = useBackgroundInfo();

const supplyDelimiter = ref(COIN);

const getSize = () => {
  let size = blockchainData.value?.size_on_disk ?? t("Core.NoData");
  if (blockchainData.value != null) {
    size =
      ((size as number) / 1024 / 1024 / 1024).toFixed(2).toString() + " GB";
  }
  return size;
};

const calculateBlocksplit = (val) => {
  const overall =
    backgroundData.value?.algoStats?.pos +
    backgroundData.value?.algoStats?.progpow +
    backgroundData.value?.algoStats?.randomx +
    backgroundData.value?.algoStats?.sha256d;
  return ((val / overall) * 100).toFixed(2);
};

const calculateZerocoinTotal = () =>
  (
    blockchainData.value?.zerocoinsupply.filter(
      (filter) => filter.denom == "total"
    )[0].amount / supplyDelimiter.value
  ).toFixed(2);
</script>

<style scoped>
.max-right-width {
  max-width: 120px;
}
</style>