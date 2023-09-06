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
    
    private readonly ContextMap _contextMap = ContextMap.Instance;    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
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

        _contextMap.Add(StepConstants.SENDER_KEY, senderKey);
        _contextMap.Add(StepConstants.RECEIVER_KEY, receiverKey);

    }

    [Given(@"the transfer amount is (.*)")]
    public void GivenTheTransferAmountIs(long amount) {
        WriteLine("the transfer amount is {0}", amount);
        
        _contextMap.Add(StepConstants.TRANSFER_AMOUNT, new BigInteger(amount));
    }

    [Given(@"the transfer gas price is (.*)")]
    public void GivenTheTransferGasPriceIs(long price) {
        WriteLine("the transfer gas price is {0}", price);

        _contextMap.Add(StepConstants.GAS_PRICE, (ulong)price);

    }

    [Given(@"the deploy is given a ttl of (.*)m")]
    public void GivenTheDeployIsGivenATtlOfM(int ttlMinutes) {
        WriteLine("the deploy is given a ttl of {0}", ttlMinutes);  

        _contextMap.Add(StepConstants.TTL, (ulong)ttlMinutes);
   
    }

    [When(@"the deploy is put on chain ""(.*)""")]
    public async Task WhenTheDeployIsPutOnChain(string chainName) {
        WriteLine("the deploy is put on chain {0}", chainName);  

        var senderKey = _contextMap.Get<KeyPair>(StepConstants.SENDER_KEY);
        
        // var deploy = DeployTemplates.StandardTransfer(
        //     senderKey.PublicKey,
        //     (PublicKey)_contextMap["receiverKey"],
        //     (BigInteger)_contextMap["transferAmount"],
        //     100_000_000,
        //     chainName,
        //     null,
        //     (ulong)_contextMap["gasPrice"],
        //     (ulong)_contextMap["ttl"]);
        //
        var deploy = DeployTemplates.StandardTransfer(
            senderKey.PublicKey,
            _contextMap.Get<PublicKey>(StepConstants.RECEIVER_KEY),
            (BigInteger)_contextMap.Get<BigInteger>(StepConstants.TRANSFER_AMOUNT),
            100_000_000,
            chainName);
    
        deploy.Sign(senderKey);
        
        var putResponse = await GetCasperService().PutDeploy(deploy);
        
        WriteLine(putResponse);
        
        _contextMap.Add(StepConstants.DEPLOY_RESULT, putResponse);
        
    }

    [Then(@"wait for a block added event with a timeout of (.*) seconds")]
    public void ThenWaitForABlockAddedEventWithATimeoutOfSeconds(int timeout) {
        WriteLine("wait for a block added event with a timeout of {0} seconds", timeout);
        
        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);
        
        var sseBlockAdded = new BlockAddedTask();
        sseBlockAdded.HasTransferHashWithin(deployResult.Parse().DeployHash, timeout);
        
    }
}