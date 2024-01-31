<template>
  <div v-for="tx in props.txdata" :key="'tx-' + tx.txId">
    <h2 class="font-semibold pt-4">
      <NuxtLink :to="chainPath('/tx/' + tx.txId)" class="
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
        ">
        {{ tx.txId }}
      </NuxtLink>
    </h2>
    <SharedTransactionData :tx="tx" />
  </div>
</template>

<script setup lang="ts">
import type { TransactionSimpleDecoded } from "@/models/API/BlockResponse";

const props = defineProps<{
  txdata: Array<TransactionSimpleDecoded>;
}>();

const { chainPath } = useRoutingHelper();
</script>