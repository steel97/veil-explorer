<template>
  <div>
    <h1 class="font-semibold py-4">
      <span class="uppercase">{{ t("Address.ViewTitle") }}:</span>
      &nbsp;
      <button
        class="inline-flex items-center max-w-full"
        @click="copyToClipboard()"
        :title="t('Address.CopyToClipboard')"
      >
        <span
          class="
            text-sky-700
            dark:text-sky-400
            overflow-hidden
            text-ellipsis
            max-address-width
          "
          >{{ address }}</span
        >&nbsp;
        <DuplicateIcon
          class="
            inline-block
            h-5
            w-5
            mr-2
            text-sky-700
            dark:text-sky-400
            cursor-pointer
          "
        />
      </button>
    </h1>
    <div class="rounded p-4 bg-gray-50 dark:bg-gray-800"></div>
  </div>
</template>

<script setup lang="ts">
import { DuplicateIcon } from "@heroicons/vue/solid";
import Toastify from "toastify-js";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const route = useRoute();
const config = useRuntimeConfig();

const address: string = route.params.address;

const copyToClipboard = () => {
  try {
    navigator.clipboard.writeText(address).then(
      () => {
        Toastify({
          text: t("Address.CopySuccess"),
          duration: 2000,
          close: false,
          gravity: "top",
          position: "right",
          className: "",
          stopOnFocus: true,
        }).showToast();
      },
      () => {
        console.log("Can't copy text to clipboard (1)");
      }
    );
  } catch {
    console.log("Can't copy text to clipboard (2)");
  }
};

const meta = computed(() => {
  return {
    title: t("Address.Meta.Title", { address: address }),
    meta: [
      {
        name: "description",
        content: t("Address.Meta.Description", { address: address }),
      },
      {
        name: "og:title",
        content: t("Address.Meta.Title", { address: address }),
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

<style scoped>
.max-address-width {
  max-width: calc(100% - 30px);
}
</style>