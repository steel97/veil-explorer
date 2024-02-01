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
import "@/assets/css/tailwind.css";
import "@/assets/css/common.css";
import "toastify-js/src/toastify.css";
import {
    useThemeState
} from "@/composables/States";
import Cookie from "js-cookie";
import type { NuxtError } from "#app";

const config = useRuntimeConfig();
const props = defineProps({
    error: Object as () => NuxtError
});
const { t } = useI18n();
const img = useImage();
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
onMounted(() => {
    if (usedMedia) {
        document.getElementById("clfix")?.classList.toggle("dark");
    }
});
</script>