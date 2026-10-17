using System.Text;
using System.Text.RegularExpressions;

namespace ACZeroAdapter.Core;

public static class GameFiles
{
    public static readonly string[] Resolutions =
    [
        "3840x2160", "3200x1800", "2560x1440", "2048x1152", "1920x1080", "1600x900", "1280x720",
        "2880x2160", "2400x1800", "1920x1440", "1536x1152", "1440x1080", "1200x900", "960x720",
    ];

    public const string DefaultRoot = @"D:\SteamLibrary\steamapps\common\ACE COMBAT 8\ACZero";

    public static string? ResolveRoot(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (File.Exists(Path.Combine(path, "settings", "dinput.ini"))) return path;
        var nested = Path.Combine(path, "ACZero");
        if (File.Exists(Path.Combine(nested, "settings", "dinput.ini"))) return nested;
        return null;
    }

    public static (string ScreenMode, string Resolution) ReadDisplay(string gameRoot)
    {
        var text = File.ReadAllText(Path.Combine(gameRoot, "settings", "setting.ini"));
        var mode = Regex.Match(text, @"(?m)^ScreenMode=(\S+)");
        var resolution = Regex.Match(text, @"(?m)^Resolution=(\d+x\d+)");
        return (
            mode.Success ? mode.Groups[1].Value : "Borderless",
            resolution.Success ? resolution.Groups[1].Value : "1920x1080");
    }

    public const string OriginalCopyName = "dinput.original.ini";

    public static FlightOptions ReadFlight(string gameRoot) =>
        FlightIni.Parse(File.ReadAllText(Path.Combine(gameRoot, "settings", "dinput.ini")));

    public static string EnsureOriginalCopy(string gameRoot)
    {
        var settings = Path.Combine(gameRoot, "settings");
        var destination = Path.Combine(settings, OriginalCopyName);
        if (File.Exists(destination)) return destination;

        foreach (var name in new[] { "dinputSample.ini", "dinput.ini.bak", "dinput.ini" })
        {
            var source = Path.Combine(settings, name);
            if (!File.Exists(source)) continue;
            File.Copy(source, destination);
            return destination;
        }

        throw new FileNotFoundException("Could not find a default dinput.ini to copy for restore.", destination);
    }

    public static string ReadOriginalSection(string gameRoot)
    {
        var text = File.ReadAllText(EnsureOriginalCopy(gameRoot));
        var marker = text.LastIndexOf("[KeyboardFlight]", StringComparison.Ordinal);
        if (marker < 0) throw new InvalidDataException("The restore copy has no [KeyboardFlight] section.");
        var section = text[marker..];
        if (!section.EndsWith('\n')) section += "\r\n";
        return section;
    }

    public static void Apply(string gameRoot, string section, string screenMode, string resolution)
    {
        EnsureOriginalCopy(gameRoot);
        var settings = Path.Combine(gameRoot, "settings");
        var dinput = Path.Combine(settings, "dinput.ini");
        var setting = Path.Combine(settings, "setting.ini");
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        foreach (var path in new[] { dinput, setting })
        {
            var backup = path + ".before-adapter";
            if (!File.Exists(backup)) File.Copy(path, backup);
        }

        var dinputText = File.ReadAllText(dinput);
        var marker = dinputText.LastIndexOf("[KeyboardFlight]", StringComparison.Ordinal);
        if (marker < 0) throw new InvalidDataException("dinput.ini has no [KeyboardFlight] section.");
        var newline = dinputText.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var sectionToWrite = section.Replace("\r\n", "\n").Replace("\n", newline).TrimEnd() + newline;
        File.WriteAllText(dinput, dinputText[..marker] + sectionToWrite, utf8);

        var settingText = File.ReadAllText(setting);
        var screen = new Regex(@"(?m)^(ScreenMode=)\S+");
        var updated = screen.Replace(settingText, match => match.Groups[1].Value + screenMode, 1);
        if (updated == settingText && !Regex.IsMatch(settingText, @"(?m)^ScreenMode="))
            throw new InvalidDataException("setting.ini has no ScreenMode line.");
        var size = new Regex(@"(?m)^(Resolution=)\d+x\d+");
        updated = size.Replace(updated, match => match.Groups[1].Value + resolution, 1);
        File.WriteAllText(setting, updated, utf8);
    }
}
