
using UnityEngine;
using System;
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
    [YamlMember(Alias = "input", ApplyNamingConventions = false)]
    public string input;

}

[RequireComponent(typeof(basicAGE))]

public class CabinetAGEBasic : MonoBehaviour
{

    basicAGE ageBasic;

    string pathBase;

    CompilationException afterInsertCoinException;

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
    }
    private void execute(string prgName, bool blocking = false)
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
                    afterInsertCoinException = e;
                    ConfigManager.WriteConsoleException($"[Execute] parsing {prgName}", (Exception)e);
                }
            }

            ageBasic.Run(prgName, blocking); //async blocking=false
        }
    }

    public void ExecInsertCoinBas()
    {
        execute(AGEInfo.afterInsertCoin);
    }
    public void ExecAfterLeaveBas()
    {
        execute(AGEInfo.afterLeave);
    }
    public void ExecAfterLoadBas()
    {
        execute(AGEInfo.afterLoad);
    }

}
