using Casper.Network.SDK;
using CsprSdkStandardTestsNet.Test.Utils;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

[Binding]
public class NestedListsStepDefinitions {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;
    
    [BeforeScenario()]
    private void SetUp() {
        _contextMap.Clear();
    }
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"a list is created with ""(.*)"" values of \(""(.*)"", ""(.*)"", ""(.*)""\)")]
    public void GivenAListIsCreatedWithValuesOf(string type, string val1, string val2, string val3) {
        WriteLine("a list is created with '{0}' values of '{1}', '{2}', '{3}'", true, val1, val2, val3);
        
    }

    [Then(@"the list's bytes are ""(.*)""")]
    public void ThenTheListsBytesAre(string hexBytes) {
        WriteLine("the list's bytes are '{0}'", hexBytes);
        
    }

    [Then(@"the list's length is (.*)")]
    public void ThenTheListsLengthIs(int length) {
        WriteLine("the list's length is {0}", length);
        
    }

    [Then(@"the list's ""(.*)"" item is a CLValue with ""(.*)"" value of ""(.*)""")]
    public void ThenTheListsItemIsAclValueWithValueOf(string nth, string type, string value) {
        WriteLine("the list's '{0}' item is a CLValue with '{1}' value of '{2}'", nth, type, value);
        
    }

    [Given(@"that the list is deployed in a transfer")]
    public void GivenThatTheListIsDeployedInATransfer() {
        WriteLine("that the list is deployed in a transfer");
        
    }

    [Given(@"the transfer containing the list is successfully executed")]
    public void GivenTheTransferContainingTheListIsSuccessfullyExecuted() {
        WriteLine("the transfer containing the list is successfully executed");
        
    }

    [When(@"the list is read from the deploy")]
    public void WhenTheListIsReadFromTheDeploy() {
        WriteLine("the list is read from the deploy");
        
    }

    [Given(@"a list is created with (.*) values of \((.*), (.*), (.*)\)")]
    public void GivenAListIsCreatedWithIValuesOf(int type, int val1, int val2, int val3) {
        WriteLine("a list is created with {0} values of {1}, {2}, {3}", type, val1, val2, val3);
        
    }

    [Then(@"the list's ""(.*)"" item is a CLValue with (.*) value of (.*)")]
    public void ThenTheListsItemIsAclValueWithIValueOf(string nth, int type, int value) {
        WriteLine("the list's '{0}' item is a CLValue with {1} value of {2}", type, nth, value);
        
    }

    [Given(@"a nested list is created with (.*) values of \(\((.*), (.*), (.*)\),\((.*), (.*), (.*)\)\)")]
    public void GivenANestedListIsCreatedWithUValuesOf(string type, int val1, int val2, int val3, int val4, int val5, int val6) {
        WriteLine("a nested list is created with {0} values of {1}, {2}, {3},{4}, {5}, {6}", type, val1, val2, val3, val4, val5, val6);
        
    }

    [Then(@"the ""(.*)"" nested list's ""(.*)"" item is a CLValue with (.*) value of (.*)")]
    public void ThenTheNestedListsItemIsAclValueWithUValueOf(string nth1, string nth2, string type, int value) {
        WriteLine("the '{0}' nested list's '{1}' item is a CLValue with {2} value of {3}", nth1, nth2, type, value);
        
    }
}
