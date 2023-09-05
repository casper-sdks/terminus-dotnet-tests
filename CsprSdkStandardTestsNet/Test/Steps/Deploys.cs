using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using CsprSdkStandardTestsNet.Test.Tasks;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

[Binding]
public class Deploys {
    
    private readonly Dictionary<string, object> _contextMap = new();
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    
    [Given(@"that user-(.*) initiates a transfer to user-(.*)")]
    public void GivenThatUserInitiatesATransferToUser(int senderId, int receiverId) {
        WriteLine("that user-{0} initiates a transfer to user-{1}", senderId, receiverId);
        
        var senderPem = AssetUtils.GetUserKeyAsset(1, 1, "secret_key.pem");
        var senderKey = KeyPair.FromPem(senderPem);

        Assert.IsNotNull(senderKey);
        var receiverPem = AssetUtils.GetUserKeyAsset(1, 2, "public_key.pem");

        var receiverKey = PublicKey.FromPem(receiverPem);

        Assert.IsNotNull(receiverKey);

        _contextMap["senderKey"] = senderKey;
        _contextMap["receiverKey"] = receiverKey;
        
    }

    [Given(@"the transfer amount is (.*)")]
    public void GivenTheTransferAmountIs(long amount) {
        WriteLine("the transfer amount is {0}", amount);
        
        _contextMap["transferAmount"] = new BigInteger(amount);
    }

    [Given(@"the transfer gas price is (.*)")]
    public void GivenTheTransferGasPriceIs(long price) {
        WriteLine("the transfer gas price is {0}", price);

        _contextMap["gasPrice"] = (ulong)price;

    }

    [Given(@"the deploy is given a ttl of (.*)m")]
    public void GivenTheDeployIsGivenATtlOfM(int ttlMinutes) {
        WriteLine("the deploy is given a ttl of {0}", ttlMinutes);  
        
        _contextMap["ttl"] = (ulong)ttlMinutes;
     
    }

    [When(@"the deploy is put on chain ""(.*)""")]
    public async Task WhenTheDeployIsPutOnChain(string chainName) {
        WriteLine("the deploy is put on chain {0}", chainName);  

        var senderKey = (KeyPair)_contextMap["senderKey"];
        
        var deploy = DeployTemplates.StandardTransfer(
            senderKey.PublicKey,
            (PublicKey)_contextMap["receiverKey"],
            (BigInteger)_contextMap["transferAmount"],
            100_000_000,
            chainName,
            null,
            (ulong)_contextMap["gasPrice"],
            (ulong)_contextMap["ttl"]);
        
        _contextMap["putDeploy"] = deploy;
        
        WriteLine(deploy);
   
        deploy.Sign(senderKey);
        
        var putResponse = await GetCasperService().PutDeploy(deploy);
        
        WriteLine(putResponse);
        
        _contextMap["deployResult"] = putResponse;
        
    }

    [Then(@"wait for a block added event with a timeout of (.*) seconds")]
    public void ThenWaitForABlockAddedEventWithATimeoutOfSeconds(int timeout) {
        WriteLine("wait for a block added event with a timeout of {0} seconds", timeout);
        
        var deployResult = (RpcResponse<PutDeployResult>)_contextMap["deployResult"];
        
        var sseBlockAdded = new BlockAddedTask();
        sseBlockAdded.HasTransferHashWithin(deployResult.Parse().DeployHash, timeout);
        
    }
}