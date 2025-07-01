using System.Net;
using CommandLine;

[Verb("send", HelpText = "Send a game to the netdimm")]
public class SendGameDataOptions
{
    [Option('f', "file", Required = true, HelpText = "Game file to send")]
    public string? GameFile { get; set; }
    
    [Option('d', "ip", Required = true, HelpText = "IP Address of the netdimm you want to send the game to")]
    public IPAddress? IPAddress { get; set; }
}