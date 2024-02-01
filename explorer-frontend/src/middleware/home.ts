export default defineNuxtRouteMiddleware((to, from) => {
    if (process.client) return;

    const nuxtApp = useNuxtApp();
    const chainState = useChainState();

    return navigateTo(nuxtApp.$localePath("/" + chainState.value), { redirectCode: 301 });
})