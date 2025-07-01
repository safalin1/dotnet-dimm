using CommandLine;

return Parser
    .Default
    .ParseArguments<SendGameDataOptions>(args)
    .MapResult(
        _ => 255,
        _ => 1);