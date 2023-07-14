using System;
using System.Collections.Generic;
using System.IO;

class CommandSETCABPOS : ICommandBase
{
    public string CmdToken { get; } = "SETCABPOS";
    public CommandType.Type Type { get; } = CommandType.Type.Command;

    CommandExpression room = new();
    CommandExpression position = new();


    public CommandSETCABPOS()
    {
    }

    
    public bool Parse(TokenConsumer tokens)
    {
        return false;
    }

    public BasicValue Execute(BasicVars vars)
    {
        return null;
    }

}