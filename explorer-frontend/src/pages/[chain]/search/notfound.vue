<template>
  <div>
    <h1 class="uppercase font-semibold py-4">
      {{ t("Search.Title") }}
    </h1>
    <div class="
        rounded
        p-8
        bg-gray-50
        dark:bg-gray-800
        flex
        justify-center
        items-center
        mb-4
      ">
      {{ t("Search.Description") }}
    </div>
    <NuxtLink :to="chainPath('/')" class="
        uppercase
        text-sky-700
        dark:text-sky-400
        hover:underline
        underline-offset-4
      ">
      {{ t("Search.ToHome") }}
    </NuxtLink>
  </div>
</template>

<script setup lang="ts">
const { t } = useI18n();
const { chainPath } = useRoutingHelper();
const config = useRuntimeConfig();

const meta = computed(() => {
  return {
    title: t("Search.Meta.Title"),
    meta: [
      {
        name: "description",
        content: t("Search.Meta.Description"),
      },
      {
        name: "og:title",
        content: t("Search.Meta.Title"),
      },
      {
        name: "og:url",
        content: `${config.public.baseUrl}/search/notfound`,
      },
    ],
  };
});

useHead(meta);
</script>