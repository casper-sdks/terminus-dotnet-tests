using System.Collections.Generic;
using Casper.Network.SDK.Types;

namespace CsprSdkStandardTestsNet.Test.Utils; 

public class TransferDeploy : TransferDeployItem {
    public override byte Tag() => 5;
    
    public override string JsonPropertyName() => "Transfer";
    
    public TransferDeploy() { }
    
    public TransferDeploy(List<NamedArg> args) {
        RuntimeArgs = args;
    }
    
}