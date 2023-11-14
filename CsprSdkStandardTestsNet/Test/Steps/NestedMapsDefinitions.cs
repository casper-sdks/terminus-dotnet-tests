using System;
using System.Collections.Generic;
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

[Binding]
public class NestedMapsDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    private CLValue _map;
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"a map is created \{""(.*)"": (.*)}")]
    public void GivenAMapIsCreated(string key, int value) {
        WriteLine("a map is created {0}: {1}", key, value);
        
        var innerMap = new Dictionary<CLValue, CLValue>
            {{ CLValue.String(key), CLValue.U32((uint)value) }};

        _map = CLValue.Map(innerMap);
        
        Assert.That(_map, Is.Not.Null);
    }

    [Then(@"the map's key type is ""(.*)"" and the maps value type is ""(.*)""")]
    public void ThenTheMapsKeyTypeIsAndTheMapsValueTypeIs(string keyType, string valueType) {
        WriteLine("the map's key type is '{0}' and the maps value type is '{1}'", keyType, valueType);
        
        /*
         * TODO
         */
        
    }

    [Then(@"the map's bytes are ""(.*)""")]
    public void ThenTheMapsBytesAre(string hexBytes) {
        WriteLine("the map's bytes are '{0}'", hexBytes);
        
        Assert.That(CLTypeUtils.GetHexValue(_map), Is.EqualTo(hexBytes).IgnoreCase);
    }

    [Given(@"that the nested map is deployed in a transfer")]
    public async Task GivenThatTheNestedMapIsDeployedInATransfer() {
        WriteLine("that the nested map is deployed in a transfer");
        
        var runtimeArgs = new List<NamedArg>{ 
            new ("MAP", _map)
        };

        _map = null;
        
        await DeployUtils.DeployArgs(runtimeArgs, GetCasperService());
        
    }

    [Given(@"the transfer containing the nested map is successfully executed")]
    public async Task GivenTheTransferContainingTheNestedMapIsSuccessfullyExecuted() {
        WriteLine("the transfer containing the nested map is successfully executed");
        
        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);

        RpcResponse<GetDeployResult> deploy = await GetCasperService().GetDeploy(
            deployResult.Parse().DeployHash, 
            true,
            new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token);
       
        Assert.That(deploy!.Parse().ExecutionResults[0].IsSuccess);

        _contextMap.Add(StepConstants.DEPLOY, deploy);
        
    }

    [When(@"the map is read from the deploy")]
    public void WhenTheMapIsReadFromTheDeploy() {
        WriteLine("the map is read from the deploy");
        
        var deploy = _contextMap.Get<RpcResponse<GetDeployResult>>(StepConstants.DEPLOY).Parse();

        _map = deploy.Deploy.Session.RuntimeArgs.Find(n => n.Name.Equals("MAP")).Value;
        
        Assert.That(_map, Is.Not.Null);
        
    }

    [When(@"the map's bytes are ""(.*)""")]
    public void WhenTheMapsBytesAre(string hexBytes) {
        ThenTheMapsBytesAre(hexBytes);
    }

    [Then(@"the map's key is ""(.*)"" and value is ""(.*)""")]
    public void ThenTheMapsKeyIsAndValueIs(string key, string vlaue) {
        WriteLine("the map's key is '{0}' and value is '{1}'", key, vlaue);
        
    }

    [Given(@"a nested map is created \{""(.*)"": \{""(.*)"": (.*)}, ""(.*)"": \{""(.*)"", (.*)}}")]
    public void GivenANestedMapIsCreated(string key0, string key1, int value1, string key2, string key3, int value3) {
        WriteLine("a nested map is created '{0}': '{1}': '{2}', '{3}': '{4}', '{5}'", key0, key2, value1, key2, key3, value3);
        
        var innerMap1 = new Dictionary<CLValue, CLValue>
            {{ CLValue.String(key1), CLValue.U32((uint)value1) }};

        var innerMap2 = new Dictionary<CLValue, CLValue>
            {{ CLValue.String(key3), CLValue.U32((uint)value3) }};

        var rootMap = new Dictionary<CLValue, CLValue> {
            { CLValue.String(key0), CLValue.Map(innerMap1) },
            { CLValue.String(key2), CLValue.Map(innerMap2) }
        };

        _map = CLValue.Map(rootMap);

    }

    [Then(@"the 1st nested map's key is ""(.*)"" and value is ""(.*)""")]
    public void ThenTheStNestedMapsKeyIsAndValueIs(string key, string value) {
        WriteLine("the 1st nested map's key is {0}' and value is {1}'", key, value);
    }

    [Given(@"a nested map is created  \{(.*): \{(.*): \{(.*): ""(.*)""}, (.*): \{(.*): ""(.*)""}}, (.*): \{(.*): \{(.*): ""(.*)""}, (.*): \{(.*): ""(.*)""}}}")]
    public void GivenANestedMapIsCreated(int key1, int key11, int key111, string value111, int key12, int key121, string value121, int key2, int key21, int key211, string value211, int key22, int key221, string value221) {
        WriteLine("a nested map is created  '{0}':'{1}': '{2}': '{3}', '{4}': '{5}': '{6}', '{7}': '{8}': '{9}': '{10}', '{11}': '{12}': '{13}'", 
            key1, key11, key111, value111, key12, key121, value121, key2, key21, key211, value211, key22, key221, value221 );
        
        /*
         * The Map type in the SDK accepts a Dictionary
         * The dictionary that's built up below has a mix of keys, U32 and U256
         * This fails with 'All keys and values must have the same type (Parameter 'dict')'
         * The SDK Map should use a HashTable
         */
        
        var innerMap111 = new Dictionary<CLValue, CLValue>
            {{ CLValue.U256(key111), CLValue.String(value111) }};
        var innerMap121 = new Dictionary<CLValue, CLValue>
            {{ CLValue.U32((uint)key121), CLValue.String(value121) }};
        var innerMap11 = new Dictionary<CLValue, CLValue> {
            { CLValue.U256(key11), CLValue.Map(innerMap111) },
            { CLValue.U256(key12), CLValue.Map(innerMap121) }
        };
        
        var innerMap211 = new Dictionary<CLValue, CLValue>
            {{ CLValue.U256(key211), CLValue.String(value211) }};
        var innerMap221 = new Dictionary<CLValue, CLValue>
            {{ CLValue.U32((uint)key221), CLValue.String(value221) }};
        var innerMap21 = new Dictionary<CLValue, CLValue> {
            { CLValue.U256(key21), CLValue.Map(innerMap211) },
            { CLValue.U256(key22), CLValue.Map(innerMap221) }
        };
        
        var rootMap = new Dictionary<CLValue, CLValue> {
            { CLValue.U256(key1), CLValue.Map(innerMap11) },
            { CLValue.U256(key2), CLValue.Map(innerMap21) }
        };
        
        _map = CLValue.Map(rootMap);
        
    }
}
