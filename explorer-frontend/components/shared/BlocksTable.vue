<template>
  <div
    class="hidden md:grid md:grid-cols-7 p-1 py-4 text-sm gap-3 font-semibold"
  >
    <div>{{ t("Blocks.Height") }}</div>
    <div>{{ t("Blocks.Timestamp") }}</div>
    <div>{{ t("Blocks.Age") }}</div>
    <div>{{ t("Blocks.Type") }}</div>
    <div>{{ t("Blocks.Transactions") }}</div>
    <div>{{ t("Blocks.Size") }}</div>
    <div>{{ t("Blocks.Weight") }}</div>
  </div>
  <div
    class="grid grid-cols-2 md:grid-cols-7 p-1 py-4 text-sm gap-3"
    v-for="(val, index) in props.data"
    :key="'block-' + val.height"
    :class="index < props.data.length - 1 ? 'border-b' : ''"
    :style="getStyle(index)"
  >
    <div>
      <NuxtLink
        :to="'/block-height/' + val.height"
        class="
          text-sky-700
          dark:text-sky-400
          hover:underline
          underline-offset-4
        "
        >#{{ val.height }}</NuxtLink
      >
    </div>
    <button
      :aria-label="t('Blocks.Expand')"
      class="flex justify-end items-center md:hidden"
      @click="toggleBlockInfo(val)"
    >
      <span>{{ getAge(val) }}</span>
      <ChevronDownIcon class="h-5 w-5 text-sky-700 dark:text-sky-400" />
    </button>
    <div
      class="md:hidden grid grid-cols-2 col-span-2"
      v-if="openedBlock.indexOf(val.height) > -1"
    >
      <div>{{ t("Blocks.Timestamp") }}</div>
      <div class="text-right">{{ formatDateLocal(val.time) }}</div>

      <div>{{ t("Blocks.Type") }}</div>
      <div class="text-right">
        <div>{{ getPow(val)[0] }}</div>
        <div
          class="text-xs text-gray-500 dark:text-gray-400"
          v-html="getPow(val)[1]"
        ></div>
      </div>

      <div>{{ t("Blocks.Transactions") }}</div>
      <div class="text-right">{{ val.txCount }}</div>

      <div>{{ t("Blocks.Size") }}</div>
      <div class="text-right">{{ val.size }}</div>

      <div>{{ t("Blocks.Weight") }}</div>
      <div class="text-right">
        <div>{{ val.weight }} ({{ getBlockWeightRaw(val) }}%)</div>
        <div class="rounded bg-gray-200 dark:bg-gray-600 h-1">
          <div
            class="rounded bg-sky-700 dark:bg-sky-400 h-full"
            :style="getBlockWeight(val)"
          ></div>
        </div>
      </div>
    </div>
    <div class="hidden md:block">{{ formatDateLocal(val.time) }}</div>
    <div class="hidden md:block">{{ getAge(val) }}</div>
    <div class="hidden md:block">
      <div>{{ getPow(val)[0] }}</div>
      <div
        class="text-xs text-gray-500 dark:text-gray-400"
        v-html="getPow(val)[1]"
      ></div>
    </div>
    <div class="hidden md:block">{{ val.txCount }}</div>
    <div class="hidden md:block">{{ val.size }}</div>
    <div class="hidden md:block">
      <div>{{ val.weight }} ({{ getBlockWeightRaw(val) }}%)</div>
      <div class="rounded bg-gray-200 dark:bg-gray-600 h-1">
        <div
          class="rounded bg-sky-700 dark:bg-sky-400 h-full"
          :style="getBlockWeight(val)"
        ></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ChevronDownIcon, ChevronUpIcon } from "@heroicons/vue/solid";
import { useI18n } from "vue-i18n";
import { useFormatting } from "@/composables/Formatting";
import { BlockType, SimplifiedBlock } from "@/models/API/SimplifiedBlock";
import locale from "~~/localization/en";

const config = useRuntimeConfig();

const props = defineProps<{
  data: Array<SimplifiedBlock>;
}>();

const { formatDateLocal } = useFormatting();
const { t } = useI18n();

const getAge = (block: SimplifiedBlock) => {
  const diff = Date.now() / 1000 - block.time;

  const minuteCheck = 60;
  const hourCheck = minuteCheck * 60;
  const dayCheck = hourCheck * 24;
  const weekCheck = dayCheck * 7;
  const monthCheck = dayCheck * 30;
  const yearCheck = dayCheck * 365;

  const allChecks = [
    {
      locale: "Year",
      val: yearCheck,
    },
    {
      locale: "Month",
      val: monthCheck,
    },
    {
      locale: "Week",
      val: weekCheck,
    },
    {
      locale: "Day",
      val: dayCheck,
    },
    {
      locale: "Hour",
      val: hourCheck,
    },
    {
      locale: "Minute",
      val: minuteCheck,
    },
    {
      locale: "Second",
      val: 0,
    },
  ];

  let resp = "";
  for (let index = 0; index < allChecks.length; i++) {
    const check = allChecks[index];
    if (diff < check.val) continue;

    const infFormatted = diff / check.val;
    let secondaryInfFormatted = 0;
    if (index + 1 < allChecks.length) {
      let scheck = allChecks[index + 1];
      let previnf = (Math.floor(infFormatted) * check.val) / scheck.val;
      secondaryInfFormatted = diff / scheck.val - previnf;
    }
    switch (check.locale) {
      case "Year":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Year")}`;
        if (secondaryInfFormatted > 0)
          resp += `, ${Math.floor(secondaryInfFormatted)} ${t(
            "Core.Ages.Month"
          )}`;
        break;
      case "Month":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Month")}`;
        if (secondaryInfFormatted > 0)
          resp += `, ${Math.floor(secondaryInfFormatted)} ${t(
            "Core.Ages.Week"
          )}`;
        break;
      case "Week":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Week")}`;
        if (secondaryInfFormatted > 0)
          resp += `, ${Math.floor(secondaryInfFormatted)} ${t(
            "Core.Ages.Day"
          )}`;
        break;
      case "Day":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Day")}`;
        if (secondaryInfFormatted > 0)
          resp += `, ${Math.floor(secondaryInfFormatted)} ${t(
            "Core.Ages.Hour"
          )}`;
        break;
      case "Hour":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Hour")}`;
        if (secondaryInfFormatted > 0)
          resp += `, ${Math.floor(secondaryInfFormatted)} ${t(
            "Core.Ages.Minute"
          )}`;
        break;
      case "Minute":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Minute")}`;
        if (secondaryInfFormatted > 0)
          resp += `, ${Math.floor(secondaryInfFormatted)} ${t(
            "Core.Ages.Second"
          )}`;
        break;
      case "Second":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Second")}`;
        break;
    }
    break;
  }
  return resp;
};

const getPow = (block: SimplifiedBlock) => {
  const proofType = block.proofType;

  let high = "";
  let low = "&nbsp;";

  switch (proofType) {
    case BlockType.UNKNOWN: {
      high = "Unknown";
      break;
    }
    case BlockType.POW_X16RT: {
      high = "Proof-of-work";
      low = "X16RT";
      break;
    }
    case BlockType.POW_ProgPow: {
      high = "Proof-of-work";
      low = "ProgPow";
      break;
    }
    case BlockType.POW_RandomX: {
      high = "Proof-of-work";
      low = "RandomX";
      break;
    }
    case BlockType.POW_Sha256D: {
      high = "Proof-of-work";
      low = "Sha256D";
      break;
    }
    case BlockType.POS: {
      high = "Proof-of-stake";
      break;
    }
  }

  return [high, low];
};

const getBlockWeightRaw = (block: SimplifiedBlock) =>
  ((100 * block.weight) / config.MAX_BLOCK_WEIGHT).toFixed(1);

const getBlockWeight = (block: SimplifiedBlock) =>
  `width: ${getBlockWeightRaw(block)}%`;

let targetOpacity = ref(1.0);
watch(props.data, (nval) => {
  targetOpacity.value = 0;
  lastTime = null;
  requestAnimationFrame(blockAppearAnimator);
});

const getStyle = (index: number) => {
  if (index == 0) return { opacity: targetOpacity.value };
  return {};
};

const openedBlock = reactive<Array<number>>([]);
const toggleBlockInfo = (block: SimplifiedBlock) => {
  const blockIndex = openedBlock.indexOf(block.height);
  if (blockIndex > -1) openedBlock.splice(blockIndex, 1);
  else openedBlock.push(block.height);
};

let stopUpdates = false;
let lastTime: number | null;

const blockAppearAnimator = (timestamp: number) => {
  if (lastTime == null) lastTime = timestamp;
  var elapsed = timestamp - lastTime;
  lastTime = timestamp;

  if (stopUpdates) return;

  let currentOpacity = targetOpacity.value + 0.001 * elapsed;
  if (currentOpacity > 1.0) {
    targetOpacity.value = 1.0;
    return;
  }
  targetOpacity.value = currentOpacity;
  requestAnimationFrame(blockAppearAnimator);
};

onBeforeUnmount(() => (stopUpdates = true));
</script>