using ACZeroAdapter.Core;

namespace ACZeroAdapter.Tests;

public class BindRulesTests
{
    private static Dictionary<string, string> Sample()
    {
        var keys = FlightOptions.CreateDefault().ToKeys();
        return keys;
    }

    [Fact]
    public void TakenKey_UnsetsTheNewControl()
    {
        var keys = Sample();

        var accepted = BindRules.TryAssign(keys, "Yaw left", "W", extraThrottle: true, altLook: true, out var message);

        Assert.False(accepted);
        Assert.Equal(KeyMap.None, keys["Yaw left"]);
        Assert.Equal("W", keys["Throttle up"]);
        Assert.Contains("Throttle up", message);
    }

    [Fact]
    public void SameControl_CanKeepItsOwnKey()
    {
        var keys = Sample();

        var accepted = BindRules.TryAssign(keys, "Throttle up", "W", extraThrottle: true, altLook: true, out _);

        Assert.True(accepted);
        Assert.Equal("W", keys["Throttle up"]);
    }

    [Fact]
    public void ExtraThrottle_ReservesSpaceAndCtrl()
    {
        var keys = Sample();

        var accepted = BindRules.TryAssign(keys, "Radar", "Space", extraThrottle: true, altLook: false, out var message);

        Assert.False(accepted);
        Assert.Equal(KeyMap.None, keys["Radar"]);
        Assert.Contains("throttle", message);
    }

    [Fact]
    public void ExtraThrottle_RefusesWhenSpaceIsAlreadyUsed()
    {
        var keys = Sample();
        keys["Gun"] = "Space";

        var enabled = BindRules.TryEnableExtra(keys, out var message);

        Assert.False(enabled);
        Assert.Contains("Gun", message);
    }

    [Fact]
    public void MouseClick_CannotBeShared()
    {
        var clicks = FlightOptions.CreateDefault().ToClicks();

        var accepted = BindRules.TryAssignClick(clicks, "Missile click", "Left", out var message);

        Assert.False(accepted);
        Assert.Equal("Off", clicks["Missile click"]);
        Assert.Equal("Left", clicks["Gun click"]);
        Assert.Contains("Gun click", message);
    }

    [Fact]
    public void ClearDuplicates_KeepsTheFirstControl()
    {
        var keys = Sample();
        keys["Radar"] = "W";
        var clicks = FlightOptions.CreateDefault().ToClicks();
        clicks["Missile click"] = "Left";
        var extra = true;
        var alt = true;

        var notes = BindRules.ClearDuplicates(keys, clicks, ref extra, ref alt);

        Assert.Equal("W", keys["Throttle up"]);
        Assert.Equal(KeyMap.None, keys["Radar"]);
        Assert.Equal("Left", clicks["Gun click"]);
        Assert.Equal("Off", clicks["Missile click"]);
        Assert.NotEmpty(notes);
    }
}
