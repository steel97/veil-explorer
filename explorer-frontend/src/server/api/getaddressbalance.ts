import { setResponseHeader } from "h3";
import { parseApiString } from "~/composables/Configs";
import type { ApiEntry } from "~/composables/Configs";

export default defineEventHandler(async (event) => {
    const defaultChain = process.env.CHAIN_DEFAULT!;
    const apiPath = parseApiString(defaultChain, defaultChain, JSON.parse(process.env.CHAIN_APIS!) as Array<ApiEntry>);

    let rq = event.node.req.url ?? "";
    if (rq.length > 0)
        rq = rq.substring(1);

    // verify alpha numeric rq
    rq = rq.replace(/[^a-zA-Z0-9]/gi, '');
    // end

    const result = await $fetch(`${apiPath}/getaddressbalance/${rq}`);

    setResponseStatus(event, 200);
    setResponseHeader(event, "content-type", "application/json");

    return result;
});