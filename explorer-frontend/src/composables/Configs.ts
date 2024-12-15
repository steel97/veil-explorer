export interface ApiEntry {
  name: string;
  path: string;
}

export const parseApiString = (lookForChain: string, defaultChain: string, chainApis: Array<ApiEntry>) => {
  let cchainRes = chainApis.filter(api => api.name === lookForChain);
  if (cchainRes.length > 0)
    return cchainRes[0].path;

  cchainRes = chainApis.filter(api => api.name === defaultChain);
  if (cchainRes.length > 0)
    return cchainRes[0].path;

  // log error?
  return "";
};

export const useConfigs = () => {
  const config = useRuntimeConfig();
  const chain = useChainState();

  const defaultChain = config.public.chainDefault as string;
  const selectedChain = chain.value;

  const getApiPath = (): string => parseApiString(selectedChain, defaultChain, config.public.chainApis as Array<ApiEntry>);

  return { getApiPath };
};