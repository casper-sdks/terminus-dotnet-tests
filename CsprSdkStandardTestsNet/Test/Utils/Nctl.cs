using System;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CsprSdkStandardTestsNet.Test.Utils;

public class Nctl
{
    private readonly string _dockerName;

    public Nctl(string dockerName)
    {
        _dockerName = dockerName;
    }
    
    public JsonNode GetChainBlock(string blockHash) {
        return Execute("view_chain_block.sh", "block=" + blockHash);
    }
    
    private JsonNode Execute(string shellCommand, string parameters)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "docker",
            Arguments =
                $"exec -t {_dockerName}  /bin/bash -c \"source casper-node/utils/nctl/sh/views/{shellCommand} {parameters ?? ""}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        
        var proc = Process.Start(startInfo);
        ArgumentNullException.ThrowIfNull(proc);

        var output = proc.StandardOutput.ReadToEnd();
        
        return JsonNode.Parse(ReplaceAnsiConsoleCodes(output));

    }
    
    private static string ReplaceAnsiConsoleCodes(string response) {
        //remove any console colour ANSI info
        return Regex.Replace(response, "\u001B\\[[;\\d]*m", "");
    }
   

}