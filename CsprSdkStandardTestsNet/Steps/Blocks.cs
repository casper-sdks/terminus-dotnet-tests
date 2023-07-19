using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Steps;

[Binding]
public class Blocks
{
    [Given(@"that the latest block is requested via the sdk")]
    public void GivenThatTheLatestBlockIsRequestedViaTheSdk()
    {
        WriteLine("that the latest block is requested via the sdk");
    }

    [Then(@"request the latest block via the test node")]
    public void ThenRequestTheLatestBlockViaTheTestNode()
    {
        WriteLine("request the latest block via the test node");
    }

    [Then(@"the body of the returned block is equal to the body of the returned test node block")]
    public void ThenTheBodyOfTheReturnedBlockIsEqualToTheBodyOfTheReturnedTestNodeBlock()
    {
        WriteLine("the body of the returned block is equal to the body of the returned test node block");
    }

    [Then(@"the hash of the returned block is equal to the hash of the returned test node block")]
    public void ThenTheHashOfTheReturnedBlockIsEqualToTheHashOfTheReturnedTestNodeBlock()
    {
        WriteLine("the hash of the returned block is equal to the hash of the returned test node block");
    }

    [Then(@"the header of the returned block is equal to the header of the returned test node block")]
    public void ThenTheHeaderOfTheReturnedBlockIsEqualToTheHeaderOfTheReturnedTestNodeBlock()
    {
        WriteLine("the header of the returned block is equal to the header of the returned test node block");
    }

    [Then(@"the proofs of the returned block are equal to the proofs of the returned test node block")]
    public void ThenTheProofsOfTheReturnedBlockAreEqualToTheProofsOfTheReturnedTestNodeBlock()
    {
        WriteLine("the proofs of the returned block are equal to the proofs of the returned test node block");
    }
}