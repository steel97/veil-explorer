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
          {{ data?.chainInfo?.blocks ?? t("Core.NoData") }}
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <HashtagIcon
            class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
          /><span>{{ t("NetworkSummary.BestBlockHash") }}</span>
        </div>
        <div class="text-right overflow-hidden text-ellipsis max-right-width">
          {{ data?.chainInfo?.bestblockhash ?? t("Core.NoData") }}
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <RefreshIcon
            class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
          /><span>{{ t("NetworkSummary.LastBlockUpdate") }}</span>
        </div>
        <div class="text-right overflow-hidden text-ellipsis max-right-width">
          <span v-if="data != null && data.chainInfo != null">{{
            formatDateTimeLocal(data?.chainInfo?.mediantime)
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
          {{ data?.chainInfo?.next_super_block ?? t("Core.NoData") }}
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
          {{ data?.chainInfo?.difficulty_pos ?? t("Core.NoData") }}
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <ChipIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" /><span
            >ProgPow</span
          >
        </div>
        <div class="text-right">
          {{ data?.chainInfo?.difficulty_progpow ?? t("Core.NoData") }}
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <LightningBoltIcon
            class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
          /><span>RandomX</span>
        </div>
        <div class="text-right">
          {{ data?.chainInfo?.difficulty_randomx ?? t("Core.NoData") }}
        </div>
      </div>
      <div class="flex justify-between mb-2">
        <div class="flex items-center">
          <ServerIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
          <span>SHA256D</span>
        </div>
        <div class="text-right">
          {{ data?.chainInfo?.difficulty_sha256d ?? t("Core.NoData") }}
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
            v-if="data != null && data.algoStats != null && data.algoStats.pos"
            >{{ data?.algoStats?.pos }} ({{
              calculateBlocksplit(data?.algoStats?.pos)
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
              data != null && data.algoStats != null && data.algoStats.progpow
            "
            >{{ data?.algoStats?.progpow }} ({{
              calculateBlocksplit(data?.algoStats?.progpow)
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
              data != null && data.algoStats != null && data.algoStats.randomx
            "
            >{{ data?.algoStats?.randomx }} ({{
              calculateBlocksplit(data?.algoStats?.randomx)
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
          <span v-if="data != null && data.algoStats != null"
            >{{ data?.algoStats?.sha256d }} ({{
              calculateBlocksplit(data?.algoStats?.sha256d)
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
          <span v-if="data != null && data.chainInfo != null">
            {{ data?.chainInfo?.moneysupply / supplyDelimiter }}
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
              data != null &&
              data.chainInfo != null &&
              data.chainInfo.zerocoinsupply != null
            "
            >{{ calculateZerocoinTotal() }} ({{
              (
                (parseInt(calculateZerocoinTotal()) /
                  (data?.chainInfo?.moneysupply / supplyDelimiter)) *
                100
              ).toFixed(2)
            }}%)</span
          >
          <span v-else>{{ t("Core.NoData") }}</span>
        </div>
      </div>
      <div
        v-if="
          data != null &&
          data.chainInfo != null &&
          data.chainInfo.zerocoinsupply != null
        "
      >
        <div
          class="flex justify-between mb-2"
          v-for="(val, index) in data.chainInfo.zerocoinsupply.slice(
            0,
            data.chainInfo.zerocoinsupply.length - 1
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
                  (data?.chainInfo?.moneysupply / supplyDelimiter)) *
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
import { useChainInfo } from "@/composables/States";
import { useI18n } from "vue-i18n";
import { COIN } from "@/core/Constants";

const { t } = useI18n();
const { formatDateTimeLocal } = useFormatting();
const data = useChainInfo();

const supplyDelimiter = ref(COIN);

const getSize = () => {
  let size = data.value?.chainInfo?.size_on_disk ?? t("Core.NoData");
  if (data.value != null && data.value.chainInfo != null) {
    size =
      ((size as number) / 1024 / 1024 / 1024).toFixed(2).toString() + " GB";
  }
  return size;
};

const calculateBlocksplit = (val) => {
  const overall =
    data.value?.algoStats?.pos +
    data.value?.algoStats?.progpow +
    data.value?.algoStats?.randomx +
    data.value?.algoStats?.sha256d;
  return ((val / overall) * 100).toFixed(2);
};

const calculateZerocoinTotal = () =>
  (
    data.value?.chainInfo.zerocoinsupply.filter(
      (filter) => filter.denom == "total"
    )[0].amount / supplyDelimiter.value
  ).toFixed(2);
</script>

<style scoped>
.max-right-width {
  max-width: 120px;
}
</style>