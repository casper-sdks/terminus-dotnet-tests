using System.Numerics;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using TerminusDotNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace TerminusDotNet.Test.Steps;

/**
 * State Get Balance steps
 */
[Binding]
public class StateGetBalanceStepDefinitions {
    
    private static readonly TestProperties TestProperties = new();
    private Node _node = new(TestProperties.DockerName);
    private readonly ContextMap _contextMap = ContextMap.Instance;    
    private readonly SimpleRcpClient _simpleRcpClient = new(TestProperties.Hostname, TestProperties.RcpPort);

    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that the state_get_balance RPC method is invoked against nclt user-1 purse")]
    public async Task GivenThatTheStateGetBalanceRpcMethodIsInvokedAgainstNcltUserPurse() {
        WriteLine("that the state_get_balance RPC method is invoked against nclt user-1 purse");

        var stateRootHash =  _node.GetStateRootHash(1);
        var accountMainPurse = _node.GetAccountMainPurse(1);
        var balance = await GetCasperService().GetAccountBalance(accountMainPurse, stateRootHash);

        _contextMap.Add(StepConstants.STATE_GET_BALANCE_RESULT, balance);

        var json = await _simpleRcpClient.GetBalance(stateRootHash, accountMainPurse);
        
        _contextMap.Add(StepConstants.EXPECTED_JSON, json);

    }

    [Then(@"a valid state_get_balance_result is returned")]
    public void ThenAValidStateGetBalanceResultIsReturned() {
        WriteLine("a valid state_get_balance_result is returned");

        var balanceData = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.STATE_GET_BALANCE_RESULT);
        
        Assert.That(balanceData.Parse().BalanceValue, Is.Not.Null);

    }

    [Then(@"the state_get_balance_result contains the purse amount")]
    public async Task ThenTheStateGetBalanceResultContainsThePurseAmount() {
        WriteLine("the state_get_balance_result contains the purse amount");     
        
        var accountMainPurse = _node.GetAccountMainPurse(1);
        var json = await _simpleRcpClient.GetBalance(_node.GetStateRootHash(1), accountMainPurse);
        var balance = BigInteger.Parse(json["result"]!["balance_value"]!.ToString());

        var balanceData = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.STATE_GET_BALANCE_RESULT);
        
        Assert.That(balanceData.Parse().BalanceValue, Is.EqualTo(balance));

    }

    [Then(@"the state_get_balance_result contains api version ""(.*)""")]
    public void ThenTheStateGetBalanceResultContainsApiVersion(string apiVersion) {
        WriteLine("the state_get_balance_result contains api version {0}", apiVersion);        
        
        var balanceData = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.STATE_GET_BALANCE_RESULT);
        Assert.That(balanceData.Parse().ApiVersion, Is.EqualTo(apiVersion));

    }

    [Then(@"the state_get_balance_result contains a valid merkle proof")]
    public void ThenTheStateGetBalanceResultContainsAValidMerkleProof() {
        WriteLine("the state_get_balance_result contains a valid merkle proof");

        var balanceData = _contextMap.Get<RpcResponse<GetBalanceResult>>(StepConstants.STATE_GET_BALANCE_RESULT);
        Assert.That(balanceData.Parse().MerkleProof, Is.Not.Null);

        var expectedMerkleProof = _contextMap.Get<JsonNode>(StepConstants.EXPECTED_JSON)["result"]!["merkle_proof"]!.ToString();
        
        Assert.That(balanceData.Parse().MerkleProof.Length,
            Is.EqualTo(int.Parse(expectedMerkleProof.Split(" ")[0][1..])));
        
    }
    
}
