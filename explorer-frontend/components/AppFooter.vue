<template>
  <footer
    class="
      relative
      shadow
      p-6
      text-sm text-center text-gray-600
      bg-gray-50
      dark:text-gray-300 dark:bg-gray-800
    "
  >
    <div class="max-w-7xl w-full mx-auto">
      <div class="flex flex-col md:flex-row justify-between flex-wrap">
        <div class="flex flex-col md:flex-row items-start">
          <div class="font-semibold uppercase mx-2">
            {{ t("Footer.RelatedWebsites") }}:
          </div>
          <div class="mx-2 my-2 md:my-0">
            <a
              href="https://veil-project.com"
              target="blank"
              class="
                text-sky-700
                dark:text-sky-400
                hover:underline
                underline-offset-4
              "
              >Veil Project</a
            >
          </div>
          <div class="mx-2 my-2 md:my-0">
            <a
              href="https://veil-stats.com"
              target="blank"
              class="
                text-sky-700
                dark:text-sky-400
                hover:underline
                underline-offset-4
              "
              >Veil Stats</a
            >
          </div>
          <div class="mx-2 my-2 md:my-0">
            <a
              href="https://veil.tools"
              target="blank"
              class="
                text-sky-700
                dark:text-sky-400
                hover:underline
                underline-offset-4
              "
              >Veil Tools</a
            >
          </div>
        </div>
        <div class="text-left mx-2 mt-4 md:mt-0 md:flex-center">
          &copy; VEIL-PROJECT 2022
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";

const { t } = useI18n();
</script>