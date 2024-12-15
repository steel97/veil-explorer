<template>
  <div class="flex justify-center">
    <form
      method="post" class="
        block
        relative
        max-w-7xl
        w-full
        my-6
        text-gray-600
        dark:text-gray-300
      " @submit.prevent="search()"
    >
      <span class="absolute inset-y-0 left-0 flex items-center pl-3">
        <button type="submit" aria-label="Search" class="p-1 focus:outline-none focus:shadow-outline" @click="search()">
          <svg
            fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            viewBox="0 0 24 24" class="w-6 h-6" :aria-label="t('Core.Search.Action')"
          >
            <path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
          </svg>
        </button>
      </span>
      <input
        id="searchbox" v-model="searchmodel" type="text" name="q" class="
          py-4
          text-sm
          rounded
          pl-12
          pr-4
          focus:outline-none
          w-full
          text-gray-700
          bg-gray-300
          dark:text-gray-300 dark:bg-gray-600
        " :placeholder="t('Core.Search.Placeholcer')" autocomplete="off" :aria-label="t('Core.Search.Label')" @change="search()"
      />
    </form>
  </div>
</template>

<script setup lang="ts">
import type { SearchResponse } from "@/models/API/SearchResponse";
import { EntityType } from "@/models/API/SearchResponse";

const { t } = useI18n();
const { getApiPath } = useConfigs();
const searchmodel = ref<string>("");
const router = useRouter();
const { chainPath } = useRoutingHelper();

const search = async () => {
  await searchInner();
};

const searchInner = async (retry = 0) => {
  const query = searchmodel.value;
  if (query === "" || query === null)
    return;
  const resData = await $fetch<SearchResponse | null>(
    `${getApiPath()}/search`,
    {
      method: "POST",
      body: {
        query,
      },
    },
  );

  const data = resData;
  if (data === null) {
    router.replace(chainPath("/search/notfound"));
    return;
  }

  if (data.query !== query && retry < 2) {
    await searchInner(retry + 1);
    return;
  }

  switch (data.type) {
    case EntityType.UNKNOWN:
      router.replace(chainPath("/search/notfound"));
      break;
    case EntityType.BLOCK_HEIGHT:
      router.replace(chainPath(`/block-height/${data.query}`));
      break;
    case EntityType.BLOCK_HASH:
      router.replace(chainPath(`/block/${data.query}`));
      break;
    case EntityType.TRANSACTION_HASH:
      router.replace(chainPath(`/tx/${data.query}`));
      break;
    case EntityType.ADDRESS:
      router.replace(chainPath(`/address/${data.query}`));
      break;
  }
};
</script>