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
public class InfoGetValidatorChangesStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance; 
    private static readonly TestProperties TestProperties = new();
    
    private SimpleRcpClient _simpleRcpClient = new(TestProperties.Hostname, TestProperties.RcpPort);
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that the info_get_validator_changes method is invoked against a node")]
    public async Task GivenThatTheInfoGetValidatorChangesMethodIsInvokedAgainstANode() {
        WriteLine("that the info_get_validator_changes method is invoked against a node");

        var validatorsChanges = await GetCasperService().GetValidatorChanges();
        _contextMap.Add(StepConstants.VALIDATORS_CHANGES, validatorsChanges);
        Assert.That(validatorsChanges, Is.Not.Null);

        var expectedValidatorChanges = await _simpleRcpClient.GetValidatorChanges();
        Assert.That(expectedValidatorChanges, Is.Not.Null);
        _contextMap.Add(StepConstants.EXPECTED_VALIDATOR_CHANGES, expectedValidatorChanges);
        
    }

    [Then(@"a valid info_get_validator_changes_result is returned")]
    public void ThenAValidInfoGetValidatorChangesResultIsReturned() {
        WriteLine("a valid info_get_validator_changes_result is returned");

        var validatorsChanges =
            _contextMap.Get<RpcResponse<GetValidatorChangesResult>>(StepConstants.VALIDATORS_CHANGES);
        Assert.That(validatorsChanges, Is.Not.Null);

        var expectedValidatorChanges = _contextMap.Get<JsonNode>(StepConstants.EXPECTED_VALIDATOR_CHANGES);
        Assert.That(expectedValidatorChanges, Is.Not.Null);
        
        Assert.That(validatorsChanges.Parse().Changes.Count, 
            Is.EqualTo(expectedValidatorChanges["result"]!["changes"]!.AsArray().Count));

    }

    [Then(@"the info_get_validator_changes_result contains a valid API version")]
    public void ThenTheInfoGetValidatorChangesResultContainsAValidApiVersion() {
        WriteLine("the info_get_validator_changes_result contains a valid API version");
        
        var validatorsChanges =
            _contextMap.Get<RpcResponse<GetValidatorChangesResult>>(StepConstants.VALIDATORS_CHANGES);
        var expectedValidatorChanges = _contextMap.Get<JsonNode>(StepConstants.EXPECTED_VALIDATOR_CHANGES);

        Assert.That(validatorsChanges.Parse().ApiVersion.Contains('.'), Is.True);
        Assert.That(validatorsChanges.Parse().ApiVersion, 
            Is.EqualTo(expectedValidatorChanges["result"]!["api_version"]!.ToString()));

    }
}
