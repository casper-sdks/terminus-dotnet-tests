using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;


[Binding]
public class SpeculativeExecutionSteps {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    
    private static NetCasperClient GetCasperService() {
        return CasperSpeculativeClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that the ""(.*)"" account transfers (.*) to user-(.*) account with a gas payment amount of (.*) using the speculative_exec RPC API")]
    public async Task GivenThatTheAccountTransfersToUserAccountWithAGasPaymentAmountOfUsingTheSpeculativeExecRpcapi(string account, string transferAmount, int user, string paymentAmount) {
       WriteLine("that the {0} account transfers {1} to user-{2} account with a gas payment amount of {3} using the speculative_exec RPC API", account, transferAmount, user, paymentAmount);

       var faucetPrivateKey = KeyPair.FromPem(GetPrivateKey(account));
       var userPublicKey = KeyPair.FromPem(GetPrivateKey(user.ToString()));
       
       Assert.That(faucetPrivateKey, Is.Not.Null);
       Assert.That(faucetPrivateKey, Is.Not.Null);
       Assert.That(faucetPrivateKey.PublicKey, Is.Not.Null);

       Assert.That(userPublicKey, Is.Not.Null);
       Assert.That(userPublicKey, Is.Not.Null);
       Assert.That(userPublicKey.PublicKey, Is.Not.Null);
       
       
       var deploy = DeployTemplates.StandardTransfer(
           faucetPrivateKey.PublicKey,
           userPublicKey.PublicKey,
           BigInteger.Parse(transferAmount),
           BigInteger.Parse(paymentAmount),
           "casper-net-1",
           null,
           1,
           (ulong)TimeSpan.FromMinutes(30).TotalMilliseconds);

       deploy.Sign(faucetPrivateKey);
       
       var speculativeDeployData = await GetCasperService().SpeceulativeExecution(deploy);

       _contextMap.Add(StepConstants.DEPLOY_RESULT, speculativeDeployData);
        
    }

    [Then(@"the speculative_exec has an api_version of ""(.*)""")]
    public void ThenTheSpeculativeExecHasAnApiVersionOf(string apiVersion) {
        WriteLine("the speculative_exec has an api_version of {0}", apiVersion);

        var speculativeDeployData =
            _contextMap.Get<RpcResponse<SpeculativeExecutionResult>>(StepConstants.DEPLOY_RESULT).Parse();

        Assert.That(speculativeDeployData, Is.Not.Null);
        Assert.That(speculativeDeployData.ApiVersion, Is.EqualTo(apiVersion));

    }

    [Then(@"a valid speculative_exec_result will be returned with (.*) transforms")]
    public void ThenAValidSpeculativeExecResultWillBeReturnedWithTransforms(int transforms) {
        WriteLine("a valid speculative_exec_result will be returned with {0} transforms", transforms);
        
        var speculativeDeployData =
            _contextMap.Get<RpcResponse<SpeculativeExecutionResult>>(StepConstants.DEPLOY_RESULT).Parse();

        Assert.That(speculativeDeployData.ExecutionResult.Effect.Transforms.Count, Is.EqualTo(transforms));
        
    }

    [Then(@"the speculative_exec has a valid block_hash")]
    public void ThenTheSpeculativeExecHasAValidBlockHash() {
        WriteLine("the speculative_exec has a valid block_hash");
        
        var speculativeDeployData =
            _contextMap.Get<RpcResponse<SpeculativeExecutionResult>>(StepConstants.DEPLOY_RESULT).Parse();
        
        Assert.That(speculativeDeployData.BlockHash.Length, Is.EqualTo(64));
        
    }

    [Then(@"the execution_results contains a cost of (.*)")]
    public void ThenTheExecutionResultsContainsACostOf(string cost) {
        WriteLine("the execution_results contains a cost of {0}", cost);
        
        var speculativeDeployData =
            _contextMap.Get<RpcResponse<SpeculativeExecutionResult>>(StepConstants.DEPLOY_RESULT).Parse();
        
        Assert.That(speculativeDeployData.ExecutionResult.Cost, Is.EqualTo(BigInteger.Parse(cost)));
    }

    [Then(@"the speculative_exec has a valid execution_result")]
    public void ThenTheSpeculativeExecHasAValidExecutionResult() {
        WriteLine("the speculative_exec has a valid execution_result");
        
        var speculativeDeployData =
            _contextMap.Get<RpcResponse<SpeculativeExecutionResult>>(StepConstants.DEPLOY_RESULT).Parse();

        var key = speculativeDeployData.ExecutionResult.Transfers.First();
        
        var transform =
            speculativeDeployData.ExecutionResult.Effect.Transforms.Find(t => t.Key.ToHexString().ToUpper().Equals(key.ToHexString().ToUpper()));
        
        Assert.That(transform, Is.Not.Null);
        
        _contextMap.Add(StepConstants.TRANSFORM, transform);
        
    }

    [Then(@"the speculative_exec execution_result transform wth the transfer key contains the deploy_hash")]
    public void ThenTheSpeculativeExecExecutionResultTransformWthTheTransferKeyContainsTheDeployHash() {
        WriteLine("the speculative_exec execution_result transform wth the transfer key contains the deploy_hash");
    }

    [Then(@"the speculative_exec execution_result transform with the transfer key has the amount of (.*)")]
    public void ThenTheSpeculativeExecExecutionResultTransformWithTheTransferKeyHasTheAmountOf(string transferAmount) {
        WriteLine("the speculative_exec execution_result transform with the transfer key has the amount of {0}", transferAmount);
    }

    [Then(@"the speculative_exec execution_result transform with the transfer key has the ""(.*)"" field set to the ""(.*)"" account hash")]
    public void ThenTheSpeculativeExecExecutionResultTransformWithTheTransferKeyHasTheFieldSetToTheAccountHash(string from, string to) {
        WriteLine("the speculative_exec execution_result transform with the transfer key has the {0} field set to the {1} account hash", from, to);
    }

    [Then(@"the speculative_exec execution_result transform with the transfer key has the ""(.*)"" field set to the purse uref of the ""(.*)"" account")]
    public void ThenTheSpeculativeExecExecutionResultTransformWithTheTransferKeyHasTheFieldSetToThePurseUrefOfTheAccount(string to, string account) {
        WriteLine("the speculative_exec execution_result transform with the transfer key has the {0} field set to the purse uref of the {1} account", to, account);
    }

    [Then(@"the speculative_exec execution_result transform with the deploy key has the deploy_hash of the transfer's hash")]
    public void ThenTheSpeculativeExecExecutionResultTransformWithTheDeployKeyHasTheDeployHashOfTheTransfersHash() {
        WriteLine("the speculative_exec execution_result transform with the deploy key has the deploy_hash of the transfer's hash");
    }

    [Then(@"the speculative_exec execution_result transform with a deploy key has a gas field of (.*)")]
    public void ThenTheSpeculativeExecExecutionResultTransformWithADeployKeyHasAGasFieldOf(int gas) {
        WriteLine("the speculative_exec execution_result transform with a deploy key has a gas field of {0}", gas);
    }

    [Then(@"the speculative_exec execution_result transform with a deploy key has (.*) transfer with a valid transfer hash")]
    public void ThenTheSpeculativeExecExecutionResultTransformWithADeployKeyHasTransferWithAValidTransferHash(int transform) {
        WriteLine("the speculative_exec execution_result transform with a deploy key has {0} transfer with a valid transfer hash", transform);
    }

    [Then(@"the speculative_exec execution_result transform with a deploy key has as from field of the ""(.*)"" account hash")]
    public void ThenTheSpeculativeExecExecutionResultTransformWithADeployKeyHasAsFromFieldOfTheAccountHash(string account) {
        WriteLine("the speculative_exec execution_result transform with a deploy key has as from field of the {0} account hash", account);
    }

    [Then(@"the speculative_exec execution_result transform with a deploy key has as source field of the ""(.*)"" account purse uref")]
    public void ThenTheSpeculativeExecExecutionResultTransformWithADeployKeyHasAsSourceFieldOfTheAccountPurseUref(string account) {
        WriteLine("the speculative_exec execution_result transform with a deploy key has as source field of the {0} account purse uref", account);
    }

    [Then(@"the speculative_exec execution_result contains at least (.*) valid balance transforms")]
    public void ThenTheSpeculativeExecExecutionResultContainsAtLeastValidBalanceTransforms(int transforms) {
        WriteLine("the speculative_exec execution_result contains at least {0} valid balance transforms", transforms);
    }

    [Then(@"the speculative_exec execution_result (.*)st balance transform is an Identity transform")]
    public void ThenTheSpeculativeExecExecutionResultStBalanceTransformIsAnIdentityTransform(int balance) {
        WriteLine("the speculative_exec execution_result {0}st balance transform is an Identity transform", balance);
    }

    [Then(@"the speculative_exec execution_result last balance transform is an Identity transform is as WriteCLValue of type ""(.*)""")]
    public void ThenTheSpeculativeExecExecutionResultLastBalanceTransformIsAnIdentityTransformIsAsWriteClValueOfType(string type) {
        WriteLine("the speculative_exec execution_result last balance transform is an Identity transform is as WriteCLValue of type {0}", type);
    }

    [Then(@"the speculative_exec execution_result contains a valid (.*) transform with a value of (.*)")]
    public void ThenTheSpeculativeExecExecutionResultContainsAValidAddUIntTransformWithAValueOf(int transform, int value) {
        WriteLine("the speculative_exec execution_result contains a valid {0} transform with a value of {1}", transform, value);
    }


    private string GetPrivateKey(string user) {
        return (user.Equals("faucet"))
            ? AssetUtils.GetFaucetAsset(1, "secret_key.pem")
            : AssetUtils.GetUserKeyAsset(1, int.Parse(user), "secret_key.pem");
    }
    
}
