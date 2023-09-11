using System.IO;
using Casper.Network.SDK.Types;
using TechTalk.SpecFlow;
using static System.Console;

namespace CsprSdkStandardTestsNet.Test.Steps;

[Binding]
public class DeployGeneratedKeys {
    
    [Given(@"that a ""(.*)"" sender key is generated")]
    public void GivenThatASenderKeyIsGenerated(string algo) {
        WriteLine("that a {0} sender key is generated", algo);

        // var keyPair = KeyPair.CreateNew(KeyAlgo.ED25519);
        // var pk = keyPair.PublicKey;

        // var gen = new Ed25519KeyPairGenerator();
        // gen.Init(new KeyGenerationParameters(new SecureRandom(), 255));
        // var newKey = gen.GenerateKeyPair();
        // var publicKey = (Ed25519PublicKeyParameters)newKey.Public;
        //
        // var pk = PublicKey.FromRawBytes(publicKey.GetEncoded(), KeyAlgo.ED25519);
        // var sk = newKey.Private;


        var keyPair = KeyPair.CreateNew(KeyAlgo.ED25519);
        var tmpfile = Path.GetTempFileName();
        File.Delete(tmpfile);
            
        keyPair.WriteToPem(tmpfile);

        var keyPair2 = KeyPair.FromPem(tmpfile);
        

    }

    [Then(@"fund the account from the faucet user with a transfer amount of (.*) and a payment amount of (.*)")]
    public void ThenFundTheAccountFromTheFaucetUserWithATransferAmountOfAndAPaymentAmountOf(long transferAmount, long paymentAmount) {
        WriteLine("fund the account from the faucet user with a transfer amount of {0} and a payment amount of {1}", transferAmount, paymentAmount);
        
    }

    [Given(@"that a ""(.*)"" receiver key is generated")]
    public void GivenThatAReceiverKeyIsGenerated(string algo) {
        WriteLine("that a {0} receiver key is generated", algo);
        
    }

    [Then(@"transfer to the receiver account the transfer amount of (.*) and the payment amount of (.*)")]
    public void ThenTransferToTheReceiverAccountTheTransferAmountOfAndThePaymentAmountOf(long transferAmount, long paymentAmount) {
        WriteLine("transfer to the receiver account the transfer amount of {0} and the payment amount of {1}", transferAmount, paymentAmount);
        
    }
    
    
    [Then(@"the transfer approvals signer contains the ""(.*)"" algo")]
    public void ThenTheTransferApprovalsSignerContainsTheAlgo(string algo) {
        WriteLine("the transfer approvals signer contains the {0} algo", algo);
        
    }
}