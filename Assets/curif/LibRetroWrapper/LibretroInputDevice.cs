using System.Collections.Generic;

public class LibretroInputDevice
{
    private static int RETRO_DEVICE_TYPE_SHIFT = 8;
    private static uint RETRO_DEVICE_SUBCLASS(uint _base, uint _id)
    {
        return (((_id + 1) << RETRO_DEVICE_TYPE_SHIFT) | _base);
    }

    // Standard libretro devices
    private static uint RETRO_DEVICE_NONE = 0;
    private static uint RETRO_DEVICE_JOYPAD = 1;
    private static uint RETRO_DEVICE_MOUSE = 2;
    private static uint RETRO_DEVICE_KEYBOARD = 3;
    private static uint RETRO_DEVICE_LIGHTGUN = 4;
    private static uint RETRO_DEVICE_ANALOG = 5;
    private static uint RETRO_DEVICE_POINTER = 6;

    // PSX devices
    private static uint RETRO_DEVICE_PSE_STANDARD = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_JOYPAD, 0);
    private static uint RETRO_DEVICE_PSE_ANALOG = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_ANALOG, 0);
    private static uint RETRO_DEVICE_PSE_DUALSHOCK = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_ANALOG, 1);
    private static uint RETRO_DEVICE_PSE_NEGCON = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_ANALOG, 2);
    private static uint RETRO_DEVICE_PSE_GUNCON = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_LIGHTGUN, 0);
    private static uint RETRO_DEVICE_PSE_JUSTIFIER = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_LIGHTGUN, 1);
    private static uint RETRO_DEVICE_PSE_MOUSE = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_MOUSE, 0);

    // SNES9X devices
    private static uint RETRO_DEVICE_LIGHTGUN_SUPER_SCOPE = ((1 << 8) | RETRO_DEVICE_LIGHTGUN);
    private static uint RETRO_DEVICE_LIGHTGUN_JUSTIFIER = ((2 << 8) | RETRO_DEVICE_LIGHTGUN);
    private static uint RETRO_DEVICE_LIGHTGUN_JUSTIFIER_2 = (3 << 8) | RETRO_DEVICE_LIGHTGUN;
    private static uint RETRO_DEVICE_LIGHTGUN_MACS_RIFLE = ((4 << 8) | RETRO_DEVICE_LIGHTGUN);

    // GenesisPlusGX devices
    private static uint RETRO_DEVICE_PHASER = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_LIGHTGUN, 0);
    private static uint RETRO_DEVICE_MENACER = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_LIGHTGUN, 1);
    private static uint RETRO_DEVICE_JUSTIFIERS = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_LIGHTGUN, 2);

    // FCEUmm devices
    private static uint RETRO_DEVICE_ZAPPER = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_MOUSE, 0);
    private static uint RETRO_DEVICE_ARKANOID = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_MOUSE, 1);
    private static uint RETRO_DEVICE_POWERPADA = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_KEYBOARD, 0);
    private static uint RETRO_DEVICE_POWERPADB = RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_KEYBOARD, 1);

    public static LibretroInputDevice Empty = new LibretroInputDevice("empty", RETRO_DEVICE_NONE);
    public static LibretroInputDevice Gamepad = new LibretroInputDevice("gamepad", RETRO_DEVICE_JOYPAD);
    public static LibretroInputDevice Mouse = new LibretroInputDevice("mouse", RETRO_DEVICE_MOUSE, "stick");
    public static LibretroInputDevice MousePointer = new LibretroInputDevice("mouse_pointer", RETRO_DEVICE_MOUSE, "aim");
    public static LibretroInputDevice Keyboard = new LibretroInputDevice("keyboard", RETRO_DEVICE_KEYBOARD);
    public static LibretroInputDevice Lightgun = new LibretroInputDevice("lightgun", RETRO_DEVICE_LIGHTGUN);
    public static LibretroInputDevice Analog = new LibretroInputDevice("analog", RETRO_DEVICE_ANALOG);
    public static LibretroInputDevice Pointer = new LibretroInputDevice("pointer", RETRO_DEVICE_POINTER);

    public static LibretroInputDevice PsxStandard = new LibretroInputDevice("psx_standard", RETRO_DEVICE_PSE_STANDARD);
    public static LibretroInputDevice PsxAnalog = new LibretroInputDevice("psx_analog", RETRO_DEVICE_PSE_ANALOG);
    public static LibretroInputDevice PsxDualShock = new LibretroInputDevice("psx_dual_shock", RETRO_DEVICE_PSE_DUALSHOCK);
    public static LibretroInputDevice PsxNegCon = new LibretroInputDevice("psx_negcon", RETRO_DEVICE_PSE_NEGCON);
    public static LibretroInputDevice PsxGunCon = new LibretroInputDevice("psx_guncon", RETRO_DEVICE_PSE_GUNCON);
    public static LibretroInputDevice PsxJustifier = new LibretroInputDevice("psx_justifier", RETRO_DEVICE_PSE_JUSTIFIER);
    public static LibretroInputDevice PsxMouse = new LibretroInputDevice("psx_mouse", RETRO_DEVICE_PSE_MOUSE);

    public static LibretroInputDevice SnesSuperScope = new LibretroInputDevice("snes_superscope", RETRO_DEVICE_LIGHTGUN_SUPER_SCOPE);
    public static LibretroInputDevice SnesJustifier = new LibretroInputDevice("snes_justifier", RETRO_DEVICE_LIGHTGUN_JUSTIFIER);
    public static LibretroInputDevice SnesJustifier2 = new LibretroInputDevice("snes_justifier_2", RETRO_DEVICE_LIGHTGUN_JUSTIFIER_2);
    public static LibretroInputDevice SnesMacsRifle = new LibretroInputDevice("snes_macs_rifle", RETRO_DEVICE_LIGHTGUN_MACS_RIFLE);

    public static LibretroInputDevice GenesisPhaser = new LibretroInputDevice("sega_phaser", RETRO_DEVICE_PHASER);
    public static LibretroInputDevice GenesisMenacer = new LibretroInputDevice("sega_menacer", RETRO_DEVICE_MENACER);
    public static LibretroInputDevice GenesisJustifiers = new LibretroInputDevice("sega_justifiers", RETRO_DEVICE_JUSTIFIERS);

    public static LibretroInputDevice NesZapper = new LibretroInputDevice("nes_zapper", RETRO_DEVICE_ZAPPER);
    public static LibretroInputDevice NesArkanoid = new LibretroInputDevice("nes_arkanoid", RETRO_DEVICE_ARKANOID);
    public static LibretroInputDevice NesPowerPadA = new LibretroInputDevice("nes_powerpad_a", RETRO_DEVICE_POWERPADA);
    public static LibretroInputDevice NesPowerPadB = new LibretroInputDevice("nes_powerpad_b", RETRO_DEVICE_POWERPADB);

    public static Dictionary<string, LibretroInputDevice> keyValuePairs = new Dictionary<string, LibretroInputDevice>
    {
        { Empty.Name, Empty },
        { Gamepad.Name, Gamepad },
        { Mouse.Name, Mouse },
        { MousePointer.Name, MousePointer },
        { Keyboard.Name, Keyboard },
        { Lightgun.Name, Lightgun },
        { Analog.Name, Analog },
        { Pointer.Name, Pointer },
        { PsxStandard.Name, PsxStandard },
        { PsxAnalog.Name, PsxAnalog },
        { PsxDualShock.Name, PsxDualShock },
        { PsxNegCon.Name, PsxNegCon },
        { PsxGunCon.Name, PsxGunCon },
        { PsxJustifier.Name, PsxJustifier },
        { PsxMouse.Name, PsxMouse},
        { SnesSuperScope.Name, SnesSuperScope },
        { SnesJustifier.Name, SnesJustifier },
        { SnesJustifier2.Name, SnesJustifier2 },
        { SnesMacsRifle.Name, SnesMacsRifle },
        { GenesisPhaser.Name, GenesisPhaser },
        { GenesisMenacer.Name, GenesisMenacer },
        { GenesisJustifiers.Name, GenesisJustifiers },
        { NesZapper.Name, NesZapper },
        { NesArkanoid.Name, NesArkanoid },
        { NesPowerPadA.Name, NesPowerPadA },
        { NesPowerPadB.Name, NesPowerPadB }
    };

    public static LibretroInputDevice GetInputDeviceType(string name)
    {
        if (name.StartsWith("device_"))
        {
            return new LibretroInputDevice(name, uint.Parse(name.Substring(7)));
        }

        if (keyValuePairs.ContainsKey(name)) { return keyValuePairs[name]; }
        return Empty;
    }

    public string Name { get; }
    public uint Id { get; }

    public string Type { get; }

    private LibretroInputDevice(string name, uint id, string type)
    {
        this.Name = name;
        this.Id = id;
        this.Type = type;
    }

    private LibretroInputDevice(string name, uint id) : this(name, id, default(string))
    {
    }
}
