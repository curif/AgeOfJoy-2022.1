using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class LibretroControlMap : MonoBehaviour
{
    [Tooltip("The control action map.")]
    public InputActionMap actionMap;

    // [Tooltip("The global action manager in the main rig")]
    // public InputActionManager inputActionManager;
    private const int wheelDelta = 120;

    public bool InvertX = false;
    public bool InvertY = false;

    /*
    public void LoadConfigurationFromFile(string filename)
    {
      ControlMapConfiguration conf = ControlMapConfiguration.LoadFromYaml(filename);
      if (conf == null)
      {
        conf = DefaultControlMap.Instance;
      }
      ConfigManager.WriteConsole($"[LoadConfigurationFromFile] load config {conf}");
      //ConfigManager.WriteConsole(conf.asMarkdown());
      Debug.Log(conf.AsMarkdown());
      conf.ToDebug();

      actionMap = ControlMapInputAction.inputActionMapFromConfiguration(conf);
    }
    */

    //load the control map from the cabinet configuration, if not found fall to the default one.
    //in fact merge any other control map with the default. 
    public void CreateFromConfiguration(
        ControlMapConfiguration conf,
        string name = null,
        string fileNameToSaveOrEmpty = null
        )
    {
        if (!string.IsNullOrEmpty(fileNameToSaveOrEmpty))
        {
            conf.SaveAsYaml(fileNameToSaveOrEmpty);
        }

        // Debug.Log(conf.AsMarkdown());

        InvertX = conf.invertx;
        InvertY = conf.inverty;

        actionMap = ControlMapInputAction.inputActionMapFromConfiguration(conf, name);
    }

    public bool SendHapticImpulse(string mameControl, float amplitude, float duration)
    {
        InputAction action = actionMap.FindAction(mameControl+ "_0");
        ConfigManager.WriteConsole($"[LibretroControlMap.SendHapticImpulse] {mameControl} action: {action}");
        ConfigManager.WriteConsole($"[LibretroControlMap.SendHapticImpulse] {mameControl} active control: {action?.activeControl}");
        ConfigManager.WriteConsole($"[LibretroControlMap.SendHapticImpulse] {mameControl} device: {action?.activeControl?.device}");

        if (action?.activeControl?.device is XRControllerWithRumble rumbleController)
        {
            ConfigManager.WriteConsole($"[LibretroControlMap.SendHapticImpulse] SendImpulse...");
            rumbleController.SendImpulse(amplitude, duration);
            return true;
        }

        return false;
    }

    public bool isActive(string mameControl, int port = 0)
    {
        return Active(mameControl, port) !=0;
    }

    public int Active(string mameControl, int port = 0)
    {
        int ret = 0;

        if (!actionMap.enabled)
            return 0;

        string inputActionMapId = mameControl + "_" + port.ToString();

        InputAction action = actionMap.FindAction(inputActionMapId);

        if (action == null)
        {
            //ConfigManager.WriteConsoleError($"[LibretroControlMap.Active] [{inputActionMapId}] not found in controlMap");
            return 0;
        }
        try
        {
            if (!action.enabled)
            {
                // ConfigManager.WriteConsoleWarning($"[LibretroControlMap.Active] {inputActionMapId} is not enabled in the actionMap: {actionMap.name}");
                action.Enable();
            }

        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleException($"[LibretroControlMap.Active] {inputActionMapId} {action.ToString()}", e);
            return 0;
        }

        //https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/api/UnityEngine.InputSystem.InputAction.html#UnityEngine_InputSystem_InputAction_WasPerformedThisFrame
        if (action.type == InputActionType.Button)
        {
            if (action.IsPressed())
            {
                //ConfigManager.WriteConsole($"[LibretroControlMap.Active] pressed {actionMap.name}/{inputActionMapId} enabled: {action.enabled} - action: {action.ToString()}");
                return 1;
            }
            return 0;
        }
        else if (action.type == InputActionType.Value)
        {
            Vector2 resultVector = new Vector2(0, 0);
            float resultFloat = 0;

            var result = action.ReadValueAsObject();
            if (result is Vector2)
            {
                resultVector = (Vector2)result;
            }
            else if (result is float)
            {
                resultFloat = (float)result;
            }

            if (InvertX) resultVector.x = -resultVector.x;
            if (InvertY) resultVector.y = -resultVector.y;

            switch (mameControl)
            {
                case "JOYPAD_UP":
                case "LIGHTGUN_DPAD_UP":
                    if (resultVector.y > 0.5 || resultFloat == 1.0)
                    {
                        // ConfigManager.WriteConsole($"{inputActionMapId}: val: {val}");
                        return 1;
                    }
                    break;
                case "JOYPAD_DOWN":
                case "LIGHTGUN_DPAD_DOWN":
                    if (resultVector.y < -0.5 || resultFloat == 1.0)
                    {
                        // ConfigManager.WriteConsole($"{inputActionMapId}: val: {val}");
                        return 1;
                    }
                    break;
                case "JOYPAD_RIGHT":
                case "LIGHTGUN_DPAD_RIGHT":
                    if (resultVector.x > 0.5 || resultFloat == 1.0)
                    {
                        // ConfigManager.WriteConsole($"{inputActionMapId}: val: {val}");
                        return 1;
                    }
                    break;
                case "JOYPAD_LEFT":
                case "LIGHTGUN_DPAD_LEFT":
                    if (resultVector.x < -0.5 || resultFloat == 1.0)
                    {
                        // ConfigManager.WriteConsole($"{inputActionMapId}: val: {val}");
                        return 1;
                    }
                    break;
                case "MOUSE_X":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.x > 0.5)
                        ret = 10;
                    else if (resultVector.x < -0.5)
                        ret = -10;
                    break;
                case "MOUSE_Y":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.y > 0.5)
                        ret = 10;
                    else if (resultVector.y < -0.5)
                        ret = -10;
                    break;
                case "MOUSE_WHEELUP":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.y > wheelDelta)
                        ret = 10;
                    break;
                case "MOUSE_WHEELDOWN":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.y < -wheelDelta)
                        ret = -10;
                    break;
                case "MOUSE_HORIZ_WHEELUP":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.x > wheelDelta)
                        ret = 10;
                    break;
                case "MOUSE_HORIZ_WHEELDOWN":
                    //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
                    if (resultVector.x < -wheelDelta)
                        ret = -10;
                    break;
            }
        }
        return ret;
    }


    // --- Raw analog reads for the Flycast HW core (core: flycast) -------------------------------
    // Active() thresholds axis actions into digital presses (the d-pad path). These helpers instead
    // return the raw, proportional value of the same existing actions so a cabinet with
    // `input: { analog-stick: true }` can drive the Dreamcast analog stick + analog triggers. Purely
    // additive: Active() and every existing (MAME/FBNeo) caller are unchanged.

    // Raw value of an axis/trigger action, without the digital thresholding Active() applies.
    // Vector2 for a thumbstick binding (InvertX/Y honored); a trigger/button binding reports its
    // 0..1 actuation mirrored into both components. Vector2.zero if the action is missing/disabled.
    public Vector2 ReadAxisRaw(string mameControl, int port = 0)
    {
        if (actionMap == null || !actionMap.enabled)
            return Vector2.zero;

        InputAction action = actionMap.FindAction(mameControl + "_" + port.ToString());
        if (action == null)
            return Vector2.zero;

        if (!action.enabled)
        {
            try { action.Enable(); }
            catch { return Vector2.zero; }
        }

        var result = action.ReadValueAsObject();
        if (result is Vector2 v)
        {
            if (InvertX) v.x = -v.x;
            if (InvertY) v.y = -v.y;
            return v;
        }
        if (result is float f)
            return new Vector2(f, f); // trigger/button actuation (invert is meaningless here)

        return Vector2.zero;
    }

    // Left thumbstick as libretro analog-stick values [-0x7fff, 0x7fff]. Y is negated: Unity stick-up
    // is +y, libretro/Dreamcast analog-up is -y.
    public void ReadStick(out short x, out short y, int port = 0)
    {
        Vector2 v = ReadAxisRaw(LibretroControlMapDictionnary.JOYPAD_UP, port);
        x = (short)Mathf.Clamp(Mathf.RoundToInt(v.x * 0x7fff), -0x7fff, 0x7fff);
        y = (short)Mathf.Clamp(Mathf.RoundToInt(-v.y * 0x7fff), -0x7fff, 0x7fff);
    }

    // Analog trigger actuation as a libretro ANALOG_BUTTON value [0, 0x7fff]. Pass JOYPAD_L (left
    // trigger) or JOYPAD_R (right trigger) — the actions the default map binds to the triggers.
    public short ReadTrigger(string mameControl, int port = 0)
    {
        float t = Mathf.Clamp01(Mathf.Abs(ReadAxisRaw(mameControl, port).x));
        return (short)Mathf.RoundToInt(t * 0x7fff);
    }

    public void Enable(bool enable)
    {
        if (enable)
        {
            actionMap.Enable();
            ConfigManager.WriteConsole($"[LibretroControlMap.Enable] actionMap Enabled: {actionMap}");
            // inputActionManager.DisableInput();
            return;
        }
        ConfigManager.WriteConsole($"[LibretroControlMap.Enable] actionMap Disabled: {actionMap}");
        actionMap.Disable();
        // inputActionManager.EnableInput();
        return;
    }

    //when its not longer neccesary
    public void Clean()
    {
        actionMap.Dispose();
        actionMap = null;
    }

}
