import { defineNuxtConfig } from "nuxt3";

// temporal fix for intlify
import { IntlifyModuleOptions } from "@intlify/nuxt3";

declare module "@nuxt/schema" {
    export interface NuxtConfig {
        intlify?: IntlifyModuleOptions;
    }
}
//

// https://v3.nuxtjs.org/docs/directory-structure/nuxt.config
export default defineNuxtConfig({
    publicRuntimeConfig: {
        CHAIN_DEFAULT: process.env.CHAIN_DEFAULT,
        CHAIN_APIS: JSON.parse(process.env.CHAIN_APIS),
        BASE_URL: process.env.BASE_URL,
        RECENT_BLOCKS_COUNT: process.env.RECENT_BLOCKS_COUNT,
        BLOCKS_PER_PAGE: process.env.BLOCKS_PER_PAGE,
        TXS_PER_PAGE: process.env.TXS_PER_PAGE,
        MAX_BLOCK_WEIGHT: process.env.MAX_BLOCK_WEIGHT,
        SYNC_NOTICE_CASE: process.env.SYNC_NOTICE_CASE,
        COOKIE_SAVE_DAYS: process.env.COOKIE_SAVE_DAYS,
        locales: {
            "en": "English",
            "ru": "Русский"
        }
    },
    privateRuntimeConfig: {
        BASE_URL: process.env.BASE_URL,
        CHAIN_APIS: process.env.CHAIN_APIS
    },
    srcDir: "src/",
    css: ["~/assets/css/tailwind.css"],
    alias: {
        "chart.js": "chart.js/dist/chart.esm.js",
    },
    build: {
        transpile: [
            "@heroicons/vue", "chart.js"
        ],
        postcss: {
            postcssOptions: require("./postcss.config.js"),
        },
    },
    buildModules: ["@intlify/nuxt3"],
    intlify: {
        vueI18n: "vue-i18n.mjs"
    }
})
