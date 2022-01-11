export interface TxStatsDataPoint {
    x: number;
    y: number;
}

export interface TxStatsEntry {
    labels: Array<string>;
    txCounts: Array<TxStatsDataPoint>;
    txRates: Array<TxStatsDataPoint>;
}

export interface TxStatsResponse {
    txStats: any;
}