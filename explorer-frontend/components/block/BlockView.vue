<template>
  <div class="rounded p-4 bg-gray-50 dark:bg-gray-800 text-sm">
    <div class="md:grid grid-cols-2 gap-6">
      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b">
        <div>{{ t("Block.Prev") }}</div>
        <div class="text-right">
          <!-- should never happend? -->
          <div v-if="props.block.prevBlock != null">
            <div class="max-w-full overflow-hidden text-ellipsis">
              <RouterLink
                :to="'/block/' + props.block.prevBlock.hash"
                class="
                  text-sky-700
                  dark:text-sky-400
                  hover:underline
                  underline-offset-4
                "
                >{{ props.block.prevBlock.hash }}</RouterLink
              >
            </div>
            <div class="text-xs text-gray-500 dark:text-gray-400">
              #{{ props.block.prevBlock.height }}
            </div>
          </div>
          <div v-else>{{ t("Core.NoData") }}</div>
        </div>
      </div>

      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b">
        <div>{{ t("Block.Next") }}</div>
        <div class="text-right">
          <div v-if="props.block.nextBlock != null">
            <div class="max-w-full overflow-hidden text-ellipsis">
              <RouterLink
                :to="'/block/' + props.block.nextBlock.hash"
                class="
                  text-sky-700
                  dark:text-sky-400
                  hover:underline
                  underline-offset-4
                "
                >{{ props.block.nextBlock.hash }}</RouterLink
              >
            </div>
            <div class="text-xs text-gray-500 dark:text-gray-400">
              #{{ props.block.nextBlock.height }}
            </div>
          </div>
          <div v-else>{{ t("Block.LatestBlock") }}</div>
        </div>
      </div>
    </div>

    <div class="md:grid grid-cols-2 gap-6">
      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b">
        <div>{{ t("Block.ProofType") }}</div>
        <div class="text-right">
          <div>{{ getPow(props.block.block.proof_type)[0] }}</div>
          <div
            class="text-xs text-gray-500 dark:text-gray-400"
            v-html="getPow(props.block.block.proof_type)[1]"
          ></div>
        </div>
      </div>

      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b">
        <div>{{ t("Block.Version") }}</div>
        <div class="text-right">
          <div>0x{{ props.block.versionHex }}</div>
          <div class="text-xs text-gray-500 dark:text-gray-400">
            {{ props.block.block.version }}
          </div>
        </div>
      </div>
    </div>

    <div class="md:grid grid-cols-2 gap-6">
      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b">
        <div>{{ t("Block.Time") }}</div>
        <div class="text-right">
          <div>{{ formatDateLocal(props.block.block.time) }}</div>
          <div class="text-xs text-gray-500 dark:text-gray-400">
            {{ formatTimeLocal(props.block.block.time) }}
          </div>
        </div>
      </div>

      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b">
        <div>{{ t("Block.Nonce") }}</div>
        <div class="text-right">
          {{ props.block.block.nonce }}
        </div>
      </div>
    </div>

    <div class="md:grid grid-cols-2 gap-6">
      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b">
        <div>{{ t("Block.Txs") }}</div>
        <div class="text-right">
          {{ props.block.transactions.length }}
        </div>
      </div>

      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b">
        <div>{{ t("Block.MerkleRoot") }}</div>
        <div class="text-right overflow-hidden text-ellipsis">
          {{ props.block.block.merkleroot_hex }}
        </div>
      </div>
    </div>

    <div class="md:grid grid-cols-2 gap-6">
      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b">
        <div>{{ t("Block.Confirmations.Title") }}</div>
        <div class="text-right" :class="getConfirmationClass">
          <div
            class="flex items-center float-right"
            data-tooltip-target="tooltip-confirmations"
            ref="confirmationsEl"
            aria-describedby="tooltip-confirmations"
            @mouseenter="showTooltip"
            @focus="showTooltip"
            @mouseleave="hideTooltip"
            @blur="hideTooltip"
            @click.prevent="toggleTooltip"
          >
            <LockClosedIcon class="h-5 w-5" :class="getConfirmationClass" />
            {{ calculateConfirmations }}
          </div>
          <div
            ref="confirmationsTooltipEl"
            id="tooltip-confirmations"
            role="tooltip"
            class="
              tooltip
              bg-gray-300
              dark:bg-gray-600
              text-gray-800
              dark:text-gray-50
            "
          >
            {{ getTitle }}
            <div id="arrow" data-popper-arrow></div>
          </div>
        </div>
      </div>

      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b">
        <div>{{ t("Block.Bits") }}</div>
        <div class="text-right">
          {{ props.block.block.bits_hex }}
        </div>
      </div>
    </div>
    <div class="md:grid grid-cols-2 gap-6">
      <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b md:border-0">
        <div>{{ t("Block.Size") }}</div>
        <div class="text-right">
          <div>
            {{ props.block.block.weight }} ({{
              getBlockWeightRaw(props.block.block.weight)
            }}%)
          </div>
          <div class="rounded bg-gray-200 dark:bg-gray-600 h-1 mt-2">
            <div
              class="rounded bg-sky-700 dark:bg-sky-400 h-full"
              :style="getBlockWeight(props.block.block.weight)"
            ></div>
          </div>
        </div>
      </div>

      <div class="grid grid-cols-2 gap-0.5 w-full py-4">
        <div>{{ t("Block.Difficulty") }}</div>
        <div class="text-right overflow-hidden text-ellipsis">
          {{ props.block.block.difficulty }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { BlockResponse } from "@/models/API/BlockResponse";
import { useBlockchain } from "@/composables/Blockchain";
import { useFormatting } from "@/composables/Formatting";
import { useChainInfo } from "@/composables/States";
import { LockClosedIcon } from "@heroicons/vue/solid";
import { useI18n } from "vue-i18n";
import {
  createPopperLite as createPopper,
  preventOverflow,
  flip,
  hide,
} from "@popperjs/core";

const props = defineProps<{
  block: BlockResponse;
}>();

const { t } = useI18n();
const { getPow } = useBlockchain();
const { formatDateLocal, formatTimeLocal } = useFormatting();
const confirmationsEl = ref<HTMLElement>(null);
const confirmationsTooltipEl = ref<HTMLElement>(null);
const config = useRuntimeConfig();
const data = useChainInfo();

const calculateConfirmations = computed(
  () => data.value.chainInfo.blocks - props.block.block.height + 1
);

const getConfirmationClass = computed(() => {
  const confirmations = calculateConfirmations.value;
  if (confirmations == 0) return "text-rose-700 dark:text-rose-400";
  if (confirmations < 12) return "text-yellow-700 dark:text-yellow-400";
  return "text-green-700 dark:text-green-400";
});

const getTitle = computed(() =>
  calculateConfirmations.value < 12
    ? t("Block.Confirmations.Low")
    : t("Block.Confirmations.Enough")
);

const getBlockWeightRaw = (weight: number) =>
  ((100 * weight) / config.MAX_BLOCK_WEIGHT).toFixed(1);

const getBlockWeight = (weight: number) =>
  `width: ${getBlockWeightRaw(weight)}%`;

const showTooltip = () => {
  confirmationsTooltipEl.value.setAttribute("data-show", "");
};

const hideTooltip = () => {
  confirmationsTooltipEl.value.removeAttribute("data-show");
};

const tooltipState = ref(false);

const toggleTooltip = () => {
  tooltipState.value = !tooltipState.value;
  if (tooltipState.value) showTooltip();
  else hideTooltip();
};

onMounted(() => {
  createPopper(confirmationsEl.value, confirmationsTooltipEl.value, {
    placement: "top",
  });
});
</script>