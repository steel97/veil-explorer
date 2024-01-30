<template>
  <div class="rounded p-4 bg-gray-50 dark:bg-gray-800 text-sm mt-2">
    <div class="md:grid md:grid-cols-11">
      <div class="col-span-5">
        <div v-for="(input, inputId) in props.tx.inputs" :key="'input-' + inputId"
          :id="'input-' + (inputId + 0).toString()" class="p-3 bg-gray-200 dark:bg-gray-700 rounded"
          :class="inputId == (props.tx.inputs?.length ?? 0) - 1 ? '' : 'mb-4'">
          <div class="block lg:flex justify-between">
            <div v-if="props.tx.isBasecoin" class="flex flex-wrap basis-0 flex-grow items-center">
              <div class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  mr-2
                ">
                {{ t("Tx.Coinbase") }}
              </div>
              <div class="text-xs">{{ t("Tx.NoInputs") }}</div>
            </div>
            <div v-if="input.type == TxInType.ZEROCOIN_SPEND" class="flex flex-wrap basis-0 flex-grow items-center">
              <div class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  mr-2
                ">
                {{ t("Tx.Zerocoin") }}
              </div>
              <div class="text-xs">{{ t("Tx.ZerocoinSpend") }}</div>
            </div>
            <div v-if="input.type == TxInType.ANON" class="flex flex-wrap basis-0 flex-grow items-center">
              <div class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  mr-2
                ">
                {{ t("Tx.RingCT") }}
              </div>
              <div class="text-xs">{{ t("Tx.RingCTInput") }}</div>
            </div>
            <div v-else-if="input.prevOutAddresses != null &&
              input.prevOutAddresses.length > 0
              ">
              <div class="block lg:flex flex-wrap basis-0 flex-grow items-center">
                <ArrowRightCircleIcon class="
                    inline-block
                    lg:block
                    h-4
                    w-4
                    text-sky-700
                    dark:text-sky-400
                    mr-2
                  " />
                <RouterLink :to="'/address/' + input.prevOutAddresses[0]" class="
                    inline-block
                    max-width-hack
                    align-middle
                    lg:block lg:align-baseline lg:max-w-none
                    text-sky-700
                    dark:text-sky-400
                    hover:underline
                    underline-offset-4
                    text-xs
                    max-w-full
                    overflow-x-hidden
                    text-ellipsis
                    height-offset-fix
                  ">
                  {{ input.prevOutAddresses[0] }}
                </RouterLink>
              </div>
              <div class="
                  text-xs
                  whitespace-nowrap
                  max-w-full
                  lg:max-w-xs
                  overflow-x-hidden
                  text-ellipsis
                  height-offset-fix
                ">
                via&nbsp;
                <RouterLink :to="'/tx/' + input.prevOutTx + '#output-' + input.prevOutNum" class="
                    text-sky-700
                    dark:text-sky-400
                    hover:underline
                    underline-offset-4
                  ">
                  [{{ input.prevOutNum }}] {{ input.prevOutTx }}
                </RouterLink>
              </div>
            </div>

            <div class="basis-auto mt-2 lg:m-0">
              <span class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  whitespace-nowrap
                " v-if="shouldRenderInputAmount(input)">{{ getAmountForInput(input) }} VEIL</span>
            </div>
          </div>
        </div>
      </div>
      <div class="my-2 md:my-0">
        <ChevronRightIcon class="h-8 w-8 m-auto text-sky-700 dark:text-sky-400 hidden md:block" />
        <ChevronDownIcon class="h-8 w-8 m-auto text-sky-700 dark:text-sky-400 block md:hidden" />
      </div>
      <!-- OUTPUTS -->
      <div class="col-span-5">
        <div v-for="(output, outputId) in props.tx.outputs" :key="'output-' + outputId.toString() + '-' + reactivityFix.toString()
          " :id="'output-' + (outputId + 0).toString()" class="p-3 bg-gray-200 dark:bg-gray-700 rounded"
          :class="getOutputClasses(outputId + 0, output)">
          <div class="block lg:flex justify-between">
            <div v-if="output.addresses != null && output.addresses.length > 0">
              <div class="mr-2 block lg:inline-block">#{{ outputId + 0 }}</div>
              <RouterLink :to="'/address/' + output.addresses[0]" class="
                  inline-block
                  align-middle
                  lg:align-bottom lg:max-w-none
                  text-sky-700
                  dark:text-sky-400
                  hover:underline
                  underline-offset-4
                  text-xs
                  max-w-full
                  overflow-x-hidden
                  text-ellipsis
                  height-offset-fix
                ">
                {{ output.addresses[0] }}
              </RouterLink>
            </div>

            <!-- start others -->
            <div v-if="output.isOpreturn" class="flex flex-wrap basis-0 flex-grow items-center">
              <div class="mr-2">#{{ outputId + 0 }}</div>
              <div class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  mr-2
                ">
                {{ t("Tx.OpReturn") }}
              </div>
            </div>

            <div v-if="output.type == OutputTypes.OUTPUT_STANDARD &&
              output.scriptPubKeyType == txnouttype.TX_ZEROCOINMINT
              " class="flex flex-wrap basis-0 flex-grow items-center">
              <div class="mr-2">#{{ outputId + 0 }}</div>
              <div class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  mr-2
                ">
                {{ t("Tx.Zerocoin") }}
              </div>
              <div class="text-xs">{{ t("Tx.ZerocoinMint") }}</div>
            </div>

            <div v-if="output.type == OutputTypes.OUTPUT_STANDARD &&
              output.scriptPubKeyType == txnouttype.TX_NONSTANDARD &&
              !output.isCoinBase
              " class="flex flex-wrap basis-0 flex-grow items-center">
              <div class="mr-2">#{{ outputId + 0 }}</div>
              <div class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  mr-2
                ">
                {{ t("Tx.CoinStake") }}
              </div>
            </div>

            <div v-if="output.isCoinBase &&
              output.scriptPubKeyType == txnouttype.TX_NONSTANDARD
              " class="flex flex-wrap basis-0 flex-grow items-center">
              <div class="mr-2">#{{ outputId + 0 }}</div>
              <div class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  mr-2
                ">
                {{ t("Tx.Coinbase") }}
              </div>
            </div>

            <div v-if="output.type == OutputTypes.OUTPUT_DATA" class="flex flex-wrap basis-0 flex-grow items-center">
              <div class="mr-2">#{{ outputId + 0 }}</div>
              <div class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  mr-2
                ">
                {{ t("Tx.RangeProof") }}
              </div>
            </div>

            <div v-if="output.type == OutputTypes.OUTPUT_RINGCT" class="flex flex-wrap basis-0 flex-grow items-center">
              <div class="mr-2">#{{ outputId + 0 }}</div>
              <div class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  mr-2
                ">
                {{ t("Tx.RingCT") }}
              </div>
            </div>

            <div class="basis-auto mt-2 lg:m-0">
              <span class="
                  bg-sky-700
                  dark:bg-sky-400
                  text-gray-50
                  dark:text-gray-700
                  rounded
                  font-bold
                  text-xs
                  p-1
                  whitespace-nowrap
                " v-if="output.type != OutputTypes.OUTPUT_STANDARD ||
                  output.amount > 0
                  ">
                <span v-if="output.type != OutputTypes.OUTPUT_STANDARD">
                  {{ t("Tx.HiddenAmount") }}
                </span>
                <span v-else-if="output.amount > 0">
                  {{ formatAmount(output.amount) }} VEIL</span>
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import ArrowRightCircleIcon from "@heroicons/vue/24/solid/ArrowRightCircleIcon";
import ChevronRightIcon from "@heroicons/vue/24/solid/ChevronRightIcon";
import ChevronDownIcon from "@heroicons/vue/24/solid/ChevronDownIcon";
import {
  TxInType,
  OutputTypes,
  txnouttype
} from "@/models/API/BlockResponse";
import type {
  TransactionSimpleDecoded,
  TxVinSimpleDecoded,
  TxVoutSimpleDecoded,
} from "@/models/API/BlockResponse";
import { COIN } from "@/core/Constants";

const { t } = useI18n();

const props = defineProps<{
  tx: TransactionSimpleDecoded;
}>();

const reactivityFix = ref(0);

const formatAmount = (amount: number) => {
  return amount / COIN;
};

const computeTotalInputsAmount = () => {
  let result = 0;
  props.tx.inputs?.forEach((input) => (result += input.prevOutAmount));
  return result;
};

const computeTotalOutputsAmount = () => {
  let result = 0;
  props.tx.outputs?.forEach((out) => (result += out.amount));
  return result;
};

const shouldRenderInputAmount = (input: TxVinSimpleDecoded) =>
  input.prevOutAmount > -1;

const getAmountForInput = (input: TxVinSimpleDecoded) => {
  let amount = input.prevOutAmount;
  if (input.type == TxInType.ANON)
    amount = computeTotalOutputsAmount() - computeTotalInputsAmount();
  return formatAmount(amount);
};

const getOutputClasses = (outputId: number, output: TxVoutSimpleDecoded) => {
  let classes = [outputId == props.tx.outputs!.length - 1 ? "" : "mb-4"];

  if (process.client) {
    if (
      window.location.hash &&
      (window.location.hash as any as string) == `#output-${outputId}`
    ) {
      classes.push(
        ...["outline", "outline-1", "outline-sky-700", "dark:outline-sky-400"]
      );
    }
  }

  return classes;
};

onMounted(() => reactivityFix.value++);
</script>