using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Casper.Network.SDK.Types;

namespace CsprSdkStandardTestsNet.Test.Utils;

/**
 *  Converts complex and simple values to their required types 
 */
public class CLValueFactory {

    public static CLValue CreateValue(CLType clTypeData, string strValue) {

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

                return CLValue.Key(new HashKey("hash-" + strValue));

            case CLType.PublicKey:
                return CLValue.PublicKey(PublicKey.FromHexString(strValue));

            case CLType.URef:
                return CLValue.URef("uref-" + strValue + "-007");
            
            default:
                throw new ArgumentException($"Unexpected CL Type: {clTypeData}");

        }

    }

    public static CLValue CreateComplexValue(CLType clTypeData, List<CLType> innerTypes, List<string> innerStrValues) {
        
        var innerValues = new List<CLValue>();

        for (var i = 0; i < innerTypes.Count; i++) {
            innerValues.Add(CreateValue(innerTypes[i], innerStrValues[i]));
        }
    
        switch (clTypeData) {
            case CLType.List:
                return CLValue.List(innerValues.ToArray());
    
            case CLType.Map:
                return  CLValue.Map(BuildMap(innerValues));
            
            case CLType.Option:
                return CLValue.Option(innerValues[0]);
            
            case CLType.Tuple1:
                return CLValue.Tuple1(innerValues[0]);
            
            case CLType.Tuple2:
                return CLValue.Tuple2(innerValues[0], innerValues[1]);
            
            case CLType.Tuple3:
                return CLValue.Tuple3(innerValues[0], innerValues[1], innerValues[2]);
            
            default:
                throw new ArgumentException($"Unexpected CL Type: {clTypeData}");
        }
    }

    private static Dictionary<CLValue, CLValue> BuildMap(IEnumerable<CLValue> innerValues) {
        var i = 0;
        return innerValues.ToDictionary(_ => CLValue.String((i++).ToString()));
    }

}        
    

