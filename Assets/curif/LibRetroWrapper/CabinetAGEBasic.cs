
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization; //https://github.com/aaubry/YamlDotNet
using YamlDotNet.Serialization.NamingConventions;
using UnityEditor;

[Serializable]
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

    // Serialize this field to show it in the editor
    [SerializeField]
    private List<Variable> variables;
    public List<Variable> Variables { get { return variables; } set { variables = value; } }

    [Serializable]
    public class Variable
    {
        public string name; //variable name
        public string type; //STRING or NUMBER
        public string value = ""; //first asigned value.
    }

}

[RequireComponent(typeof(basicAGE))]

public class CabinetAGEBasic : MonoBehaviour
{

    basicAGE ageBasic;

    BasicVars vars = new(); //variable's space.

    public string pathBase;

    public CabinetAGEBasicInformation AGEInfo = new();
    public void SetDebugMode(bool debug)
    {
        ageBasic.DebugMode = debug;
    }
    public void Init(CabinetAGEBasicInformation AGEInfo,
            string pathBase,
            Cabinet cabinet,
            CoinSlotController coinSlot)
    {
        if (ageBasic == null)
            ageBasic = GetComponent<basicAGE>();

        this.AGEInfo = AGEInfo;
        this.pathBase = pathBase;

        ageBasic.SetCoinSlot(coinSlot);
        ageBasic.SetCabinet(cabinet);

        if (AGEInfo.Variables != null)
        {        //variable injection
            foreach (CabinetAGEBasicInformation.Variable var in AGEInfo.Variables)
            {
                BasicValue bv;
                if (var.type.ToUpper() == "STRING")
                    bv = new BasicValue(var.value, forceType: BasicValue.BasicValueType.String);
                else if (var.type.ToUpper() == "NUMBER")
                    bv = new BasicValue(var.value, forceType: BasicValue.BasicValueType.Number);
                else
                    throw new Exception($"[CabinetAGEBasic.init] AGEBasic variable injection error var: {var.name} value type unknown: {var.type}");

                vars.SetValue(var.name, bv);
                ConfigManager.WriteConsole($"[CabinetAGEBasic.Init] inject variable: {var.name}: {bv}");
            }
        }
    }

    private bool execute(string prgName, bool blocking = false, int maxExecutionLines = 10000)
    {
        if (string.IsNullOrEmpty(prgName))
            return false;

        if (!ageBasic.Exists(prgName) /*&& afterInsertCoinException == null*/)
        {
            try
            {
                ageBasic.ParseFile(pathBase + "/" + prgName);
            }
            catch (CompilationException e)
            {
                ConfigManager.WriteConsoleException($"[CabinetAGEBasic.execute] parsing {prgName}", (Exception)e);
                return false;
            }
        }

        ConfigManager.WriteConsole($"[CabinetAGEBasic.execute] exec {prgName}");
        ageBasic.Run(prgName, blocking, vars, maxExecutionLines); //async blocking=false
        return true;
    }

    public void ExecInsertCoinBas()
    {
        ageBasic.DebugMode = AGEInfo.debug;
        execute(AGEInfo.afterInsertCoin, maxExecutionLines: 0);
    }

    public void StopInsertCoinBas()
    {
        if (ageBasic.IsRunning(AGEInfo.afterInsertCoin))
            ageBasic.ForceStop();

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
    /*    public bool ExecAfterStartBas()
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
    */
}

#if UNITY_EDITOR


[CustomEditor(typeof(CabinetAGEBasic))]
public class CabinetAGEBasicEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CabinetAGEBasic myScript = (CabinetAGEBasic)target;

        if (GUILayout.Button("Exec Insert Coin Bas"))
        {
            myScript.ExecInsertCoinBas();
        }

        if (GUILayout.Button("Stop Insert Coin Bas"))
        {
            myScript.StopInsertCoinBas();
        }

        if (GUILayout.Button("Exec After Leave Bas"))
        {
            myScript.ExecAfterLeaveBas();
        }

        if (GUILayout.Button("Exec After Load Bas"))
        {
            myScript.ExecAfterLoadBas();
        }

        /*
        if (GUILayout.Button("Exec After Start Bas"))
        {
            myScript.ExecAfterStartBas();
        }

        if (GUILayout.Button("Stop After Start Bas"))
        {
            myScript.StopAfterStartBas();
        }
        */
    }
}
#endif