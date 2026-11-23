using System.Runtime.InteropServices;

namespace AutoClicker9x;

public sealed class MainForm : Form
{
    private const int HOTKEY_ID = 0x9A01;

    private readonly System.Windows.Forms.Timer _timer = new();

    private RadioButton rbFixed    = null!;
    private RadioButton rbCurrent  = null!;
    private NumericUpDown numX     = null!;
    private NumericUpDown numY     = null!;

    private ComboBox cmbButton     = null!;
    private NumericUpDown numInterval = null!;
    private CheckBox chkRepeats    = null!;
    private NumericUpDown numRepeat = null!;

    private Button btnStart        = null!;
    private Label  lblStatus       = null!;
    private Label  lblCount        = null!;

    private bool _running;
    private int  _clickCount;

    public MainForm()
    {
        Text            = "Автокликер 9x";
        ClientSize      = new Size(370, 268);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterScreen;
        Font            = new Font("Tahoma", 8.25f);

        BuildUi();
        ApplyWin9xStyle();

        _timer.Tick += (_, _) => DoClick();

        Load += OnLoaded;
        FormClosed += OnClosed;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (!NativeMethods.RegisterHotKey(Handle, HOTKEY_ID, 0, NativeMethods.VK_F6))
            lblStatus.Text = "F6 занята другой программой";
    }

    private void OnClosed(object? sender, FormClosedEventArgs e)
    {
        NativeMethods.UnregisterHotKey(Handle, HOTKEY_ID);
        _timer.Stop();
    }

    private void BuildUi()
    {
        // ---- Группа "Позиция курсора" ----
        var grpPos = new GroupBox
        {
            Text     = " Позиция курсора ",
            Location = new Point(8, 8),
            Size     = new Size(346, 96)
        };

        rbFixed = new RadioButton
        {
            Text     = "Кликать в точку:",
            Location = new Point(10, 20),
            AutoSize = true,
            Checked  = true
        };

        numX = new NumericUpDown
        {
            Location = new Point(140, 18),
            Size     = new Size(70, 22),
            Minimum  = 0,
            Maximum  = 10000,
            Value    = 500
        };

        numY = new NumericUpDown
        {
            Location = new Point(218, 18),
            Size     = new Size(70, 22),
            Minimum  = 0,
            Maximum  = 10000,
            Value    = 500
        };

        rbCurrent = new RadioButton
        {
            Text     = "Кликать там, где сейчас курсор",
            Location = new Point(10, 48),
            AutoSize = true
        };

        var btnPick = new Button
        {
            Text     = "Взять текущие",
            Location = new Point(10, 72),
            Size     = new Size(110, 20)
        };
        btnPick.Click += (_, _) =>
        {
            var p = Cursor.Position;
            numX.Value = Math.Clamp(p.X, 0, 10000);
            numY.Value = Math.Clamp(p.Y, 0, 10000);
            rbFixed.Checked = true;
        };

        grpPos.Controls.AddRange(new Control[]
        {
            rbFixed, numX, numY, rbCurrent, btnPick
        });

        // ---- Группа "Параметры клика" ----
        var grpClick = new GroupBox
        {
            Text     = " Параметры клика ",
            Location = new Point(8, 110),
            Size     = new Size(346, 110)
        };

        var lblButton = new Label
        {
            Text     = "Кнопка:",
            Location = new Point(10, 24),
            AutoSize = true
        };

        cmbButton = new ComboBox
        {
            Location      = new Point(80, 22),
            Size          = new Size(120, 22),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbButton.Items.AddRange(new object[] { "Левая", "Правая", "Средняя" });
        cmbButton.SelectedIndex = 0;

        var lblInterval = new Label
        {
            Text     = "Интервал (мс):",
            Location = new Point(10, 52),
            AutoSize = true
        };

        numInterval = new NumericUpDown
        {
            Location  = new Point(140, 50),
            Size      = new Size(90, 22),
            Minimum   = 1,
            Maximum   = 3600000,
            Increment = 50,
            Value     = 100
        };

        chkRepeats = new CheckBox
        {
            Text     = "Повторений (0 = бесконечно):",
            Location = new Point(10, 82),
            AutoSize = true
        };

        numRepeat = new NumericUpDown
        {
            Location = new Point(218, 80),
            Size     = new Size(80, 22),
            Minimum  = 0,
            Maximum  = 1000000,
            Value    = 0,
            Enabled  = false
        };
        chkRepeats.CheckedChanged += (_, _) => numRepeat.Enabled = chkRepeats.Checked;

        grpClick.Controls.AddRange(new Control[]
        {
            lblButton, cmbButton, lblInterval, numInterval, chkRepeats, numRepeat
        });

        // ---- Нижняя панель ----
        btnStart = new Button
        {
            Text     = "Старт (F6)",
            Location = new Point(8, 228),
            Size     = new Size(100, 30)
        };
        btnStart.Click += (_, _) => Toggle();

        lblStatus = new Label
        {
            Text     = "Ожидание",
            Location = new Point(120, 236),
            AutoSize = true
        };

        lblCount = new Label
        {
            Text     = "Кликов: 0",
            Location = new Point(240, 236),
            AutoSize = true
        };

        Controls.AddRange(new Control[]
        {
            grpPos, grpClick, btnStart, lblStatus, lblCount
        });
    }

    private void ApplyWin9xStyle()
    {
        var gray = Color.FromArgb(192, 192, 192);
        BackColor = gray;

        foreach (var c in AllControls(this))
        {
            switch (c)
            {
                case Button b:
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 1;
                    b.FlatAppearance.BorderColor = Color.Black;
                    b.BackColor = gray;
                    b.ForeColor = Color.Black;
                    break;

                case GroupBox g:
                    g.BackColor = gray;
                    g.ForeColor = Color.Black;
                    break;

                case Label l:
                    l.BackColor = Color.Transparent;
                    break;

                case RadioButton rb:
                    rb.BackColor = gray;
                    break;

                case CheckBox cb:
                    cb.BackColor = gray;
                    break;

                case NumericUpDown n:
                    n.BackColor = Color.White;
                    n.BorderStyle = BorderStyle.Fixed3D;
                    break;

                case ComboBox cm:
                    cm.BackColor = Color.White;
                    cm.FlatStyle = FlatStyle.Standard;
                    break;
            }
        }
    }

    private static IEnumerable<Control> AllControls(Control root)
    {
        foreach (Control c in root.Controls)
        {
            yield return c;
            foreach (var sub in AllControls(c))
                yield return sub;
        }
    }

    private void Toggle()
    {
        if (_running) StopClicker();
        else StartClicker();
    }

    private void StartClicker()
    {
        _running = true;
        _clickCount = 0;
        lblCount.Text = "Кликов: 0";
        lblStatus.Text = "Работает";

        var p = Cursor.Position;
        numX.Value = Math.Clamp(p.X, 0, 10000);
        numY.Value = Math.Clamp(p.Y, 0, 10000);

        btnStart.Text = "Стоп (F6)";
        _timer.Interval = (int)numInterval.Value;
        _timer.Start();
    }

    private void StopClicker()
    {
        _running = false;
        _timer.Stop();
        lblStatus.Text = "Остановлено";
        btnStart.Text = "Старт (F6)";
    }

    private void DoClick()
    {
        if (chkRepeats.Checked && numRepeat.Value > 0 &&
            _clickCount >= (int)numRepeat.Value)
        {
            StopClicker();
            return;
        }

        if (rbFixed.Checked)
            NativeMethods.SetCursorPos((int)numX.Value, (int)numY.Value);

        var (down, up) = cmbButton.SelectedIndex switch
        {
            1 => (NativeMethods.MOUSEEVENTF_RIGHTDOWN,  NativeMethods.MOUSEEVENTF_RIGHTUP),
            2 => (NativeMethods.MOUSEEVENTF_MIDDLEDOWN, NativeMethods.MOUSEEVENTF_MIDDLEUP),
            _ => (NativeMethods.MOUSEEVENTF_LEFTDOWN,   NativeMethods.MOUSEEVENTF_LEFTUP)
        };

        NativeMethods.mouse_event(down, 0, 0, 0, UIntPtr.Zero);
        NativeMethods.mouse_event(up,   0, 0, 0, UIntPtr.Zero);

        _clickCount++;
        lblCount.Text = $"Кликов: {_clickCount}";
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
        {
            Toggle();
            return;
        }
        base.WndProc(ref m);
    }
}
