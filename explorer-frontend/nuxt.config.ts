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
        BASE_URL: process.env.BASE_URL! as string,
        CHAIN_DEFAULT: process.env.CHAIN_DEFAULT! as string,
        CHAIN_APIS: JSON.parse(process.env.CHAIN_APIS! as string),
        RECENT_BLOCKS_COUNT: parseInt(process.env.RECENT_BLOCKS_COUNT!),
        BLOCKS_PER_PAGE: parseInt(process.env.BLOCKS_PER_PAGE!),
        TXS_PER_PAGE: parseInt(process.env.TXS_PER_PAGE!),
        MAX_BLOCK_WEIGHT: parseInt(process.env.MAX_BLOCK_WEIGHT!),
        SYNC_NOTICE_CASE: parseInt(process.env.SYNC_NOTICE_CASE!),
        COOKIE_SAVE_DAYS: parseInt(process.env.COOKIE_SAVE_DAYS!),
        locales: {
            "en": "English",
            "ru": "Русский"
        }
    },
    privateRuntimeConfig: {
        BASE_URL: process.env.BASE_URL! as string,
        CHAIN_APIS: process.env.CHAIN_APIS! as string
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
    typescript: {
        strict: true
    },
    buildModules: ["@intlify/nuxt3"],
    intlify: {
        vueI18n: "vue-i18n.mjs"
    }
})
