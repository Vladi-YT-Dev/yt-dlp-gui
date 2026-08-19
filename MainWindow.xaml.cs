using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace YtDlpGui;

public partial class MainWindow : Window
{
    private Process? _currentProcess;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => UrlInput.Focus();
        Tag = "idle";
    }

    private void UrlInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            StartDownload();
        }
    }

    private void RunBtn_Click(object sender, RoutedEventArgs e)
    {
        StartDownload();
    }

    private void ClearBtn_Click(object sender, RoutedEventArgs e)
    {
        OutputBox.Document.Blocks.Clear();
    }

    private void SetStatus(string state, string text)
    {
        Tag = state;
        StatusText.Text = text;
    }

    private void AppendLine(string text, string type = "")
    {
        var p = new Paragraph();
        var line = text.EndsWith('\n') ? text.Substring(0, text.Length - 1) : text;
        var run = new Run(line);
        Brush fg = type switch
        {
            "info" => new SolidColorBrush(Color.FromRgb(0xBA, 0xE6, 0xFD)) { Opacity = 0.9 },
            "error" => Brushes.LightSalmon,
            "success" => Brushes.LightGreen,
            _ => new SolidColorBrush(Color.FromRgb(0xE5, 0xE5, 0xE5)) { Opacity = 0.9 }
        };
        run.Foreground = fg;
        p.Inlines.Add(run);
        p.Margin = new Thickness(0);
        p.Padding = new Thickness(0);
        OutputBox.Document.Blocks.Add(p);
        OutputBox.ScrollToEnd();
    }

    private void StartDownload()
    {
        if (_currentProcess != null) return;

        var url = UrlInput.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(url))
        {
            UrlInput.Focus();
            return;
        }

        RunBtn.IsEnabled = false;
        SetStatus("running", "Downloading...");
        AppendLine($"> yt-dlp -f \"bv*[ext=mp4]+ba[ext=m4a]/b[ext=mp4]\" \"{url}\"", "info");

        var exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
        var guiDir = exeDir ?? Directory.GetCurrentDirectory();
        var parentDir = Directory.GetParent(guiDir)?.FullName ?? guiDir;

        string workingDir;
        string ytdlpPath;
        if (File.Exists(Path.Combine(guiDir, "yt-dlp.exe")))
        {
            workingDir = guiDir;
            ytdlpPath = Path.Combine(guiDir, "yt-dlp.exe");
        }
        else
        {
            workingDir = parentDir;
            ytdlpPath = Path.Combine(parentDir, "yt-dlp.exe");
        }

        try
        {
            var formatArg = "bv*[ext=mp4]+ba[ext=m4a]/b[ext=mp4]";
            var psi = new ProcessStartInfo
            {
                FileName = ytdlpPath,
                ArgumentList = { "-f", formatArg, url },
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            _currentProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };

            _currentProcess.OutputDataReceived += (_, args) =>
            {
                if (args.Data != null) Dispatcher.Invoke(() => AppendLine(args.Data));
            };

            _currentProcess.ErrorDataReceived += (_, args) =>
            {
                if (args.Data != null) Dispatcher.Invoke(() => AppendLine(args.Data, "error"));
            };

            _currentProcess.Exited += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var code = _currentProcess?.ExitCode ?? -1;
                    _currentProcess?.Dispose();
                    _currentProcess = null;
                    RunBtn.IsEnabled = true;
                    if (code == 0)
                    {
                        SetStatus("success", "Completed successfully");
                        AppendLine("[ Process completed with exit code 0 ]", "success");
                    }
                    else
                    {
                        SetStatus("error", $"Failed (exit code {code})");
                        AppendLine($"[ Process exited with code {code} ]", "error");
                    }
                });
            };

            if (!_currentProcess.Start())
            {
                throw new InvalidOperationException("Failed to start process.");
            }

            _currentProcess.BeginOutputReadLine();
            _currentProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            _currentProcess?.Dispose();
            _currentProcess = null;
            RunBtn.IsEnabled = true;
            SetStatus("error", "Error");
            AppendLine($"Error: {ex.Message}", "error");
        }
    }
}
