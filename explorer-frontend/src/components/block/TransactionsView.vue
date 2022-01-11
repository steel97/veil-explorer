<template>
  <div v-for="tx in props.txdata" :key="'tx-' + tx.txId">
    <h2 class="font-semibold pt-4">
      <RouterLink
        :to="'/tx/' + tx.txId"
        class="
          block
          text-sky-700
          dark:text-sky-400
          hover:underline
          underline-offset-4
          text-xs
          max-w-full
          overflow-x-hidden
          text-ellipsis
          height-offset-fix
        "
      >
        {{ tx.txId }}
      </RouterLink>
    </h2>
    <TransactionData :tx="tx" />
  </div>
</template>

<script setup lang="ts">
import { TransactionSimpleDecoded } from "@/models/API/BlockResponse";
import TransactionData from "@/components/shared/TransactionData";

const config = useRuntimeConfig();

const props = defineProps<{
  txdata: Array<TransactionSimpleDecoded>;
}>();
</script>