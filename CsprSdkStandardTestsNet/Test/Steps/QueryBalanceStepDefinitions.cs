using System.Text.Json.Nodes;
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
        
        var faucetPem = AssetUtils.GetFaucetAsset(1, "secret_key.pem");
        Assert.That(faucetPem, Is.Not.Null);
        
        var faucetKey = KeyPair.FromPem(faucetPem);
        Assert.That(faucetKey, Is.Not.Null);
        Assert.That(faucetKey.PublicKey, Is.Not.Null);

        _contextMap.Add(StepConstants.FAUCET_PRIVATE_KEY, faucetKey);
        
        var balanceData = await GetCasperService().GetAccountBalance(faucetKey.PublicKey);

        _contextMap.Add(StepConstants.BALANCE_DATA, balanceData);

        var json = await _simpleRcpClient.QueryBalance("main_purse_under_public_key", faucetKey.PublicKey.ToString());

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
        
        var account = _contextMap.Get<KeyPair>(StepConstants.FAUCET_PRIVATE_KEY).PublicKey.ToAccountHex();

        var accountHash = (AccountHashKey)GlobalStateKey.FromString("account-hash-" + account);

        var balanceData = await GetCasperService().GetAccountBalance(accountHash);

        _contextMap.Add(StepConstants.BALANCE_DATA, balanceData);

        var json = await _simpleRcpClient.QueryBalance("main_purse_under_account_hash", accountHash.ToString());

        _contextMap.Add(StepConstants.BALANCE_DATA_RCP, json);

    }

    [Given(@"that a query balance is obtained by main purse uref")]
    public async Task GivenThatAQueryBalanceIsObtainedByMainPurseUref() {
        WriteLine("that a query balance is obtained by main purse uref");

        var block = await GetCasperService().GetBlock();

        var accountInfo = await GetCasperService().GetAccountInfo(
            _contextMap.Get<KeyPair>(StepConstants.FAUCET_PRIVATE_KEY).PublicKey, block.Parse().Block.Hash);

        var purseUref = accountInfo.Parse().Account.MainPurse;
        
        var balanceData = await GetCasperService().GetAccountBalance(purseUref);

        _contextMap.Add(StepConstants.BALANCE_DATA, balanceData);

        var json = await _simpleRcpClient.QueryBalance("purse_uref", purseUref.ToString());

        _contextMap.Add(StepConstants.BALANCE_DATA_RCP, json);

    }
    
}
