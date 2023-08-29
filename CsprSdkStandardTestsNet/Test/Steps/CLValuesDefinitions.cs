using System;
using System.Collections.Generic;
using Casper.Network.SDK.Types;
using CsprSdkStandardTestsNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

[Binding]
public class CLValuesDefinitions {
    
    
    private readonly CLValueFactory _cLValueFactory = new();
    private readonly Dictionary<string, object> _contextMap = new();
    
    [Given(@"that a CL value of type ""(.*)"" has a value of ""(.*)""")]
    public void GivenThatAclValueOfTypeHasAValueOf(CLType typeName, string strValue) {
        WriteLine("that a CL value of type {0} has a value of {1}", typeName, strValue);

        _contextMap["clValue"] = _cLValueFactory.CreateValue(typeName, strValue);;

    }

    [Then(@"it's bytes will be ""(.*)""")]
    public void ThenItsBytesWillBe(string hexBytes) {
        WriteLine("it's bytes will be {0}", hexBytes);
        
        CLValue clValue = (CLValue)_contextMap["clValue"];
        
        Assert.That(hexBytes.ToUpper(), Is.EqualTo(GetHexValue(clValue)));    

    }

    [Given(@"that the CL complex value of type ""(.*)"" with an internal types of ""(.*)"" values of ""(.*)""")]
    public void GivenThatTheClComplexValueOfTypeWithAnInternalTypesOfValuesOf(CLType type, string innerTypes, string innerValues) {
        WriteLine("that the CL complex value of type {0} with an internal types of {1} values of {2}", type, innerTypes, innerValues);
        
        var types = GetInnerClTypeData(innerTypes);
        var values = new List<string>(innerValues.Split(","));
        
        var complexValue = _cLValueFactory.CreateComplexValue(type, types, values);

        _contextMap["clValue"] = complexValue;

    }

    [When(@"the values are added as arguments to a deploy")]
    public void WhenTheValuesAreAddedAsArgumentsToADeploy() {
        WriteLine("the values are added as arguments to a deploy");
    }

    [Then(@"the deploys NamedArgument ""(.*)"" has a value of ""(.*)"" and bytes of ""(.*)""")]
    public void ThenTheDeploysNamedArgumentHasAValueOfAndBytesOf(string name, string strValue, string hexBytes) {
        WriteLine("the deploys NamedArgument {0} has a value of {1} and bytes of {2}", name, strValue, hexBytes);
    }

    [Then(@"the deploys NamedArgument Complex value ""(.*)"" has internal types of ""(.*)"" and values of ""(.*)"" and bytes of ""(.*)""")]
    public void ThenTheDeploysNamedArgumentComplexValueHasInternalTypesOfAndValuesOfAndBytesOf(string name, string types, string values, string bytes) {
        WriteLine("the deploys NamedArgument Complex value {0} has internal types of {1} and values of {2} and bytes of {3}");
    }

    [When(@"the deploy is put on chain")]
    public void WhenTheDeployIsPutOnChain() {
        WriteLine("the deploy is put on chain");
    }

    [Then(@"the deploy has successfully executed")]
    public void ThenTheDeployHasSuccessfullyExecuted() {
        WriteLine("the deploy has successfully executed");
    }

    [When(@"the deploy is obtained from the node")]
    public void WhenTheDeployIsObtainedFromTheNode() {
        WriteLine("the deploy is obtained from the node");
    }

    [Then(@"the deploy response contains a valid deploy hash of length (.*) and an API version ""(.*)""")]
    public void ThenTheDeployResponseContainsAValidDeployHashOfLengthAndAnApiVersion(int hashLength, string apiVersion) {
        WriteLine("the deploy response contains a valid deploy hash of length {0} and an API version {1}", hashLength, apiVersion);
    }
    
    private string GetHexValue(CLValue clValue) {

        var clValueHex = BitConverter.ToString(clValue.Bytes).Replace("-", "");
        
        if (clValue.TypeInfo.Type.Equals(CLType.Key)) {
            clValueHex = clValueHex[2..];
        }

        return clValueHex;

    }
    
    private List<CLType> GetInnerClTypeData(string innerTypes) {

        List<CLType> types = new List<CLType>();
        
        var list = new List<string>(innerTypes.Split(","));
        
        foreach (var s in list) {
            types!.Add((CLType)Enum.Parse(typeof(CLType), s, true));
        }

        return types;

    }
    
    
    
    
}
