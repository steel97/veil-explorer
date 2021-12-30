export interface ApiEntry {
    name: string,
    path: string
}

export const parseApiString = (chainApis: Array<ApiEntry>) => chainApis.filter(api => api.name == "main")[0].path;

export const useConfigs = () => {
    const config = useRuntimeConfig();

    const getApiPath = (): string => parseApiString(config.CHAIN_APIS as Array<ApiEntry>);

    return { getApiPath };
}