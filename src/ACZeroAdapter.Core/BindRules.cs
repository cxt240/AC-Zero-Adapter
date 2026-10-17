namespace ACZeroAdapter.Core;

public static class BindRules
{
    public static readonly string[] KeyOrder =
    [
        "Throttle up", "Throttle down", "Roll left", "Roll right", "Yaw left", "Yaw right",
        "Look left", "Look right", "Look up", "Look down", "Change view", "Boresight",
        "Radar", "Autopilot", "Autopilot 2", "Gun", "Missile", "Weapon", "Target",
    ];

    public static readonly string[] ClickOrder = ["Gun click", "Missile click", "Target click"];

    public static string? FindOwner(
        IDictionary<string, string> keys,
        string keyName,
        string? exceptControl,
        bool extraThrottle,
        bool altLook)
    {
        if (string.IsNullOrEmpty(keyName) || keyName == KeyMap.None) return null;

        foreach (var pair in keys)
        {
            if (pair.Key == exceptControl) continue;
            if (pair.Value == keyName) return pair.Key;
        }

        if (extraThrottle)
        {
            if (keyName == "Space" && exceptControl != "Throttle up") return "throttle";
            if (keyName == "Left Ctrl" && exceptControl != "Throttle down") return "throttle";
        }

        if (altLook && keyName == "Left Alt") return "mouse look";
        return null;
    }

    public static bool TryAssign(
        IDictionary<string, string> keys,
        string control,
        string keyName,
        bool extraThrottle,
        bool altLook,
        out string message)
    {
        var owner = FindOwner(keys, keyName, control, extraThrottle, altLook);
        if (owner is not null)
        {
            keys[control] = KeyMap.None;
            message = $"{keyName} is already {owner}, so this control was left unset.";
            return false;
        }

        keys[control] = keyName;
        message = "";
        return true;
    }

    public static bool TryEnableExtra(IDictionary<string, string> keys, out string message)
    {
        var blocked = new List<string>();
        foreach (var pair in keys)
        {
            if (pair.Value == "Space" && pair.Key != "Throttle up")
                blocked.Add($"Space is already {pair.Key}");
            if (pair.Value == "Left Ctrl" && pair.Key != "Throttle down")
                blocked.Add($"Left Ctrl is already {pair.Key}");
        }

        if (blocked.Count == 0)
        {
            message = "";
            return true;
        }

        message = string.Join(" and ", blocked) + ", so extra throttle was left off.";
        return false;
    }

    public static bool TryEnableAltLook(IDictionary<string, string> keys, out string message)
    {
        foreach (var pair in keys)
        {
            if (pair.Value == "Left Alt")
            {
                message = $"Left Alt is already {pair.Key}, so mouse look was left off.";
                return false;
            }
        }

        message = "";
        return true;
    }

    public static bool TryAssignClick(
        IDictionary<string, string> clicks,
        string control,
        string which,
        out string message)
    {
        if (string.IsNullOrEmpty(which) || which == "Off")
        {
            clicks[control] = "Off";
            message = "";
            return true;
        }

        foreach (var pair in clicks)
        {
            if (pair.Key == control) continue;
            if (pair.Value == which)
            {
                clicks[control] = "Off";
                message = $"{which} click is already {pair.Key}, so this control was left unset.";
                return false;
            }
        }

        clicks[control] = which;
        message = "";
        return true;
    }

    public static List<string> ClearDuplicates(
        IDictionary<string, string> keys,
        IDictionary<string, string> clicks,
        ref bool extraThrottle,
        ref bool altLook)
    {
        var notes = new List<string>();
        var seen = new Dictionary<string, string>();
        foreach (var control in KeyOrder)
        {
            if (!keys.TryGetValue(control, out var key) || string.IsNullOrEmpty(key) || key == KeyMap.None)
                continue;
            if (seen.TryGetValue(key, out var owner))
            {
                keys[control] = KeyMap.None;
                notes.Add($"{control} was unset because {key} is already {owner}");
            }
            else
            {
                seen[key] = control;
            }
        }

        var seenClicks = new Dictionary<string, string>();
        foreach (var control in ClickOrder)
        {
            if (!clicks.TryGetValue(control, out var which) || string.IsNullOrEmpty(which) || which == "Off")
                continue;
            if (seenClicks.TryGetValue(which, out var owner))
            {
                clicks[control] = "Off";
                notes.Add($"{control} was unset because {which} is already {owner}");
            }
            else
            {
                seenClicks[which] = control;
            }
        }

        if (extraThrottle && !TryEnableExtra(keys, out var extraMessage))
        {
            extraThrottle = false;
            notes.Add(extraMessage);
        }

        if (altLook && !TryEnableAltLook(keys, out var altMessage))
        {
            altLook = false;
            notes.Add(altMessage);
        }

        return notes;
    }
}
