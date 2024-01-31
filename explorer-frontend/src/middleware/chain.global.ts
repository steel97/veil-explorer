import Cookie from "js-cookie";

export default defineNuxtRouteMiddleware(to => {
    const chainState = useChainState();
    const config = useRuntimeConfig();

    const chain = useCookie("chain").value ?? "";
    let currentChain = chain;

    if (process.client && currentChain == "") {
        currentChain = config.public.chainDefault;
        const now = new Date();
        now.setDate(now.getDate() + config.public.cookieSaveDays);
        Cookie.set("chain", currentChain, {
            expires: now,
            sameSite: "lax",
        });
    }
    // validate currentChain
    const apiEndpoints = config.public.chainApis as Array<ApiEntry>;
    const epFound = apiEndpoints.filter(a => a.name == currentChain).length > 0;
    if (!epFound) {
        currentChain = config.public.chainDefault;
    }

    chainState.value = currentChain;
});