using System;
using System.Collections.Generic;
using System.IO;

class CommandFunctionROOMNAME : CommandFunctionBase
{
    public CommandFunctionROOMNAME(ConfigurationCommands config) : base(config)
    {
        cmdToken = "ROOMNAME";
    }

    public override BasicValue Execute(BasicVars vars)
    {
        if (config == null)
            throw new Exception($"AGEBasic not configured. can't access the room name, the program should be excecuted as part of a configuration controller.");
        if (config.ConfigurationController == null)
            throw new Exception($"AGEBasic no .ConfigurationController. can't access the room name, the program should be excecuted as part of a configuration controller.");
        string roomName = config?.ConfigurationController?.GetRoomName();
        if (roomName == null)
            throw new Exception($"AGEBasic can't access the room name, the program should be excecuted as part of a configuration controller.");

        return new BasicValue(roomName);
    }
}