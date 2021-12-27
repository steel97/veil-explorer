<template>
  <div>
    <h1 class="font-semibold py-4">
      <div class="inline-block max-w-full">
        {{ t("Block.BlockTitle") }}: {{ getBlock }}
      </div>
    </h1>
    <Block />
  </div>
</template>

<script setup lang="ts">
import Block from "@/components/block/Block";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const { getApiPath } = useConfigs();
const route = useRoute();
const config = useRuntimeConfig();

const getBlock = computed(() => `${route.params.blockhash}`);
const pageAddition = route.params.page != "" ? `/${route.params.page}` : "";

const meta = computed(() => {
  return {
    title: t("Block.Meta.Title", { block: getBlock.value }),
    meta: [
      {
        name: "description",
        content: t("Block.Meta.Description", { block: getBlock.value }),
      },
      {
        name: "og:title",
        content: t("Block.Meta.Title", { block: getBlock.value }),
      },
      {
        name: "og:url",
        content: `${config.BASE_URL}/block/${route.params.blockhash}${pageAddition}`,
      },
    ],
  };
});
useMeta(meta);
</script>