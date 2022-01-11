<template>
  <div>
    <h1 class="uppercase font-semibold py-4">
      {{ t("Home.NetworkSummary") }}
    </h1>
    <NetworkSummary />
    <div class="flex pb-4 pt-6 justify-between">
      <h1 class="uppercase font-semibold">
        {{ t("Home.RecentBlocks") }}
      </h1>
      <div class="text-right">
        <NuxtLink
          to="/blocks"
          class="
            uppercase
            text-sky-700
            dark:text-sky-400
            hover:underline
            underline-offset-4
          "
        >
          {{ t("Home.BrowseBlocks") }}
        </NuxtLink>
      </div>
    </div>
    <RecentBlocks />
  </div>
</template>

<script setup lang="ts">
import NetworkSummary from "@/components/home/NetworkSummary";
import RecentBlocks from "@/components/home/RecentBlocks";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const config = useRuntimeConfig();

const meta = computed(() => {
  return {
    title: t("Home.Meta.Title"),
    meta: [
      {
        name: "description",
        content: t("Home.Meta.Description"),
      },
      {
        name: "og:title",
        content: t("Home.Meta.Title"),
      },
      {
        name: "og:url",
        content: `${config.BASE_URL}/`,
      },
    ],
  };
});
useMeta(meta);
</script>