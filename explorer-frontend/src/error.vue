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
import type { LocaleObject } from "vue-i18n-routing";

const config = useRuntimeConfig();
const props = defineProps({
    error: Object as () => NuxtError
});
const localePath = useLocalePath();
const { t, locale, locales } = useI18n();
const themeState = useThemeState();
const theme = useCookie("theme").value ?? "";
const chain = useCookie("chain").value ?? config.public.chainDefault;

if (props.error != null) {
    console.log(props.error.message);
    let turl = props.error.message.substring((props.error.message.indexOf(":") ?? 0) + 1).trim(); // stupid workaround..

    if (!turl.startsWith("/")) {
        turl = "/" + turl;
    }

    for (const locale of locales.value) {
        const localeObj = locale as LocaleObject;
        const lookup = "/" + localeObj.code;
        if (turl.startsWith(lookup)) {
            turl = turl.substring(lookup.length);
            break;
        }
    }

    if (!turl.startsWith("/")) {
        turl = "/" + turl;
    }

    if (turl.startsWith("/tx/") ||
        turl.startsWith("/block/") ||
        turl.startsWith("/block-height/") ||
        turl.startsWith("/blocks") ||
        turl.startsWith("/tx-stats") ||
        turl.startsWith("/unconfirmed-tx")) {
        clearError();
        navigateTo(localePath("/" + chain + turl), { redirectCode: 301 });
    }
}

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
onMounted(() => {
    if (usedMedia) {
        document.getElementById("clfix")?.classList.toggle("dark");
    }
});
</script>