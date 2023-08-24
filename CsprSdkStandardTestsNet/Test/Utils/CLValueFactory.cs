using System;
using System.Numerics;
using Casper.Network.SDK.Types;

namespace CsprSdkStandardTestsNet.Test.Utils;


public class CLValueFactory {

    public CLValue CreateValue(CLType clTypeData, string strValue) {

        switch (clTypeData) {
            case CLType.String:
                return CLValue.String(strValue);

            case CLType.Bool:
                return CLValue.Bool(bool.TrueString.Equals(strValue, StringComparison.OrdinalIgnoreCase));
            
            case CLType.U8:
               return CLValue.U8(byte.Parse(strValue));
            
            case CLType.U32:
               return CLValue.U32((uint)long.Parse(strValue));
            
            case CLType.U64:
                return CLValue.U64((ulong)BigInteger.Parse(strValue));
            
            case CLType.U256:
                return CLValue.U256(BigInteger.Parse(strValue));
            
            case CLType.I32:
               return CLValue.I32(int.Parse(strValue));
            
            case CLType.I64:
               return CLValue.I64(long.Parse(strValue));
            
            case CLType.ByteArray:
                
                var bytes = new byte[strValue.Length / 2];
                for (var i = 0; i < strValue.Length; i += 2)
                    bytes[i / 2] = Convert.ToByte(strValue.Substring(i, 2), 16);
                
                return CLValue.ByteArray(bytes);
            
            case CLType.Key:
               return CLValue.Key(GlobalStateKey.FromString(strValue));
            
            
            // case CLType.PublicKey:
            //     return new CLValuePublicKey(PublicKey.fromTaggedHexString(strValue));
            //
            // case CLType.URef:
            //     return new CLValueURef(new URef(Hex.decode(strValue), URefAccessRight.READ_ADD_WRITE));
            //
            // default:
            //     throw new ValueSerializationException("Not a supported type: " + clTypeData);
        }

        return null;

    }

}