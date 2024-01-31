<template>
    <div>
        <NuxtLayout>
            <NuxtPage v-if="shouldRenderPage" />
        </NuxtLayout>
    </div>
</template>
<script setup lang="ts">
import type { ApiEntry } from "~/composables/Configs";

const route = useRoute();
const shouldRenderPage = ref(true);
const config = useRuntimeConfig();
const localePath = useLocalePath();

const chain = route.params.chain;
const apiEndpoints = config.public.chainApis as Array<ApiEntry>;
const epFound = apiEndpoints.filter(a => a.name == chain).length > 0;
if (!epFound) {
    // probably triggered
    await navigateTo(localePath(`/${config.public.chainDefault}`));
}
</script>