<template>
  <div>
    <h1 class="uppercase font-semibold py-4">
      {{ t("Errors." + errLocale + ".Title") }}
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
      {{ t("Errors." + errLocale + ".Description") }}
    </div>
    <NuxtLink to="javascript:void(0)" @click="clearErrorAndRedirect" class="
        uppercase
        text-sky-700
        dark:text-sky-400
        hover:underline
        underline-offset-4
      ">
      {{ t("Errors.ToHome") }}
    </NuxtLink>
  </div>
</template>

<script setup lang="ts">
import type { NuxtError } from "#app";

const props = defineProps({
  error: Object as () => NuxtError
});

const { chainPath } = useRoutingHelper();
const { t } = useI18n();
const config = useRuntimeConfig();
const route = useRoute();

const errLocale = computed(() => {
  let res = "Error404";
  switch (props.error?.statusCode) {
    case 500:
      res = "Error500";
      break;
  }
  return res;
});

const clearErrorAndRedirect = () => {
  clearError({ redirect: chainPath("/") });
};

const meta = computed(() => {
  return {
    title: t("Errors." + errLocale.value + ".Meta.Title"),
    meta: [
      {
        name: "description",
        content: t("Errors." + errLocale.value + ".Meta.Description"),
      },
      {
        name: "og:title",
        content: t("Errors." + errLocale.value + ".Meta.Title"),
      },
      {
        name: "og:url",
        content: `${config.public.baseUrl}/${route.name as string}`,
      },
    ],
  };
});

useHead(meta);
</script>