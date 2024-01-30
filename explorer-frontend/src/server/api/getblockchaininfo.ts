import { setResponseHeader } from "h3";
import { parseApiString } from "~/composables/Configs";
import type { ApiEntry } from "~/composables/Configs";

export default defineEventHandler(async (event) => {
    const defaultChain = process.env.CHAIN_DEFAULT!;
    const apiPath = parseApiString(defaultChain, defaultChain, JSON.parse(process.env.CHAIN_APIS!) as Array<ApiEntry>);

    const result = await $fetch(`${apiPath}/getblockchaininfo`);

    setResponseStatus(event, 200);
    setResponseHeader(event, "content-type", "application/json");

    return result;
});