// WORKSHOPRELOAD(): redeploys the workshop's test cabinet from the on-disk
// cabinetsdb/test folder, without needing a new test.zip. Workshop-room only:
// returns 0 (no-op, logged) when there is no active workshop test-cabinet loader.
// Deploy happens within a couple seconds; test.log is rewritten on completion.
class CommandFunctionWORKSHOPRELOAD : CommandFunctionNoExpressionBase
{
    public CommandFunctionWORKSHOPRELOAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "WORKSHOPRELOAD";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");
        return CabinetAutoReload.RequestReload() ? BasicValue.True : BasicValue.False;
    }
}
