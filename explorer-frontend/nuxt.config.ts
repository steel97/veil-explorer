import { defineNuxtConfig } from "nuxt3";

// https://v3.nuxtjs.org/docs/directory-structure/nuxt.config
export default defineNuxtConfig({
    publicRuntimeConfig: {
        CHAIN_APIS: JSON.parse(process.env.CHAIN_APIS),
        BASE_URL: process.env.BASE_URL,
        RECENT_BLOCKS_COUNT: process.env.RECENT_BLOCKS_COUNT,
        BLOCKS_PER_PAGE: process.env.BLOCKS_PER_PAGE,
        MAX_BLOCK_WEIGHT: process.env.MAX_BLOCK_WEIGHT,
        SYNC_NOTICE_CASE: process.env.SYNC_NOTICE_CASE,
        COOKIE_SAVE_DAYS: process.env.COOKIE_SAVE_DAYS,
        locales: {
            "en": "English",
            "ru": "Русский"
        }
    },
    css: ["~/assets/css/tailwind.css"],
    build: {
        transpile: ["@heroicons/vue"],
        postcss: {
            postcssOptions: require("./postcss.config.js"),
        },
    },
    buildModules: ["@intlify/nuxt3"],
    intlify: {
        vueI18n: "vue-i18n.mjs"
    }
})
