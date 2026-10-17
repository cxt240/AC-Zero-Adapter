namespace ACZeroAdapter.Core;

public sealed class FlightOptions
{
    public string ThrottleUp { get; set; } = "W";
    public string ThrottleDown { get; set; } = "S";
    public string RollLeft { get; set; } = "A";
    public string RollRight { get; set; } = "D";
    public string YawLeft { get; set; } = "Q";
    public string YawRight { get; set; } = "E";
    public string MouseVertical { get; set; } = "Pitch";
    public string MouseHorizontal { get; set; } = "Yaw";
    public int PitchSpan { get; set; } = 2;
    public int HorizontalSpan { get; set; } = 8;
    public bool ExtraThrottle { get; set; } = true;
    public bool InvertPitch { get; set; } = true;
    public string CameraLeft { get; set; } = "Num 4";
    public string CameraRight { get; set; } = "Num 6";
    public string CameraUp { get; set; } = "Num 8";
    public string CameraDown { get; set; } = "Num 2";
    public string CameraMode { get; set; } = "V";
    public string Boresight { get; set; } = "R";
    public string Radar { get; set; } = "M";
    public bool AltLook { get; set; } = true;
    public bool CornerLook { get; set; } = true;
    public string Gun { get; set; } = "J";
    public string Missile { get; set; } = "L";
    public string Weapon { get; set; } = "K";
    public string Target { get; set; } = "T";
    public string GunMouse { get; set; } = "Left";
    public string MissileMouse { get; set; } = "Right";
    public string TargetMouse { get; set; } = "Middle";
    public bool WeaponWheel { get; set; } = true;
    public string Autopilot { get; set; } = "Z";
    public string Autopilot2 { get; set; } = "C";

    public static FlightOptions CreateDefault() => new();

    public Dictionary<string, string> ToKeys() => new()
    {
        ["Throttle up"] = ThrottleUp, ["Throttle down"] = ThrottleDown,
        ["Roll left"] = RollLeft, ["Roll right"] = RollRight,
        ["Yaw left"] = YawLeft, ["Yaw right"] = YawRight,
        ["Look left"] = CameraLeft, ["Look right"] = CameraRight,
        ["Look up"] = CameraUp, ["Look down"] = CameraDown,
        ["Change view"] = CameraMode, ["Boresight"] = Boresight,
        ["Radar"] = Radar, ["Autopilot"] = Autopilot, ["Autopilot 2"] = Autopilot2,
        ["Gun"] = Gun, ["Missile"] = Missile, ["Weapon"] = Weapon, ["Target"] = Target,
    };

    public Dictionary<string, string> ToClicks() => new()
    {
        ["Gun click"] = GunMouse, ["Missile click"] = MissileMouse, ["Target click"] = TargetMouse,
    };

    public void ApplyKeys(IReadOnlyDictionary<string, string> keys, IReadOnlyDictionary<string, string> clicks)
    {
        ThrottleUp = keys["Throttle up"];
        ThrottleDown = keys["Throttle down"];
        RollLeft = keys["Roll left"];
        RollRight = keys["Roll right"];
        YawLeft = keys["Yaw left"];
        YawRight = keys["Yaw right"];
        CameraLeft = keys["Look left"];
        CameraRight = keys["Look right"];
        CameraUp = keys["Look up"];
        CameraDown = keys["Look down"];
        CameraMode = keys["Change view"];
        Boresight = keys["Boresight"];
        Radar = keys["Radar"];
        Autopilot = keys["Autopilot"];
        Autopilot2 = keys["Autopilot 2"];
        Gun = keys["Gun"];
        Missile = keys["Missile"];
        Weapon = keys["Weapon"];
        Target = keys["Target"];
        GunMouse = clicks["Gun click"];
        MissileMouse = clicks["Missile click"];
        TargetMouse = clicks["Target click"];
    }

    public string DescribeFlight()
    {
        var bits = new List<string>();
        if (ThrottleUp != KeyMap.None || ThrottleDown != KeyMap.None)
            bits.Add($"{ThrottleUp} / {ThrottleDown} throttle");
        if (RollLeft != KeyMap.None || RollRight != KeyMap.None)
            bits.Add($"{RollLeft} / {RollRight} roll");
        if (MouseVertical == "Pitch")
            bits.Add(InvertPitch ? "mouse up pitches up" : "mouse up pitches down");
        if (MouseHorizontal == "Yaw") bits.Add("mouse left/right yaws");
        else if (MouseHorizontal == "Roll") bits.Add("mouse left/right rolls");
        return string.Join(". ", bits) + ".";
    }
}
