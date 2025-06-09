using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Security.Authentication;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using H3C_BPMT_E.Models;
using Microsoft.Win32;
using Wpf.Ui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using Renci.SshNet;
using H3C_BPMT_E.Services;
using System.Data;

namespace H3C_BPMT_E.ViewModels.Pages
{
    public partial class ConsoleChangeViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<SwitchInfoConsole>? _switchList;

        [ObservableProperty]
        private string? _statusText;

        [ObservableProperty]
        private double _progressValue;

        [ObservableProperty]
        private string _startText = "开始";

        private int successCount = 0;
        private int failCount = 0;
        private readonly object _lock = new object();

        private bool _isInitialized = false;

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom() { }

        private void InitializeViewModel()
        {
            SwitchList = new ObservableCollection<SwitchInfoConsole>();
            StatusText = "执行状态: 完成0，剩余0";
            ProgressValue = 0;


            _isInitialized = true;
        }

        [RelayCommand]
        private void Upload()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Select an XLSX file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                SwitchList = ReadExcelFile(filePath);
                StatusText = $"执行状态: 完成0，剩余{SwitchList.Count}";
            }
        }

        private ObservableCollection<SwitchInfoConsole> ReadExcelFile(string filePath)
        {
            List<SwitchInfoConsole> switchList = new List<SwitchInfoConsole>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                for (int row = 2; row <= rowCount; row++) // 假设第一行是标题行
                {
                    switchList.Add(new SwitchInfoConsole
                    {
                        IPAddress = worksheet.Cells[row, 1].Value?.ToString(),
                        Port = worksheet.Cells[row, 2].Value != null ? Convert.ToInt32(worksheet.Cells[row, 2].Value) : 22,
                        Username = worksheet.Cells[row, 3].Value?.ToString(),
                        Password = worksheet.Cells[row, 4].Value?.ToString(),
                        ConsolePassword = worksheet.Cells[row, 5].Value?.ToString(),
                        CompletionStatus = "待处理"
                    });
                }
            }

            return new ObservableCollection<SwitchInfoConsole>(switchList);
        }

        [RelayCommand]
        private async Task Start()
        {
            StartText = "执行中...";
            if (SwitchList == null || SwitchList.Count == 0)
            {
                StatusText = "执行状态: 完成0，剩余0";
                StartText = "开始";
                return;
            }

            int totalSwitches = SwitchList.Count;
            int completedSwitches = 0;
            int successCount = 0;
            int failCount = 0; // 新增独立失败计数器

            StatusText = $"执行状态: 完成0，剩余{totalSwitches}";
            ProgressValue = 0;
            var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Min(8, totalSwitches) };

            await Task.Run(() =>
            {
                Parallel.ForEach(SwitchList, options, switchInfoConsole =>
                {
                    bool success = false;
                    try
                    {
                        success = ChangePassword(switchInfoConsole);
                    }
                    catch (Exception ex)
                    {
                        switchInfoConsole.CompletionStatus = $"错误: {ex.Message}";
                        logging($"未知错误: {switchInfoConsole.IPAddress} - {ex.Message}");
                    }

                    if (success)
                    {
                        switchInfoConsole.CompletionStatus = "已完成";
                        Interlocked.Increment(ref successCount);
                    }
                    else
                    {
                        switchInfoConsole.CompletionStatus = "失败";
                        Interlocked.Increment(ref failCount); // 失败时单独计数
                    }

                    int currentCompleted = Interlocked.Increment(ref completedSwitches); // 总完成数+1

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 修改点：直接使用 successCount 和 failCount
                        StatusText = $"执行状态: 成功{successCount}, 失败{failCount}";
                        ProgressValue = (currentCompleted / (double)totalSwitches) * 100;
                    });
                });
            });

            StatusText = $"处理完成: 成功 {successCount} 台, 失败 {failCount} 台";
            StartText = "开始";
        }

        private bool ChangePassword(SwitchInfoConsole switchInfoConsole)
        {
            var deviceInfo = new ConnectionInfo(switchInfoConsole.IPAddress, switchInfoConsole.Port, switchInfoConsole.Username,
                new PasswordAuthenticationMethod(switchInfoConsole.Username, switchInfoConsole.Password));

            using (var conn = new SshClient(deviceInfo))
            {
                try
                {
                    conn.Connect();

                    if (!conn.IsConnected)
                    {
                        logging($"认证失败: {switchInfoConsole.IPAddress} - 无法连接");
                        conn.Disconnect();
                        return false;
                    }

                    // 创建交互式 SSH 会话
                    var shellStream = conn.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

                    var commands = new List<string>
                    {
                        "system-view",
                        $"user-interface console 0",
                        $"set authentication password cipher {switchInfoConsole.ConsolePassword}",
                        "quit",
                        "quit"
                    };

                    var output = string.Empty;
                    foreach (var command in commands)
                    {
                        shellStream.WriteLine(command);
                        // 等待命令执行完成
                        output += WaitForPrompt(shellStream, "]", 30);
                    }

                    // 保存配置
                    shellStream.WriteLine("save force");
                    var saveOutput = WaitForPrompt(shellStream, "]", 30);
                    if (saveOutput.Contains("Continue? [Y/N]"))
                    {
                        shellStream.WriteLine("y");
                        saveOutput += WaitForPrompt(shellStream, "]", 30);
                    }

                    output += saveOutput;

                    if (output.ToLower().Contains("successfully") || output.ToLower().Contains("configuration is saved"))
                    {
                        logging($"Console 密码修改并保存成功: {switchInfoConsole.IPAddress}");
                        conn.Disconnect();
                        return true;
                    }
                    else
                    {
                        logging($"保存Console 密码可能失败: {switchInfoConsole.IPAddress} - 输出: {output.Substring(0, Math.Min(output.Length, 1000))}...");
                        conn.Disconnect();
                        return false;
                    }
                }
                catch (AuthenticationException)
                {
                    logging($"认证失败: {switchInfoConsole.IPAddress} - 密码错误");
                    conn.Disconnect();
                    return false;
                }
                catch (TimeoutException)
                {
                    logging($"连接超时: {switchInfoConsole.IPAddress} - 检查网络");
                    conn.Disconnect();
                    return false;
                }
                catch (Exception ex)
                {
                    logging($"未知错误: {switchInfoConsole.IPAddress} - {ex.Message}");
                    conn.Disconnect();
                    return false;
                }
            }
        }

        private string WaitForPrompt(ShellStream shellStream, string prompt, int timeout)
        {
            var output = string.Empty;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (stopwatch.Elapsed.TotalSeconds < timeout)
            {
                if (shellStream.DataAvailable)
                {
                    output += shellStream.Read();
                    if (output.Contains(prompt))
                    {
                        break;
                    }
                }
                System.Threading.Thread.Sleep(100);
            }
            return output;
        }

        // 对话日志数据库服务
        private readonly LogService _logService = new();

        private void logging(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Log newLog = new Log()
                {
                    Message = message,
                    Timestamp = DateTime.Now
                };
                _logService.AddDialogContent(newLog);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SwitchInfoConsole : INotifyPropertyChanged
    {
        private string _ipAddress;
        private int _port;
        private string _username;
        private string _password;
        private string _consolePassword;
        private string _completionStatus;

        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                OnPropertyChanged(nameof(IPAddress));
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ConsolePassword
        {
            get { return _consolePassword; }
            set
            {
                _consolePassword = value;
                OnPropertyChanged(nameof(ConsolePassword));
            }
        }

        public string CompletionStatus
        {
            get { return _completionStatus; }
            set
            {
                _completionStatus = value;
                OnPropertyChanged(nameof(CompletionStatus));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
