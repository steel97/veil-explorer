<template>
  <div>
    <NuxtLayout>
      <div id="clfix" :class="themeState">
        <div
          class="
          bg-gray-200
          dark:bg-gray-700
          transition-colors
          ease-linear
          duration-200
        "
        >
          <div class="min-h-screen mb-10">
            <AppHeader />
            <div class="w-full px-2 text-gray-800 dark:text-gray-300">
              <SearchBox />
              <main class="max-w-7xl w-full mx-auto">
                <Error :error="error" />
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
import type { NuxtError } from "#app";
import {
  useThemeState,
} from "@/composables/States";
import Cookie from "js-cookie";
import "toastify-js/src/toastify.css";

const props = defineProps({
  error: Object as () => NuxtError,
});
const config = useRuntimeConfig();
const { t } = useI18n();
const img = useImage();
const themeState = useThemeState();
const theme = useCookie("theme").value ?? "";

let currentTheme = theme;

let usedMedia = false;
if (import.meta.client && currentTheme === "") {
  if (
    window.matchMedia
    && window.matchMedia("(prefers-color-scheme: dark)").matches
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

themeState.value = currentTheme === "dark" ? "dark" : "";

const i18nHead = useLocaleHead({});
const meta = computed(() => {
  return {
    meta: [
      {
        name: "viewport",
        content: "width=device-width, initial-scale=1, maximum-scale=5",
      },
      {
        "http-equiv": "X-UA-Compatible",
        "content": "IE=edge",
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
      lang: i18nHead.value.htmlAttrs!.lang,
    },
  };
});

useHead(meta);
onMounted(() => {
  if (usedMedia) {
    document.getElementById("clfix")?.classList.toggle("dark");
  }
});
</script>