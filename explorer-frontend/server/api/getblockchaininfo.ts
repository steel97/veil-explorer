import type { IncomingMessage, ServerResponse } from "http";
import { parseApiString, ApiEntry } from "~~/composables/Configs";

export default async (req: IncomingMessage, res: ServerResponse) => {
    const apiPath = parseApiString(JSON.parse(process.env.CHAIN_APIS) as Array<ApiEntry>);
    const result = await $fetch(`${apiPath}/getblockchaininfo`);

    res.statusCode = 200;
    res.setHeader("content-type", "application/json");

    res.end(JSON.stringify(result));
}
