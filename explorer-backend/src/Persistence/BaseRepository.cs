using Npgsql;

namespace explorer_backend.Persistence;

public class BaseRepository
{
    private readonly IConfiguration _config;
    public BaseRepository(IConfiguration config)
    {
        _config = config;
    }

    protected NpgsqlConnection Connection
    {
        get
        {
            return new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
        }
    }

    protected string? TransformDouble(double input) => TransformHex(BitConverter.ToString(BitConverter.GetBytes(input)).Replace("-", "").ToLowerInvariant());


    protected string? TransformHex(string? hexInput) => hexInput != null ? $@"'\x{hexInput}'::bytea" : "NULL";

    protected async Task<double> ReadDoubleFromBytea(NpgsqlDataReader reader, int ordinal)
    {
        if (await reader.IsDBNullAsync(ordinal)) return 0;

        var hash_array = await ReadBytea(reader, ordinal);
        if (hash_array == null) return 0;

        return BitConverter.ToDouble(hash_array, 0);
    }

    protected async Task<byte[]?> ReadBytea(NpgsqlDataReader reader, int ordinal)
    {
        if (await reader.IsDBNullAsync(ordinal)) return null;
        var hash_size = reader.GetBytes(ordinal, 0, null, 0, 0);
        var hash_array = new byte[hash_size];
        reader.GetBytes(ordinal, 0, hash_array, 0, (int)hash_size);

        return hash_array;
    }

    protected async Task<string?> ReadHexFromBytea(NpgsqlDataReader reader, int ordinal)
    {
        var hash_array = await ReadBytea(reader, ordinal);
        if (hash_array == null) return null;

        return BitConverter.ToString(hash_array).Replace("-", "").ToLowerInvariant();
    }
}