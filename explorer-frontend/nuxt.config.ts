// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
    devtools: { enabled: true },
    runtimeConfig: {
        public: {
            baseUrl: process.env.NUXT_BASE_URL! as string,
            chainDefault: process.env.NUXT_CHAIN_DEFAULT! as string,
            chainApis: JSON.parse(process.env.NUXT_CHAIN_APIS! as string),
            recentBlocksCount: parseInt(process.env.NUXT_RECENT_BLOCKS_COUNT!),
            blocksPerPage: parseInt(process.env.NUXT_BLOCKS_PER_PAGE!),
            txsPerPage: parseInt(process.env.NUXT_TXS_PER_PAGE!),
            maxBlockWeight: parseInt(process.env.NUXT_MAX_BLOCK_WEIGHT!),
            syncNoticeCase: parseInt(process.env.NUXT_SYNC_NOTICE_CASE!),
            cookieSaveDays: parseInt(process.env.NUXT_COOKIE_SAVE_DAYS!)
        }
    },
    app: {
        pageTransition: { name: "page", mode: "out-in" }
    },
    modules: [
        "@nuxt/image",
        "@nuxtjs/i18n",
        "@nuxtjs/tailwindcss"
    ],
    i18n: {
        baseUrl: process.env.NUXT_BASE_URL!,
        locales: [
            {
                name: "English",
                code: "en",
                iso: "en-US",
                file: "en.ts"
            },
            {
                name: "Русский",
                code: "ru",
                iso: "ru-RU",
                file: "ru.ts"
            }
        ],
        defaultLocale: "en",
        lazy: true,
        langDir: "localization",
        strategy: "prefix_and_default",
        detectBrowserLanguage: {
            useCookie: true,
            cookieKey: "lang",
            redirectOn: "all",
            alwaysRedirect: true
        }
    },
    image: {
        format: ["webp", "png"],
        provider: "ipx",
        quality: 100,
        ipx: {
            modifiers: {
                format: "webp",
                quality: 100
            },
        },
    },
    srcDir: "src/",
    css: ["~/assets/css/tailwind.css"],
    postcss: {
        plugins: {
            tailwindcss: {},
            autoprefixer: {},
        },
    },
    /*alias: {
        "chart.js": "chart.js/dist/chart.js",
    },*/
    build: {
        transpile: [
            "@heroicons/vue",
            "chart.js"
        ]
    },
    typescript: {
        typeCheck: true,
        strict: true
    }
})
