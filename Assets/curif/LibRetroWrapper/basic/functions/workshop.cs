// WORKSHOPRELOAD([cabinetName$]): Workshop-room only.
// No argument (or the same name as whatever is currently deployed): redeploys the workshop's
// cabinet from its on-disk cabinetsdb/<name> folder, without needing a test.zip.
// A different cabinetName$ (must already exist under cabinetsdb/): switches the workshop slot to
// that cabinet and persists the choice, so it's still there after an app restart.
// Returns 0 (no-op, logged) when there is no active workshop test-cabinet loader, or the named
// cabinet doesn't exist. Deploy happens within a couple seconds; test.log is rewritten on completion.
class CommandFunctionWORKSHOPRELOAD : CommandFunctionExpressionListBase
{
    public CommandFunctionWORKSHOPRELOAD(ConfigurationCommands config) : base(config)
    {
        cmdToken = "WORKSHOPRELOAD";
    }

    // Overridden (rather than using the base's exact-count Parse) because this function accepts
    // either zero or one argument - none of the existing function base classes can express that.
    public override bool Parse(TokenConsumer tokens)
    {
        if (tokens.Next() != "(")
            throw new System.Exception($"function without enclosing (): {tokens.ToString()}");
        tokens++; //consumes (

        if (tokens.Token != ")")
            exprs.Parse(tokens);

        if (tokens.Token != ")")
            throw new System.Exception($"function without enclosing () END is missing: {tokens.ToString()}");

        return true;
    }

    public override BasicValue Execute(BasicVars vars)
    {
        AGEBasicDebug.WriteConsole($"[AGE BASIC RUN {CmdToken}]");

        if (exprs.Count == 0)
            return CabinetAutoReload.RequestReload() ? BasicValue.True : BasicValue.False;

        BasicValue[] vals = exprs.ExecuteList(vars);
        FunctionHelper.ExpectedNonEmptyString(vals[0], " - cabinet name");

        return CabinetAutoReload.RequestReload(vals[0].GetString()) ? BasicValue.True : BasicValue.False;
    }
}
