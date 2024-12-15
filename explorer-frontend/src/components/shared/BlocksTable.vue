<template>
  <div class="hidden md:grid md:grid-cols-7 p-1 py-4 text-sm gap-3 font-semibold">
    <div>{{ t("Blocks.Height") }}</div>
    <div>{{ t("Blocks.Timestamp") }}</div>
    <div>{{ t("Blocks.Age") }}</div>
    <div>{{ t("Blocks.Type") }}</div>
    <div>{{ t("Blocks.Transactions") }}</div>
    <div>{{ t("Blocks.Size") }}</div>
    <div>{{ t("Blocks.Weight") }}</div>
  </div>
  <div
    v-for="(val, index) in props.data" :id="`block-${val.height}`"
    :key="`block-${val.height}`" class="grid grid-cols-2 md:grid-cols-7 p-1 py-4 text-sm gap-3" :class="index < props.data.length - 1 ? 'border-b' : ''"
  >
    <div>
      <NuxtLink
        :to="chainPath(`/block-height/${val.height}`)" class="
          text-sky-700
          dark:text-sky-400
          hover:underline
          underline-offset-4
        "
      >
        #{{ val.height }}
      </NuxtLink>
    </div>
    <button
      :aria-label="t('Blocks.Expand')" class="flex justify-end items-center md:hidden"
      @click="toggleBlockInfo(val)"
    >
      <span>{{ getAge(val) }}</span>
      <ChevronDownIcon class="h-5 w-5 text-sky-700 dark:text-sky-400" />
    </button>
    <div v-if="openedBlock.indexOf(val.height) > -1" class="md:hidden grid grid-cols-2 col-span-2">
      <div>{{ t("Blocks.Timestamp") }}</div>
      <div class="text-right">
        <div>{{ formatDateLocal(val.time) }}</div>
        <div class="text-xs text-gray-500 dark:text-gray-400">
          {{ formatTimeLocal(val.time) }}
        </div>
      </div>

      <div>{{ t("Blocks.Type") }}</div>
      <div class="text-right">
        <div>{{ getPow(val.proofType)[0] }}</div>
        <div
          v-if="getPow(val.proofType)[2] != null" class="text-xs text-gray-500 dark:text-gray-400 yy"
          v-html="getPow(val.proofType)[1]"
        ></div>
      </div>

      <div>{{ t("Blocks.Transactions") }}</div>
      <div class="text-right">
        {{ val.txCount }}
      </div>

      <div>{{ t("Blocks.Size") }}</div>
      <div class="text-right">
        {{ val.size }}
      </div>

      <div>{{ t("Blocks.Weight") }}</div>
      <div class="text-right">
        <div>{{ val.weight }} ({{ getBlockWeightRaw(val) }}%)</div>
        <div class="rounded bg-gray-200 dark:bg-gray-600 h-1">
          <div class="rounded bg-sky-700 dark:bg-sky-400 h-full" :style="getBlockWeight(val)"></div>
        </div>
      </div>
    </div>
    <div class="hidden md:block">
      <div>{{ formatDateLocal(val.time) }}</div>
      <div class="text-xs text-gray-500 dark:text-gray-400">
        {{ formatTimeLocal(val.time) }}
      </div>
    </div>
    <div class="hidden md:block">
      {{ getAge(val) }}
    </div>
    <div class="hidden md:block">
      <div>{{ getPow(val.proofType)[0] }}</div>
      <div class="text-xs text-gray-500 dark:text-gray-400" v-html="getPow(val.proofType)[1]"></div>
    </div>
    <div class="hidden md:block">
      {{ val.txCount }}
    </div>
    <div class="hidden md:block">
      {{ val.size }}
    </div>
    <div class="hidden md:block">
      <div>{{ val.weight }} ({{ getBlockWeightRaw(val) }}%)</div>
      <div class="rounded bg-gray-200 dark:bg-gray-600 h-1">
        <div class="rounded bg-sky-700 dark:bg-sky-400 h-full" :style="getBlockWeight(val)"></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { SimplifiedBlock } from "@/models/API/SimplifiedBlock";
import { useBlockchain } from "@/composables/Blockchain";
import { useFormatting } from "@/composables/Formatting";
import { ChevronDownIcon/* , ChevronUpIcon */ } from "@heroicons/vue/24/solid";

const props = defineProps<{
  data: Array<SimplifiedBlock>;
  reactivityFix: number;
}>();
const config = useRuntimeConfig();
const watcher = ref(0);
let isActive = true;

const { chainPath } = useRoutingHelper();

const date = useState("date", () => Date.now());
let mounted = false;

onMounted(() => {
  mounted = true;
  setTimeout(watcherTimer, 0);
});

const watcherTimer = () => {
  if (!isActive)
    return;
  watcher.value++;
  setTimeout(watcherTimer, 1000);
};

const { getPow } = useBlockchain();
const { formatDateLocal, formatTimeLocal } = useFormatting();
const { t } = useI18n();

const getAge = (block: SimplifiedBlock) => {
  // 0 / watcher - reactivity trigger
  const reactivity = 0 / watcher.value;
  const diff = (mounted ? Date.now() : date.value) / 1000 - block.time;

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
  for (let index = 0; index < allChecks.length; index++) {
    const check = allChecks[index];
    if (diff <= check.val)
      continue;

    const infFormatted = diff / (check.val > 0 ? check.val : 1);
    if (Math.floor(infFormatted) === 0)
      continue;
    let secondaryInfFormatted = 0;
    let secondaryInfFloor = 0;
    if (index + 1 < allChecks.length) {
      const scheck = allChecks[index + 1];
      const previnf
        = (Math.floor(infFormatted) * check.val)
        / (scheck.val > 0 ? scheck.val : 1);
      secondaryInfFormatted
        = diff / (scheck.val > 0 ? scheck.val : 1) - previnf;
      secondaryInfFloor = Math.floor(secondaryInfFormatted);
    }
    switch (check.locale) {
      case "Year":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Year")}`;
        if (secondaryInfFloor > 0)
          resp += `, ${secondaryInfFloor} ${t("Core.Ages.Month")}`;
        break;
      case "Month":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Month")}`;
        if (secondaryInfFloor > 0)
          resp += `, ${secondaryInfFloor} ${t("Core.Ages.Week")}`;
        break;
      case "Week":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Week")}`;
        if (secondaryInfFloor > 0)
          resp += `, ${secondaryInfFloor} ${t("Core.Ages.Day")}`;
        break;
      case "Day":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Day")}`;
        if (secondaryInfFloor > 0)
          resp += `, ${secondaryInfFloor} ${t("Core.Ages.Hour")}`;
        break;
      case "Hour":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Hour")}`;
        if (secondaryInfFloor > 0)
          resp += `, ${secondaryInfFloor} ${t("Core.Ages.Minute")}`;
        break;
      case "Minute":
        resp = `${Math.floor(infFormatted)} ${t("Core.Ages.Minute")}`;
        if (secondaryInfFloor > 0)
          resp += `, ${secondaryInfFloor} ${t("Core.Ages.Second")}`;
        break;
      case "Second":
        const sec = Math.floor(infFormatted);
        if (Number.isFinite(sec) && !Number.isNaN(sec))
          resp = `${sec} ${t("Core.Ages.Second")}`;
        break;
    }
    break;
  }
  if (resp === "")
    resp = `0 ${t("Core.Ages.Second")}`;
  return resp;
};

const getBlockWeightRaw = (block: SimplifiedBlock) =>
  ((100 * block.weight) / config.public.maxBlockWeight).toFixed(1);

const getBlockWeight = (block: SimplifiedBlock) =>
  `width: ${getBlockWeightRaw(block)}%`;

const reactivityWatcher = computed(() => props.reactivityFix);

let targetOpacity = 1.0;
const latestBlock = ref<number>(0);
watch(reactivityWatcher, () => {
  if (props.data.length === 0)
    return;

  const el = document.getElementById(`block-${latestBlock.value}`);
  if (el != null)
    el.style.opacity = "1.0";

  latestBlock.value = props.data[0].height;
  targetOpacity = 0;
  lastTime = null;
  if (!document.hidden)
    requestAnimationFrame(blockAppearAnimator);
});

const getStyle = (index: number) => {
  if (index === 0)
    return { opacity: targetOpacity };
  return {};
};

const openedBlock = reactive<Array<number>>([]);
const toggleBlockInfo = (block: SimplifiedBlock) => {
  const blockIndex = openedBlock.indexOf(block.height);
  if (blockIndex > -1)
    openedBlock.splice(blockIndex, 1);
  else openedBlock.push(block.height);
};

let stopUpdates = false;
let lastTime: number | null;

const blockAppearAnimator = (timestamp: number) => {
  if (lastTime === null)
    lastTime = timestamp;

  const elapsed = timestamp - lastTime;
  lastTime = timestamp;

  if (stopUpdates)
    return;

  const el = document.getElementById(`block-${latestBlock.value}`);

  const currentOpacity = targetOpacity + 0.001 * elapsed;
  if (currentOpacity > 1.0) {
    targetOpacity = 1.0;
    if (el != null)
      el.style.opacity = targetOpacity.toString();
    return;
  }
  targetOpacity = currentOpacity;

  if (el != null)
    el.style.opacity = targetOpacity.toString();
  requestAnimationFrame(blockAppearAnimator);
};

onBeforeUnmount(() => {
  isActive = false;
  stopUpdates = true;
});
</script>