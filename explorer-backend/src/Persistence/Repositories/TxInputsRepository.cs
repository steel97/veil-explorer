using Npgsql;
using explorer_backend.Models.Data;

namespace explorer_backend.Persistence.Repositories;

public class TxInputsRepository : BaseRepository, ITxInputsRepository
{
    public TxInputsRepository(IConfiguration config) : base(config) { }

    public async Task<Guid?> InsertTxInputAsync(TxInput txInputTemplate)
    {
        using var conn = Connection;
        await conn.OpenAsync();

        using (var cmd = new NpgsqlCommand(@"INSERT INTO tx_inputs (txid,input_index,""type"",num_inputs,ring_size,prev_txid,denomination,serial,pubcoin,vout,scriptsig_asm,scriptsig_hex,txinwitness,""sequence"") VALUES (" +
                                            $"{TransformHex(txInputTemplate.txid_hex)}, {txInputTemplate.input_index}, {(short)txInputTemplate.type}, {txInputTemplate.num_inputs}, {txInputTemplate.ring_size}, {TransformHex(txInputTemplate.prev_txid_hex)}, @denomination, {TransformHex(txInputTemplate.serial_hex)}, {TransformHex(txInputTemplate.pubcoin_hex)}, {txInputTemplate.vout}, @scriptsigasm, {TransformHex(txInputTemplate.scriptsig_hex)}, @txinwitness, {txInputTemplate.sequence}) RETURNING id;", conn))
        {
            var txwitnesses = new List<byte[]>();
            txInputTemplate.txinwitnes_hexes?.ForEach(witnessHex =>
            {
                txwitnesses.Add(Enumerable.Range(0, witnessHex.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(witnessHex.Substring(x, 2), 16))
                    .ToArray());
            });

            cmd.Parameters.Add(new NpgsqlParameter<string?>("denomination", txInputTemplate.denomination));
            cmd.Parameters.Add(new NpgsqlParameter<string?>("scriptsigasm", txInputTemplate.scriptsig_asm));
            cmd.Parameters.Add(new NpgsqlParameter<List<byte[]>>("txinwitness", txwitnesses));
            await cmd.PrepareAsync();
            return (Guid?)await cmd.ExecuteScalarAsync();
        }
    }
}