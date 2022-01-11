<template>
  <div>
    <h1 class="font-semibold py-4">
      <div class="uppercase">{{ t("Tx.Title") }}</div>
      <div
        class="
          text-xs text-gray-500
          dark:text-gray-400
          max-w-full
          overflow-hidden
          text-ellipsis
        "
        v-if="tx != null && tx.txId != null"
      >
        {{ tx.txId }}
      </div>
    </h1>

    <div
      class="rounded p-4 bg-gray-50 dark:bg-gray-800 text-sm"
      v-if="tx != null"
    >
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
          <RouterLink
            :to="'/block-height/' + tx.blockHeight"
            class="
              text-sky-700
              dark:text-sky-400
              hover:underline
              underline-offset-4
            "
            >#{{ tx.blockHeight }}</RouterLink
          >
        </div>

        <!-- timestamp -->
        <div class="border-b py-4" v-if="tx.timestamp > 0">
          {{ t("Tx.Timestamp") }}
        </div>
        <div class="border-b py-4" v-if="tx.timestamp > 0">
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
          <span v-if="tx.size != tx.vSize">&nbsp;({{ tx.size }} vB)</span>
        </div>

        <!-- locktime -->
        <div class="border-b py-4" v-if="tx.locktime > 0">
          {{ t("Tx.Locktime") }}
        </div>
        <div class="border-b py-4" v-if="tx.locktime > 0">
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
    <SharedTransactionData
      :tx="tx.transaction"
      v-if="tx != null && tx.transaction != null"
    />
  </div>
</template>

<script setup lang="ts">
import { useFormatting } from "@/composables/Formatting";
import { useBlockchainInfo } from "@/composables/States";
import { TxRequest } from "@/models/API/TxRequest";
import { TxResponse } from "@/models/API/TxResponse";
import { LockClosedIcon } from "@heroicons/vue/solid";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const { getApiPath } = useConfigs();
const { formatDateTimeLocal } = useFormatting();
const data = useBlockchainInfo();
const route = useRoute();
const router = useRouter();
const config = useRuntimeConfig();

const mtxid = (route.params.txid as string).split("#")[0];

const fetchTx = async () =>
  await useFetch<string, TxResponse>(`${getApiPath()}/tx`, {
    method: "POST",
    body: {
      hash: mtxid,
    },
  });
const cdata = await fetchTx();
if (cdata.error.value) {
  router.replace("/search/notfound");
}
const tx = ref(cdata.data);

const getConfirmationClass = computed(() => {
  const confirmations = calculateConfirmations.value;
  if (confirmations == 0) return "text-rose-700 dark:text-rose-400";
  if (confirmations < 12) return "text-yellow-700 dark:text-yellow-400";
  return "text-green-700 dark:text-green-400";
});

const calculateConfirmations = computed(() => {
  if (!tx.value.confirmed) return 0;
  return data.value.blocks - tx.value.blockHeight + 1;
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
        content: `${config.BASE_URL}/tx/${mtxid}`,
      },
    ],
  };
});
useMeta(meta);
</script>