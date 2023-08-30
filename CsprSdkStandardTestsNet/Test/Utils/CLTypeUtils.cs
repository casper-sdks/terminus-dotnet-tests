using System;
using System.Numerics;
using Casper.Network.SDK.Types;
using Org.BouncyCastle.Utilities.Encoders;

namespace CsprSdkStandardTestsNet.Test.Utils; 

public class CLTypeUtils {
    public static object ConvertToClTypeValue(CLType typeName, string value) {
        try {
            switch (typeName) {
                case CLType.String:
                    return value;

                case CLType.U8:
                    return byte.Parse(value);

                case CLType.U32:
                case CLType.I64:
                    return long.Parse(value);

                case CLType.U64:
                case CLType.U256:
                    return BigInteger.Parse(value);

                case CLType.Bool:
                    return Boolean.Parse(value);

                case CLType.I32:
                    return int.Parse(value);

                case CLType.ByteArray:
                    return Hex.Decode(value);

                // case CLType.Key:
                //     return CLValue.Key(value);
                //
                // case PUBLIC_KEY:
                //     return PublicKey.fromTaggedHexString(value);
                //
                // case UREF:
                //     return new URef(Hex.decode(value), URefAccessRight.READ_ADD_WRITE);

                default:
                    throw new ArgumentException("Not implemented conversion for type " + typeName);
            }
        } catch (Exception e) {
            throw new ArgumentException(e.Message);
        }
    }

}