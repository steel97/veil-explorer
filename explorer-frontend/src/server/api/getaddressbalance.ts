import type { IncomingMessage, ServerResponse } from "http";
import { parseApiString, ApiEntry } from "~/composables/Configs";

export default async (req: IncomingMessage, res: ServerResponse) => {
    const defaultChain = process.env.CHAIN_DEFAULT;
    const apiPath = parseApiString(defaultChain, defaultChain, JSON.parse(process.env.CHAIN_APIS) as Array<ApiEntry>);

    let rq = req.url ?? "";
    if (rq.length > 0)
        rq = rq.substring(1);

    // verify alpha numeric rq
    rq = rq.replace(/[^a-zA-Z0-9]/gi, '');
    // end

    const result = await $fetch(`${apiPath}/getaddressbalance/${rq}`);

    res.statusCode = 200;
    res.setHeader("content-type", "application/json");

    res.end(JSON.stringify(result));
}
