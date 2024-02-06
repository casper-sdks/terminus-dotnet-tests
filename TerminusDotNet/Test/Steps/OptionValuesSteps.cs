using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.JsonRpc;
using Casper.Network.SDK.JsonRpc.ResultTypes;
using Casper.Network.SDK.Types;
using TerminusDotNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace TerminusDotNet.Test.Steps;

[Binding]
public class OptionValuesSteps {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    private CLValue _option;
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that an Option value has an empty value")]
    public void GivenThatAnOptionValueHasAnEmptyValue() {
       WriteLine("that an Option value has an empty value");

       //Not possible to test in the SDK
       
    }

    [Then(@"the Option value is not present")]
    public void ThenTheOptionValueIsNotPresent() {
        WriteLine("the Option value is not present");

        //Not possible to test in the SDK

    }

    [Given(@"an Option value contains a ""(.*)"" value of ""(.*)""")]
    public void GivenAnOptionValueContainsAValueOf(CLType type, string value) {
        WriteLine("an Option value contains a {0} value of {1}", type, value);
        
        _option = CLValue.Option(CLValueFactory.CreateValue(type, value));
        
        Assert.That(_option, Is.Not.Null);
        
    }

    [Then(@"the Option value's bytes are ""(.*)""")]
    public void ThenTheOptionValuesBytesAre(string hexBytes) {
        WriteLine("the Option value's bytes are {0}", hexBytes);
        
        Assert.That(CLTypeUtils.GetHexValue(_option), Is.EqualTo(hexBytes).IgnoreCase);
    }

    [Given(@"that the Option value is deployed in a transfer as a named argument")]
    public async Task GivenThatTheOptionValueIsDeployedInATransferAsANamedArgument() {
        WriteLine("that the Option value is deployed in a transfer as a named argument");
        
        var runtimeArgs = new List<NamedArg>{ 
            new ("OPTION", _option)
        };

        _option = null;
        
        await DeployUtils.DeployArgs(runtimeArgs, GetCasperService());
        
    }

    [Given(@"the transfer containing the Option value is successfully executed")]
    public async Task GivenTheTransferContainingTheOptionValueIsSuccessfullyExecuted() {
        WriteLine("the transfer containing the Option value is successfully executed");
        
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

    [Then(@"the type of the Option is ""(.*)"" with a value of ""(.*)""")]
    public void ThenTheTypeOfTheOptionIsWithAValueOf(string type, string value) {
        WriteLine("the type of the Option is {0} with a value of {1}", type, value);
        
        // The SDK needs to expose the CLType's values
        
        Assert.Fail();
        
    }
}
