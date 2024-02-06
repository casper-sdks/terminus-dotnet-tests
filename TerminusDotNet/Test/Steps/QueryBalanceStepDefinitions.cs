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
using TerminusDotNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace TerminusDotNet.Test.Steps;

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
        var accountHash = new AccountHashKey(account.GetAccountHash());

        var balanceData = await GetCasperService().GetAccountBalance(accountHash);
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
        
        _contextMap.Add(StepConstants.TRANSFER_AMOUNT, BigInteger.Parse(amount));

        var initialBlock = await GetCasperService().GetBlock();
        var initialStateRootHash = await GetCasperService().GetStateRootHash();
        
        _contextMap.Add(StepConstants.INITIAL_BLOCK, initialBlock);
        _contextMap.Add(StepConstants.INITIAL_STATE_ROOT, initialStateRootHash);
        
        var faucetKey = GetFaucetKey();
        var userPublicKey = GetUserPublicKey(user);

        var initialBalance = await GetCasperService().GetAccountBalance(userPublicKey);
        _contextMap.Add(StepConstants.INITIAL_BALANCE, initialBalance);
        
        var deploy = DeployTemplates.StandardTransfer(
            faucetKey,
            userPublicKey,
            BigInteger.Parse(amount),
            100_000_000,
            "casper-net-1",
            null,
            1,
            (ulong)TimeSpan.FromMinutes(30).TotalMilliseconds);
        
        deploy.Sign(GetFaucetKeyPair());
        
        var putResponse = await GetCasperService().PutDeploy(deploy);

        RpcResponse<GetDeployResult> deployData = await GetCasperService().GetDeploy(
            putResponse.Parse().DeployHash, 
            true,
            new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token);

        Assert.That(deployData.Parse().ExecutionResults.Count, Is.GreaterThan(0));
        Assert.That(deployData!.Parse().ExecutionResults.First().IsSuccess);
        Assert.That(deployData!.Parse().ExecutionResults.First().BlockHash, Is.Not.EqualTo(initialBlock.Parse().Block.Hash));
        
        _contextMap.Add(StepConstants.DEPLOY_RESULT, deployData);

    }

    [When(@"that a query balance is obtained by user-(.*)'s main purse public and latest block identifier")]
    public async Task WhenThatAQueryBalanceIsObtainedByUsersMainPursePublicAndLatestBlockIdentifier(int user) {
        WriteLine("that a query balance is obtained by user-{0}'s main purse public and latest block identifier", user);

        var deployData = _contextMap.Get<RpcResponse<GetDeployResult>>(StepConstants.DEPLOY_RESULT);
        var userPublicKey = GetUserPublicKey(user);
        
        var queryBalanceData = await GetCasperService().GetAccountBalanceWithBlockHash(userPublicKey, 
            deployData.Parse().ExecutionResults.First().BlockHash);
        
        _contextMap.Add(StepConstants.BALANCE_DATA, queryBalanceData);

    }

    [Then(@"the balance includes the transferred amount")]
    public void ThenTheBalanceIncludesTheTransferredAmount() {
        WriteLine("the balance includes the transferred amount");

        var queryBalanceData = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.BALANCE_DATA);
        var initialBalance = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.INITIAL_BALANCE);
        var amount = _contextMap.Get<BigInteger>(StepConstants.TRANSFER_AMOUNT);
        
        Assert.That(queryBalanceData.Parse().BalanceValue, Is.EqualTo(initialBalance.Parse().BalanceValue + amount));

    }

    [When(@"that a query balance is obtained by user-(.*)'s main purse public key and previous block identifier")]
    public async Task WhenThatAQueryBalanceIsObtainedByUsersMainPursePublicKeyAndPreviousBlockIdentifier(int user) {
        WriteLine("that a query balance is obtained by user-{0}'s main purse public key and previous block identifier", user);

        var initialBlock = _contextMap.Get<RpcResponse<GetBlockResult>>(StepConstants.INITIAL_BLOCK);
        var userPublicKey = GetUserPublicKey(user);

        var queryBalanceData = await GetCasperService().GetAccountBalanceWithBlockHash(userPublicKey, 
            initialBlock.Parse().Block.Hash);
        
        _contextMap.Add(StepConstants.BALANCE_DATA, queryBalanceData);
        
    }

    [Then(@"the balance is the pre transfer amount")]
    public void ThenTheBalanceIsThePreTransferAmount() {
        WriteLine("the balance is the pre transfer amount");
        
        var queryBalanceData = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.BALANCE_DATA);
        var initialBalance = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.INITIAL_BALANCE);
        
        Assert.That(queryBalanceData.Parse().BalanceValue, Is.EqualTo(initialBalance.Parse().BalanceValue));
        
    }

    [When(@"that a query balance is obtained by user-(.*)'s main purse public and latest state root hash identifier")]
    public async Task WhenThatAQueryBalanceIsObtainedByUsersMainPursePublicAndLatestStateRootHashIdentifier(int user) {
        WriteLine("that a query balance is obtained by user-{0}'s main purse public and latest state root hash identifier", user);
        
        var userPublicKey = GetUserPublicKey(user);
        var stateRootHash = await GetCasperService().GetStateRootHash();
        
        Assert.That(stateRootHash, Is.Not.EqualTo(_contextMap.Get<string>(StepConstants.INITIAL_STATE_ROOT)));

        var queryBalanceData = await GetCasperService().GetAccountBalance(userPublicKey, stateRootHash);
        _contextMap.Add(StepConstants.BALANCE_DATA, queryBalanceData);
        
    }

    [When(@"that a query balance is obtained by user-(.*)'s main purse public key and previous state root hash identifier")]
    public async Task WhenThatAQueryBalanceIsObtainedByUsersMainPursePublicKeyAndPreviousStateRootHashIdentifier(int user) {
        WriteLine(
            "that a query balance is obtained by user-{0}'s main purse public key and previous state root hash identifier",
            user);
        
        var userPublicKey = GetUserPublicKey(user);
        var initialStateRootHash = _contextMap.Get<string>(StepConstants.INITIAL_STATE_ROOT);
        
        var queryBalanceData = await GetCasperService().GetAccountBalance(userPublicKey, initialStateRootHash);
        _contextMap.Add(StepConstants.BALANCE_DATA, queryBalanceData);

    }
    
    private static PublicKey GetFaucetKey() {
        
        var faucetPem = AssetUtils.GetFaucetAsset(1, "secret_key.pem");
        Assert.That(faucetPem, Is.Not.Null);
        
        var faucetKey = KeyPair.FromPem(faucetPem);
        Assert.That(faucetKey, Is.Not.Null);
        Assert.That(faucetKey.PublicKey, Is.Not.Null);

        return faucetKey.PublicKey;

    }

    private static KeyPair GetFaucetKeyPair() {
        
        var faucetPem = AssetUtils.GetFaucetAsset(1, "secret_key.pem");
        Assert.That(faucetPem, Is.Not.Null);
        
        var faucetKey = KeyPair.FromPem(faucetPem);
        Assert.That(faucetKey, Is.Not.Null);
        Assert.That(faucetKey.PublicKey, Is.Not.Null);

        return faucetKey;

    }
    
    private static PublicKey GetUserPublicKey(int userId) {
        var keyUrl = AssetUtils.GetUserKeyAsset(1, userId, "public_key.pem");
        
        var key = PublicKey.FromPem(keyUrl);
        Assert.IsNotNull(key);

        return key;

    }
    
}
