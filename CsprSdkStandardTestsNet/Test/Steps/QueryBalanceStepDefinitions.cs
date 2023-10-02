using System;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Threading;
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

/**
 * Get Balance steps
 */
[Binding]
public class QueryBalanceStepDefinitions {
    
    private static readonly TestProperties TestProperties = new();
    private readonly ContextMap _contextMap = ContextMap.Instance;    
    private readonly SimpleRcpClient _simpleRcpClient = new(TestProperties.Hostname, TestProperties.RcpPort);
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that a query balance is obtained by main purse public key")]
    public async Task GivenThatAQueryBalanceIsObtainedByMainPursePublicKey() {
        WriteLine("that a query balance is obtained by main purse public key");
        
        
        var balanceData = await GetCasperService().GetAccountBalance(GetFaucetKey());
        _contextMap.Add(StepConstants.BALANCE_DATA, balanceData);

        var json = await _simpleRcpClient.QueryBalance("main_purse_under_public_key",  GetFaucetKey().ToString());
        _contextMap.Add(StepConstants.BALANCE_DATA_RCP, json);
        
    }

    [Then(@"a valid query_balance_result is returned")]
    public void ThenAValidQueryBalanceResultIsReturned() {
        WriteLine("a valid query_balance_result is returned");

        var balanceData = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.BALANCE_DATA);
        Assert.That(balanceData.Parse(), Is.Not.Null);
        
    }

    [Then(@"the query_balance_result has an API version of ""(.*)""")]
    public void ThenTheQueryBalanceResultHasAnApiVersionOf(string apiVersion) {
        WriteLine("the query_balance_result has an API version of {0}", apiVersion);

        var balanceData = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.BALANCE_DATA);
        Assert.That(balanceData.Parse().ApiVersion, Is.EqualTo(apiVersion));
        
    }

    [Then(@"the query_balance_result has a valid balance")]
    public void ThenTheQueryBalanceResultHasAValidBalance() {
        WriteLine("the query_balance_result has a valid balance");
        
        var balanceData = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.BALANCE_DATA);
        var json = _contextMap.Get<JsonNode>(StepConstants.BALANCE_DATA_RCP);

        Assert.That(balanceData.Parse().BalanceValue.ToString(), 
            Is.EqualTo(json["result"]!["balance"]!.ToString()));
    }

    [Given(@"that a query balance is obtained by main purse account hash")]
    public async Task GivenThatAQueryBalanceIsObtainedByMainPurseAccountHash() {
        WriteLine("that a query balance is obtained by main purse account hash");

        var account = GetFaucetKey();
        
        var accountHash = GlobalStateKey.FromString("account-hash-" + account);

        var balanceData = await GetCasperService().GetAccountBalance((AccountHashKey)accountHash);
        _contextMap.Add(StepConstants.BALANCE_DATA, balanceData);

        var json = await _simpleRcpClient.QueryBalance("main_purse_under_account_hash", accountHash.ToString());
        _contextMap.Add(StepConstants.BALANCE_DATA_RCP, json);

    }

    [Given(@"that a query balance is obtained by main purse uref")]
    public async Task GivenThatAQueryBalanceIsObtainedByMainPurseUref() {
        WriteLine("that a query balance is obtained by main purse uref");
        
        var block = await GetCasperService().GetBlock();
        var accountInfo = await GetCasperService().GetAccountInfo(GetFaucetKey(), block.Parse().Block.Hash);

        var purseUref = accountInfo.Parse().Account.MainPurse;
        var balanceData = await GetCasperService().GetAccountBalance(purseUref);
        _contextMap.Add(StepConstants.BALANCE_DATA, balanceData);

        var json = await _simpleRcpClient.QueryBalance("purse_uref", purseUref.ToString());
        _contextMap.Add(StepConstants.BALANCE_DATA_RCP, json);

    }

    [When(@"a transfer of (.*) is made to user-(.*)'s purse")]
    public async Task WhenATransferOfIsMadeToUsersPurse(string amount, int user) {
        WriteLine("a transfer of {0} is made to user-{1}'s purse", amount, user);

        var initialBlock = await GetCasperService().GetBlock();
        var initialStateRootHash = await GetCasperService().GetStateRootHash();
        var faucetKey = GetFaucetKey();
        var userPublicKey = GetUserPublicKey(user);

        var initialBalance = await GetCasperService().GetAccountBalance(userPublicKey);
        
        var deploy = DeployTemplates.StandardTransfer(
            faucetKey,
            userPublicKey,
            BigInteger.Parse(amount),
            100_000_000,
            "casper-net-1",
            null,
            1,
            (ulong)TimeSpan.FromMinutes(_contextMap.Get<ulong>(StepConstants.TTL)).TotalMilliseconds);
        
        deploy.Sign(GetFaucetKeyPair());
        
        var putResponse = await GetCasperService().PutDeploy(deploy);

        RpcResponse<GetDeployResult> deployData = await GetCasperService().GetDeploy(
            putResponse.Parse().DeployHash, 
            true,
            new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token);

        Assert.That(deployData.Parse().ExecutionResults, Is.GreaterThan(0));
        Assert.That(deployData!.Parse().ExecutionResults.First().IsSuccess);
        Assert.That(deployData!.Parse().ExecutionResults.First().BlockHash, Is.Not.EqualTo(initialBlock.Parse().Block.Hash));

    }

    [When(@"that a query balance is obtained by user-(.*)'s main purse public and latest block identifier")]
    public void WhenThatAQueryBalanceIsObtainedByUsersMainPursePublicAndLatestBlockIdentifier(int user) {
        WriteLine("that a query balance is obtained by user-{0}'s main purse public and latest block identifier", user);
        
    }

    [Then(@"the balance includes the transferred amount")]
    public void ThenTheBalanceIncludesTheTransferredAmount() {
        WriteLine("the balance includes the transferred amount");
        
    }

    [When(@"that a query balance is obtained by user-(.*)'s main purse public key and previous block identifier")]
    public void WhenThatAQueryBalanceIsObtainedByUsersMainPursePublicKeyAndPreviousBlockIdentifier(int user) {
        WriteLine("that a query balance is obtained by user-{0}'s main purse public key and previous block identifier", user);
        
    }

    [Then(@"the balance is the pre transfer amount")]
    public void ThenTheBalanceIsThePreTransferAmount() {
        WriteLine("the balance is the pre transfer amount");
        
    }

    [When(@"that a query balance is obtained by user-(.*)'s main purse public and latest state root hash identifier")]
    public void WhenThatAQueryBalanceIsObtainedByUsersMainPursePublicAndLatestStateRootHashIdentifier(int user) {
        WriteLine("that a query balance is obtained by user-{0}'s main purse public and latest state root hash identifier", user);
        
    }

    [When(@"that a query balance is obtained by user(.*)'s main purse public key and previous state root hash identifier")]
    public void WhenThatAQueryBalanceIsObtainedByUsersMainPursePublicKeyAndPreviousStateRootHashIdentifier(int user) {
        WriteLine(
            "that a query balance is obtained by user-{0}'s main purse public key and previous state root hash identifier",
            user);

    }
    
    private PublicKey GetFaucetKey() {
        
        var faucetPem = AssetUtils.GetFaucetAsset(1, "secret_key.pem");
        Assert.That(faucetPem, Is.Not.Null);
        
        var faucetKey = KeyPair.FromPem(faucetPem);
        Assert.That(faucetKey, Is.Not.Null);
        Assert.That(faucetKey.PublicKey, Is.Not.Null);

        return faucetKey.PublicKey;

    }

    private KeyPair GetFaucetKeyPair() {
        
        var faucetPem = AssetUtils.GetFaucetAsset(1, "secret_key.pem");
        Assert.That(faucetPem, Is.Not.Null);
        
        var faucetKey = KeyPair.FromPem(faucetPem);
        Assert.That(faucetKey, Is.Not.Null);
        Assert.That(faucetKey.PublicKey, Is.Not.Null);

        return faucetKey;

    }
    
    private static PublicKey GetUserPublicKey(int userId) {
        var keyUrl = AssetUtils.GetUserKeyAsset(1, userId, "public_key.pem");
        var key = KeyPair.FromPem(keyUrl);

        Assert.IsNotNull(key);

        return key.PublicKey;

    }
    
   
}
