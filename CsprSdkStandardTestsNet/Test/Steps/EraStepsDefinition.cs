using System.Threading.Tasks;
using Casper.Network.SDK;
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
       
        _contextMap.Add("eraSummary", eraSummary);

    }

    [Then(@"request the era summary via the node")]
    public async Task ThenRequestTheEraSummaryViaTheNode() {
        WriteLine("request the era summary via the node");
        
        var nodeEraSummary = await _simpleRcpClient.GetEraSummary(_contextMap.Get<string>("blockHash"));

        _contextMap.Add("nodeEraSummary", nodeEraSummary);
        
        _contextMap.Add("nodeEraSummary", nodeEraSummary["result"]!["era_summary"]);
        
    }

    [Then(@"the block hash of the returned era summary is equal to the block hash of the test node era summary")]
    public void ThenTheBlockHashOfTheReturnedEraSummaryIsEqualToTheBlockHashOfTheTestNodeEraSummary() {
        WriteLine("the block hash of the returned era summary is equal to the block hash of the test node era summary");
    }

    [Then(@"the era of the returned era summary is equal to the era of the returned test node era summary")]
    public void ThenTheEraOfTheReturnedEraSummaryIsEqualToTheEraOfTheReturnedTestNodeEraSummary() {
        WriteLine("the era of the returned era summary is equal to the era of the returned test node era summary");
    }

    [Then(@"the merkle proof of the returned era summary is equal to the merkle proof of the returned test node era summary")]
    public void ThenTheMerkleProofOfTheReturnedEraSummaryIsEqualToTheMerkleProofOfTheReturnedTestNodeEraSummary() {
        WriteLine("the merkle proof of the returned era summary is equal to the merkle proof of the returned test node era summary");
    }

    [Then(@"the state root hash of the returned era summary is equal to the state root hash of the returned test node era summary")]
    public void ThenTheStateRootHashOfTheReturnedEraSummaryIsEqualToTheStateRootHashOfTheReturnedTestNodeEraSummary() {
        WriteLine(
            "the state root hash of the returned era summary is equal to the state root hash of the returned test node era summary");
    }

    [Then(@"the delegators data of the returned era summary is equal to the delegators data of the returned test node era summary")]
    public void ThenTheDelegatorsDataOfTheReturnedEraSummaryIsEqualToTheDelegatorsDataOfTheReturnedTestNodeEraSummary() {
        WriteLine(
            "the delegators data of the returned era summary is equal to the delegators data of the returned test node era summary");
    }

    [Then(@"the validators data of the returned era summary is equal to the validators data of the returned test node era summary")]
    public void ThenTheValidatorsDataOfTheReturnedEraSummaryIsEqualToTheValidatorsDataOfTheReturnedTestNodeEraSummary() {
        WriteLine(
            "the validators data of the returned era summary is equal to the validators data of the returned test node era summary");
    }
}