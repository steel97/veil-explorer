<template>
  <div
    class="
      bg-gray-200
      dark:bg-gray-700
      transition-colors
      ease-linear
      duration-200
    "
  >
    <div class="min-h-screen">
      <AppHeader />
      <div class="w-full px-2 text-gray-800 dark:text-gray-300">
        <SearchBox />
        <main class="max-w-7xl w-full mx-auto">
          <NuxtPage />
        </main>
      </div>
    </div>
    <AppFooter />
  </div>
</template>

<script setup lang="ts">
import "@/assets/css/tailwind.css";
import "@/assets/css/common.css";
import AppHeader from "@/components/AppHeader";
import SearchBox from "@/components/SearchBox";
import AppFooter from "@/components/AppFooter";
import { useConfigs } from "@/composables/Configs";
import { useChainInfo } from "@/composables/States";
import { useNetworkManager } from "@/composables/NetworkManager";
import { BlockchainInfo } from "@/models/API/BlockchainInfo";
import { useI18n } from "vue-i18n";

const { getApiPath } = useConfigs();
const { connect } = useNetworkManager();
const { t, availableLocales, fallbackLocale, locale } = useI18n();
const theme = useCookie("theme") || "";
const lang = useCookie("lang") || fallbackLocale.value;

let currentLang = lang.value;

if (availableLocales.indexOf(currentLang) == -1) {
  currentLang = fallbackLocale.value;
}

locale.value = currentLang;

const meta = computed(() => {
  return {
    meta: [
      {
        name: "viewport",
        content: "width=device-width, initial-scale=1, maximum-scale=5",
      },
      {
        "http-equiv": "X-UA-Compatible",
        content: "IE=edge",
      },
      {
        name: "robots",
        content: "index,follow",
      },
      {
        name: "og:image",
        content: "/assets/images/ogimage.png",
      },
      {
        name: "og:site_name",
        content: t("Meta.SiteName"),
      },
      {
        name: "og:type",
        content: "website",
      },
    ],
    link: [
      {
        rel: "icon",
        href: "/favicon.ico",
      },
      {
        rel: "preconnect",
        href: "https://fonts.gstatic.com",
      },
    ],
    htmlAttrs: {
      lang: locale.value,
    },
    bodyAttrs: {
      class: theme.value == "dark" ? "dark" : "", // prevent xss
    },
  };
});
useMeta(meta);

const chainInfoDataState = useChainInfo();

const chainInfo = await useAsyncData("blockchaininfo", () =>
  $fetch<BlockchainInfo>(`${getApiPath()}/blockchaininfo`)
);

chainInfoDataState.value = chainInfo.data.value;

onMounted(() => {
  connect();
});
</script>