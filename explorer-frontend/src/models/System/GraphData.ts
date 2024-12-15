import type { TxStatsDataPoint } from "@/models/API/TxStatsResponse";

export interface GraphData {
  data: Array<TxStatsDataPoint>;
  labels: Array<string>;
  title: string;
  xaxisTitle: string;
  xaxisStep: number;
  yaxisTitle: string;
};