using System.Collections.Generic;
using Casper.Network.SDK.Types;

namespace TerminusDotNet.Test.Utils; 

/**
 * Overrides the SDK's TranferDeploy to allow multiple RunTimeArguments
 * Needed when testing CLTypes 
 */
public class TransferDeploy : TransferDeployItem {
    public override byte Tag() => 5;
    
    public override string JsonPropertyName() => "Transfer";

    public TransferDeploy(List<NamedArg> args) {
        RuntimeArgs = args;
    }
    
}
