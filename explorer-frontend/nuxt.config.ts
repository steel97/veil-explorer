// https://nuxt.com/docs/api/configuration/nuxt-config

const locales = [
  {
    name: "English",
    code: "en",
    language: "en-US",
    file: "en.ts",
  },
  {
    name: "Русский",
    code: "ru",
    language: "ru-RU",
    file: "ru.ts",
  },
];

const i18nSwrRules = locales.reduce(
  (acc, locale) => {
    acc[`/${locale.code}`] = { swr: 60 };
    return acc;
  },
  {} as Record<string, any>,
);

// @nuxtjs/tailwindcss registers "tailwindcss/nesting", but Nuxt 4 cannot resolve
// that extensionless subpath (tailwindcss 3 has no exports map). Point it at the
// actual file so PostCSS can load it instead of warning NUXT_B7027.
function fixTailwindNesting(_options: any, nuxt: any) {
  nuxt.hook("modules:done", () => {
    const plugins = nuxt.options.postcss?.plugins;
    if (plugins?.["tailwindcss/nesting"]) {
      plugins["tailwindcss/nesting/index.js"] = plugins["tailwindcss/nesting"];
      delete plugins["tailwindcss/nesting"];
    }
  });
}

// @ts-ignore
export default defineNuxtConfig({
  devtools: { enabled: true },
  compatibilityDate: "2026-03-15",
  future: {
    compatibilityVersion: 5,
  },
  runtimeConfig: {
    public: {
      site: {
        url: "http://localhost:3000",
      },
      baseUrl: process.env.NUXT_PUBLIC_BASE_URL! as string,
      chainDefault: process.env.NUXT_PUBLIC_CHAIN_DEFAULT! as string,
      chainApis: {} as any, // JSON.parse(process.env.NUXT_PUBLIC_CHAIN_APIS! as string),
      recentBlocksCount: Number.parseInt(process.env.NUXT_PUBLIC_RECENT_BLOCKS_COUNT!),
      blocksPerPage: Number.parseInt(process.env.NUXT_PUBLIC_BLOCKS_PER_PAGE!),
      txsPerPage: Number.parseInt(process.env.NUXT_PUBLIC_TXS_PER_PAGE!),
      maxBlockWeight: Number.parseInt(process.env.NUXT_PUBLIC_MAX_BLOCK_WEIGHT!),
      syncNoticeCase: Number.parseInt(process.env.NUXT_PUBLIC_SYNC_NOTICE_CASE!),
      cookieSaveDays: Number.parseInt(process.env.NUXT_PUBLIC_COOKIE_SAVE_DAYS!),
    },
  },
  app: {
    pageTransition: { name: "page", mode: "out-in" },
    head: {
      templateParams: {
        separator: "-",
      },
      titleTemplate: "%siteName %separator %s",
    },
  },
  modules: [
    "@nuxt/image",
    "@nuxtjs/i18n",
    "@nuxtjs/tailwindcss",
    fixTailwindNesting,
    "@nuxtjs/seo",
    "@nuxt/eslint",
  ],
  routeRules: {
    "/tx/**": {
      redirect: {
        to: "/main/tx/**",
        statusCode: 301,
      },
    },
    "/block/**": {
      redirect: {
        to: "/main/block/**",
        statusCode: 301,
      },
    },
    "/blocks/**": {
      redirect: {
        to: "/main/blocks/**",
        statusCode: 301,
      },
    },
    "/block-height/**": {
      redirect: {
        to: "/main/block-height/**",
        statusCode: 301,
      },
    },
    "/tx-stats/**": {
      redirect: {
        to: "/main/tx-stats/**",
        statusCode: 301,
      },
    },
    "/unconfirmed-tx/**": {
      redirect: {
        to: "/main/unconfirmed-tx/**",
        statusCode: 301,
      },
    },
  },
  i18n: {
    locales: locales,
    defaultLocale: "en",
    langDir: "localization",
    strategy: "prefix_except_default",
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: "lang",
      redirectOn: "root",
      alwaysRedirect: false,
    },
    baseUrl: process.env.NUXT_I18N_BASE_URL || "https://explorer.veil-project.com",
  },
  routeRules: {
    // "/": { prerender: true },
    ...i18nSwrRules,
  },
  image: {
    format: ["webp", "png"],
    provider: "ipx",
    quality: 100,
    ipx: {
      modifiers: {
        format: "webp",
        quality: 100,
      },
    },
  },
  seo: {
    redirectToCanonicalSiteUrl: process.env.NODE_ENV !== "development",
  },
  site: {
    url: process.env.NUXT_PUBLIC_SITE_URL || "https://explorer.veil-project.com",
  },
  nitro: {
    imports: {
      imports: [
        { name: "useRuntimeConfig", from: "nitropack/runtime" },
      ],
    },
  },
  schemaOrg: {
    identity: {
      type: "Organization",
      name: "Veil Project",
      url: "https://veil-project.com",
      logo: `${process.env.NUXT_PUBLIC_SITE_URL || "https://veilproject.org"}/icon-192x192-light.png`,
    },
  },
  css: ["~/assets/css/tailwind.css", "~/assets/css/common.css"],
  /* alias: {
        "chart.js": "chart.js/dist/chart.js",
    }, */
  build: {
    transpile: [
      "@heroicons/vue",
      "chart.js",
    ],
  },
  typescript: {
    typeCheck: true,
    strict: true,
  },
});