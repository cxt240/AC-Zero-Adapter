namespace ACZeroAdapter.Core;

public readonly record struct GameKey(string Name, int? Code, string? Dik)
{
    public string Hex => Code is null ? "" : $"0x{Code.Value:X}";
}

public static class KeyMap
{
    public const string None = "None";

    private static readonly GameKey[] Keys =
    [
        new("None", null, null),
        new("A", 0x1E, "DIK_A"), new("B", 0x30, "DIK_B"), new("C", 0x2E, "DIK_C"),
        new("D", 0x20, "DIK_D"), new("E", 0x12, "DIK_E"), new("F", 0x21, "DIK_F"),
        new("G", 0x22, "DIK_G"), new("H", 0x23, "DIK_H"), new("I", 0x17, "DIK_I"),
        new("J", 0x24, "DIK_J"), new("K", 0x25, "DIK_K"), new("L", 0x26, "DIK_L"),
        new("M", 0x32, "DIK_M"), new("N", 0x31, "DIK_N"), new("O", 0x18, "DIK_O"),
        new("P", 0x19, "DIK_P"), new("Q", 0x10, "DIK_Q"), new("R", 0x13, "DIK_R"),
        new("S", 0x1F, "DIK_S"), new("T", 0x14, "DIK_T"), new("U", 0x16, "DIK_U"),
        new("V", 0x2F, "DIK_V"), new("W", 0x11, "DIK_W"), new("X", 0x2D, "DIK_X"),
        new("Y", 0x15, "DIK_Y"), new("Z", 0x2C, "DIK_Z"),
        new("1", 0x02, "DIK_1"), new("2", 0x03, "DIK_2"), new("3", 0x04, "DIK_3"),
        new("4", 0x05, "DIK_4"), new("5", 0x06, "DIK_5"), new("6", 0x07, "DIK_6"),
        new("7", 0x08, "DIK_7"), new("8", 0x09, "DIK_8"), new("9", 0x0A, "DIK_9"),
        new("0", 0x0B, "DIK_0"),
        new("Space", 0x39, "DIK_SPACE"),
        new("Left Ctrl", 0x1D, "DIK_LCONTROL"), new("Right Ctrl", 0x9D, "DIK_RCONTROL"),
        new("Left Shift", 0x2A, "DIK_LSHIFT"), new("Right Shift", 0x36, "DIK_RSHIFT"),
        new("Left Alt", 0x38, "DIK_LMENU"), new("Right Alt", 0xB8, "DIK_RMENU"),
        new("Tab", 0x0F, "DIK_TAB"),
        new("Up", 0xC8, "DIK_UP"), new("Down", 0xD0, "DIK_DOWN"),
        new("Left", 0xCB, "DIK_LEFT"), new("Right", 0xCD, "DIK_RIGHT"),
        new("Num 0", 0x52, "DIK_NUMPAD0"), new("Num 1", 0x4F, "DIK_NUMPAD1"),
        new("Num 2", 0x50, "DIK_NUMPAD2"), new("Num 3", 0x51, "DIK_NUMPAD3"),
        new("Num 4", 0x4B, "DIK_NUMPAD4"), new("Num 5", 0x4C, "DIK_NUMPAD5"),
        new("Num 6", 0x4D, "DIK_NUMPAD6"), new("Num 7", 0x47, "DIK_NUMPAD7"),
        new("Num 8", 0x48, "DIK_NUMPAD8"), new("Num 9", 0x49, "DIK_NUMPAD9"),
        new("F1", 0x3B, "DIK_F1"), new("F2", 0x3C, "DIK_F2"), new("F3", 0x3D, "DIK_F3"),
        new("F4", 0x3E, "DIK_F4"), new("F5", 0x3F, "DIK_F5"), new("F6", 0x40, "DIK_F6"),
        new("F7", 0x41, "DIK_F7"), new("F8", 0x42, "DIK_F8"), new("F9", 0x43, "DIK_F9"),
        new("F10", 0x44, "DIK_F10"), new("F11", 0x57, "DIK_F11"), new("F12", 0x58, "DIK_F12"),
    ];

    public static IReadOnlyList<string> Names { get; } = Keys.Select(key => key.Name).ToArray();

    public static GameKey ByName(string name)
    {
        foreach (var key in Keys)
        {
            if (key.Name == name) return key;
        }

        if (name.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(name[2..], System.Globalization.NumberStyles.HexNumber, null, out var code))
        {
            return new GameKey(name.ToUpperInvariant(), code, name.ToUpperInvariant());
        }

        return Keys[0];
    }

    public static string NameFromCode(int code)
    {
        foreach (var key in Keys)
        {
            if (key.Code == code) return key.Name;
        }

        return $"0x{code:X}";
    }
}
