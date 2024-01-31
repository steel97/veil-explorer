<template>
    <NuxtLayout>
        <NuxtPage />
    </NuxtLayout>
</template>
<script setup lang="ts">
import type { ApiEntry } from "~/composables/Configs";

definePageMeta({
    validate: async (route) => {
        // Check if the id is made up of digits
        const configMW = useRuntimeConfig();
        const chain = route.params.chain;
        const apiEndpoints = configMW.public.chainApis as Array<ApiEntry>;
        const epFound = apiEndpoints.filter(a => a.name == chain).length > 0;
        return epFound;
    }
});
</script>