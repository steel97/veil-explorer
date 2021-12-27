export interface ValidateAddressResult {
    isvalid: boolean,
    address: string | null,
    scriptPubKey: string | null,

    isscript: boolean | null,
    iswitness: boolean | null,

    witness_version: number | null,
    witness_program: string | null,

    isextkey: boolean | null,

    isstealthaddress: boolean | null,
    prefix_num_bits: number | null,
    prefix_bitfield: string | null
}

export interface AddressResponse {
    fetched: boolean,
    address: ValidateAddressResult | null,
    amountFetched: boolean,
    amount: number,
    version: number | null,
    hash: string | null
}