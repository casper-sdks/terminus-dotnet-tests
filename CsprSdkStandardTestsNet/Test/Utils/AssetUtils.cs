using System.IO;

namespace CsprSdkStandardTestsNet.Test.Utils;

public class AssetUtils
{
    public static string GetUserKeyAsset(int networkId, int userId, string keyFilename)
    {
        var path = $"/net-{networkId}/user-{userId}/{keyFilename}";

        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent;
        if (directoryInfo != null)
            path = directoryInfo.Parent!.FullName + "/assets" + path;

        return path;
    }

    // public static Url GetFaucetAsset(int networkId, string keyFilename){
    //     
    // }
}