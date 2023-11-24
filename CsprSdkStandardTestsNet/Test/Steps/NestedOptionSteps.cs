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
    
    [Given(@"that a nested Option has an inner type of Option with a type of String and a value of ""(.*)""")]
    public void GivenThatANestedOptionHasAnInnerTypeOfOptionWithATypeOfStringAndAValueOf(string value) {
        WriteLine("that a nested Option has an inner type of Option with a type of String and a value of '{0}'", value);
        
    }

    [Then(@"the inner type is Option with a type of String and a value of ""(.*)""")]
    public void ThenTheInnerTypeIsOptionWithATypeOfStringAndAValueOf(string value) {
        WriteLine("the inner type is Option with a type of String and a value of '{0}'", value);
        
    }

    [Then(@"the bytes are ""(.*)""")]
    public void ThenTheBytesAre(string hexBytes) {
        WriteLine("the bytes are '{0}'", hexBytes);
        
    }

    [Given(@"that the nested Option is deployed in a transfer")]
    public void GivenThatTheNestedOptionIsDeployedInATransfer() {
        WriteLine("that the nested Option is deployed in a transfer");
        
    }

    [Given(@"the transfer containing the nested Option is successfully executed")]
    public void GivenTheTransferContainingTheNestedOptionIsSuccessfullyExecuted() {
        WriteLine("the transfer containing the nested Option is successfully executed");
        
    }

    [Given(@"that a nested Option has an inner type of List with a type of (.*) and a value of \((.*), (.*), (.*)\)")]
    public void GivenThatANestedOptionHasAnInnerTypeOfListWithATypeOfUAndAValueOf(CLType type, int val1, int val2, int val3) {
        WriteLine("that a nested Option has an inner type of List with a type of {0} and a value of {1}, {2}, {3}");
        
    }

    [Given(@"the bytes are ""(.*)""")]
    public void GivenTheBytesAre(string hexBytes) {
        ThenTheBytesAre(hexBytes);

    }

    [Given(@"that a nested Option has an inner type of Tuple2 with a type of ""(.*)"" values of ""(.*)""")]
    public void GivenThatANestedOptionHasAnInnerTypeOfTupleWithATypeOfValuesOf(string types, string values) {
        WriteLine("that a nested Option has an inner type of Tuple2 with a type of {0} values of {1}", types, values);
        
    }

    [Then(@"the inner type is Tuple2 with a type of ""(.*)"" and a value of ""(.*)""")]
    public void ThenTheInnerTypeIsTupleWithATypeOfAndAValueOf(string types, string values) {
        WriteLine("the inner type is Tuple2 with a type of {0} and a value of {1}", types, values);
        
    }

    [Given(@"that a nested Option has an inner type of Map with a type of ""(.*)"" values of ""(.*)""")]
    public void GivenThatANestedOptionHasAnInnerTypeOfMapWithATypeOfValuesOfOne(string types, string values) {
        WriteLine("that a nested Option has an inner type of Map with a type of {0} values of {1}", types, values);
        
    }

    [Then(@"the inner type is Map with a type of ""(.*)"" and a value of ""(.*)""")]
    public void ThenTheInnerTypeIsMapWithATypeOfAndAValueOfOne(string types, string values) {
        WriteLine("the inner type is Map with a type of {0} and a value of {1}", types, values);
        
    }

    [Given(@"that a nested Option has an inner type of Any with a value of ""(.*)""")]
    public void GivenThatANestedOptionHasAnInnerTypeOfAnyWithAValueOf(string value) {
        WriteLine("that a nested Option has an inner type of Any with a value of {0}", value);
    }

    [Then(@"the inner type is Any with a value of ""(.*)""")]
    public void ThenTheInnerTypeIsAnyWithAValueOf(string value) {
        WriteLine("the inner type is Any with a value of {0}", value);
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
}
