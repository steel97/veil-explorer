<template>
  <div>
    <h1 class="uppercase font-semibold">
      {{ t("TxStats.Title") }}
    </h1>
    <div class="rounded bg-gray-50 dark:bg-gray-800 mb-4 mt-4">
      <div class="block md:grid grid-cols-2" v-if="pageReady">
        <ChartsLineChart :data="getData('day')" class="my-2 md:mx-2" />
        <ChartsLineChart :data="getData('week')" class="my-2 md:mx-2" />
        <ChartsLineChart :data="getData('month')" class="my-2 md:mx-2" />
        <ChartsLineChart :data="getData('overall')" class="my-2 md:mx-2" />

        <ChartsLineChart :data="getData('day', true)" class="my-2 md:mx-2" />
        <ChartsLineChart :data="getData('week', true)" class="my-2 md:mx-2" />
        <ChartsLineChart :data="getData('month', true)" class="my-2 md:mx-2" />
        <ChartsLineChart :data="getData('overall', true)" class="my-2 md:mx-2" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { TxStatsResponse, TxStatsEntry } from "@/models/API/TxStatsResponse";
import { GraphData } from "@/models/System/GraphData";

const { t } = useI18n();
const { getApiPath } = useConfigs();
const config = useRuntimeConfig();
const pageReady = ref(false);

const fetchStats = async () =>
  await useFetch<string, TxStatsResponse>(`${getApiPath()}/txstats`);

const stats = ref((await fetchStats()).data);

const getData = (key: string, rate = false) => {
  const emptyData: GraphData = {
    data: [],
    labels: [],
    title: "unknown",
    xaxisTitle: "unknown",
    xaxisStep: 5,
    yaxisTitle: "unknown",
  };
  if (!pageReady.value) return emptyData;

  const cdataval = stats.value.txStats[key] as TxStatsEntry;

  const blocaleKey = `TxStats.Charts.${key}.${rate ? "Rates" : "Counts"}.`;
  const ret: GraphData = {
    data: rate ? cdataval.txRates : cdataval.txCounts,
    labels: cdataval.labels,
    title: t(blocaleKey + "Title"),
    xaxisTitle: t(blocaleKey + "XAxis"),
    xaxisStep: 5,
    yaxisTitle: t(blocaleKey + "YAxis"),
  };
  return ret;
};

onMounted(() => {
  if (!process.client) return; // should never happend?
  pageReady.value = true;
});

const meta = computed(() => {
  return {
    title: t("TxStats.Meta.Title"),
    meta: [
      {
        name: "description",
        content: t("TxStats.Meta.Description"),
      },
      {
        name: "og:title",
        content: t("TxStats.Meta.Title"),
      },
      {
        name: "og:url",
        content: `${config.BASE_URL}/tx-stats`,
      },
    ],
  };
});
useMeta(meta);
</script>