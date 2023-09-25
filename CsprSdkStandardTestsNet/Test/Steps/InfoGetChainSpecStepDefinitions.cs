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
public class InfoGetChainSpecStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    private static readonly TestProperties TestProperties = new();
    
    private readonly SimpleRcpClient _simpleRcpClient = new(TestProperties.Hostname, TestProperties.RcpPort);
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }

    [Given(@"that the info_get_chainspec is invoked against nctl")]
    public async Task GivenThatTheInfoGetChainspecIsInvokedAgainstNctl() {
        WriteLine("that the info_get_chainspec is invoked against nctl");
        
        var chainSpecNctl = await _simpleRcpClient.GetInfoGetChainspec();

        Assert.That(chainSpecNctl, Is.Not.Null);

        _contextMap.Add(StepConstants.INFO_GET_CHAINSPEC_NCTL, chainSpecNctl);
        
    }

    [Given(@"that info_get_chainspec is invoked against the sdk")]
    public async Task GivenThatInfoGetChainspecIsInvokedAgainstTheSdk() {
        WriteLine("that info_get_chainspec is invoked against the sdk");
        
        var chainSpecSdk = await GetCasperService().GetChainspec();

        Assert.That(chainSpecSdk, Is.Not.Null);

        _contextMap.Add(StepConstants.INFO_GET_CHAINSPEC_SDK, chainSpecSdk);

    }

    [Then(@"the sdk chain bytes equals the nctl chain bytes")]
    public void ThenTheSdkChainBytesEqualsTheNctlChainBytes() {
        WriteLine("the sdk chain bytes equals the nctl chain bytes");

        var chainSpecNctl = _contextMap.Get<JsonNode>(StepConstants.INFO_GET_CHAINSPEC_NCTL);
        var chainSpecSdk = _contextMap.Get<RpcResponse<GetChainspecResult>>(StepConstants.INFO_GET_CHAINSPEC_SDK);

        Assert.That(chainSpecSdk.Parse().ChainspecBytes.ChainspecBytes.ToUpper(),
            Is.EqualTo(chainSpecNctl["result"]!["chainspec_bytes"]!["chainspec_bytes"]!.ToString().ToUpper()));
        
    }

    [Then(@"the sdk genesis bytes equals the nctl genesis bytes")]
    public void ThenTheSdkGenesisBytesEqualsTheNctlGenesisBytes() {
        WriteLine("the sdk genesis bytes equals the nctl genesis bytes");

        var chainSpecNctl = _contextMap.Get<JsonNode>(StepConstants.INFO_GET_CHAINSPEC_NCTL);
        var chainSpecSdk = _contextMap.Get<RpcResponse<GetChainspecResult>>(StepConstants.INFO_GET_CHAINSPEC_SDK);
        
        Assert.That(chainSpecSdk.Parse().ChainspecBytes.MaybeGenesisAccountsBytes.ToUpper(),
            Is.EqualTo(chainSpecNctl["result"]!["chainspec_bytes"]!["maybe_genesis_accounts_bytes"]!.ToString().ToUpper()));
        
    }
    
}
