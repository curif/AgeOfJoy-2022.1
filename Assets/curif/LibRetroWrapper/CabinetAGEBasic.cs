
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;

public class CabinetAGEBasicInformation
{
    public bool active = false;
    public bool debug = false;

    [YamlMember(Alias = "after-load", ApplyNamingConventions = false)]
    public string afterLoad;

    [YamlMember(Alias = "after-insert-coin", ApplyNamingConventions = false)]
    public string afterInsertCoin;
    [YamlMember(Alias = "after-leave", ApplyNamingConventions = false)]
    public string afterLeave;
    [YamlMember(Alias = "after-start", ApplyNamingConventions = false)]
    public string afterStart;

    public List<Variable> Variables { get; set; }

    public class Variable
    {
        public string name;
        public string type;
        public string value = "";
    }

}

[RequireComponent(typeof(basicAGE))]

public class CabinetAGEBasic : MonoBehaviour
{

    basicAGE ageBasic;

    BasicVars vars = new(); //variable's space.

    string pathBase;

    CabinetAGEBasicInformation AGEInfo = new();
    public void SetDebugMode(bool debug)
    {
        ageBasic.DebugMode = debug;
    }
    public void Init(CabinetAGEBasicInformation AGEInfo,
            string pathBase,
            GameObject cabinet,
            CoinSlotController coinSlot)
    {
        if (ageBasic == null)
            ageBasic = GetComponent<basicAGE>();

        this.AGEInfo = AGEInfo;
        this.pathBase = pathBase;

        ageBasic.SetCoinSlot(coinSlot);
        ageBasic.SetCabinet(cabinet);

        //variable injection
        foreach (CabinetAGEBasicInformation.Variable var in AGEInfo.Variables)
        {
            BasicValue bv;
            if (var.type.ToUpper() == "STRING")
                bv = new BasicValue(var.value, forceType: BasicValue.BasicValueType.String);
            else if (var.type.ToUpper() == "NUMBER")
                bv = new BasicValue(var.value, forceType: BasicValue.BasicValueType.Number);
            else
                throw new Exception($"AGEBasic variable injection error var: {var.name} value type unknown: {var.type}");

            vars.SetValue(var.name, bv);
            ConfigManager.WriteConsole($"[CabinetAGEBasic.Init] inject variable: {var.name}: {bv}");
        }
    }

    private bool execute(string prgName, bool blocking = false)
    {
        if (!string.IsNullOrEmpty(prgName))
        {
            if (!ageBasic.Exists(prgName) /*&& afterInsertCoinException == null*/)
            {
                try
                {
                    ageBasic.ParseFile(pathBase + "/" + prgName);
                }
                catch (CompilationException e)
                {
                    ConfigManager.WriteConsoleException($"[Execute] parsing {prgName}", (Exception)e);
                }
            }

            ageBasic.Run(prgName, blocking, vars); //async blocking=false
            return true;
        }
        return false;
    }

    public void ExecInsertCoinBas()
    {
        ageBasic.DebugMode = AGEInfo.debug;
        execute(AGEInfo.afterInsertCoin);
    }
    public void StopInsertCoinBas()
    {
        if (ageBasic.IsRunning(AGEInfo.afterInsertCoin))
        {
            ageBasic.Stop();
            while (ageBasic.IsRunning(AGEInfo.afterInsertCoin)) { }
        }
        return;
    }

    public void ExecAfterLeaveBas()
    {
        ageBasic.DebugMode = AGEInfo.debug;
        execute(AGEInfo.afterLeave);
    }
    public void ExecAfterLoadBas()
    {
        ageBasic.DebugMode = AGEInfo.debug;
        execute(AGEInfo.afterLoad);
    }
    public bool ExecAfterStartBas()
    {
        if (ageBasic.IsRunning())
            return false;

        ageBasic.DebugMode = AGEInfo.debug;
        return execute(AGEInfo.afterStart);
    }
    public void StopAfterStartBas()
    {
        if (ageBasic.IsRunning(AGEInfo.afterStart))
            ageBasic.Stop();
    }

}
