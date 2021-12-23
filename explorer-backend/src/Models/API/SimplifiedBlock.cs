using explorer_backend.Models.Data;

namespace explorer_backend.Models.API;

public class SimplifiedBlock
{
    public int Height { get; set; }
    public int Size { get; set; }
    public int Weight { get; set; }
    public BlockType ProofType { get; set; }
    public long Time { get; set; }
    public long MedianTime { get; set; }

    public int TxCount { get; set; }
}