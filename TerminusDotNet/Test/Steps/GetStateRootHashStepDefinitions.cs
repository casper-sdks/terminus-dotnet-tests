using System.Threading.Tasks;
using Casper.Network.SDK;
using TerminusDotNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;


namespace TerminusDotNet.Test.Steps;

/**
 * Step definitions for get_state_root_hash
 */
[Binding]
public class GetStateRootHashStepDefinitions {
    
    private static readonly TestProperties TestProperties = new();
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    
    private readonly Nctl _nctl = new(TestProperties.DockerName);

    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }

    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    [Given(@"that the chain_get_state_root_hash RCP method is invoked against nctl")]
    public async Task GivenThatTheChainGetStateRootHashRcpMethodIsInvokedAgainstNctl() {
        WriteLine("that the chain_get_state_root_hash RCP method is invoked against nctl");

        var rpcResponse = await GetCasperService().GetStateRootHash();
        
        _contextMap.Add(StepConstants.STATE_ROOT_HASH ,rpcResponse);
        
    }

    [Then(@"a valid chain_get_state_root_hash_result is returned")]
    public void ThenAValidChainGetStateRootHashResultIsReturned() {
        WriteLine("a valid chain_get_state_root_hash_result is returned");

        var stateRootHash = _contextMap.Get<string>(StepConstants.STATE_ROOT_HASH);
        
        Assert.That(stateRootHash, Is.Not.Null);

        var expectedStateRootHash = _nctl.GetStateRootHash(1);
        
        Assert.That(stateRootHash, Is.EqualTo(expectedStateRootHash));

    }
}
