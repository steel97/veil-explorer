<template>
  <header class="
      relative
      shadow
      mb-3
      bg-gray-50
      text-gray-800
      dark:bg-gray-800 dark:text-gray-300
    ">
    <div class="max-w-full mx-auto pr-3">
      <div class="flex justify-between items-center py-2">
        <NuxtLink :to="localePath('/')" @click="clearError" class="flex items-center">
          <div class="px-3 pr-2">
            <div class="flex items-center">
              <img class="h-6 w-auto pr-2 my-3" src="/images/logo.png" :alt="t('Header.Title')" />
              <span class="font-semibold">{{ t("Header.Title") }}</span>
            </div>
          </div>
          <div class="border-l">
            <span class="pl-2 font-base uppercase text-sm">{{
              data?.chain ?? t("Core.NoData")
            }}</span>
          </div>
        </NuxtLink>
        <div class="-mr-2 -my-2 lg:hidden">
          <button type="button" class="
              p-2
              inline-flex
              items-center
              justify-center
              text-gray-400
              hover:text-gray-500
              focus:outline-none
            " @click="toggleMenu(false)">
            <span class="sr-only">{{ t("Header.OpenMenu") }}</span>
            <Bars3Icon class="h-6 w-6" />
          </button>
        </div>
        <nav class="hidden lg:flex uppercase px-10 grow">
          <ul class="flex space-x-4 justify-center grow text-sm">
            <li v-for="(link, index) in getLinks()" :key="'link' + index">
              <NuxtLink :to="link.link" @click="clearError" class="
                  font-medium
                  hover:underline
                  underline-offset-14
                  decoration-2
                  hover:text-sky-700 hover:dark:text-sky-400
                " :class="computeClasses(link)">
                {{ link.locale }}
              </NuxtLink>
            </li>
          </ul>
        </nav>
        <div class="hidden lg:flex items-center justify-end">
          <div class="flex justify-center">
            <div class="form-check form-switch flex items-center">
              <label class="
                  form-check-label
                  inline-block
                  text-sm
                  flex
                  items-center
                  mr-4
                " for="switchTheme">
                <MoonIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
                <span class="uppercase">{{ t("Header.DarkMode") }}</span>
              </label>
              <input class="
                  form-check-input
                  appearance-none
                  w-9
                  rounded-full
                  float-left
                  h-5
                  align-top
                  bg-no-repeat bg-contain
                  focus:outline-none
                  cursor-pointer
                  shadow-sm
                  bg-gray-300
                  dark:bg-gray-600
                " type="checkbox" role="switch" id="switchTheme" v-model="themeSwitch" />
            </div>
          </div>
          <div class="mx-2 popover-wrapper text-sm">
            <div class="flex justify-between items-center lang-width px-2">
              <span class="mr-2"><img class="locale-icon" :alt="getCurrentLocale().name"
                  :src="'/images/locales/' + getCurrentLocale().code + '.png'" /></span>
              <span class="grow cursor-default uppercase">{{
                getCurrentLocale().name
              }}</span>
            </div>
            <div class="
                lang-width
                popover-content
                bg-gray-50
                text-gray-800
                dark:bg-gray-800 dark:text-gray-300
                pt-2
              ">
              <NuxtLink :to="switchLocalePath(val.code)" @click="clearError" class="
                  flex
                  items-center
                  justify-between
                  lang-entry
                  px-2
                  cursor-pointer
                " v-for="val in getLocales()" :key="'lang-' + val.code">
                <span class="mr-2"><img class="locale-icon" :alt="val.name"
                    :src="'/images/locales/' + val.code + '.png'" /></span>
                <span class="grow uppercase">{{ val.name }}</span>
              </NuxtLink>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div v-show="initialized" :style="{ visibility: menuOpened ? 'visible' : 'hidden' }" class="lg:hidden">
      <div class="transition-[height] ease-out duration-200 menu-collapse">
        <nav class="flex flex-col lg:hidden uppercase px-4 grow">
          <ul class="flex flex-col space-y-4 grow text-sm mb-4">
            <li v-for="(link, index) in getLinks()" :key="'link' + index">
              <NuxtLink @click="toggleMenu(true)" :to="link.link" class="
                  font-medium
                  hover:text-sky-700 hover:underline
                  underline-offset-8
                  decoration-2
                " :class="computeClasses(link)">
                {{ link.locale }}
              </NuxtLink>
            </li>
          </ul>
        </nav>
        <div class="flex px-4">
          <div class="form-check form-switch flex items-center">
            <label class="
                form-check-label
                inline-block
                text-sm
                flex
                items-center
                mr-4
              " for="switchThemeMobile">
              <MoonIcon class="h-5 w-5 mr-2 text-sky-700 dark:text-sky-400" />
              <span class="uppercase">{{ t("Header.DarkMode") }}</span>
            </label>
            <input class="
                form-check-input
                appearance-none
                w-9
                rounded-full
                float-left
                h-5
                align-top
                bg-no-repeat bg-contain
                focus:outline-none
                cursor-pointer
                shadow-sm
                bg-gray-300
                dark:bg-gray-600
              " type="checkbox" role="switch" id="switchThemeMobile" v-model="themeSwitch" />
          </div>
        </div>
        <div>
          <div class="mx-2 mt-4 text-sm">
            <div class="flex justify-between items-center lang-width px-2" @click="openLocaleMenu()">
              <span class="mr-2"><img class="locale-icon"
                  :src="'/images/locales/' + getCurrentLocale().code + '.png'" /></span>
              <span class="grow cursor-default uppercase">{{
                getCurrentLocale().name
              }}</span>
            </div>
            <div v-if="menuLocaleOpened" class="
                lang-width
                bg-gray-50
                text-gray-800
                dark:bg-gray-800 dark:text-gray-300
                pt-2
              ">
              <NuxtLink :to="switchLocalePath(valm.code)" @click="clearError" class="
                  flex
                  items-center
                  justify-between
                  lang-entry
                  px-2
                  cursor-pointer
                " v-for="valm in getLocales()" :key="'lang-m-' + valm.code">
                <span class="mr-2"><img class="locale-icon" :src="'/images/locales/' + valm.code + '.png'" /></span>
                <span class="grow uppercase">{{ valm.name }}</span>
              </NuxtLink>
            </div>
          </div>
        </div>
      </div>
    </div>
  </header>
</template>
<script setup lang="ts">
import MoonIcon from "@heroicons/vue/24/solid/MoonIcon";
//import SunIcon from "@heroicons/vue/24/solid/SunIcon";
import Bars3Icon from "@heroicons/vue/24/outline/Bars3Icon";
import { useThemeState, useBlockchainInfo } from "@/composables/States";
import type { LocaleObject } from "vue-i18n-routing";
import Cookie from "js-cookie";

const { t, locales, locale, fallbackLocale } =
  useI18n();
const localePath = useLocalePath();
const switchLocalePath = useSwitchLocalePath();
const data = useBlockchainInfo();
const themeState = useThemeState();
const route = useRoute();
const config = useRuntimeConfig();

const initialized = ref(false);
const menuOpened = ref(false);
const menuHeight = ref("0px");
const menuLocaleOpened = ref(false);

const themeSwitch = ref(themeState.value == "dark" ? true : false);

export interface ILocale {
  code: string;
  name: string;
}

export interface ILink {
  locale: string;
  name: string;
  link: string;
}

watch(themeSwitch, (nval) => {
  const now = new Date();
  now.setDate(now.getDate() + config.public.cookieSaveDays);

  themeState.value = themeSwitch.value ? "dark" : "light";

  Cookie.set("theme", themeState.value, {
    expires: now,
    sameSite: "lax",
  });
});

onMounted(() => (initialized.value = true));

const getLinks = () => {
  const ret: Array<ILink> = [
    {
      locale: t("Header.Links.Home"),
      name: "index",
      link: localePath("/"),
    },
    {
      locale: t("Header.Links.Blocks"),
      name: "block",
      link: localePath("/blocks"),
    },
    {
      locale: t("Header.Links.TxStats"),
      name: "tx-stats",
      link: localePath("/tx-stats"),
    },
    {
      locale: t("Header.Links.UTxs"),
      name: "unconfirmed-tx",
      link: localePath("/unconfirmed-tx"),
    },
  ];
  return ret;
};

const getCurrentLocale = () => {
  return {
    code: locale.value,
    name: (locales.value.filter(a => (a as LocaleObject).code == locale.value)[0] as LocaleObject).name
  };
};

const getLocales = () => {
  const loc: Array<ILocale> = [];
  locales.value.forEach(locale => {
    const lang = locale as LocaleObject;
    if (lang.code == getCurrentLocale().code) return;
    const link: ILocale = {
      code: lang.code,
      name: lang.name ?? ""
    };
    loc.push(link);
  });
  return loc;
};

const computeClasses = (link: ILink) => {
  if (route.name === undefined) {
    return ["text-gray-600", "dark:text-gray-300"];
  }

  if (
    (link.name == route.name && route.name == "") ||
    (route.name as any as string).startsWith(link.name)
  )
    return ["underline", "text-sky-700", "dark:text-sky-400"];
  return ["text-gray-600", "dark:text-gray-300"];
};

const recalculateMenuSize = () => {
  let size = 220 + (menuLocaleOpened.value ? getLocales().length * 30 : 0);
  menuHeight.value = menuOpened.value ? `${size}px` : "0px";
};

const toggleMenu = (shouldClearError = false) => {
  menuOpened.value = !menuOpened.value;

  if (shouldClearError) {
    clearError();
  }

  recalculateMenuSize();
};

const openLocaleMenu = () => {
  menuLocaleOpened.value = !menuLocaleOpened.value;

  recalculateMenuSize();
};
</script>

<style lang="postcss" scoped>
.menu-collapse {
  height: v-bind(menuHeight);
}

.lang-width {
  min-width: 140px;
}

.lang-entry {
  padding-top: 0.5rem;
  padding-bottom: 0.5rem;
}

.popover-wrapper {
  position: relative;
  display: inline-block;
}

.popover-content {
  opacity: 0;
  visibility: hidden;
  position: absolute;
  right: 0px;
  top: 40px;
  transform: translate(0, 10px);
}

.popover-content:before {
  position: absolute;
  z-index: -1;
  content: "";
  top: -8px;
  transition-duration: 0.15s;
  transition-property: transform;
}

.popover-wrapper:hover .popover-content {
  z-index: 10;
  opacity: 1;
  visibility: visible;
  transform: translate(0, -20px);
  transition: all 0.25s cubic-bezier(0.75, -0.02, 0.2, 0.97);
}

.locale-icon {
  width: 24px;
}
</style>