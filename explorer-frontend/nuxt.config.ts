// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
    devtools: { enabled: true },
    runtimeConfig: {
        public: {
            i18n: {
                baseUrl: process.env.NUXT_PUBLIC_I18N_BASE_URL! as string,
            },
            baseUrl: process.env.NUXT_PUBLIC_BASE_URL! as string,
            chainDefault: process.env.NUXT_PUBLIC_CHAIN_DEFAULT! as string,
            chainApis: JSON.parse(process.env.NUXT_PUBLIC_CHAIN_APIS! as string),
            recentBlocksCount: parseInt(process.env.NUXT_PUBLIC_RECENT_BLOCKS_COUNT!),
            blocksPerPage: parseInt(process.env.NUXT_PUBLIC_BLOCKS_PER_PAGE!),
            txsPerPage: parseInt(process.env.NUXT_PUBLIC_TXS_PER_PAGE!),
            maxBlockWeight: parseInt(process.env.NUXT_PUBLIC_MAX_BLOCK_WEIGHT!),
            syncNoticeCase: parseInt(process.env.NUXT_PUBLIC_SYNC_NOTICE_CASE!),
            cookieSaveDays: parseInt(process.env.NUXT_PUBLIC_COOKIE_SAVE_DAYS!)
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
    routeRules: {
        "/tx/**": {
            redirect: {
                to: "/main/tx/**",
                statusCode: 301
            }
        },
        "/block/**": {
            redirect: {
                to: "/main/block/**",
                statusCode: 301
            }
        },
        "/blocks/**": {
            redirect: {
                to: "/main/blocks/**",
                statusCode: 301
            }
        },
        "/block-height/**": {
            redirect: {
                to: "/main/block-height/**",
                statusCode: 301
            }
        },
        "/tx-stats/**": {
            redirect: {
                to: "/main/tx-stats/**",
                statusCode: 301
            }
        },
        "/unconfirmed-tx/**": {
            redirect: {
                to: "/main/unconfirmed-tx/**",
                statusCode: 301
            }
        }
    },
    i18n: {
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
        strategy: "prefix_except_default",
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
    css: ["~/assets/css/tailwind.css", "~/assets/css/common.css"],
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
