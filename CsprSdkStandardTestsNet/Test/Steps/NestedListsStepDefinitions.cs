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
public class NestedListsStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    private CLValue _list;
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"a list is created with ""(.*)"" values of \(""(.*)"", ""(.*)"", ""(.*)""\)")]
    public void GivenAListIsCreatedWithValuesOf(CLType type, string val1, string val2, string val3) {
        WriteLine("a list is created with '{0}' values of '{1}', '{2}', '{3}'", true, val1, val2, val3);

        CLValue[] clList = {
            CLValueFactory.CreateValue(type, val1),
            CLValueFactory.CreateValue(type, val2),
            CLValueFactory.CreateValue(type, val3)
        };

        _list = CLValue.List(clList);

    }

    [Then(@"the list's bytes are ""(.*)""")]
    public void ThenTheListsBytesAre(string hexBytes) {
        WriteLine("the list's bytes are '{0}'", hexBytes);
        
        Assert.That(CLTypeUtils.GetHexValue(_list), Is.EqualTo(hexBytes).IgnoreCase);
        
    }

    [Then(@"the list's length is (.*)")]
    public void ThenTheListsLengthIs(int length) {
        WriteLine("the list's length is {0}", length);
        
        // The SDK needs to expose the CLType's values
        
        Assert.Fail();
        
    }

    [Then(@"the list's ""(.*)"" item is a CLValue with ""(.*)"" value of ""(.*)""")]
    public void ThenTheListsItemIsAclValueWithValueOf(string nth, string type, string value) {
        WriteLine("the list's '{0}' item is a CLValue with '{1}' value of '{2}'", nth, type, value);
        
        // The SDK needs to expose the CLType's values
        
        Assert.Fail();
        
    }

    [Given(@"that the list is deployed in a transfer")]
    public async Task GivenThatTheListIsDeployedInATransfer() {
        WriteLine("that the list is deployed in a transfer");
        
        var runtimeArgs = new List<NamedArg>{ 
            new ("LIST", _list)
        };

        _list = null;
        
        await DeployUtils.DeployArgs(runtimeArgs, GetCasperService());

    }

    [Given(@"the transfer containing the list is successfully executed")]
    public async Task GivenTheTransferContainingTheListIsSuccessfullyExecuted() {
        WriteLine("the transfer containing the list is successfully executed");
        
        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);

        RpcResponse<GetDeployResult> deploy = await GetCasperService().GetDeploy(
            deployResult.Parse().DeployHash, 
            true,
            new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token);
       
        Assert.That(deploy!.Parse().ExecutionResults[0].IsSuccess);

        _contextMap.Add(StepConstants.DEPLOY, deploy);
        
    }

    [When(@"the list is read from the deploy")]
    public void WhenTheListIsReadFromTheDeploy() {
        WriteLine("the list is read from the deploy");
        
        var deploy = _contextMap.Get<RpcResponse<GetDeployResult>>(StepConstants.DEPLOY).Parse();

        _list = deploy.Deploy.Session.RuntimeArgs.Find(n => n.Name.Equals("LIST")).Value;
        
        Assert.That(_list, Is.Not.Null);
        
    }

    [Given(@"a list is created with (.*) values of \((.*), (.*), (.*)\)")]
    public void GivenAListIsCreatedWithIValuesOf(CLType type, int val1, int val2, int val3) {
        WriteLine("a list is created with {0} values of {1}, {2}, {3}", type, val1, val2, val3);

        var x = CLValue.I32(int.Parse(val1.ToString()));
        
        CLValue[] clList = {
            CLValueFactory.CreateValue(type, val1.ToString()),
            CLValueFactory.CreateValue(type, val2.ToString()),
            CLValueFactory.CreateValue(type, val3.ToString())
        };

        _list = CLValue.List(clList);

    }

    [Then(@"the list's ""(.*)"" item is a CLValue with (.*) value of (.*)")]
    public void ThenTheListsItemIsAclValueWithIValueOf(string nth, CLType type, int value) {
        WriteLine("the list's '{0}' item is a CLValue with {1} value of {2}", type, nth, value);
        
        /*
        * We can't retrieve the list data from the CLtype  
        */
        
        Assert.Fail();
        
    }

    [Given(@"a nested list is created with (.*) values of \(\((.*), (.*), (.*)\),\((.*), (.*), (.*)\)\)")]
    public void GivenANestedListIsCreatedWithUValuesOf(CLType type, int val1, int val2, int val3, int val4, int val5, int val6) {
        WriteLine("a nested list is created with {0} values of {1}, {2}, {3},{4}, {5}, {6}", type, val1, val2, val3, val4, val5, val6);
        
        CLValue[] clList1 = {
            CLValueFactory.CreateValue(type, val1.ToString()),
            CLValueFactory.CreateValue(type, val2.ToString()),
            CLValueFactory.CreateValue(type, val3.ToString())
        };
        CLValue[] clList2 = {
            CLValueFactory.CreateValue(type, val4.ToString()),
            CLValueFactory.CreateValue(type, val5.ToString()),
            CLValueFactory.CreateValue(type, val6.ToString())
        };

        CLValue[] clList = { CLValue.List(clList1), CLValue.List(clList2) };

        _list = CLValue.List(clList);

    }

    [Then(@"the ""(.*)"" nested list's ""(.*)"" item is a CLValue with (.*) value of (.*)")]
    public void ThenTheNestedListsItemIsAclValueWithUValueOf(string nth1, string nth2, CLType type, int value) {
        WriteLine("the '{0}' nested list's '{1}' item is a CLValue with {2} value of {3}", nth1, nth2, type, value);
        
        // The SDK needs to expose the CLType's values
        
        Assert.Fail();
        
    }
}
