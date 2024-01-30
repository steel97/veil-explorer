<template>
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
</template>
  
<script setup lang="ts">
import "@/assets/css/tailwind.css";
import "@/assets/css/common.css";
import "toastify-js/src/toastify.css";
import {
    useThemeState
} from "@/composables/States";
import { useNetworkManager } from "@/composables/NetworkManager";
import { useLocalization } from "@/composables/Localization";
import type { LocaleObject } from "vue-i18n-routing";
import Cookie from "js-cookie";

const config = useRuntimeConfig();
const props = defineProps({
    error: Object
});

const { connect } = useNetworkManager();
const { getClientLocale } = useLocalization();
const { t, locales, fallbackLocale, locale } = useI18n();
const themeState = useThemeState();
const theme = useCookie("theme").value ?? "";
const lang = useCookie("lang").value ?? getClientLocale();

let currentLang = lang.toString();
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

if (locales.value.filter(locale => (locale as LocaleObject).code == currentLang).length == 0) {
    currentLang = fallbackLocale.value.toString();
}

locale.value = currentLang;
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
onMounted(() => {
    if (usedMedia) {
        document.getElementById("clfix")?.classList.toggle("dark");
    }

    connect();
});
</script>