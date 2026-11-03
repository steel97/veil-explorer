export default defineNuxtRouteMiddleware((to, from) => {
  if (import.meta.client)
    return;

  const nuxtApp = useNuxtApp();
  const chainState = useChainState();

  return navigateTo(nuxtApp.$localePath(`/${chainState.value}` as any), { redirectCode: 301 });
});