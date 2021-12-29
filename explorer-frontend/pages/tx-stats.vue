<template>
  <div>
    <h1 class="uppercase font-semibold">
      {{ t("TxStats.Title") }}
    </h1>

    <div class="grid grid-cols-2">
      <ClientOnly> </ClientOnly>
    </div>
  </div>
</template>

<script setup lang="ts">
const meta = computed(() => {
  return {
    title: t("TxStats.Meta.Title"),
    meta: [
      {
        name: "description",
        content: t("TxStats.Meta.Description"),
      },
      {
        name: "og:title",
        content: t("TxStats.Meta.Title"),
      },
      {
        name: "og:url",
        content: `${config.BASE_URL}/tx-stats`,
      },
    ],
  };
});
useMeta(meta);
</script>