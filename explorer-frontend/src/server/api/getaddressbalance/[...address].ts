import { setResponseHeader } from "h3";
import { parseApiString } from "~/composables/Configs";
import type { ApiEntry } from "~/composables/Configs";

export default defineEventHandler(async (event) => {
    const runtimeConfig = useRuntimeConfig();
    const defaultChain = runtimeConfig.public.chainDefault;
    const apiPath = parseApiString(defaultChain, defaultChain, runtimeConfig.public.chainApis as Array<ApiEntry>);

    let rq = event.context.params?.address ?? "";
    rq = rq.replace(/[^a-zA-Z0-9]/gi, '');

    const result = await $fetch(`${apiPath}/getaddressbalance/${rq}`);

    setResponseStatus(event, 200);
    setResponseHeader(event, "content-type", "application/json");

    return result;
});