<template>
  <div>
    <NuxtLayout>
      <div :class="themeState" id="clfix">
        <div class="
        bg-gray-200
        dark:bg-gray-700
        transition-colors
        ease-linear
        duration-200
      ">
          <div class="min-h-screen mb-10">
            <AppHeader />
            <div class="w-full px-2 text-gray-800 dark:text-gray-300">
              <SyncNotice v-if="isSynchronizing" />
              <SearchBox />
              <main class="max-w-7xl w-full mx-auto">
                <NuxtPage />
              </main>
            </div>
          </div>
          <AppFooter />
        </div>
      </div>
    </NuxtLayout>
  </div>
</template>

<script setup lang="ts">
import "@/assets/css/tailwind.css";
import "@/assets/css/common.css";
import "toastify-js/src/toastify.css";
import { useConfigs } from "@/composables/Configs";
import {
  useThemeState,
  useBackgroundInfo,
  useBlockchainInfo,
} from "@/composables/States";
import { useNetworkManager } from "@/composables/NetworkManager";
import type { BlockchainInfo } from "@/models/API/BlockchainInfo";
import Cookie from "js-cookie";

const config = useRuntimeConfig();

const { getApiPath } = useConfigs();
const { connect } = useNetworkManager();
const { t, locale } = useI18n();
const backgroundInfoDataState = useBackgroundInfo();
const blockchaininfoDataState = useBlockchainInfo();
const themeState = useThemeState();
const theme = useCookie("theme").value ?? "";

let currentTheme = theme;

let usedMedia = false;
if (process.client && currentTheme == "") {
  if (
    window.matchMedia &&
    window.matchMedia("(prefers-color-scheme: dark)").matches
  ) {
    usedMedia = true;
    currentTheme = "dark";

    const now = new Date();
    now.setDate(now.getDate() + config.public.cookieSaveDays);
    Cookie.set("theme", "dark", {
      expires: now,
      sameSite: "lax",
    });
  }
}

themeState.value = currentTheme == "dark" ? "dark" : "";

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
        content: "/images/ogimage.png",
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
  };
});

useHead(meta);

const chainInfo = await useFetch<BlockchainInfo>(
  `${getApiPath()}/blockchaininfo`
);

if (chainInfo.data.value != null) {
  backgroundInfoDataState.value = {
    currentSyncedBlock: chainInfo.data.value.currentSyncedBlock,
    algoStats: chainInfo.data.value.algoStats,
  };
  blockchaininfoDataState.value = chainInfo.data.value.chainInfo;
}

const isSynchronizing = computed(() => {
  if (
    backgroundInfoDataState.value == null ||
    blockchaininfoDataState.value == null
  )
    return false;

  const shouldSync =
    blockchaininfoDataState.value.blocks -
    (config.public.syncNoticeCase as any as number) >
    backgroundInfoDataState.value.currentSyncedBlock;

  return shouldSync;
});

onMounted(() => {
  if (usedMedia) {
    document.getElementById("clfix")?.classList.toggle("dark");
  }

  connect();
});
</script>