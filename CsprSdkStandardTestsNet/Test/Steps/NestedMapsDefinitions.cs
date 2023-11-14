using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

[Binding]
public class NestedMapsDefinitions {
    [Given(@"a map is created \{""(.*)"": (.*)}")]
    public void GivenAMapIsCreated(string key, int value) {
        WriteLine("a map is created {0}: {1}", key, value);
        
    }

    [Then(@"the map's key type is ""(.*)"" and the maps value type is ""(.*)""")]
    public void ThenTheMapsKeyTypeIsAndTheMapsValueTypeIs(string keyType, string valueType) {
        WriteLine("the map's key type is '{0}' and the maps value type is '{1}'", keyType, valueType);
        
    }

    [Then(@"the map's bytes are ""(.*)""")]
    public void ThenTheMapsBytesAre(string hexBytes) {
        WriteLine("the map's bytes are '{0}'", hexBytes);
        
    }

    [Given(@"that the nested map is deployed in a transfer")]
    public void GivenThatTheNestedMapIsDeployedInATransfer() {
        WriteLine("that the nested map is deployed in a transfer");
        
    }

    [Given(@"the transfer containing the nested map is successfully executed")]
    public void GivenTheTransferContainingTheNestedMapIsSuccessfullyExecuted() {
        WriteLine("the transfer containing the nested map is successfully executed");
        
    }

    [When(@"the map is read from the deploy")]
    public void WhenTheMapIsReadFromTheDeploy() {
        WriteLine("the map is read from the deploy");
        
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
        
    }

    [Then(@"the 1st nested map's key is ""(.*)"" and value is ""(.*)""")]
    public void ThenTheStNestedMapsKeyIsAndValueIs(string key, string value) {
        WriteLine("the 1st nested map's key is {0}' and value is {1}'", key, value);
    }

    [Given(@"a nested map is created  \{(.*): \{(.*): \{(.*): ""(.*)""}, (.*): \{(.*): ""(.*)""}}, (.*): \{(.*): \{(.*): ""(.*)""}, (.*): \{(.*): ""(.*)""}}}")]
    public void GivenANestedMapIsCreated(int key1, int key11, int key111, string value111, int key12, int key121, string value121, int key2, int key21, int key211, string value211, int key22, int key221, string value221) {
        WriteLine("a nested map is created  '{0}':'{1}': '{2}': '{3}', '{4}': '{5}': '{6}', '{7}': '{8}': '{9}': '{10}', '{11}': '{12}': '{13}'", 
            key1, key11, key111, value111, key12, key121, value121, key2, key21, key211, value211, key22, key221, value221 );
        
        
    }
}
