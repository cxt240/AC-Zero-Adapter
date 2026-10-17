using System.Globalization;
using System.Text.RegularExpressions;

namespace ACZeroAdapter.Core;

public static class FlightIni
{
    private static readonly Regex Unbound = new(@"^;\s*unbound\s+(\S+)", RegexOptions.Compiled);
    private static readonly Regex MouseButton = new(@"^(?<act>A|B|Y)=\s*Mouse(?<n>[1-4])\b", RegexOptions.Compiled);
    private static readonly Regex Corner = new(@"^RX[+-]=\s*0x[0-9A-Fa-f]+:(16384|49152)", RegexOptions.Compiled);
    private static readonly Regex ActionKey = new(@"^(?<act>RX-|RX\+|RY-|RY\+|LSC|RSC|View|A|B|X|Y)=\s*(?<hex>0x[0-9A-Fa-f]+)\b", RegexOptions.Compiled);
    private static readonly Regex Axis = new(@"^(?<act>LT|RT|LX-|LX\+|LB|RB)=\s*(?:!\s*0x38\s*\+\s*)?(?<token>Mouse[XY]|0x[0-9A-Fa-f]+)(?::(?<a>\d+):(?<b>\d+))?", RegexOptions.Compiled);
    private static readonly Regex PitchMouse = new(@"^LY-=\s*(?:!\s*0x38\s*\+\s*)?MouseY:(?<a>\d+):(?<b>\d+)", RegexOptions.Compiled);

    private static readonly Dictionary<string, string> ActionFields = new()
    {
        ["RX-"] = "CameraLeft", ["RX+"] = "CameraRight", ["RY-"] = "CameraUp", ["RY+"] = "CameraDown",
        ["LSC"] = "Boresight", ["RSC"] = "CameraMode", ["View"] = "Radar",
        ["A"] = "Gun", ["B"] = "Missile", ["X"] = "Weapon", ["Y"] = "Target",
    };

    private static readonly Dictionary<string, string> MouseFields = new()
    {
        ["A"] = "GunMouse", ["B"] = "MissileMouse", ["Y"] = "TargetMouse",
    };

    private static readonly Dictionary<string, string> MouseNames = new()
    {
        ["1"] = "Left", ["2"] = "Right", ["3"] = "Middle", ["4"] = "Mouse 4",
    };

    public static string Build(FlightOptions opt)
    {
        var pitch = Math.Max(1, opt.PitchSpan);
        var horizontal = Math.Max(1, opt.HorizontalSpan);
        var summary = new List<string>();
        if (opt.ThrottleUp != KeyMap.None || opt.ThrottleDown != KeyMap.None)
            summary.Add($"{opt.ThrottleUp}/{opt.ThrottleDown} throttle");
        if (opt.RollLeft != KeyMap.None || opt.RollRight != KeyMap.None)
            summary.Add($"{opt.RollLeft}/{opt.RollRight} roll");
        if (opt.MouseVertical == "Pitch")
            summary.Add(opt.InvertPitch ? "mouse up pitches up" : "mouse up pitches down");
        if (opt.MouseHorizontal == "Yaw") summary.Add("mouse left/right yaw");
        else if (opt.MouseHorizontal == "Roll") summary.Add("mouse left/right roll");

        var lines = new List<string>
        {
            "[KeyboardFlight]",
            "; " + string.Join(", ", summary) + "." + (opt.AltLook ? " Hold Left Alt to look around." : ""),
            $"; Mouse span is full deflection: smaller number = more sensitive. Pitch {pitch}, horizontal {horizontal}.",
            "HatL= 0xCB ;DIK_LEFT",
            "HatR= 0xCD ;DIK_RIGHT",
            "HatU= 0xC8 ;DIK_UP",
            "HatD= 0xD0 ;DIK_DOWN",
        };

        AddKey(lines, "LX-", opt.RollLeft, "roll left");
        AddKey(lines, "LX+", opt.RollRight, "roll right");
        if (opt.MouseHorizontal == "Roll")
        {
            lines.Add($"LX-=  !0x38 + MouseX:{horizontal}:0 ;roll left, not Left Alt");
            lines.Add($"LX+=  !0x38 + MouseX:0:{horizontal} ;roll right, not Left Alt");
        }

        if (opt.MouseVertical == "Pitch")
        {
            if (opt.InvertPitch)
            {
                lines.Add($"LY-=  !0x38 + MouseY:0:{pitch} ;pitch up, not Left Alt");
                lines.Add($"LY+=  !0x38 + MouseY:{pitch}:0 ;pitch down, not Left Alt");
            }
            else
            {
                lines.Add($"LY-=  !0x38 + MouseY:{pitch}:0 ;pitch up, not Left Alt");
                lines.Add($"LY+=  !0x38 + MouseY:0:{pitch} ;pitch down, not Left Alt");
            }
        }

        AddOptional(lines, "RX-", opt.CameraLeft, "camera left", "CameraLeft");
        AddOptional(lines, "RX+", opt.CameraRight, "camera right", "CameraRight");
        AddOptional(lines, "RY-", opt.CameraUp, "camera up", "CameraUp");
        AddOptional(lines, "RY+", opt.CameraDown, "camera down", "CameraDown");
        if (opt.CornerLook)
        {
            lines.Add("RX-=  0x47:16384 ;DIK_NUMPAD7");
            lines.Add("RX+=  0x49:16384 ;DIK_NUMPAD9");
            lines.Add("RX-=  0x4F:49152 ;DIK_NUMPAD1");
            lines.Add("RX+=  0x51:49152 ;DIK_NUMPAD3");
        }

        if (opt.AltLook)
        {
            lines.Add("RX-=  0x38 + MouseAX:256:0 ;DIK_LMENU(Alt) + Mouse Abs");
            lines.Add("RX+=  0x38 + MouseAX:0:256 ;DIK_LMENU(Alt) + Mouse Abs");
            lines.Add("RY-=  0x38 + MouseAY:256:0 ;DIK_LMENU(Alt) + Mouse Abs");
            lines.Add("RY+=  0x38 + MouseAY:0:256 ;DIK_LMENU(Alt) + Mouse Abs");
        }

        AddKey(lines, "LT", opt.ThrottleDown, "throttle down");
        if (opt.ExtraThrottle && opt.ThrottleDown != "Left Ctrl")
            lines.Add("LT=   0x1D ;DIK_LCONTROL throttle down");
        AddKey(lines, "RT", opt.ThrottleUp, "throttle up");
        if (opt.ExtraThrottle && opt.ThrottleUp != "Space")
            lines.Add("RT=   0x39 ;DIK_SPACE throttle up");
        AddKey(lines, "LB", opt.YawLeft, "yaw left");
        AddKey(lines, "RB", opt.YawRight, "yaw right");
        if (opt.MouseHorizontal == "Yaw")
        {
            lines.Add($"LB=   !0x38 + MouseX:{horizontal}:0 ;yaw left, not Left Alt");
            lines.Add($"RB=   !0x38 + MouseX:0:{horizontal} ;yaw right, not Left Alt");
        }

        var autos = new[] { opt.Autopilot, opt.Autopilot2 }
            .Where(name => !string.IsNullOrEmpty(name) && name != KeyMap.None)
            .Distinct()
            .ToArray();
        foreach (var name in autos)
        {
            AddKey(lines, "LB", name, "autopilot (both shoulders)");
            AddKey(lines, "RB", name, "autopilot (both shoulders)");
        }

        if (autos.Length == 0) lines.Add("; unbound Autopilot");

        AddOptional(lines, "A", opt.Gun, "gun / confirm", "Gun");
        AddOptional(lines, "B", opt.Missile, "missile / cancel", "Missile");
        AddOptional(lines, "X", opt.Weapon, "switch weapon", "Weapon");
        AddOptional(lines, "Y", opt.Target, "switch target", "Target");
        AddOptional(lines, "LSC", opt.Boresight, "boresight", "Boresight");
        AddOptional(lines, "RSC", opt.CameraMode, "camera mode", "CameraMode");
        AddOptional(lines, "View", opt.Radar, "radar", "Radar");

        AddMouse(lines, "A", opt.GunMouse);
        AddMouse(lines, "B", opt.MissileMouse);
        AddMouse(lines, "Y", opt.TargetMouse);
        if (opt.WeaponWheel)
        {
            lines.Add("X=    MouseZ:0:1 ;wheel");
            lines.Add("X=    MouseZ:1:0 ;wheel");
        }

        lines.Add("Menu= 0x01 ;DIK_ESCAPE");
        lines.Add("Menu= 0x0E ;DIK_BACK");
        return string.Join("\r\n", lines) + "\r\n";
    }

    public static FlightOptions Parse(string text)
    {
        var start = text.LastIndexOf("[KeyboardFlight]", StringComparison.Ordinal);
        if (start < 0) throw new InvalidDataException("dinput.ini has no [KeyboardFlight] section.");
        var section = text[start..];
        var opt = new FlightOptions
        {
            ExtraThrottle = true,
            InvertPitch = false,
            AltLook = false,
            CornerLook = false,
            GunMouse = "Off",
            MissileMouse = "Off",
            TargetMouse = "Off",
            WeaponWheel = false,
            Autopilot = KeyMap.None,
            Autopilot2 = KeyMap.None,
        };
        var seen = new HashSet<string>();
        var buckets = new Dictionary<string, List<string>>
        {
            ["LT"] = [], ["RT"] = [], ["LX-"] = [], ["LX+"] = [], ["LB"] = [], ["RB"] = [],
        };

        foreach (var line in section.Split(["\r\n", "\n"], StringSplitOptions.None))
        {
            var unbound = Unbound.Match(line);
            if (unbound.Success)
            {
                SetField(opt, unbound.Groups[1].Value, KeyMap.None);
                seen.Add(unbound.Groups[1].Value);
                continue;
            }

            if (line.Contains("MouseAX", StringComparison.Ordinal) || line.Contains("MouseAY", StringComparison.Ordinal))
            {
                opt.AltLook = true;
                continue;
            }

            if (Regex.IsMatch(line, @"^X=\s*MouseZ"))
            {
                opt.WeaponWheel = true;
                continue;
            }

            var mouse = MouseButton.Match(line);
            if (mouse.Success)
            {
                SetField(opt, MouseFields[mouse.Groups["act"].Value], MouseNames[mouse.Groups["n"].Value]);
                continue;
            }

            if (Corner.IsMatch(line))
            {
                opt.CornerLook = true;
                continue;
            }

            var action = ActionKey.Match(line);
            if (action.Success)
            {
                var field = ActionFields[action.Groups["act"].Value];
                if (seen.Add(field))
                    SetField(opt, field, KeyMap.NameFromCode(Convert.ToInt32(action.Groups["hex"].Value, 16)));
                continue;
            }

            var axis = Axis.Match(line);
            if (!axis.Success)
            {
                var pitch = PitchMouse.Match(line);
                if (pitch.Success)
                {
                    opt.MouseVertical = "Pitch";
                    opt.PitchSpan = Span(pitch.Groups["a"].Value, pitch.Groups["b"].Value);
                    opt.InvertPitch = int.Parse(pitch.Groups["a"].Value, CultureInfo.InvariantCulture)
                        < int.Parse(pitch.Groups["b"].Value, CultureInfo.InvariantCulture);
                }

                continue;
            }

            var act = axis.Groups["act"].Value;
            var token = axis.Groups["token"].Value;
            if (token.StartsWith("MouseX", StringComparison.Ordinal))
            {
                opt.MouseHorizontal = act is "LB" or "RB" ? "Yaw" : "Roll";
                if (axis.Groups["a"].Success)
                    opt.HorizontalSpan = Span(axis.Groups["a"].Value, axis.Groups["b"].Value);
                continue;
            }

            if (token.StartsWith("Mouse", StringComparison.Ordinal)) continue;
            buckets[act].Add(KeyMap.NameFromCode(Convert.ToInt32(token, 16)));
        }

        var down = SelectPrimary(buckets["LT"], "Left Ctrl");
        var up = SelectPrimary(buckets["RT"], "Space");
        opt.ThrottleDown = down.Key;
        opt.ThrottleUp = up.Key;
        opt.ExtraThrottle = down.Extra || up.Extra;
        if (buckets["LX-"].Count > 0) opt.RollLeft = buckets["LX-"][0];
        if (buckets["LX+"].Count > 0) opt.RollRight = buckets["LX+"][0];

        var auto = buckets["LB"].Where(key => buckets["RB"].Contains(key)).Distinct().ToList();
        if (auto.Count >= 1) opt.Autopilot = auto[0];
        if (auto.Count >= 2) opt.Autopilot2 = auto[1];
        var yawLeft = buckets["LB"].FirstOrDefault(key => !auto.Contains(key));
        var yawRight = buckets["RB"].FirstOrDefault(key => !auto.Contains(key));
        if (yawLeft is not null) opt.YawLeft = yawLeft;
        if (yawRight is not null) opt.YawRight = yawRight;
        if (!Regex.IsMatch(section, "MouseY")) opt.MouseVertical = "Off";
        if (!Regex.IsMatch(section, "MouseX")) opt.MouseHorizontal = "Off";
        return opt;
    }

    private static int Span(string a, string b)
    {
        var left = int.Parse(a, CultureInfo.InvariantCulture);
        var right = int.Parse(b, CultureInfo.InvariantCulture);
        if (left > 0) return left;
        if (right > 0) return right;
        return 1;
    }

    private static (string Key, bool Extra) SelectPrimary(List<string> names, string extraName)
    {
        var primary = names.Where(name => !string.IsNullOrEmpty(name) && name != extraName).ToList();
        var extra = names.Any(name => name == extraName);
        if (primary.Count > 0) return (primary[0], extra);
        if (names.Count > 0) return (names[0], false);
        return (KeyMap.None, false);
    }

    private static void AddKey(List<string> lines, string action, string keyName, string note)
    {
        var line = FormatKey(action, keyName, note);
        if (line is not null) lines.Add(line);
    }

    private static void AddOptional(List<string> lines, string action, string keyName, string note, string field)
    {
        var line = FormatKey(action, keyName, note);
        lines.Add(line ?? $"; unbound {field}");
    }

    private static string? FormatKey(string action, string keyName, string note)
    {
        var key = KeyMap.ByName(keyName);
        if (key.Code is null) return null;
        return $"{(action + "=").PadRight(6)}{key.Hex.PadRight(5)};{key.Dik} {note}".TrimEnd();
    }

    private static void AddMouse(List<string> lines, string action, string which)
    {
        var token = which switch
        {
            "Left" => "Mouse1 ;",
            "Right" => "Mouse2 ;",
            "Middle" => "Mouse3 ;",
            "Mouse 4" => "Mouse4 ;",
            _ => null,
        };
        if (token is not null) lines.Add((action + "=").PadRight(6) + token);
    }

    private static void SetField(FlightOptions opt, string field, string value)
    {
        switch (field)
        {
            case "CameraLeft": opt.CameraLeft = value; break;
            case "CameraRight": opt.CameraRight = value; break;
            case "CameraUp": opt.CameraUp = value; break;
            case "CameraDown": opt.CameraDown = value; break;
            case "CameraMode": opt.CameraMode = value; break;
            case "Boresight": opt.Boresight = value; break;
            case "Radar": opt.Radar = value; break;
            case "Gun": opt.Gun = value; break;
            case "Missile": opt.Missile = value; break;
            case "Weapon": opt.Weapon = value; break;
            case "Target": opt.Target = value; break;
            case "GunMouse": opt.GunMouse = value; break;
            case "MissileMouse": opt.MissileMouse = value; break;
            case "TargetMouse": opt.TargetMouse = value; break;
            case "Autopilot": opt.Autopilot = value; break;
        }
    }
}
