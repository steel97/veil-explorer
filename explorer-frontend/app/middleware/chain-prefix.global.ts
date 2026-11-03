const ROUTE_PREFIXES = new Set([
  "address",
  "block",
  "block-height",
  "blocks",
  "search",
  "tx",
  "unconfirmed-tx",
  "tx-stats",
]);
const DEFAULT_CHAIN = "main";

export default defineNuxtRouteMiddleware((to) => {
  const segments = to.path.split("/").filter(Boolean);
  if (segments.length === 0) {
    return;
  }

  const first = segments[0];

  if (ROUTE_PREFIXES.has(first ?? "")) {
    return navigateTo(`/${DEFAULT_CHAIN}${to.path}`, { redirectCode: 301 });
  }
});