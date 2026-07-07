using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

internal sealed class MainForm : Form
{
    private static readonly Color PageBack = Color.FromArgb(246, 248, 251);
    private static readonly Color CardBack = Color.White;
    private static readonly Color TextMain = Color.FromArgb(17, 24, 39);
    private static readonly Color TextMuted = Color.FromArgb(75, 85, 99);
    private static readonly Color Border = Color.FromArgb(203, 213, 225);
    private static readonly Color Primary = Color.FromArgb(37, 99, 235);

    private readonly string root = AppDomain.CurrentDomain.BaseDirectory;
    private readonly string outputRoot;
    private readonly TextBox inputBox = new TextBox();
    private readonly Label infoLabel = new Label();
    private readonly Label statusLabel = new Label();
    private readonly Button startButton = new Button();
    private readonly Button openVideoButton = new Button();
    private readonly Button openFolderButton = new Button();
    private readonly ProgressBar progressBar = new ProgressBar();
    private readonly RichTextBox logBox = new RichTextBox();
    private Process worker;
    private string inputPath;
    private string outputPath;

    public MainForm()
    {
        Text = "\u0047\u0065\u006d\u0069\u006e\u0069\u0020\u53ef\u89c1\u89d2\u6807\u4fee\u590d\u5de5\u5177";
        ClientSize = new Size(860, 620);
        MinimumSize = new Size(780, 540);
        StartPosition = FormStartPosition.CenterScreen;
        AllowDrop = true;
        BackColor = PageBack;
        Font = new Font("Microsoft YaHei UI", 9);
        outputRoot = ResolveOutputRoot();
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        var title = new Label { Text = "\u0047\u0065\u006d\u0069\u006e\u0069\u0020\u002f\u0020\u004f\u006d\u006e\u0069\u0020\u53ef\u89c1\u89d2\u6807\u4fee\u590d\u5de5\u5177", ForeColor = TextMain, Font = new Font("Microsoft YaHei UI", 18, FontStyle.Bold), AutoSize = true, Location = new Point(24, 20) };
        var subtitle = new Label { Text = "\u6587\u4ef6\u53ea\u5728\u672c\u673a\u5904\u7406\uff1b\u53ea\u4fee\u590d\u53ef\u89c1\u89d2\u6807\uff0c\u4e0d\u68c0\u6d4b\u6216\u4fee\u6539\u0020\u0053\u0079\u006e\u0074\u0068\u0049\u0044\u3002", ForeColor = TextMuted, AutoSize = true, Location = new Point(27, 60) };

        var dropPanel = new Panel { Location = new Point(24, 92), Size = new Size(812, 112), BackColor = CardBack, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        dropPanel.Paint += delegate(object sender, PaintEventArgs e) { using (var pen = new Pen(Border, 2)) e.Graphics.DrawRectangle(pen, 1, 1, dropPanel.Width - 3, dropPanel.Height - 3); };
        var dropText = new Label { Text = "\u62d6\u5165\u89c6\u9891\uff0c\u6216\u70b9\u51fb\u201c\u9009\u62e9\u89c6\u9891\u201d", ForeColor = TextMain, Font = new Font("Microsoft YaHei UI", 13, FontStyle.Bold), AutoSize = true, Location = new Point(24, 22) };
        inputBox.Location = new Point(24, 62);
        inputBox.Size = new Size(610, 26);
        inputBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        inputBox.ReadOnly = true;
        inputBox.BackColor = Color.FromArgb(249, 250, 251);
        inputBox.ForeColor = TextMain;
        inputBox.BorderStyle = BorderStyle.FixedSingle;
        var chooseButton = new Button { Text = "\u9009\u62e9\u89c6\u9891", Location = new Point(652, 58), Size = new Size(132, 32), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        StylePrimaryButton(chooseButton);
        chooseButton.Click += delegate { ChooseInput(); };
        dropPanel.Controls.AddRange(new Control[] { dropText, inputBox, chooseButton });

        infoLabel.Text = "\u5c1a\u672a\u9009\u62e9\u89c6\u9891";
        infoLabel.ForeColor = TextMuted;
        infoLabel.AutoSize = true;
        infoLabel.Location = new Point(28, 220);

        startButton.Text = "\u5f00\u59cb\u4fee\u590d";
        startButton.Location = new Point(28, 250);
        startButton.Size = new Size(120, 36);
        startButton.Enabled = false;
        StylePrimaryButton(startButton);
        startButton.Click += delegate { StartRepair(); };

        openVideoButton.Text = "\u6253\u5f00\u89c6\u9891";
        openVideoButton.Location = new Point(164, 250);
        openVideoButton.Size = new Size(120, 36);
        openVideoButton.Enabled = false;
        StyleSecondaryButton(openVideoButton);
        openVideoButton.Click += delegate { if (File.Exists(outputPath)) Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true }); };

        openFolderButton.Text = "\u6253\u5f00\u8f93\u51fa\u76ee\u5f55";
        openFolderButton.Location = new Point(300, 250);
        openFolderButton.Size = new Size(140, 36);
        StyleSecondaryButton(openFolderButton);
        openFolderButton.Click += delegate { OpenOutputFolder(); };

        statusLabel.Text = "\u7b49\u5f85\u4efb\u52a1";
        statusLabel.ForeColor = TextMuted;
        statusLabel.AutoSize = true;
        statusLabel.Location = new Point(28, 304);

        progressBar.Location = new Point(28, 330);
        progressBar.Size = new Size(808, 18);
        progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        var logTitle = new Label { Text = "\u5904\u7406\u65e5\u5fd7", ForeColor = TextMain, Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(28, 364) };

        logBox.Location = new Point(28, 390);
        logBox.Size = new Size(808, 200);
        logBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        logBox.BackColor = Color.White;
        logBox.ForeColor = Color.FromArgb(31, 41, 55);
        logBox.Font = new Font("Consolas", 9);
        logBox.BorderStyle = BorderStyle.FixedSingle;
        logBox.ReadOnly = true;
        logBox.Text = "\u7b49\u5f85\u4efb\u52a1...\n";

        Controls.AddRange(new Control[] { title, subtitle, dropPanel, infoLabel, startButton, openVideoButton, openFolderButton, statusLabel, progressBar, logTitle, logBox });

        DragEnter += delegate(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        };
        DragDrop += delegate(object sender, DragEventArgs e) {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0) SetInput(files[0]);
        };
    }

    private static void StylePrimaryButton(Button button)
    {
        button.BackColor = Primary;
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = new Font("Microsoft YaHei UI", 9, FontStyle.Bold);
        button.UseVisualStyleBackColor = false;
    }

    private static void StyleSecondaryButton(Button button)
    {
        button.BackColor = Color.White;
        button.ForeColor = TextMain;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Border;
        button.FlatAppearance.BorderSize = 1;
        button.Font = new Font("Microsoft YaHei UI", 9);
        button.UseVisualStyleBackColor = false;
    }

    private void ChooseInput()
    {
        using (var dialog = new OpenFileDialog())
        {
            dialog.Filter = "Video files|*.mp4;*.mov;*.mkv;*.webm;*.avi|All files|*.*";
            if (dialog.ShowDialog(this) == DialogResult.OK) SetInput(dialog.FileName);
        }
    }

    private void SetInput(string path)
    {
        if (!File.Exists(path)) return;
        inputPath = path;
        inputBox.Text = path;
        openVideoButton.Enabled = false;
        outputPath = null;
        startButton.Enabled = true;
        statusLabel.Text = "\u5df2\u9009\u62e9\u89c6\u9891";
        infoLabel.Text = "\u6b63\u5728\u8bfb\u53d6\u89c6\u9891\u4fe1\u606f...";
        logBox.Text = "";
        ThreadPool.QueueUserWorkItem(delegate { ProbeInput(path); });
    }

    private void ProbeInput(string path)
    {
        try
        {
            var ffprobe = Path.Combine(root, "tools", "ffmpeg", "bin", "ffprobe.exe");
            var text = RunAndCapture(ffprobe, "-v error -select_streams v:0 -show_entries stream=width,height,r_frame_rate,duration,nb_frames -show_entries format=duration -of default=noprint_wrappers=1 \"" + path + "\"");
            BeginInvoke((Action)delegate { infoLabel.Text = text.Replace("\r", "").Replace("\n", "  "); });
        }
        catch (Exception ex)
        {
            BeginInvoke((Action)delegate { infoLabel.Text = ex.Message; });
        }
    }

    private static string RunAndCapture(string exe, string args)
    {
        var process = new Process();
        process.StartInfo = new ProcessStartInfo(exe, args) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0) throw new Exception(error.Trim());
        return output.Trim();
    }

    private void StartRepair()
    {
        if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath)) return;
        Directory.CreateDirectory(outputRoot);
        var name = Path.GetFileNameWithoutExtension(inputPath);
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        outputPath = Path.Combine(outputRoot, MakeSafeFileName(name + "_visible_mark_repaired_" + stamp + ".mp4"));

        startButton.Enabled = false;
        openVideoButton.Enabled = false;
        progressBar.Style = ProgressBarStyle.Marquee;
        statusLabel.Text = "\u6b63\u5728\u4fee\u590d\uff0c\u8bf7\u4fdd\u6301\u7a97\u53e3\u6253\u5f00...";
        logBox.Text = "";

        var script = Path.Combine(root, "repair-video.ps1");
        var args = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"" + script + "\" -InputPath \"" + inputPath + "\" -OutputPath \"" + outputPath + "\"";
        worker = new Process();
        worker.StartInfo = new ProcessStartInfo("powershell.exe", args) { WorkingDirectory = root, UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
        worker.OutputDataReceived += OnOutput;
        worker.ErrorDataReceived += OnOutput;
        worker.EnableRaisingEvents = true;
        worker.Exited += delegate { BeginInvoke((Action)OnWorkerExited); };
        worker.Start();
        worker.BeginOutputReadLine();
        worker.BeginErrorReadLine();
    }

    private static string MakeSafeFileName(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
        return value;
    }

    private void OnOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null) return;
        BeginInvoke((Action)delegate {
            logBox.AppendText(Regex.Replace(e.Data, "\\x1b\\[[0-9;]*m", "") + Environment.NewLine);
            logBox.SelectionStart = logBox.TextLength;
            logBox.ScrollToCaret();
        });
    }

    private void OnWorkerExited()
    {
        progressBar.Style = ProgressBarStyle.Blocks;
        progressBar.Value = worker.ExitCode == 0 ? 100 : 0;
        startButton.Enabled = true;
        if (worker.ExitCode == 0 && File.Exists(outputPath))
        {
            statusLabel.Text = "\u4fee\u590d\u5b8c\u6210";
            openVideoButton.Enabled = true;
        }
        else
        {
            statusLabel.Text = "\u5904\u7406\u5931\u8d25\uff0c\u8bf7\u67e5\u770b\u65e5\u5fd7";
        }
    }

    private void OpenOutputFolder()
    {
        Directory.CreateDirectory(outputRoot);
        Process.Start(new ProcessStartInfo("explorer.exe", "\"" + outputRoot + "\"") { UseShellExecute = true });
    }

    private static string ResolveOutputRoot()
    {
        var configured = Environment.GetEnvironmentVariable("GVMR_OUTPUT_DIR");
        if (!string.IsNullOrWhiteSpace(configured)) return configured;
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        try { if (worker != null && !worker.HasExited) worker.Kill(); } catch { }
        base.OnFormClosing(e);
    }

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
