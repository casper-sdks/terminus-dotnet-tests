using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

[Binding]
public class EraStepsDefinition {
    
    private readonly ContextMap _contextMap = ContextMap.Instance; 
    private static readonly TestProperties TestProperties = new();
    
    private SimpleRcpClient _simpleRcpClient = new(TestProperties.Hostname, TestProperties.RcpPort);
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that the era summary is requested via the sdk")]
    public async Task GivenThatTheEraSummaryIsRequestedViaTheSdk() {
        WriteLine("that the era summary is requested via the sdk");
        
        var block = await GetCasperService().GetBlock();
        
        Assert.That(block, Is.Not.Null);
        _contextMap.Add("blockHash", block.Parse().Block.Hash);

        var eraSummary = await GetCasperService().GetEraSummary(block.Parse().Block.Hash);
        
        Assert.That(eraSummary, Is.Not.Null);
       
        _contextMap.Add(StepConstants.SDK_ERA_SUMMARY, eraSummary);

    }

    [Then(@"request the era summary via the node")]
    public async Task ThenRequestTheEraSummaryViaTheNode() {
        WriteLine("request the era summary via the node");
        
        var nodeEraSummary = await _simpleRcpClient.GetEraSummary(_contextMap.Get<string>("blockHash"));

        _contextMap.Add(StepConstants.NODE_ERA_SUMMARY, nodeEraSummary["result"]!["era_summary"]);
        
    }

    [Then(@"the block hash of the returned era summary is equal to the block hash of the test node era summary")]
    public void ThenTheBlockHashOfTheReturnedEraSummaryIsEqualToTheBlockHashOfTheTestNodeEraSummary() {
        WriteLine("the block hash of the returned era summary is equal to the block hash of the test node era summary");
        
        var eraSummary = _contextMap.Get<RpcResponse<GetEraSummaryResult>>(StepConstants.SDK_ERA_SUMMARY);
        var nodeEraSummary = _contextMap.Get<JsonNode>(StepConstants.NODE_ERA_SUMMARY);
        var blockHash = nodeEraSummary["block_hash"];

        Assert.That(blockHash!.ToString().ToUpper(), Is.EqualTo(eraSummary.Parse().EraSummary.BlockHash.ToUpper()));
        
    }

    [Then(@"the era of the returned era summary is equal to the era of the returned test node era summary")]
    public void ThenTheEraOfTheReturnedEraSummaryIsEqualToTheEraOfTheReturnedTestNodeEraSummary() {
        WriteLine("the era of the returned era summary is equal to the era of the returned test node era summary");
        
        var eraSummary = _contextMap.Get<RpcResponse<GetEraSummaryResult>>(StepConstants.SDK_ERA_SUMMARY);
        var nodeEraSummary = _contextMap.Get<JsonNode>(StepConstants.NODE_ERA_SUMMARY);
        var eraId = nodeEraSummary["era_id"];

        Assert.That(eraId!.ToString(), Is.EqualTo(eraSummary.Parse().EraSummary.EraId.ToString()));
        
    }

    [Then(@"the merkle proof of the returned era summary is equal to the merkle proof of the returned test node era summary")]
    public void ThenTheMerkleProofOfTheReturnedEraSummaryIsEqualToTheMerkleProofOfTheReturnedTestNodeEraSummary() {
        WriteLine("the merkle proof of the returned era summary is equal to the merkle proof of the returned test node era summary");
        
        var eraSummary = _contextMap.Get<RpcResponse<GetEraSummaryResult>>(StepConstants.SDK_ERA_SUMMARY);
        var nodeEraSummary = _contextMap.Get<JsonNode>(StepConstants.NODE_ERA_SUMMARY);

        Assert.That(eraSummary.Parse().EraSummary.MerkleProof.ToUpper(), Is.EqualTo(nodeEraSummary["merkle_proof"]!.ToString().ToUpper()));

    }

    [Then(@"the state root hash of the returned era summary is equal to the state root hash of the returned test node era summary")]
    public void ThenTheStateRootHashOfTheReturnedEraSummaryIsEqualToTheStateRootHashOfTheReturnedTestNodeEraSummary() {
        WriteLine(
            "the state root hash of the returned era summary is equal to the state root hash of the returned test node era summary");
        
        var eraSummary = _contextMap.Get<RpcResponse<GetEraSummaryResult>>(StepConstants.SDK_ERA_SUMMARY);
        var nodeEraSummary = _contextMap.Get<JsonNode>(StepConstants.NODE_ERA_SUMMARY);

        Assert.That(nodeEraSummary["state_root_hash"]!.ToString().ToUpper(), Is.EqualTo(eraSummary.Parse().EraSummary.StateRootHash.ToUpper()));
        
    }

    [Then(@"the delegators data of the returned era summary is equal to the delegators data of the returned test node era summary")]
    public void ThenTheDelegatorsDataOfTheReturnedEraSummaryIsEqualToTheDelegatorsDataOfTheReturnedTestNodeEraSummary() {
        WriteLine(
            "the delegators data of the returned era summary is equal to the delegators data of the returned test node era summary");
        
        var eraSummary = _contextMap.Get<RpcResponse<GetEraSummaryResult>>(StepConstants.SDK_ERA_SUMMARY);
        var nodeEraSummary = _contextMap.Get<JsonNode>(StepConstants.NODE_ERA_SUMMARY);

        var allocations = nodeEraSummary["stored_value"]!["EraInfo"]!["seigniorage_allocations"];

        var delegatorsSdk = eraSummary.Parse().EraSummary.StoredValue.
            EraInfo.SeigniorageAllocations.FindAll(e => e.IsDelegator);
        
        foreach (var alloc in allocations!.AsArray()) {
            
            if (alloc["Delegator"] != null) {
                
                var found = delegatorsSdk
                    .Find(e => e.DelegatorPublicKey.ToString().ToUpper()
                        .Equals(alloc["Delegator"]["delegator_public_key"]!.ToString().ToUpper()));
                
                Assert.That(found, Is.Not.Null);

                Assert.That(alloc["Delegator"]["validator_public_key"]!.ToString().ToUpper(), Is.EqualTo(found.ValidatorPublicKey.ToString().ToUpper()));
                Assert.That(alloc["Delegator"]["amount"]!.ToString(), Is.EqualTo(found.Amount.ToString()));
            }
        }
        
    }

    [Then(@"the validators data of the returned era summary is equal to the validators data of the returned test node era summary")]
    public void ThenTheValidatorsDataOfTheReturnedEraSummaryIsEqualToTheValidatorsDataOfTheReturnedTestNodeEraSummary() {
        WriteLine(
            "the validators data of the returned era summary is equal to the validators data of the returned test node era summary");
        
        var eraSummary = _contextMap.Get<RpcResponse<GetEraSummaryResult>>(StepConstants.SDK_ERA_SUMMARY);
        var nodeEraSummary = _contextMap.Get<JsonNode>(StepConstants.NODE_ERA_SUMMARY);

        var allocations = nodeEraSummary["stored_value"]!["EraInfo"]!["seigniorage_allocations"];

        var validatorsSdk = eraSummary.Parse().EraSummary.StoredValue.
            EraInfo.SeigniorageAllocations.FindAll(e => !e.IsDelegator);
        
        foreach (var alloc in allocations!.AsArray()) {
            
            if (alloc["Validator"] != null) {
                
                var found = validatorsSdk
                    .Find(e => e.ValidatorPublicKey.ToString().ToUpper()
                        .Equals(alloc["Validator"]["validator_public_key"]!.ToString().ToUpper()));
                
                Assert.That(found, Is.Not.Null);

                Assert.That(alloc["Validator"]["validator_public_key"]!.ToString().ToUpper(), Is.EqualTo(found.ValidatorPublicKey.ToString().ToUpper()));
                Assert.That(alloc["Validator"]["amount"]!.ToString(), Is.EqualTo(found.Amount.ToString()));
            }
        }
        
    }
    
}
