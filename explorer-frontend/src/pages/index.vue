<template>
  <div>
    <h1 class="uppercase font-semibold py-4">
      {{ t("Home.NetworkSummary") }}
    </h1>
    <HomeNetworkSummary />
    <div class="flex pb-4 pt-6 justify-between">
      <h1 class="uppercase font-semibold">
        {{ t("Home.RecentBlocks") }}
      </h1>
      <div class="text-right">
        <NuxtLink :to="localePath('/blocks')" class="
            uppercase
            text-sky-700
            dark:text-sky-400
            hover:underline
            underline-offset-4
          ">
          {{ t("Home.BrowseBlocks") }}
        </NuxtLink>
      </div>
    </div>
    <HomeRecentBlocks />
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const localePath = useLocalePath();
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
        content: `${config.public.baseUrl}/`,
      },
    ],
  };
});
useHead(meta);
</script>