using Npgsql;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Persistence;

public class BaseRepository
{
    private readonly IConfiguration _config;
    private readonly IUtilityService _utilityService;

    public BaseRepository(IConfiguration config, IUtilityService utilityService)
    {
        _config = config;
        _utilityService = utilityService;
    }

    protected NpgsqlConnection Connection
    {
        get
        {
            return new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
        }
    }

    protected string? TransformDouble(double input) => TransformHex(BitConverter.ToString(BitConverter.GetBytes(input)).Replace("-", "").ToLowerInvariant());


    protected string? TransformHex(string? hexInput)
    {
        if (string.IsNullOrEmpty(hexInput)) return "NULL";
        if (!_utilityService.VerifyHex(hexInput)) throw new Exception("HEX value is wrong");

        return $@"'\x{hexInput}'::bytea";
    }

    protected async Task<double> ReadDoubleFromByteaAsync(NpgsqlDataReader reader, int ordinal, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (await reader.IsDBNullAsync(ordinal, cancellationToken)) return 0;

        var hash_array = await ReadByteaAsync(reader, ordinal, cancellationToken);
        if (hash_array == null) return 0;

        return BitConverter.ToDouble(hash_array, 0);
    }

    protected async Task<byte[]?> ReadByteaAsync(NpgsqlDataReader reader, int ordinal, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (await reader.IsDBNullAsync(ordinal, cancellationToken)) return null;
        var hash_size = reader.GetBytes(ordinal, 0, null, 0, 0);
        var hash_array = new byte[hash_size];
        reader.GetBytes(ordinal, 0, hash_array, 0, (int)hash_size);

        return hash_array;
    }

    protected async Task<string?> ReadHexFromByteaAsync(NpgsqlDataReader reader, int ordinal, CancellationToken cancellationToken = default(CancellationToken))
    {
        var hash_array = await ReadByteaAsync(reader, ordinal, cancellationToken);
        if (hash_array == null) return null;

        return BitConverter.ToString(hash_array).Replace("-", "").ToLowerInvariant();
    }
}