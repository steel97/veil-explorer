export enum EntityType {
    UNKNOWN = 0,
    BLOCK_HEIGHT = 1,
    BLOCK_HASH = 2,
    TRANSACTION_HASH = 3,
    ADDRESS = 4
}

export interface SearchResponse {
    found: boolean;
    query: string | null;
    type: EntityType;
}