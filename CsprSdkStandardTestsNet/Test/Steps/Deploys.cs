using System;
using System.Numerics;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.SSE;
using Casper.Network.SDK.Types;
using Casper.Network.SDK.Utils;
using CsprSdkStandardTestsNet.Test.Tasks;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

/**
 * Steps to test deploys
 */

[Binding]
public class Deploys {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;    
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
        
        _contextMap.Add(StepConstants.DEPLOY_TIMESTAMP, DateUtils.ToEpochTime(DateTime.UtcNow));
        
        var deploy = DeployTemplates.StandardTransfer(
            senderKey.PublicKey,
            _contextMap.Get<PublicKey>(StepConstants.RECEIVER_KEY),
            _contextMap.Get<BigInteger>(StepConstants.TRANSFER_AMOUNT),
            100_000_000,
            chainName,
            null,
            _contextMap.Get<ulong>(StepConstants.GAS_PRICE),
            (ulong)TimeSpan.FromMinutes(_contextMap.Get<ulong>(StepConstants.TTL)).TotalMilliseconds);
        
        deploy.Sign(senderKey);
        
        var putResponse = await GetCasperService().PutDeploy(deploy);
        
        _contextMap.Add(StepConstants.DEPLOY_RESULT, putResponse);
        
    }

    [Then(@"wait for a block added event with a timeout of (.*) seconds")]
    public void ThenWaitForABlockAddedEventWithATimeoutOfSeconds(int timeout) {
        WriteLine("wait for a block added event with a timeout of {0} seconds", timeout);
        
        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);
        
        var sseBlockAdded = new BlockAddedTask();
        sseBlockAdded.HasTransferHashWithin(deployResult.Parse().DeployHash, timeout);
        
    }

    [Given(@"that a Transfer has been successfully deployed")]
    public void GivenThatATransferHasBeenSuccessfullyDeployed() {
        WriteLine("that a Transfer has been successfully deployed");
        
        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);
        Assert.That(deployResult, Is.Not.Null);
        
    }

    [When(@"a deploy is requested via the info_get_deploy RCP method")]
    public async Task WhenADeployIsRequestedViaTheInfoGetDeployRcpMethod() {
        WriteLine("a deploy is requested via the info_get_deploy RCP method");
        
        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);
        var deploy = await GetCasperService().GetDeploy(deployResult.Parse().DeployHash);
        
        Assert.That(deploy, Is.Not.Null);
        
        _contextMap.Add(StepConstants.INFO_GET_DEPLOY, deploy);
        Assert.That(deploy.Parse().ExecutionResults.Count, Is.GreaterThan(0));

    }

    [Then(@"the deploy data has an API version of ""(.*)""")]
    public void ThenTheDeployDataHasAnApiVersionOf(string apiVersion) {
        WriteLine("the deploy data has an API version of {0}", apiVersion);
        
        Assert.That(GetDeployData().Parse().ApiVersion, Is.EqualTo(apiVersion));
        
    }

    [Then(@"the deploy execution result has ""(.*)"" block hash")]
    public void ThenTheDeployExecutionResultHasBlockHash(string lastBlockAdded) {
        WriteLine("the deploy execution result has {0} block hash", lastBlockAdded);

        Assert.That(GetDeployData().Parse().ExecutionResults[0].BlockHash,
            Is.EqualTo(_contextMap.Get<BlockAdded>(lastBlockAdded).BlockHash));

    }

    [Then(@"the deploy execution has a cost of (.*) motes")]
    public void ThenTheDeployExecutionHasACostOfMotes(int cost) {
        WriteLine("the deploy execution has a cost of {0} motes", cost);
        
        Assert.That(GetDeployData().Parse().ExecutionResults[0].Cost, Is.EqualTo(new BigInteger(cost)));
    }

    [Then(@"the deploy has a payment amount of (.*)")]
    public void ThenTheDeployHasAPaymentAmountOf(int amount) {
        WriteLine("the deploy has a payment amount of {0}", amount);
        
        var amountArg = GetDeployData().Parse().Deploy.Payment.RuntimeArgs.Find(n => n.Name.Equals(StepConstants.AMOUNT));
        
        Assert.That(amountArg, Is.Not.Null);
        Assert.That(amountArg.Value.TypeInfo.Type, Is.EqualTo(CLType.U512));
        Assert.That(int.Parse(amountArg.Value.Parsed.ToString()!), Is.EqualTo(amount));

    }

    [Then(@"the deploy has a valid hash")]
    public void ThenTheDeployHasAValidHash() {
        WriteLine("the deploy has a valid hash");

        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);
        
        Assert.That(GetDeployData().Parse().Deploy.Hash, Is.EqualTo(deployResult.Parse().DeployHash));
    }

    [Then(@"the deploy has a valid timestamp")]
    public void ThenTheDeployHasAValidTimestamp() {
        WriteLine("the deploy has a valid timestamp");
        
        //There's no way to pass the timestamp to a deploy
        //The best we can do is check that it's greater or equal than a recorded time
        //And do some ISO conversion to validate it's a date
        var timestampExpected = _contextMap.Get<ulong>(StepConstants.DEPLOY_TIMESTAMP);
        var timestampActual = GetDeployData().Parse().Deploy.Header.Timestamp;

        Assert.That(timestampExpected, Is.LessThanOrEqualTo(timestampActual));

        var timestampActualDate = DateTime.Parse(DateUtils.ToISOString(timestampActual)).Date;
        Assert.That(timestampActualDate, Is.Not.Null);

    }

    [Then(@"the deploy has a valid body hash")]
    public void ThenTheDeployHasAValidBodyHash() {
        WriteLine("the deploy has a valid body hash");
        
        // Obtain the hash of the put deploy and compare to one obtained with info_get_deploy
        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);
        Assert.That(GetDeployData().Parse().Deploy.Hash, Is.EqualTo(deployResult.Parse().DeployHash));
        
    }

    [Then(@"the deploy has a session type of ""(.*)""")]
    public void ThenTheDeployHasASessionTypeOf(string sessionType) {
        WriteLine("the deploy has a session type of {0}", sessionType);

        var actualSessionType = GetDeployData().Parse().Deploy.Session.JsonPropertyName().ToLower();
        var expectedSessionType = sessionType.Replace(" ", "").ToLower();
        Assert.That(actualSessionType, Is.EqualTo(expectedSessionType));

    }

    [Then(@"the deploy is approved by user-(.*)")]
    public void ThenTheDeployIsApprovedByUser(int user) {
        WriteLine("the deploy is approved by user-{0}", user);

        var approvals = GetDeployData().Parse().Deploy.Approvals;
        Assert.That(approvals.Count, Is.EqualTo(1));
        
        var userKeyAsset = AssetUtils.GetUserKeyAsset(1, user, StepConstants.PUBLIC_KEY_PEM);

        var approvalKey = PublicKey.FromPem(userKeyAsset);

        Assert.That(approvals[0].Signer, Is.EqualTo(approvalKey));
        

    }

    [Then(@"the deploy has a gas price of (.*)")]
    public void ThenTheDeployHasAGasPriceOf(int gasPrice) {
        WriteLine("the deploy has a gas price of {0}", gasPrice);
        
        Assert.That(GetDeployData().Parse().Deploy.Header.GasPrice, Is.EqualTo(gasPrice));

    }

    [Then(@"the deploy has a ttl of (.*)m")]
    public void ThenTheDeployHasATtlOfM(int ttl) {
        WriteLine("the deploy has a ttl of {0}m", ttl);
        
        Assert.That(GetDeployData().Parse().Deploy.Header.Ttl, Is.EqualTo(TimeSpan.FromMinutes(ttl).TotalMilliseconds));

    }

    [Then(@"the deploy session has a ""(.*)"" argument value of type ""(.*)""")]
    public void ThenTheDeploySessionHasAArgumentValueOfType(string arg, string type) {
        WriteLine("the deploy session has a {0} argument value of type {1}", arg, type);

        var session = GetDeployData().Parse().Deploy.Session;
        var namedArg = session.RuntimeArgs.Find(n => n.Name.Equals(arg));

        Assert.That(namedArg, Is.Not.Null);
        
        // Fails if the type is PublicKey
        // This fails because the receiver public key is stored as an Account Hash which is then stored as CLType ByteArray
        Assert.That(namedArg.Value.TypeInfo.Type.ToString(), Is.EqualTo(type));

    }

    [Then(@"the deploy session has a ""(.*)"" argument with a numeric value of (.*)")]
    public void ThenTheDeploySessionHasAArgumentWithANumericValueOf(string arg, string value) {
        WriteLine("the deploy session has a {0} argument with a numeric value of {1}", arg, value);
        
        var session = GetDeployData().Parse().Deploy.Session;
        var namedArg = session.RuntimeArgs.Find(n => n.Name.Equals(arg));

        Assert.That(namedArg, Is.Not.Null);

        if (namedArg.Value.TypeInfo.Type.Equals(CLType.U512)) {
            Assert.That(namedArg.Value.Parsed, Is.EqualTo(value));
        } else {
            throw new ArgumentException(namedArg.Value.TypeInfo.Type + " not yet implemented");
        }
        
    }

    [Then(@"the deploy session has a ""(.*)"" argument with the public key of user-(.*)")]
    public void ThenTheDeploySessionHasAArgumentWithThePublicKeyOfUser(string arg, int user) {
        WriteLine("the deploy session has a {0} argument with the public key of user-{1}", arg, user);
        
        var session = GetDeployData().Parse().Deploy.Session;
        var namedArg = session.RuntimeArgs.Find(n => n.Name.Equals(arg));
        
        Assert.That(namedArg, Is.Not.Null);
        
        var userKeyAsset = AssetUtils.GetUserKeyAsset(1, user, StepConstants.PUBLIC_KEY_PEM);

        var publicKey = PublicKey.FromPem(userKeyAsset);

        //Have to convert the PublicKey to an Account Hash
        var accountHash = publicKey.GetAccountHash();
        
        // This only works if we compare the Account Hash of the user's PublicKey
        Assert.That(namedArg.Value.Parsed.ToString()?.ToUpper(), Is.EqualTo(accountHash[13..].ToUpper()));

    }
    
    private RpcResponse<GetDeployResult> GetDeployData() {
        return _contextMap.Get<RpcResponse<GetDeployResult>>(StepConstants.INFO_GET_DEPLOY);
    }

}
