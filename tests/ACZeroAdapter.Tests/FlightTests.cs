using ACZeroAdapter.Core;

namespace ACZeroAdapter.Tests;

public class FlightTests
{
    [Fact]
    public void DefaultLayout_WritesInvertedPitchAndMouseYaw()
    {
        var section = FlightIni.Build(FlightOptions.CreateDefault());

        Assert.Contains("RT=   0x11 ;DIK_W throttle up", section);
        Assert.Contains("LT=   0x1F ;DIK_S throttle down", section);
        Assert.Contains("LB=   !0x38 + MouseX:8:0 ;yaw left, not Left Alt", section);
        Assert.Contains("LY-=  !0x38 + MouseY:0:2 ;pitch up, not Left Alt", section);
        Assert.Contains("LY+=  !0x38 + MouseY:2:0 ;pitch down, not Left Alt", section);
        Assert.DoesNotContain("LX-=  !0x38 + MouseX", section);
    }

    [Fact]
    public void UncheckedPitch_UsesThePortDirection()
    {
        var opt = FlightOptions.CreateDefault();
        opt.InvertPitch = false;

        var section = FlightIni.Build(opt);

        Assert.Contains("LY-=  !0x38 + MouseY:2:0 ;pitch up, not Left Alt", section);
    }

    [Fact]
    public void RollMode_RoundTrips()
    {
        var opt = FlightOptions.CreateDefault();
        opt.MouseHorizontal = "Roll";

        var section = FlightIni.Build(opt);
        var loaded = FlightIni.Parse(section);

        Assert.Contains("LX-=  !0x38 + MouseX:8:0 ;roll left, not Left Alt", section);
        Assert.DoesNotContain("yaw left, not Left Alt", section);
        Assert.Equal("Roll", loaded.MouseHorizontal);
        Assert.Equal(8, loaded.HorizontalSpan);
    }

    [Fact]
    public void ClearedRadarAndCamera_RoundTrip()
    {
        var opt = FlightOptions.CreateDefault();
        opt.Radar = KeyMap.None;
        opt.AltLook = false;
        opt.CameraLeft = "Left";

        var loaded = FlightIni.Parse(FlightIni.Build(opt));

        Assert.Equal(KeyMap.None, loaded.Radar);
        Assert.False(loaded.AltLook);
        Assert.Equal("Left", loaded.CameraLeft);
    }

    [Fact]
    public void Apply_UpdatesOnlyFlightSectionAndDisplay()
    {
        var root = Path.Combine(Path.GetTempPath(), "aczero-adapter-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "settings"));
        File.WriteAllText(Path.Combine(root, "settings", "dinput.ini"), "keep\r\n[KeyboardFlight]\r\nLX-=  0x1E ;DIK_A\r\n");
        File.WriteAllText(Path.Combine(root, "settings", "setting.ini"), "Resolution=1920x1080\t;comment\r\nScreenMode=Windowed\t\t;Windowed Borderless\r\n");

        var opt = FlightOptions.CreateDefault();
        opt.ThrottleUp = "I";
        opt.HorizontalSpan = 12;
        GameFiles.Apply(root, FlightIni.Build(opt), "Windowed", "2560x1440");

        var dinput = File.ReadAllText(Path.Combine(root, "settings", "dinput.ini"));
        var setting = File.ReadAllText(Path.Combine(root, "settings", "setting.ini"));
        var loaded = FlightIni.Parse(dinput);

        Assert.StartsWith("keep", dinput);
        Assert.Contains("0x17 ;DIK_I throttle up", dinput);
        Assert.Contains("ScreenMode=Windowed", setting);
        Assert.Contains("Resolution=2560x1440", setting);
        Assert.Equal("I", loaded.ThrottleUp);
        Assert.Equal("Yaw", loaded.MouseHorizontal);
        Assert.Equal(12, loaded.HorizontalSpan);
        Assert.True(loaded.InvertPitch);
        Assert.Equal("M", loaded.Radar);
        Assert.Equal("Num 4", loaded.CameraLeft);
        Assert.Equal("Left", loaded.GunMouse);
        Assert.Equal("Z", loaded.Autopilot);
        Assert.True(loaded.AltLook);
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void OriginalCopy_ComesFromTheShippedSampleAndStaysPut()
    {
        var root = Path.Combine(Path.GetTempPath(), "aczero-original-" + Guid.NewGuid().ToString("N"));
        var settings = Path.Combine(root, "settings");
        Directory.CreateDirectory(settings);
        File.WriteAllText(Path.Combine(settings, "dinput.ini"), "live\r\n[KeyboardFlight]\r\nRT=   0x11 ;DIK_W\r\n");
        File.WriteAllText(Path.Combine(settings, "dinputSample.ini"), "stock\r\n[KeyboardFlight]\r\nLY-=  0x11 ;DIK_W\r\n");

        var copy = GameFiles.EnsureOriginalCopy(root);
        var section = GameFiles.ReadOriginalSection(root);
        File.WriteAllText(Path.Combine(settings, "dinputSample.ini"), "changed\r\n");
        GameFiles.EnsureOriginalCopy(root);

        Assert.Equal(Path.Combine(settings, GameFiles.OriginalCopyName), copy);
        Assert.StartsWith("[KeyboardFlight]", section);
        Assert.Contains("LY-=  0x11 ;DIK_W", section);
        Assert.DoesNotContain("changed", File.ReadAllText(copy));
        Assert.DoesNotContain("live", File.ReadAllText(copy));
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void ResolveRoot_AcceptsTheGameFolderOrItsParent()
    {
        var root = Path.Combine(Path.GetTempPath(), "aczero-root-" + Guid.NewGuid().ToString("N"));
        var game = Path.Combine(root, "ACZero");
        Directory.CreateDirectory(Path.Combine(game, "settings"));
        File.WriteAllText(Path.Combine(game, "settings", "dinput.ini"), "");

        Assert.Equal(game, GameFiles.ResolveRoot(game));
        Assert.Equal(game, GameFiles.ResolveRoot(root));
        Assert.Null(GameFiles.ResolveRoot(Path.GetTempPath()));
        Directory.Delete(root, recursive: true);
    }
}
