using System;
using System.Collections.Generic;

public static class Commands
{
    private static Dictionary<string, Type> commands = new();
    private static Dictionary<string, Type> functions = new();

    static Commands()
    {
        commands["REM"] = typeof(CommandREM);
        commands["'"] = typeof(CommandREM2);
        commands["LET"] = typeof(CommandLET);
        commands["LETS"] = typeof(CommandLETS);
        commands["DIM"] = typeof(CommandDIM);
        commands["GOTO"] = typeof(CommandGOTO);
        commands["IF"] = typeof(CommandIFTHEN);
        commands["END"] = typeof(CommandEND);
        commands["SHUTDOWN"] = typeof(CommandSHUTDOWN);
        commands["GOSUB"] = typeof(CommandGOSUB);
        commands["RETURN"] = typeof(CommandRETURN);
        commands["CALL"] = typeof(CommandCALL);
        commands["FOR"] = typeof(CommandFORTO);
        commands["NEXT"] = typeof(CommandNEXT);
        commands["SLEEP"] = typeof(CommandSLEEP);
        commands["RUN"] = typeof(CommandRUN);

        //screen
        commands["CLS"] = typeof(CommandCLS);
        commands["PRINT"] = typeof(CommandPRINT);
        commands["PRINTLN"] = typeof(CommandPRINTLN);
        commands["SHOW"] = typeof(CommandSHOW);
        commands["FGCOLOR"] = typeof(CommandFGCOLOR);
        commands["BGCOLOR"] = typeof(CommandBGCOLOR);
        commands["RESETCOLOR"] = typeof(CommandRESETCOLOR);
        commands["SETCOLORSPACE"] = typeof(CommandSETCOLORSPACE);
        commands["INVERTCOLOR"] = typeof(CommandINVERTCOLOR);
        commands["PRINTCENTERED"] = typeof(CommandPRINTCENTERED);
        commands["SCROLL"] = typeof(CommandSCROLL);
        commands["SCROLLRECT"] = typeof(CommandSCROLLRECT);
        
        //sprites
        commands["SPRITELOAD"] = typeof(CommandSPRITELOAD);
        commands["SPRITE"] = typeof(CommandSPRITE);
        commands["SPRITEREMOVE"] = typeof(CommandSPRITEREMOVE);
        commands["SPRITEDISABLE"] = typeof(CommandSPRITEDISABLE);
        commands["SPRITEENABLE"] = typeof(CommandSPRITEENABLE);

        //functions ------

        //math
        functions["ABS"] = typeof(CommandFunctionABS);
        functions["MAX"] = typeof(CommandFunctionMAX);
        functions["MIN"] = typeof(CommandFunctionMIN);
        functions["RND"] = typeof(CommandFunctionRND);
        functions["COS"] = typeof(CommandFunctionCOS);
        functions["SIN"] = typeof(CommandFunctionSIN);
        functions["TAN"] = typeof(CommandFunctionTAN);
        functions["MOD"] = typeof(CommandFunctionMOD);
        functions["INT"] = typeof(CommandFunctionINT);
        functions["VAL"] = typeof(CommandFunctionVAL);
        functions["NOT"] = typeof(CommandFunctionNOT);
        functions["AND"] = typeof(CommandFunctionAND);
        functions["OR"] = typeof(CommandFunctionOR);
        functions["IIF"] = typeof(CommandFunctionIIF);
        functions["HEXTODEC"] = typeof(CommandFunctionHEXTODEC);

        // strings
        functions["LEN"] = typeof(CommandFunctionLEN);
        functions["UCASE"] = typeof(CommandFunctionUCASE);
        functions["LCASE"] = typeof(CommandFunctionLCASE);
        functions["SUBSTR"] = typeof(CommandFunctionSUBSTR);
        functions["RTRIM"] = typeof(CommandFunctionRTRIM);
        functions["LTRIM"] = typeof(CommandFunctionLTRIM);
        functions["TRIM"] = typeof(CommandFunctionTRIM);
        functions["STR"] = typeof(CommandFunctionSTR);
        functions["GETMEMBER"] = typeof(CommandFunctionGETMEMBER);
        functions["COUNTMEMBERS"] = typeof(CommandFunctionCOUNTMEMBERS);
        functions["ISMEMBER"] = typeof(CommandFunctionISMEMBER);
        functions["INDEXMEMBER"] = typeof(CommandFunctionINDEXMEMBER);
        functions["REMOVEMEMBER"] = typeof(CommandFunctionREMOVEMEMBER);
        functions["ADDMEMBER"] = typeof(CommandFunctionADDMEMBER);
        functions["STRINGMATCH"] = typeof(CommandFunctionSTRINGMATCH);
        functions["ARRAY"] = typeof(CommandFunctionARRAY);
        functions["SORT"] = typeof(CommandFunctionSORT);
        
        //files
        functions["GETFILES"] = typeof(CommandFunctionGETFILES);
        functions["GETFILESARRAY"] = typeof(CommandFunctionGETFILESARRAY);
        functions["READM3UARRAY"] = typeof(CommandFunctionREADM3UARRAY);
        functions["FILEEXISTS"] = typeof(CommandFunctionFILEEXISTS);
        functions["COMBINEPATH"] = typeof(CommandFunctionCOMBINEPATH);
        functions["CONFIGPATH"] = typeof(CommandFunctionCONFIGPATH);
        functions["AGEBASICPATH"] = typeof(CommandFunctionAGEBASICPATH);
        functions["CABINETSDBPATH"] = typeof(CommandFunctionCABINETSDBPATH);
        functions["CABINETSPATH"] = typeof(CommandFunctionCABINETSPATH);
        functions["CABINETPATH"] = typeof(CommandFunctionCABINETPATH);
        functions["ROOTPATH"] = typeof(CommandFunctionROOTPATH);
        functions["MUSICPATH"] = typeof(CommandFunctionMUSICPATH);
        functions["DEBUGPATH"] = typeof(CommandFunctionDEBUGPATH);
        functions["FILEOPEN"] = typeof(CommandFunctionFILEOPEN);
        functions["FILEREAD"] = typeof(CommandFunctionFILEREAD);
        functions["FILECLOSE"] = typeof(CommandFunctionFILECLOSE);
        functions["FILEEOF"] = typeof(CommandFunctionFILEEOF);
        functions["FILEWRITE"] = typeof(CommandFunctionFILEWRITE);
        functions["FILEDELETE"] = typeof(CommandFunctionFILEDELETE);
        functions["FILECOPY"] = typeof(CommandFunctionFILECOPY);

        //download manager
        //functions["DOWNLOAD"] = typeof(CommandFunctionDOWNLOAD);
        //functions["DOWNLOADSTATUS"] = typeof(CommandFunctionDOWNLOADSTATUS);
        //functions["DOWNLOADPROGRESS"] = typeof(CommandFunctionDOWNLOADPROGRESS);

        //introspection
        functions["EXISTS"] = typeof(CommandFunctionEXIST);
        functions["TYPE"] = typeof(CommandFunctionTYPE);


        // configuration settings -------

        //ROOMs
        functions["ROOMNAME"] = typeof(CommandFunctionROOMNAME);
        functions["ROOMCOUNT"] = typeof(CommandFunctionROOMCOUNT);
        functions["ROOMGET"] = typeof(CommandFunctionROOMGET);
        functions["ROOMGETNAME"] = typeof(CommandFunctionROOMGETNAME);
        functions["ROOMGETDESC"] = typeof(CommandFunctionROOMGETDESC);
        functions["ROOMTELEPORT"] = typeof(CommandFunctionROOMTELEPORT);

        //posters
        functions["POSTERROOMCOUNT"] = typeof(CommandFunctionPOSTERROOMCOUNT);
        functions["POSTERROOMREPLACE"] = typeof(CommandFunctionPOSTERROOMREPLACE);

        //Controllers
        functions["CONTROLACTIVE"] = typeof(CommandFunctionCONTROLACTIVE);
        functions["CONTROLRUMBLE"] = typeof(CommandFunctionCONTROLRUMBLE);

        //cabinets in the room
        functions["CABROOMCOUNT"] = typeof(CommandFunctionCABROOMCOUNT);
        functions["CABROOMGETNAME"] = typeof(CommandFunctionCABROOMGETNAME);
        functions["CABROOMREPLACE"] = typeof(CommandFunctionCABROOMREPLACE);

        //cabinets in registry
        functions["CABDBCOUNT"] = typeof(CommandFunctionCABDBCOUNT);
        functions["CABDBCOUNTINROOM"] = typeof(CommandFunctionCABDBCOUNTINROOM);
        //functions["CABDBREPLACE"] = typeof(CommandFunctionCABDBREPLACE);
        functions["CABDBGETNAME"] = typeof(CommandFunctionCABDBGETNAME);
        functions["CABDBGETINFO"] = typeof(CommandFunctionCABDBGETINFO);
        functions["CABDBSETINFO"] = typeof(CommandFunctionCABDBSETINFO);
        functions["CABDBSEARCH"] = typeof(CommandFunctionCABDBSEARCH);
        functions["CABDBSEARCHARRAY"] = typeof(CommandFunctionCABDBSEARCHARRAY);
        //functions["CABDBGET"] = typeof(CommandFunctionCABDBGET);
        functions["CABDBDELETE"] = typeof(CommandFunctionCABDBDELETE);
        functions["CABDBADD"] = typeof(CommandFunctionCABDBADD);
        functions["CABDBSAVE"] = typeof(CommandFunctionCABDBSAVE);
        functions["CABDBRELOAD"] = typeof(CommandFunctionCABDBRELOAD);
        functions["CABDBGETASSIGNED"] = typeof(CommandFunctionCABDBGETASSIGNED);
        functions["CABDBASSIGN"] = typeof(CommandFunctionCABDBASSIGN);
        functions["CABINSERTCOIN"] = typeof(CommandFunctionCABINSERTCOIN);
        functions["CABCOINSLOTSOUND"] = typeof(CommandFunctionCABCOINSLOTSOUND);

        //workshop test-cabinet loader
        functions["WORKSHOPRELOAD"] = typeof(CommandFunctionWORKSHOPRELOAD);

        //cabinet in AGEBasic for the actual cabinet
        functions["CABPARTSCOUNT"] = typeof(CommandFunctionCABPARTSCOUNT);
        functions["CABPARTSNAME"] = typeof(CommandFunctionCABPARTSNAME);
        functions["CABPARTSPOSITION"] = typeof(CommandFunctionCABPARTSPOSITION);
        functions["CABPARTSLIST"] = typeof(CommandFunctionCABPARTSLIST);

        functions["CABPARTSENABLE"] = typeof(CommandFunctionCABPARTSENABLE);

        functions["CABPARTSGETCOORDINATE"] = typeof(CommandFunctionCABPARTSGETCOORDINATE);
        functions["CABPARTSGETGLOBALCOORDINATE"] = typeof(CommandFunctionCABPARTSGETGLOBALCOORDINATE);

        functions["CABPARTSSETCOORDINATE"] = typeof(CommandFunctionCABPARTSSETCOORDINATE);
        functions["CABPARTSSETGLOBALCOORDINATE"] = typeof(CommandFunctionCABPARTSSETGLOBALCOORDINATE);

        functions["CABPARTSSETROTATION"] = typeof(CommandFunctionCABPARTSSETROTATION);
        functions["CABPARTSROTATE"] = typeof(CommandFunctionCABPARTSROTATE);
        functions["CABPARTSSETGLOBALROTATION"] = typeof(CommandFunctionCABPARTSSETGLOBALROTATION);

        functions["CABPARTSGETROTATION"] = typeof(CommandFunctionCABPARTSGETROTATION);
        functions["CABPARTSGETGLOBALROTATION"] = typeof(CommandFunctionCABPARTSGETGLOBALROTATION);

        functions["CABPARTSGETTRANSPARENCY"] = typeof(CommandFunctionCABPARTSGETTRANSPARENCY);
        functions["CABPARTSSETTRANSPARENCY"] = typeof(CommandFunctionCABPARTSSETTRANSPARENCY);
        functions["CABPARTSEMISSION"] = typeof(CommandFunctionCABPARTSEMISSION);
        functions["CABPARTSSETEMISSIONCOLOR"] = typeof(CommandFunctionCABPARTSSETEMISSIONCOLOR);
        functions["CABPARTSSETCOLOR"] = typeof(CommandFunctionCABPARTSSETCOLOR);
        functions["CABPARTSETTEXTURE"] = typeof(CommandFunctionCABPARTSETTEXTURE);

        ////audio
        functions["CABPARTSAUDIOVOLUME"] = typeof(CommandFunctionCABPARTSAUDIOVOLUME);
        functions["CABPARTSAUDIODISTANCE"] = typeof(CommandFunctionCABPARTSAUDIODISTANCE);
        functions["CABPARTSAUDIOFILE"] = typeof(CommandFunctionCABPARTSAUDIOFILE);
        functions["CABPARTSAUDIOLOOP"] = typeof(CommandFunctionCABPARTSAUDIOLOOP);
        functions["CABPARTSAUDIOPLAY"] = typeof(CommandFunctionCABPARTSAUDIOPLAY);
        functions["CABPARTSAUDIOSTOP"] = typeof(CommandFunctionCABPARTSAUDIOSTOP);
        functions["CABPARTSAUDIOPAUSE"] = typeof(CommandFunctionCABPARTSAUDIOPAUSE);

        // physics
        functions["CABPARTSPHYACTIVATEGRAVITY"] = typeof(CommandFunctionCABPARTSPHYACTIVATEGRAVITY);
        functions["CABPARTSPHYDEACTIVATEGRAVITY"] = typeof(CommandFunctionCABPARTSPHYDEACTIVATEGRAVITY);

        //debug
        functions["DEBUGMODE"] = typeof(CommandFunctionDEBUGMODE);
        functions["LOG"] = typeof(CommandFunctionLOG);
        functions["LOGERROR"] = typeof(CommandFunctionLOGERROR);
        functions["LOGWARNING"] = typeof(CommandFunctionLOGWARNING);
        functions["ASSERT"] = typeof(CommandFunctionASSERT);

        //lights (deprecated)
        functions["GETLIGHTS"] = typeof(CommandFunctionGETLIGHTS);
        functions["GETLIGHTINTENSITY"] = typeof(CommandFunctionGETLIGHTINTENSITY);
        functions["SETLIGHTINTENSITY"] = typeof(CommandFunctionSETLIGHTINTENSITY);
        functions["LIGHTSCOUNT"] = typeof(CommandFunctionLIGHTSCOUNT);
        functions["SETLIGHTCOLOR"] = typeof(CommandFunctionSETLIGHTCOLOR);
        //-
        functions["GETGLOBALLIGHT"] = typeof(CommandFunctionGETGLOBALLIGHT);
        functions["SETGLOBALLIGHT"] = typeof(CommandFunctionSETGLOBALLIGHT);

        //audio
        functions["AUDIOGAMEGETVOLUME"] = typeof(CommandFunctionAUDIOGAMEGETVOLUME);
        functions["AUDIOMUSICGETVOLUME"] = typeof(CommandFunctionAUDIOMUSICGETVOLUME);
        functions["AUDIOAMBIENCEGETVOLUME"] = typeof(CommandFunctionAUDIOAMBIENCEGETVOLUME);
        functions["AUDIOGAMESETVOLUME"] = typeof(CommandFunctionAUDIOGAMESETVOLUME);
        functions["AUDIOMUSICSETVOLUME"] = typeof(CommandFunctionAUDIOMUSICSETVOLUME);
        functions["AUDIOAMBIENCESETVOLUME"] = typeof(CommandFunctionAUDIOAMBIENCESETVOLUME);


        //player
        functions["PLAYERSETHEIGHT"] = typeof(CommandFunctionPLAYERSETHEIGHT);
        functions["PLAYERGETHEIGHT"] = typeof(CommandFunctionPLAYERGETHEIGHT);
        functions["PLAYERSETCOORDINATE"] = typeof(CommandFunctionPLAYERSETCOORDINATE);
        functions["PLAYERGETCOORDINATE"] = typeof(CommandFunctionPLAYERGETCOORDINATE);
        functions["PLAYERLOOKAT"] = typeof(CommandFunctionPLAYERLOOKAT);
        functions["PLAYERTELEPORT"] = typeof(CommandFunctionPLAYERTELEPORT);

        //Music
        functions["MUSICPLAY"] = typeof(CommandFunctionMUSICPLAY);
        functions["MUSICADD"] = typeof(CommandFunctionMUSICADD);
        functions["MUSICREMOVE"] = typeof(CommandFunctionMUSICREMOVE);
        functions["MUSICEXIST"] = typeof(CommandFunctionMUSICEXIST);
        functions["MUSICCLEAR"] = typeof(CommandFunctionMUSICCLEAR);
        functions["MUSICLOOP"] = typeof(CommandFunctionMUSICLOOP);
        functions["MUSICLOOPSTATUS"] = typeof(CommandFunctionMUSICLOOPSTATUS);
        functions["MUSICADDLIST"] = typeof(CommandFunctionMUSICADDLIST);
        functions["MUSICADDLISTARRAY"] = typeof(CommandFunctionMUSICADDLISTARRAY);
        functions["MUSICPREVIOUS"] = typeof(CommandFunctionMUSICPREVIOUS);
        functions["MUSICNEXT"] = typeof(CommandFunctionMUSICNEXT);
        functions["MUSICRESET"] = typeof(CommandFunctionMUSICRESET);
        functions["MUSICCOUNT"] = typeof(CommandFunctionMUSICCOUNT);
        functions["MUSICGETLIST"] = typeof(CommandFunctionMUSICGETLIST);
        
        //screen
        functions["SCREENWIDTH"] = typeof(CommandFunctionSCREENWIDTH);
        functions["SCREENHEIGHT"] = typeof(CommandFunctionSCREENHEIGHT);
        functions["SCREENSIZE"] = typeof(CommandFunctionSCREENSIZE);
        functions["DSCREENWIDTH"] = typeof(CommandFunctionDSCREENWIDTH);
        functions["DSCREENHEIGHT"] = typeof(CommandFunctionDSCREENHEIGHT);
        functions["DSCREENSIZE"] = typeof(CommandFunctionDSCREENSIZE);
        functions["SPRITESTATUS"] = typeof(CommandFunctionSPRITESTATUS);
        functions["SPRITECOLLISIONCOUNT"] = typeof(CommandFunctionSPRITECOLLISIONCOUNT);
        functions["SPRITECOLLISIONDATA"] = typeof(CommandFunctionSPRITECOLLISIONDATA);
        functions["DCHARPIXELX"] = typeof(CommandFunctionDCHARPIXELX);
        functions["DCHARPIXELY"] = typeof(CommandFunctionDCHARPIXELY);
        functions["DCHARPIXEL"] = typeof(CommandFunctionDCHARPIXEL);
        functions["DPSET"] = typeof(CommandPSET);
        functions["DLINE"] = typeof(CommandLINE);
        functions["DOVAL"] = typeof(CommandOVAL);
        functions["DCIRCLE"] = typeof(CommandCIRCLE);
        functions["DBOX"] = typeof(CommandBOX);
        functions["GETCOLOR"] = typeof(CommandGETCOLOR);

        // READ DATA RESTORE
        commands["DATA"] = typeof(CommandDATA);
        commands["READ"] = typeof(CommandREAD); 
        commands["RESTORE"] = typeof(CommandRESTORE);

        //CPU
        functions["SETCPU"] = typeof(CommandFunctionSETCPU);
        functions["GETCPU"] = typeof(CommandFunctionGETCPU);

        //EVENTS
        commands["ONEVENT"] = typeof(CommandONEVENT);
        commands["OFFEVENT"] = typeof(CommandOFFEVENT);
        functions["EVENTTRIGGER"] = typeof(CommandFunctionEVENTTRIGGER);
        functions["ONCUSTOM"] = typeof(CommandFunctionONCUSTOM);
        functions["ONTIMER"] = typeof(CommandFunctionONTIMER);
        functions["ONCONTROL"] = typeof(CommandFunctionONCONTROL);
        functions["ONTOUCH"] = typeof(CommandFunctionONTOUCH);
        functions["ONGRAB"] = typeof(CommandFunctionONGRAB);
        functions["ONCOLLISION"] = typeof(CommandFunctionONCOLLISION);
        functions["ONSPRITECOLLISION"] = typeof(CommandFunctionONSPRITECOLLISION);
        functions["ONSPRITECOLLISIONEND"] = typeof(CommandFunctionONSPRITECOLLISIONEND);
        functions["ONMEMORY"] = typeof(CommandFunctionONMEMORY);
        functions["ONLED"] = typeof(CommandFunctionONLED);
        functions["LEDSTATE"] = typeof(CommandFunctionLEDSTATE);

        //lightgun
        functions["LIGHTGUNGETPOINTEDPART"] = typeof(CommandFunctionCABPARTLIGHTGUNHIT);

        // Video player
        commands["VIDEOLOAD"]    = typeof(CommandVIDEOLOAD);
        commands["VIDEOLOADURL"] = typeof(CommandVIDEOLOADURL);
        commands["VIDEOPLAY"]  = typeof(CommandVIDEOPLAY);
        commands["VIDEOPAUSE"] = typeof(CommandVIDEOPAUSE);
        commands["VIDEOSTOP"]  = typeof(CommandVIDEOSTOP);
        commands["VIDEOSEEK"]  = typeof(CommandVIDEOSEEK);
        commands["VIDEOLOOP"]  = typeof(CommandVIDEOLOOP);

        functions["VIDEOTIME"]       = typeof(CommandFunctionVIDEOTIME);
        functions["VIDEODURATION"]   = typeof(CommandFunctionVIDEODURATION);
        functions["VIDEOSTATUS"]     = typeof(CommandFunctionVIDEOSTATUS);
        functions["VIDEOLOOPSTATUS"] = typeof(CommandFunctionVIDEOLOOPSTATUS);
        functions["VIDEOPATH"]       = typeof(CommandFunctionVIDEOPATH);

        // SID player
        commands["SIDLOAD"]     = typeof(CommandSIDLOAD);
        commands["SIDLOADDATA"] = typeof(CommandSIDLOADDATA);
        commands["SIDPLAY"]     = typeof(CommandSIDPLAY);
        commands["SIDSTOP"]     = typeof(CommandSIDSTOP);
        commands["SIDPAUSE"]    = typeof(CommandSIDPAUSE);
        commands["SIDRESUME"]   = typeof(CommandSIDRESUME);
        commands["SIDUNLOAD"]   = typeof(CommandSIDUNLOAD);
        commands["SIDVOLUME"]   = typeof(CommandSIDVOLUME);

        functions["SIDSTATUS"]      = typeof(CommandFunctionSIDSTATUS);
        functions["SIDTITLE"]       = typeof(CommandFunctionSIDTITLE);
        functions["SIDAUTHOR"]      = typeof(CommandFunctionSIDAUTHOR);
        functions["SIDRELEASED"]    = typeof(CommandFunctionSIDRELEASED);
        functions["SIDCOUNT"]       = typeof(CommandFunctionSIDCOUNT);
        functions["SIDDEFAULTSONG"] = typeof(CommandFunctionSIDDEFAULTSONG);

    }

    public static ICommandBase GetNew(string CommandType, ConfigurationCommands config)
    {
        CommandType = CommandType.ToUpper();

        if (commands.ContainsKey(CommandType))
            return (ICommandBase)Activator.CreateInstance(commands[CommandType], config);
        if (functions.ContainsKey(CommandType))
            return (ICommandBase)Activator.CreateInstance(functions[CommandType], config);

        return null;
    }
    public static bool IsCommand(string command)
    {
        return commands.ContainsKey(command.ToUpper());
    }
    public static bool IsFunction(string function)
    {
        return functions.ContainsKey(function.ToUpper());
    }
}
