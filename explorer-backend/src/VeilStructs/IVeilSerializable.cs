namespace ExplorerBackend.VeilStructs;

public interface IVeilSerializable
{
    void Deserialize(VeilSerialization serializationContext, int mode);
}