export const useRoutingHelper = () => {
  const chain = useChainState();
  const localePath = useLocalePath();

  const chainPath = (input: string) => {
    return localePath(`/${chain.value}${input}`);
  };

  return { chainPath };
};