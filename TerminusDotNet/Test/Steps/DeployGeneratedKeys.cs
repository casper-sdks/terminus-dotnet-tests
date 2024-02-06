using System;
using System.Numerics;
using System.Threading.Tasks;
using Casper.Network.SDK;
using Casper.Network.SDK.SSE;
using Casper.Network.SDK.Types;
using TerminusDotNet.Test.Utils;
using NUnit.Framework;
using TechTalk.SpecFlow;
using static System.Console;

namespace TerminusDotNet.Test.Steps;

/**
 * Steps to test key generation
 */

[Binding]
public class DeployGeneratedKeys {
    
    private readonly ContextMap _contextMap = ContextMap.Instance;    
    
    private static NetCasperClient GetCasperService() {
        return CasperClientProvider.GetInstance().CasperService;
    }
    
    [Given(@"that a ""(.*)"" sender key is generated")]
    public void GivenThatASenderKeyIsGenerated(string algo) {
        WriteLine("that a {0} sender key is generated", algo);

        var keyPair = KeyPair.CreateNew(algo.Equals("Ed25519") ? KeyAlgo.ED25519 : KeyAlgo.SECP256K1);

        Assert.That(keyPair, Is.Not.Null);
        Assert.That(keyPair.PublicKey, Is.Not.Null);
        
        _contextMap.Add(StepConstants.SENDER_KEY_PAIR, keyPair);

    }

    [Then(@"fund the account from the faucet user with a transfer amount of (.*) and a payment amount of (.*)")]
    public async Task ThenFundTheAccountFromTheFaucetUserWithATransferAmountOfAndAPaymentAmountOf(string transferAmount, string paymentAmount) {
        WriteLine("fund the account from the faucet user with a transfer amount of {0} and a payment amount of {1}", transferAmount, paymentAmount);
        
        var faucetPem = AssetUtils.GetFaucetAsset(1, "secret_key.pem");
        Assert.That(faucetPem, Is.Not.Null);
        
        var faucetKey = KeyPair.FromPem(faucetPem);

        Assert.That(faucetKey, Is.Not.Null);
        Assert.That(faucetKey.PublicKey, Is.Not.Null);

        _contextMap.Add(StepConstants.TRANSFER_AMOUNT, BigInteger.Parse(transferAmount));
        _contextMap.Add(StepConstants.PAYMENT_AMOUNT, BigInteger.Parse(paymentAmount));

        await DoDeploy(faucetKey, _contextMap.Get<KeyPair>(StepConstants.SENDER_KEY_PAIR));

    }

    [Given(@"that a ""(.*)"" receiver key is generated")]
    public void GivenThatAReceiverKeyIsGenerated(string algo) {
        WriteLine("that a {0} receiver key is generated", algo);

        var keyPair = KeyPair.CreateNew(algo.Equals("Ed25519") ? KeyAlgo.ED25519 : KeyAlgo.SECP256K1);

        Assert.That(keyPair, Is.Not.Null);
        Assert.That(keyPair.PublicKey, Is.Not.Null);

        byte[] msg = "this is the receiver"u8.ToArray();
        byte[] signature = keyPair.Sign(msg);
        
        Assert.That(keyPair.PublicKey.VerifySignature(msg, signature), Is.True);
        
        _contextMap.Add(StepConstants.RECEIVER_KEY_PAIR, keyPair);

    }

    [Then(@"transfer to the receiver account the transfer amount of (.*) and the payment amount of (.*)")]
    public async Task ThenTransferToTheReceiverAccountTheTransferAmountOfAndThePaymentAmountOf(string transferAmount, string paymentAmount) {
        WriteLine("transfer to the receiver account the transfer amount of {0} and the payment amount of {1}", transferAmount, paymentAmount);
        
        _contextMap.Add(StepConstants.TRANSFER_AMOUNT, BigInteger.Parse(transferAmount));
        _contextMap.Add(StepConstants.PAYMENT_AMOUNT, BigInteger.Parse(paymentAmount));

        await DoDeploy(_contextMap.Get<KeyPair>(StepConstants.SENDER_KEY_PAIR), _contextMap.Get<KeyPair>(StepConstants.RECEIVER_KEY_PAIR));
        
    }
    
    
    [Then(@"the transfer approvals signer contains the ""(.*)"" algo")]
    public async Task ThenTheTransferApprovalsSignerContainsTheAlgo(string algo) {
        WriteLine("the transfer approvals signer contains the {0} algo", algo);
        
        var matchingBlockHash =  _contextMap.Get<BlockAdded>(StepConstants.LAST_BLOCK_ADDED);
        
        var block = await GetCasperService().GetBlock(matchingBlockHash.BlockHash);

        Assert.That(block, Is.Not.Null);
        Assert.That(block.Parse().Block.Body.Proposer.PublicKey.KeyAlgorithm.ToString(), Is.EqualTo(algo.ToUpper()));

    }
    
    private async Task DoDeploy(KeyPair senderKeys, KeyPair receiverKeys) {
        
        Deploy deploy = DeployTemplates.StandardTransfer(
            senderKeys.PublicKey,
            receiverKeys.PublicKey,
            _contextMap.Get<BigInteger>(StepConstants.TRANSFER_AMOUNT),
            _contextMap.Get<BigInteger>(StepConstants.PAYMENT_AMOUNT),
            "casper-net-1",
            null,
            1,
            (ulong)TimeSpan.FromMinutes(30).TotalMilliseconds);
        
        deploy.Sign(senderKeys);
        
        var putResponse = await GetCasperService().PutDeploy(deploy);
        
        _contextMap.Add(StepConstants.DEPLOY_RESULT, putResponse);

    }
    
}
