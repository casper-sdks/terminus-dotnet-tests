using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using NUnit.Framework;
using static System.Console;

namespace TerminusDotNet.Test.Utils; 

public class DeployUtils {

    private static readonly ContextMap _contextMap = ContextMap.Instance;
    private static readonly TestProperties TestProperties = new();

    public static async Task DeployArgs(List<NamedArg> args, NetCasperClient client) {

        args.Add(new NamedArg("amount", CLValue.U512(BigInteger.Parse("2500000000"))));
        args.Add(new NamedArg("target", CLValue.PublicKey(PublicKey.FromPem(AssetUtils.GetUserKeyAsset(1, 2, "public_key.pem")))));
        args.Add(new NamedArg("id", CLValue.Option(CLValue.U64((ulong)BigInteger.Parse("200")))));
    
        var session = new TransferDeploy(args);

        var senderKey = KeyPair.FromPem(AssetUtils.GetUserKeyAsset(1, 1, "secret_key.pem"));
       
        var header = new DeployHeader {
            Account = senderKey.PublicKey,
            Timestamp = DateUtils.ToEpochTime(DateTime.UtcNow),
            Ttl = 1800000,
            ChainName = TestProperties.ChainName,
            GasPrice = 1
        };
        var payment = new ModuleBytesDeployItem(100000000);
        
        var deploy = new Deploy(header, payment, session);
        
        deploy.Sign(senderKey);

        _contextMap.Add(StepConstants.PUT_DEPLOY, deploy);

        WriteLine(deploy.SerializeToJson());

        var deployResult = await client.PutDeploy(deploy);
        
        Assert.IsNotNull(deployResult);
        Assert.IsNotNull(deployResult.Parse().DeployHash);

        _contextMap.Add(StepConstants.DEPLOY_RESULT, deployResult);

        
        
    }
    
}
