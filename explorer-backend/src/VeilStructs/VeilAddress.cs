using NBitcoin;
using ExplorerBackend.Models.System;

namespace ExplorerBackend.VeilStructs;

public class VeilAddress
{
    private static string ToHex(byte[] val) => BitConverter.ToString(val).Replace("-", "").ToLowerInvariant();
    public static ValidateAddress ValidateAddress(string input)
    {
        var conv = new Converters();
        var dest = conv.DecodeDestination(input);
        bool isValid = dest != null ? conv.IsValidDestination(dest) : false;

        var ret = new ValidateAddress();
        ret.isvalid = isValid;
        if (dest != null)
        {
            ret.address = Converters.EncodeDestination(dest);

            if (isValid)
            {
                var scriptPubKey = ToHex(dest.ScriptPubKey.ToBytes());
                ret.scriptPubKey = scriptPubKey;
                if (ret.scriptPubKey == "") ret.scriptPubKey = null;

                if (dest is KeyId)
                {
                    ret.isscript = false;
                    ret.iswitness = false;
                }

                if (dest is ScriptId)
                {
                    ret.isscript = true;
                    ret.iswitness = false;
                }

                if (dest is WitKeyId)
                {
                    var xdest = (WitKeyId)dest;

                    ret.isscript = false;
                    ret.iswitness = true;
                    ret.witness_version = 0;
                    ret.witness_program = ToHex(xdest.ToBytes());
                }

                if (dest is WitScript)
                {
                    var xdest = (WitScript)dest;

                    ret.isscript = false;
                    ret.iswitness = true;
                    ret.witness_version = 0;
                    ret.witness_program = ToHex(xdest.ToBytes());
                }

                if (dest is VeilWitnessUnknown)
                {
                    var xdest = (VeilWitnessUnknown)dest;

                    ret.iswitness = true;
                    ret.witness_version = xdest.version;
                    ret.witness_program = xdest.program != null ? ToHex(xdest.program) : null;
                }

                if (dest is ExtKey)
                    ret.isextkey = true;

                if (dest is VeilStealthAddress)
                {
                    var xdest = (VeilStealthAddress)dest;
                    ret.isstealthaddress = true;
                    ret.prefix_num_bits = xdest.prefix.number_bits;
                    ret.prefix_bitfield = ToHex(BitConverter.GetBytes(xdest.prefix.bitfield));
                }
            }
        }
        return ret;
    }
}