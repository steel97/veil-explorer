import { defineNuxtConfig } from "nuxt3";

// https://v3.nuxtjs.org/docs/directory-structure/nuxt.config
export default defineNuxtConfig({
    publicRuntimeConfig: {
        CHAIN_APIS: JSON.parse(process.env.CHAIN_APIS),
        BASE_URL: process.env.BASE_URL,
        RECENT_BLOCKS_COUNT: process.env.RECENT_BLOCKS_COUNT,
        MAX_BLOCK_WEIGHT: process.env.MAX_BLOCK_WEIGHT,
        SYNC_NOTICE_CASE: process.env.SYNC_NOTICE_CASE
    },
    css: ["~/assets/css/tailwind.css"],
    build: {
        postcss: {
            postcssOptions: require("./postcss.config.js"),
        },
    },
    buildModules: ["@intlify/nuxt3"],
    intlify: {
        vueI18n: "vue-i18n.mjs"
    }
})
