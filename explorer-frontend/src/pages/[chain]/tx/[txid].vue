<template>
  <div>
    <h1 class="font-semibold py-4">
      <div class="uppercase">
        {{ t("Tx.Title") }}
      </div>
      <div
        v-if="tx !== null && tx.txId !== null" class="
          text-xs text-gray-500
          dark:text-gray-400
          max-w-full
          overflow-hidden
          text-ellipsis
        "
      >
        {{ tx.txId }}
      </div>
    </h1>

    <div v-if="tx !== null" class="rounded p-4 bg-gray-50 dark:bg-gray-800 text-sm">
      <div class="grid grid-cols-2">
        <div v-if="!tx.confirmed" class="col-span-2 py-4">
          <div class="text-rose-700 dark:text-rose-400">
            {{ t("Tx.Unconfirmed") }}
          </div>
        </div>

        <!-- block height -->
        <div v-if="tx.confirmed" class="border-b py-4">
          {{ t("Tx.BlockHeight") }}
        </div>
        <div v-if="tx.confirmed" class="border-b py-4">
          <NuxtLink
            :to="chainPath(`/block-height/${tx.blockHeight}`)" class="
              text-sky-700
              dark:text-sky-400
              hover:underline
              underline-offset-4
            "
          >
            #{{ tx.blockHeight }}
          </NuxtLink>
        </div>

        <!-- timestamp -->
        <div v-if="tx.timestamp > 0" class="border-b py-4">
          {{ t("Tx.Timestamp") }}
        </div>
        <div v-if="tx.timestamp > 0" class="border-b py-4">
          {{ formatDateTimeLocal(tx.timestamp) }}
        </div>

        <!-- version -->
        <div class="border-b py-4">
          {{ t("Tx.Version") }}
        </div>
        <div class="border-b py-4">
          {{ tx.version }}
        </div>

        <!-- size -->
        <div class="border-b py-4">
          {{ t("Tx.Size") }}
        </div>
        <div class="border-b py-4">
          <span>{{ tx.size }} B</span>
          <span v-if="tx.size !== tx.vSize">&nbsp;({{ tx.size }} vB)</span>
        </div>

        <!-- locktime -->
        <div v-if="tx.locktime > 0" class="border-b py-4">
          {{ t("Tx.Locktime") }}
        </div>
        <div v-if="tx.locktime > 0" class="border-b py-4">
          {{ t("Tx.SpendableInBlock") }} #{{ tx.locktime }}
        </div>

        <!-- confirmations -->
        <div class="py-4">
          {{ t("Tx.Confirmations") }}
        </div>
        <div class="py-4 flex items-center" :class="getConfirmationClass">
          <LockClosedIcon class="h-5 w-5" :class="getConfirmationClass" />
          {{ calculateConfirmations }}
        </div>
      </div>
    </div>

    <h1 class="font-semibold pt-4 uppercase">
      {{ t("Tx.InputsOutputs") }}
    </h1>
    <SharedTransactionData v-if="tx !== null && tx.transaction !== null" :tx="tx.transaction" />
  </div>
</template>

<script setup lang="ts">
import type { TxResponse } from "@/models/API/TxResponse";
import { useFormatting } from "@/composables/Formatting";
import { useBlockchainInfo } from "@/composables/States";
import LockClosedIcon from "@heroicons/vue/24/solid/LockClosedIcon";

const { t } = useI18n();
const { getApiPath } = useConfigs();
const { formatDateTimeLocal } = useFormatting();
const { chainPath } = useRoutingHelper();
const data = useBlockchainInfo();
const route = useRoute();
const config = useRuntimeConfig();

const mtxid = (route.params.txid as string).split("#")[0];

const fetchTx = async () =>
  await useFetch<TxResponse>(`${getApiPath()}/tx`, {
    method: "POST",
    body: {
      hash: mtxid,
    },
  });
const cdata = await fetchTx();
if (cdata.error.value) {
  await navigateTo(chainPath("/search/notfound"));
}
const tx = ref(cdata.data);

const getConfirmationClass = computed(() => {
  const confirmations = calculateConfirmations.value;
  if (confirmations === 0)
    return "text-rose-700 dark:text-rose-400";
  if (confirmations < 12)
    return "text-yellow-700 dark:text-yellow-400";
  return "text-green-700 dark:text-green-400";
});

const calculateConfirmations = computed(() => {
  if (tx.value === null)
    return 0;
  if (!tx.value.confirmed)
    return 0;
  return (data.value?.blocks ?? 0) - tx.value.blockHeight + 1;
});

const meta = computed(() => {
  return {
    title: t("Tx.Meta.Title", { txid: mtxid }),
    meta: [
      {
        name: "description",
        content: t("Tx.Meta.Description", {
          txid: mtxid,
        }),
      },
      {
        name: "og:title",
        content: t("Tx.Meta.Title", { txid: mtxid }),
      },
      {
        name: "og:url",
        content: `${config.public.baseUrl}/tx/${mtxid}`,
      },
    ],
  };
});
useHead(meta);
</script>