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
public class NestedOptionSteps {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    private CLValue _option;
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that a nested Option has an inner type of Option with a type of String and a value of ""(.*)""")]
    public void GivenThatANestedOptionHasAnInnerTypeOfOptionWithATypeOfStringAndAValueOf(string value) {
        WriteLine("that a nested Option has an inner type of Option with a type of String and a value of '{0}'", value);
        
        // _option = CLValue.Option(CLValue.String(value));
        _option = CLValue.Option(CLValue.Option(CLValue.String(value)));
        
    }

    [Then(@"the inner type is Option with a type of String and a value of ""(.*)""")]
    public void ThenTheInnerTypeIsOptionWithATypeOfStringAndAValueOf(string value) {
        WriteLine("the inner type is Option with a type of String and a value of '{0}'", value);
        
        // The SDK needs to expose the CLType's values
        
        // Assert.Fail();
        
    }

    [Then(@"the bytes are ""(.*)""")]
    public void ThenTheBytesAre(string hexBytes) {
        WriteLine("the bytes are '{0}'", hexBytes);
        
        Assert.That(CLTypeUtils.GetHexValue(_option), Is.EqualTo(hexBytes).IgnoreCase);
        
    }

    [Given(@"that the nested Option is deployed in a transfer")]
    public async Task GivenThatTheNestedOptionIsDeployedInATransfer() {
        WriteLine("that the nested Option is deployed in a transfer");
        
        var runtimeArgs = new List<NamedArg>{ 
            new ("OPTION", _option)
        };

        _option = null;
        
        await DeployUtils.DeployArgs(runtimeArgs, GetCasperService());

    }

    [Given(@"the transfer containing the nested Option is successfully executed")]
    public async Task GivenTheTransferContainingTheNestedOptionIsSuccessfullyExecuted() {
        WriteLine("the transfer containing the nested Option is successfully executed");
        
        var deployResult = _contextMap.Get<RpcResponse<PutDeployResult>>(StepConstants.DEPLOY_RESULT);

        RpcResponse<GetDeployResult> deploy = await GetCasperService().GetDeploy(
            deployResult.Parse().DeployHash, 
            true,
            new CancellationTokenSource(TimeSpan.FromSeconds(300)).Token);
       
        Assert.That(deploy!.Parse().ExecutionResults[0].IsSuccess);

        _contextMap.Add(StepConstants.DEPLOY, deploy);
        
    }
    
    
    [When(@"the Option is read from the deploy")]
    public void WhenTheOptionIsReadFromTheDeploy() {
        WriteLine("the Option is read from the deploy");

        var deploy = _contextMap.Get<RpcResponse<GetDeployResult>>(StepConstants.DEPLOY).Parse();

        _option = deploy.Deploy.Session.RuntimeArgs.Find(n => n.Name.Equals("OPTION")).Value;
        
        Assert.That(_option, Is.Not.Null);
        
    }

    [Given(@"that a nested Option has an inner type of List with a type of (.*) and a value of \((.*), (.*), (.*)\)")]
    public void GivenThatANestedOptionHasAnInnerTypeOfListWithATypeOfUAndAValueOf(CLType type, string val1, string val2, string val3) {
        WriteLine("that a nested Option has an inner type of List with a type of {0} and a value of {1}, {2}, {3}");
        
        CLValue[] clList = {
            CLValueFactory.CreateValue(type, val1),
            CLValueFactory.CreateValue(type, val2),
            CLValueFactory.CreateValue(type, val3)
        };
        
        _option = CLValue.Option(CLValue.List(clList));
        
        Assert.That(_option, Is.Not.Null);
        
    }

    [Given(@"the bytes are ""(.*)""")]
    public void GivenTheBytesAre(string hexBytes) {
        ThenTheBytesAre(hexBytes);
    }

    [Given(@"that a nested Option has an inner type of Tuple2 with a type of ""(.*)"" values of ""(.*)""")]
    public void GivenThatANestedOptionHasAnInnerTypeOfTupleWithATypeOfValuesOf(string types, string values) {
        WriteLine("that a nested Option has an inner type of Tuple2 with a type of {0} values of {1}", types, values);
     
        var val = values.Replace("(", "").Replace(")", "").Split(",");
        var type = types.Replace("(", "").Replace(")", "").Split(",");
        
        var tuple = CLValue.Tuple2(getType(type[0], val[0]), getType(type[1], val[1]));

        _option = CLValue.Option(tuple);
        
        Assert.That(_option, Is.Not.Null);

    }

    [Then(@"the inner type is Tuple2 with a type of ""(.*)"" and a value of ""(.*)""")]
    public void ThenTheInnerTypeIsTupleWithATypeOfAndAValueOf(string types, string values) {
        WriteLine("the inner type is Tuple2 with a type of {0} and a value of {1}", types, values);
        
        // The SDK needs to expose the CLType's values
        
        Assert.Fail();
        
    }

    [Given(@"that a nested Option has an inner type of Map with a type of ""(.*)"" values of ""(.*)""")]
    public void GivenThatANestedOptionHasAnInnerTypeOfMapWithATypeOfValuesOfOne(string types, string values) {
        WriteLine("that a nested Option has an inner type of Map with a type of {0} values of {1}", types, values);
        
        var val = values.Replace("{", "").Replace("}", "")
            .Replace(" ", "").Replace("\"", "").Split(":");
        var type = types.Replace("(", "").Replace(")", "").Split(",");
        
        var innerMap = new Dictionary<CLValue, CLValue>
            {{getType(type[0], val[0]), getType(type[1], val[1])}};

        var map = CLValue.Map(innerMap);

        _option = CLValue.Option(map);
        
        Assert.That(_option, Is.Not.Null);
        
    }

    [Then(@"the inner type is Map with a type of ""(.*)"" and a value of ""(.*)""")]
    public void ThenTheInnerTypeIsMapWithATypeOfAndAValueOfOne(string types, string values) {
        WriteLine("the inner type is Map with a type of {0} and a value of {1}", types, values);
        
        // The SDK needs to expose the CLType's values
        
        Assert.Fail();
        
    }

    [Given(@"that a nested Option has an inner type of Any with a value of ""(.*)""")]
    public void GivenThatANestedOptionHasAnInnerTypeOfAnyWithAValueOf(string value) {
        WriteLine("that a nested Option has an inner type of Any with a value of {0}", value);

        // The SDK needs implement the CLType Any
        
        Assert.Fail();
      
    }

    [Then(@"the inner type is Any with a value of ""(.*)""")]
    public void ThenTheInnerTypeIsAnyWithAValueOf(string value) {
        WriteLine("the inner type is Any with a value of {0}", value);
        
        // The SDK needs to expose the CLType's values
        
        Assert.Fail();
        
    }

    [When(@"the bytes are ""(.*)""")]
    public void WhenTheBytesAre(string hexBytes) {
        ThenTheBytesAre(hexBytes);
    }

    [Given(@"the list's length is (.*)")]
    public void GivenTheListsLengthIs(int length) {
        WriteLine("the list's length is {0}", length);
        
        // The SDK needs to expose the CLType's values
        
        Assert.Fail();
        
    }

    [Given(@"the list's ""(.*)"" item is a CLValue with (.*) value of (.*)")]
    public void GivenTheListsItemIsAclValueWithUValueOf(string item, string type, string value) {
        WriteLine("the list's {0} item is a CLValue with {1} value of {2}", item, type, value);
        
        // The SDK needs to expose the CLType's values
        
        Assert.Fail();

    }

    [When(@"the list's length is (.*)")]
    public void WhenTheListsLengthIs(int length) {
        GivenTheListsLengthIs(length);
    }

    [When(@"the list's ""(.*)"" item is a CLValue with (.*) value of (.*)")]
    public void WhenTheListsItemIsAclValueWithUValueOf(string item, string type, string value) {
        GivenTheListsItemIsAclValueWithUValueOf(item, type, value);
    }

    private static CLValue getType(string type, string value) {
        return type switch {
            "String" => CLValue.String(value),
            "U32" => CLValue.U32((uint)long.Parse(value)),
            _ => throw new ArgumentException("Not implemented conversion for type " + type)
        };
    }
    
}
