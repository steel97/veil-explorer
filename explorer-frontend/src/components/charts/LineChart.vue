<template>
  <div>
    <canvas ref="ctxRef" />
  </div>
</template>

<script setup lang="ts">
import type { GraphData } from "@/models/System/GraphData";
import { Chart } from "chart.js";
import type { ChartOptions } from "chart.js";

const ctxRef = ref<HTMLCanvasElement | null>(null);

const props = defineProps<{
  data: GraphData;
}>();

watch(props, (np) => {
  if (chart == null) return;
  chart.options = getChartOptions();
  chart.update();
});

let chart: Chart | null = null;
const getChartOptions = (): ChartOptions => {
  return {
    responsive: true,
    maintainAspectRatio: true,
    plugins: {
      title: {
        display: true,
        text: props.data.title,
      },
      legend: {
        display: false,
        labels: {
          usePointStyle: true,
        },
      },
    },
    scales: {
      x: {
        type: "linear",
        position: "bottom",
        display: true,

        title: {
          display: true,
          text: props.data.xaxisTitle,
        },
        ticks: {
          stepSize: props.data.xaxisStep,
        },
      },
      y: {
        display: true,
        title: {
          display: true,
          text: props.data.yaxisTitle,
        },
        ticks: {
          callback: function (value, index, values) {
            const numValue =
              typeof value == "string" ? parseFloat(value) : value;
            if (numValue > 1000000) {
              return (numValue / 1000000).toLocaleString() + "M";
            } else {
              return parseFloat(numValue.toFixed(3)).toString();
            }
          },
        },
      },
    },
  };
};

onBeforeUnmount(() => chart?.destroy());
onMounted(() => {
  const ctx = ctxRef.value?.getContext("2d");
  chart = new Chart(ctx!, {
    type: "scatter",
    data: {
      labels: props.data.labels,
      datasets: [
        {
          label: "",
          data: props.data.data,
          borderColor: "#36a2eb",
          borderWidth: 1,
          backgroundColor: "#84CBFA",
          fill: "origin",
          pointRadius: 1,
        },
      ],
    },
    options: getChartOptions(),
  });
});
</script>