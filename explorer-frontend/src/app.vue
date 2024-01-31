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
import type { ApiEntry } from "~/composables/Configs";
import Cookie from "js-cookie";

const config = useRuntimeConfig();

const { getApiPath } = useConfigs();
const { connect } = useNetworkManager();
const { t } = useI18n();
const localePath = useLocalePath();
const img = useImage();
const route = useRoute();
const backgroundInfoDataState = useBackgroundInfo();
const blockchaininfoDataState = useBlockchainInfo();
const chainState = useChainState();
const themeState = useThemeState();
const theme = useCookie("theme").value ?? "";
const chain = useCookie("chain").value ?? "";

let currentTheme = theme;
let currentChain = chain;

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

if (process.client && currentChain == "") {
  currentChain = config.public.chainDefault;
  const now = new Date();
  now.setDate(now.getDate() + config.public.cookieSaveDays);
  Cookie.set("chain", currentChain, {
    expires: now,
    sameSite: "lax",
  });
}

// validate currentChain
const apiEndpoints = config.public.chainApis as Array<ApiEntry>;
const epFound = apiEndpoints.filter(a => a.name == currentChain).length > 0;
if (!epFound) {
  currentChain = config.public.chainDefault;
}

themeState.value = currentTheme == "dark" ? "dark" : "";
chainState.value = currentChain;
if (route.path == localePath("/")) {
  await navigateTo(localePath(`/${chainState.value}`), { redirectCode: 301 });
}

const i18nHead = useLocaleHead({});
const meta = computed(() => {
  return {
    meta: [
      {
        name: "viewport",
        content: "width=device-width, initial-scale=1, maximum-scale=5",
      },
      {
        name: "theme-color",
        content: themeState.value == "dark" ? "#1F2937" : "#F9FAFB",
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
        content: img("/images/ogimage.png", { width: 251 }),
      },
      {
        name: "og:site_name",
        content: t("Meta.SiteName"),
      },
      {
        name: "og:type",
        content: "website",
      },
      ...(i18nHead.value.meta || []),
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
      ...(i18nHead.value.link || []),
    ],
    htmlAttrs: {
      lang: i18nHead.value.htmlAttrs!.lang
    }
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