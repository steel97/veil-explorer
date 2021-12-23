using Npgsql;
using explorer_backend.Models.Data;

namespace explorer_backend.Persistence.Repositories;

public class TxOutputsRepository : BaseRepository, ITxOutputsRepository
{
    public TxOutputsRepository(IConfiguration config) : base(config) { }

    public async Task<Guid?> InsertTxOutputAsync(TxOutput txOutputTemplate)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        using (var cmd = new NpgsqlCommand(@"INSERT INTO tx_outputs (txid,output_index,""type"",valuesat,vout_n,scriptpub_asm,scriptpub_hex,scriptpub_type,reqsigs,addresses) VALUES (" +
                                            $"{TransformHex(txOutputTemplate.txid_hex)}, {txOutputTemplate.output_index}, {(short)txOutputTemplate.type}, {txOutputTemplate.valuesat}, {txOutputTemplate.vout_n}, @scriptpubasm, {TransformHex(txOutputTemplate.scriptpub_hex)}, {(short)txOutputTemplate.scriptpub_type}, {txOutputTemplate.reqsigs}, @addresses) RETURNING id;", conn))
        {
            cmd.Parameters.Add(new NpgsqlParameter<string?>("scriptpubasm", txOutputTemplate.scriptpub_asm));
            cmd.Parameters.Add(new NpgsqlParameter<List<string>>("addresses", txOutputTemplate.addresses ?? new List<string>()));
            await cmd.PrepareAsync();
            return (Guid?)await cmd.ExecuteScalarAsync();
        }
    }
}