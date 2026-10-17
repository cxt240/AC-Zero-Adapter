using ACZeroAdapter.Core;

namespace ACZeroAdapter;

public sealed class MainForm : Form
{
    private readonly Dictionary<string, string> _keys = FlightOptions.CreateDefault().ToKeys();
    private readonly Dictionary<string, string> _clicks = FlightOptions.CreateDefault().ToClicks();
    private readonly Dictionary<string, Button> _buttons = new();
    private readonly TextBox _path = new();
    private readonly RadioButton _borderless = new() { Text = "Borderless fullscreen", Checked = true, AutoSize = true };
    private readonly RadioButton _windowed = new() { Text = "Windowed", AutoSize = true };
    private readonly ComboBox _resolution = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly ComboBox _mouseY = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 134 };
    private readonly ComboBox _mouseX = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
    private readonly NumericUpDown _pitchSpan = new() { Minimum = 1, Maximum = 200, Value = 2, Width = 80 };
    private readonly NumericUpDown _sideSpan = new() { Minimum = 1, Maximum = 200, Value = 8, Width = 80 };
    private readonly CheckBox _extra = new() { Text = "Also bind Space and Left Ctrl to throttle", Checked = true, AutoSize = true };
    private readonly CheckBox _invert = new() { Text = "Mouse up raises the nose", Checked = true, AutoSize = true };
    private readonly CheckBox _altLook = new() { Text = "Hold Left Alt and move the mouse to look", Checked = true, AutoSize = true };
    private readonly CheckBox _corners = new() { Text = "Numpad corners also look", Checked = true, AutoSize = true };
    private readonly CheckBox _wheel = new() { Text = "Mouse wheel switches weapon", Checked = true, AutoSize = true };
    private readonly ComboBox _gunClick = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly ComboBox _missileClick = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly ComboBox _targetClick = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly Label _preview = new() { AutoSize = false, Height = 40, Width = 680 };
    private readonly Label _status = new() { AutoSize = false, Height = 40, Width = 390 };
    private readonly Panel _flight = new() { Width = 712, Height = 440 };
    private readonly Panel _systems = new() { Width = 712, Height = 440, Visible = false };
    private Button? _capture;
    private bool _suppress;

    private static readonly Color Bg = Color.FromArgb(22, 24, 28);
    private static readonly Color Card = Color.FromArgb(34, 38, 46);
    private static readonly Color Field = Color.FromArgb(46, 51, 61);
    private static readonly Color Line = Color.FromArgb(78, 86, 99);
    private static readonly Color Ink = Color.FromArgb(28, 22, 10);
    private static readonly Color Amber = Color.FromArgb(232, 176, 74);
    private static readonly Color Paper = Color.FromArgb(236, 238, 241);
    private static readonly Color Muted = Color.FromArgb(164, 172, 184);

    public MainForm()
    {
        Text = "AC Zero Adapter";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(760, 832);
        BackColor = Bg;
        ForeColor = Paper;
        Font = new Font("Segoe UI", 10);
        KeyPreview = true;

        Controls.Add(Label("AC Zero Adapter", 24, 16, 420, 36, Paper, new Font("Segoe UI", 20, FontStyle.Bold)));
        Controls.Add(Label("Flight controls and fullscreen for the Ace Combat Zero port.", 26, 54, 680, 24, Muted, Font));
        Controls.Add(Label("Game folder", 24, 88, 200, 20, Muted, Font));
        _path.SetBounds(24, 110, 548, 28);
        StyleField(_path);
        _path.Text = GameFiles.DefaultRoot;
        _path.Leave += (_, _) => LoadFromPath(_path.Text);
        Controls.Add(_path);
        var browse = Button("Browse", 584, 108, 152, 32, Card, Paper);
        browse.Click += (_, _) => Browse();
        Controls.Add(browse);

        var display = CardPanel(24, 156, 712, 112);
        display.Controls.Add(Label("Display", 16, 12, 200, 22, Paper, new Font(Font, FontStyle.Bold)));
        StyleChoice(_borderless, 16, 44);
        StyleChoice(_windowed, 240, 44);
        display.Controls.Add(_borderless);
        display.Controls.Add(_windowed);
        display.Controls.Add(Label("Resolution", 16, 76, 90, 24, Muted, Font));
        _resolution.Location = new Point(112, 72);
        StyleCombo(_resolution);
        _resolution.Items.AddRange(GameFiles.Resolutions);
        _resolution.SelectedItem = "1920x1080";
        display.Controls.Add(_resolution);
        var desktop = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
        display.Controls.Add(Label($"This desktop is {desktop.Width}x{desktop.Height}. Borderless fills that screen.", 280, 74, 410, 24, Muted, Font));
        Controls.Add(display);

        var flightTab = Button("Flight", 24, 280, 120, 32, Amber, Ink);
        var systemsTab = Button("Camera and weapons", 152, 280, 190, 32, Card, Paper);
        flightTab.Font = new Font(Font, FontStyle.Bold);
        flightTab.Click += (_, _) => ShowPage(true, flightTab, systemsTab);
        systemsTab.Click += (_, _) => ShowPage(false, flightTab, systemsTab);
        Controls.Add(flightTab);
        Controls.Add(systemsTab);

        _flight.Location = new Point(24, 320);
        _flight.BackColor = Card;
        BuildFlight();
        Controls.Add(_flight);
        _systems.Location = new Point(24, 320);
        _systems.BackColor = Card;
        BuildSystems();
        Controls.Add(_systems);

        var apply = Button("Apply", 24, 776, 140, 36, Amber, Ink);
        apply.Font = new Font(Font, FontStyle.Bold);
        apply.Click += (_, _) => ApplySettings();
        var restore = Button("Restore original", 176, 776, 160, 36, Card, Paper);
        restore.Click += (_, _) => RestoreOriginal();
        _status.SetBounds(348, 776, 390, 40);
        _status.ForeColor = Muted;
        _status.Text = "A key already used by one control is left unset on the other.";
        Controls.Add(apply);
        Controls.Add(restore);
        Controls.Add(_status);
        KeyDown += OnKeyDown;
        LoadFromPath(_path.Text);
    }

    private void BuildFlight()
    {
        _flight.Controls.Add(Label("Flight", 16, 12, 200, 22, Paper, new Font(Font, FontStyle.Bold)));
        AddRow(_flight, "Throttle up", "Throttle down", 48);
        AddRow(_flight, "Roll left", "Roll right", 88);
        AddRow(_flight, "Yaw left", "Yaw right", 128);
        _flight.Controls.Add(Label("Mouse up/down", 16, 174, 120, 28, Muted, Font));
        _mouseY.Location = new Point(144, 170);
        StyleCombo(_mouseY);
        _mouseY.Items.AddRange(["Pitch", "Off"]);
        _mouseY.SelectedItem = "Pitch";
        _mouseY.SelectedIndexChanged += (_, _) => RefreshPreview();
        _flight.Controls.Add(_mouseY);
        _flight.Controls.Add(Label("Mouse left/right", 360, 174, 130, 28, Muted, Font));
        _mouseX.Location = new Point(500, 170);
        StyleCombo(_mouseX);
        _mouseX.Items.AddRange(["Yaw", "Roll", "Off"]);
        _mouseX.SelectedItem = "Yaw";
        _mouseX.SelectedIndexChanged += (_, _) => RefreshPreview();
        _flight.Controls.Add(_mouseX);
        _flight.Controls.Add(Label("Pitch sensitivity", 16, 214, 130, 28, Muted, Font));
        _pitchSpan.Location = new Point(154, 210);
        StyleNumber(_pitchSpan);
        _pitchSpan.ValueChanged += (_, _) => RefreshPreview();
        _flight.Controls.Add(_pitchSpan);
        _flight.Controls.Add(Label("Side sensitivity", 360, 214, 120, 28, Muted, Font));
        _sideSpan.Location = new Point(500, 210);
        StyleNumber(_sideSpan);
        _sideSpan.ValueChanged += (_, _) => RefreshPreview();
        _flight.Controls.Add(_sideSpan);
        _extra.Location = new Point(16, 250);
        _extra.ForeColor = Paper;
        _extra.BackColor = Card;
        _extra.CheckedChanged += (_, _) =>
        {
            if (_suppress || !_extra.Checked) { RefreshPreview(); return; }
            if (!BindRules.TryEnableExtra(_keys, out var message))
            {
                _suppress = true;
                _extra.Checked = false;
                _suppress = false;
                _status.Text = message;
            }
            RefreshPreview();
        };
        _flight.Controls.Add(_extra);
        _invert.Location = new Point(400, 250);
        _invert.ForeColor = Paper;
        _invert.BackColor = Card;
        _invert.CheckedChanged += (_, _) => RefreshPreview();
        _flight.Controls.Add(_invert);
        _preview.Location = new Point(16, 286);
        _preview.ForeColor = Muted;
        _flight.Controls.Add(_preview);
        _flight.Controls.Add(Label("Camera, radar, guns, and autopilot are on Camera and weapons.", 16, 340, 660, 24, Muted, Font));
    }

    private void BuildSystems()
    {
        _systems.Controls.Add(Label("Camera", 16, 12, 90, 22, Paper, new Font(Font, FontStyle.Bold)));
        _systems.Controls.Add(Label("Change view cycles third person and cockpit.", 110, 14, 420, 22, Muted, Font));
        AddRow(_systems, "Look left", "Look right", 48);
        AddRow(_systems, "Look up", "Look down", 88);
        AddRow(_systems, "Change view", "Boresight", 128);
        _altLook.Location = new Point(16, 168);
        _altLook.ForeColor = Paper;
        _altLook.BackColor = Card;
        _altLook.CheckedChanged += (_, _) =>
        {
            if (_suppress || !_altLook.Checked) return;
            if (!BindRules.TryEnableAltLook(_keys, out var message))
            {
                _suppress = true;
                _altLook.Checked = false;
                _suppress = false;
                _status.Text = message;
            }
        };
        _systems.Controls.Add(_altLook);
        _corners.Location = new Point(390, 168);
        _corners.ForeColor = Paper;
        _corners.BackColor = Card;
        _systems.Controls.Add(_corners);
        _systems.Controls.Add(Label("Radar and weapons", 16, 204, 200, 22, Paper, new Font(Font, FontStyle.Bold)));
        _wheel.Location = new Point(360, 204);
        _wheel.ForeColor = Paper;
        _wheel.BackColor = Card;
        _systems.Controls.Add(_wheel);
        AddRow(_systems, "Radar", "Autopilot", 240);
        AddRow(_systems, "Autopilot 2", "Gun", 280);
        AddRow(_systems, "Missile", "Weapon", 320);
        AddRow(_systems, "Target", "", 360);
        _systems.Controls.Add(Label("Target click", 360, 364, 120, 28, Muted, Font));
        PlaceClick(_targetClick, "Target click", 490, 360);
        _systems.Controls.Add(Label("Gun click", 16, 404, 110, 28, Muted, Font));
        PlaceClick(_gunClick, "Gun click", 142, 400);
        _systems.Controls.Add(Label("Missile click", 360, 404, 120, 28, Muted, Font));
        PlaceClick(_missileClick, "Missile click", 490, 400);
    }

    private void AddRow(Control parent, string left, string right, int y)
    {
        parent.Controls.Add(Label(left, 16, y, 124, 32, Muted, Font));
        parent.Controls.Add(KeyButton(left, 142, y - 2));
        if (right.Length > 0)
        {
            parent.Controls.Add(Label(right, 360, y, 124, 32, Muted, Font));
            parent.Controls.Add(KeyButton(right, 490, y - 2));
        }
    }

    private Button KeyButton(string control, int x, int y)
    {
        var button = Button(_keys[control], x, y, 150, 32, Field, Paper);
        button.Tag = control;
        button.Click += (_, _) =>
        {
            if (_capture is not null && _capture != button)
                _capture.Text = _keys[(string)_capture.Tag!];
            _capture = button;
            button.Text = "Press a key";
            button.FlatAppearance.BorderColor = Amber;
        };
        _buttons[control] = button;
        return button;
    }

    private void PlaceClick(ComboBox box, string control, int x, int y)
    {
        box.Location = new Point(x, y);
        StyleCombo(box);
        box.Items.AddRange(["Off", "Left", "Right", "Middle", "Mouse 4"]);
        box.SelectedItem = _clicks[control];
        box.Tag = control;
        box.SelectedIndexChanged += (_, _) =>
        {
            if (_suppress) return;
            var which = box.SelectedItem?.ToString() ?? "Off";
            if (!BindRules.TryAssignClick(_clicks, control, which, out var message))
            {
                _suppress = true;
                box.SelectedItem = "Off";
                _suppress = false;
                _status.Text = message;
            }
        };
        box.Parent = _systems;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_capture is null) return;
        e.SuppressKeyPress = true;
        e.Handled = true;
        var control = (string)_capture.Tag!;
        _capture.FlatAppearance.BorderColor = Line;
        if (e.KeyCode == Keys.Escape)
        {
            _capture.Text = _keys[control];
            _capture = null;
            return;
        }

        var name = NameFromKey(e.KeyCode);
        if (name is null) return;
        if (!BindRules.TryAssign(_keys, control, name, _extra.Checked, _altLook.Checked, out var message))
        {
            _capture.Text = KeyMap.None;
            _status.Text = message;
        }
        else
        {
            _capture.Text = name;
        }

        _capture = null;
        RefreshPreview();
    }

    private void ShowOptions(FlightOptions opt)
    {
        _suppress = true;
        foreach (var pair in _buttons)
            pair.Value.Text = _keys[pair.Key];
        _mouseY.SelectedItem = opt.MouseVertical;
        _mouseX.SelectedItem = opt.MouseHorizontal;
        _pitchSpan.Value = Math.Clamp(opt.PitchSpan, 1, 200);
        _sideSpan.Value = Math.Clamp(opt.HorizontalSpan, 1, 200);
        _extra.Checked = opt.ExtraThrottle;
        _invert.Checked = opt.InvertPitch;
        _altLook.Checked = opt.AltLook;
        _corners.Checked = opt.CornerLook;
        _wheel.Checked = opt.WeaponWheel;
        _gunClick.SelectedItem = _clicks["Gun click"];
        _missileClick.SelectedItem = _clicks["Missile click"];
        _targetClick.SelectedItem = _clicks["Target click"];
        _suppress = false;
        RefreshPreview();
    }

    private FlightOptions ReadOptions()
    {
        var opt = new FlightOptions
        {
            MouseVertical = _mouseY.SelectedItem?.ToString() ?? "Pitch",
            MouseHorizontal = _mouseX.SelectedItem?.ToString() ?? "Yaw",
            PitchSpan = (int)_pitchSpan.Value,
            HorizontalSpan = (int)_sideSpan.Value,
            ExtraThrottle = _extra.Checked,
            InvertPitch = _invert.Checked,
            AltLook = _altLook.Checked,
            CornerLook = _corners.Checked,
            WeaponWheel = _wheel.Checked,
        };
        opt.ApplyKeys(_keys, _clicks);
        return opt;
    }

    private void LoadFromPath(string path)
    {
        var root = GameFiles.ResolveRoot(path);
        if (root is null)
        {
            _status.Text = @"Choose the ACZero folder. It should contain settings\dinput.ini.";
            return;
        }

        try
        {
            _path.Text = root;
            GameFiles.EnsureOriginalCopy(root);
            var opt = GameFiles.ReadFlight(root);
            var display = GameFiles.ReadDisplay(root);
            _keys.Clear();
            foreach (var pair in opt.ToKeys()) _keys[pair.Key] = pair.Value;
            _clicks.Clear();
            foreach (var pair in opt.ToClicks()) _clicks[pair.Key] = pair.Value;
            var extra = opt.ExtraThrottle;
            var alt = opt.AltLook;
            var notes = BindRules.ClearDuplicates(_keys, _clicks, ref extra, ref alt);
            opt.ExtraThrottle = extra;
            opt.AltLook = alt;
            opt.ApplyKeys(_keys, _clicks);
            ShowOptions(opt);
            if (!_resolution.Items.Contains(display.Resolution)) _resolution.Items.Add(display.Resolution);
            _windowed.Checked = display.ScreenMode == "Windowed";
            _borderless.Checked = display.ScreenMode != "Windowed";
            _resolution.SelectedItem = display.Resolution;
            _status.Text = notes.Count == 0
                ? "Loaded the settings currently in the game."
                : string.Join(" ", notes) + " Apply to save that.";
        }
        catch (Exception ex)
        {
            _status.Text = ex.Message;
        }
    }

    private void ApplySettings()
    {
        var root = GameFiles.ResolveRoot(_path.Text);
        if (root is null)
        {
            MessageBox.Show("Select the ACZero folder inside the Ace Combat 8 install.", "AC Zero Adapter");
            return;
        }

        var extra = _extra.Checked;
        var alt = _altLook.Checked;
        var notes = BindRules.ClearDuplicates(_keys, _clicks, ref extra, ref alt);
        _suppress = true;
        _extra.Checked = extra;
        _altLook.Checked = alt;
        _suppress = false;
        ShowOptions(ReadOptions());
        if (System.Diagnostics.Process.GetProcessesByName("acz").Length > 0)
        {
            var answer = MessageBox.Show(
                "Ace Combat Zero is running. It keeps the old controls until you quit and start it again. Apply anyway?",
                "AC Zero Adapter",
                MessageBoxButtons.YesNo);
            if (answer != DialogResult.Yes) return;
        }

        try
        {
            var opt = ReadOptions();
            GameFiles.Apply(root, FlightIni.Build(opt), _borderless.Checked ? "Borderless" : "Windowed", _resolution.SelectedItem?.ToString() ?? "1920x1080");
            _status.Text = notes.Count == 0
                ? "Applied. Quit the game completely, then start it again."
                : string.Join(" ", notes) + " Applied the rest. Restart the game.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "AC Zero Adapter");
        }
    }

    private void RestoreOriginal()
    {
        var root = GameFiles.ResolveRoot(_path.Text);
        if (root is null)
        {
            MessageBox.Show("Select the ACZero folder inside the Ace Combat 8 install.", "AC Zero Adapter");
            return;
        }

        if (MessageBox.Show("Put back the port's original keyboard layout and windowed mode?", "AC Zero Adapter", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;

        try
        {
            var display = GameFiles.ReadDisplay(root);
            GameFiles.Apply(root, GameFiles.ReadOriginalSection(root), "Windowed", display.Resolution);
            LoadFromPath(root);
            _status.Text = "Original controls restored. Restart the game.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "AC Zero Adapter");
        }
    }

    private void Browse()
    {
        using var dialog = new FolderBrowserDialog { Description = "Select the ACZero folder" };
        if (Directory.Exists(_path.Text)) dialog.SelectedPath = _path.Text;
        if (dialog.ShowDialog(this) == DialogResult.OK) LoadFromPath(dialog.SelectedPath);
    }

    private void RefreshPreview()
    {
        _pitchSpan.Enabled = (_mouseY.SelectedItem?.ToString() ?? "") == "Pitch";
        _sideSpan.Enabled = (_mouseX.SelectedItem?.ToString() ?? "Off") != "Off";
        _preview.Text = ReadOptions().DescribeFlight();
    }

    private void ShowPage(bool flight, Button flightTab, Button systemsTab)
    {
        _flight.Visible = flight;
        _systems.Visible = !flight;
        flightTab.BackColor = flight ? Amber : Card;
        flightTab.ForeColor = flight ? Ink : Paper;
        flightTab.FlatAppearance.BorderSize = flight ? 0 : 1;
        systemsTab.BackColor = flight ? Card : Amber;
        systemsTab.ForeColor = flight ? Paper : Ink;
        systemsTab.FlatAppearance.BorderSize = flight ? 1 : 0;
    }

    private static string? NameFromKey(Keys key)
    {
        if (key is >= Keys.A and <= Keys.Z) return key.ToString();
        if (key is >= Keys.F1 and <= Keys.F12) return key.ToString();
        if (key is >= Keys.NumPad0 and <= Keys.NumPad9) return "Num " + ((int)key - (int)Keys.NumPad0);
        return key switch
        {
            Keys.Space => "Space",
            Keys.LControlKey => "Left Ctrl",
            Keys.RControlKey => "Right Ctrl",
            Keys.LShiftKey => "Left Shift",
            Keys.RShiftKey => "Right Shift",
            Keys.LMenu => "Left Alt",
            Keys.RMenu => "Right Alt",
            Keys.Tab => "Tab",
            Keys.Up => "Up",
            Keys.Down => "Down",
            Keys.Left => "Left",
            Keys.Right => "Right",
            Keys.D1 => "1", Keys.D2 => "2", Keys.D3 => "3", Keys.D4 => "4", Keys.D5 => "5",
            Keys.D6 => "6", Keys.D7 => "7", Keys.D8 => "8", Keys.D9 => "9", Keys.D0 => "0",
            _ => null,
        };
    }

    private static Label Label(string text, int x, int y, int w, int h, Color color, Font font) => new()
    {
        Text = text, Location = new Point(x, y), Size = new Size(w, h), ForeColor = color, Font = font, BackColor = Color.Transparent,
    };

    private static Button Button(string text, int x, int y, int w, int h, Color back, Color fore)
    {
        var button = new Button
        {
            Text = text, Location = new Point(x, y), Size = new Size(w, h),
            FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = fore, Cursor = Cursors.Hand,
        };
        button.FlatAppearance.BorderColor = Line;
        button.FlatAppearance.BorderSize = back == Amber ? 0 : 1;
        return button;
    }

    private static void StyleField(TextBox box)
    {
        box.BackColor = Field;
        box.ForeColor = Paper;
        box.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void StyleCombo(ComboBox box)
    {
        box.BackColor = Field;
        box.ForeColor = Paper;
        box.FlatStyle = FlatStyle.Flat;
    }

    private static void StyleNumber(NumericUpDown box)
    {
        box.BackColor = Field;
        box.ForeColor = Paper;
    }

    private static void StyleChoice(RadioButton box, int x, int y)
    {
        box.Location = new Point(x, y);
        box.ForeColor = Paper;
        box.BackColor = Card;
    }

    private static Panel CardPanel(int x, int y, int w, int h) => new()
    {
        Location = new Point(x, y), Size = new Size(w, h), BackColor = Card,
    };
}
