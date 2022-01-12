import type { IncomingMessage, ServerResponse } from "http";
import { parseApiString, ApiEntry } from "~/composables/Configs";

export default async (req: IncomingMessage, res: ServerResponse) => {
    const defaultChain = process.env.CHAIN_DEFAULT!;
    const apiPath = parseApiString(defaultChain, defaultChain, JSON.parse(process.env.CHAIN_APIS!) as Array<ApiEntry>);

    const result = await $fetch(`${apiPath}/getmoneysupply`);

    res.statusCode = 200;
    res.setHeader("content-type", "application/json");

    res.end(JSON.stringify(result));
}
