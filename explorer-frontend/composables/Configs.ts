export const useConfigs = () => {
    const config = useRuntimeConfig();

    const getApiPath = (): string => {
        // static for now
        return config.CHAIN_APIS.filter(api => api.name == "main")[0].path;
    };
    return { getApiPath };
}