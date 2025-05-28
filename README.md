# H3C BPMT E (Batch Password Modification Tool Edition)

## 简介
H3C BPMT E 是一款用于批量修改 H3C 设备密码的工具，采用 WPF（Windows Presentation Foundation）技术构建，提供了直观的用户界面和便捷的操作流程。用户可以上传包含设备信息的 XLSX 文件，工具将自动连接到设备并修改密码，同时记录操作日志。此外，工具还支持主题切换和历史记录查询功能。

## 功能特性
1. **批量密码修改**：通过上传 XLSX 文件，一次性修改多台 H3C 设备的密码。
2. **日志记录**：记录所有操作日志，方便用户查看和审计。
3. **历史记录查询**：支持按日期范围查询操作历史记录。
4. **主题切换**：提供亮色和暗色两种主题，满足不同用户的视觉需求。

## 目录结构
```
H3C-BPMT-E/
├── H3C BPMT E/
│   ├── Resources/
│   │   └── Translations.cs
│   ├── Views/
│   │   ├── Pages/
│   │   │   ├── DataPage.xaml
│   │   │   ├── DataPage.xaml.cs
│   │   │   ├── DashboardPage.xaml
│   │   │   ├── DashboardPage.xaml.cs
│   │   │   ├── SettingsPage.xaml
│   │   │   └── SettingsPage.xaml.cs
│   │   └── Windows/
│   │       ├── MainWindow.xaml
│   │       └── MainWindow.xaml.cs
│   ├── ViewModels/
│   │   ├── Pages/
│   │   │   ├── DataViewModel.cs
│   │   │   ├── DashboardViewModel.cs
│   │   │   └── SettingsViewModel.cs
│   │   └── Windows/
│   │       └── MainWindowViewModel.cs
│   ├── Models/
│   │   ├── Log.cs
│   │   └── DataColor.cs
│   ├── Services/
│   │   ├── ApplicationHostService.cs
│   │   └── LogService.cs
│   ├── Helpers/
│   │   └── EnumToBooleanConverter.cs
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── Usings.cs
│   └── AssemblyInfo.cs
├── .gitignore
```

## 安装与运行
### 依赖项
- .NET 环境
- OfficeOpenXml 库
- Renci.SshNet 库
- Wpf.Ui 库

### 步骤
1. 克隆项目到本地：
```bash
git clone https://github.com/your-repo/H3C-BPMT-E.git
```
2. 打开项目文件夹，使用 Visual Studio 或其他开发工具打开解决方案文件。
3. 确保所有依赖项已正确安装。
4. 编译并运行项目。

## 使用说明
### 主界面
- **首页**：提供文件上传和开始执行的按钮，可查看设备信息和执行进度。
- **历史**：显示操作历史记录，支持按日期范围查询。
- **设置**：可切换主题和查看应用版本信息。

### 批量密码修改
1. 点击“上传 XLSX 文件”按钮，选择包含设备信息的文件。
2. 文件上传后，工具将自动解析文件内容并显示设备列表。
3. 点击“开始”按钮，工具将开始连接设备并修改密码，同时显示执行进度。

### 历史记录查询
1. 在“历史”页面，选择开始日期和结束日期。
2. 点击“搜索历史”按钮，工具将显示指定日期范围内的操作历史记录。

### 主题切换
在“设置”页面，选择亮色或暗色主题，点击相应的单选按钮即可切换主题。

## 代码说明
### 主要类和功能
- **MainWindow**：主窗口，包含导航栏和内容区域。
- **DashboardPage**：首页，负责文件上传和密码修改操作。
- **DataPage**：历史页面，负责历史记录查询。
- **SettingsPage**：设置页面，负责主题切换和版本信息显示。
- **DashboardViewModel**：首页的视图模型，处理文件上传、密码修改和进度更新。
- **DataViewModel**：历史页面的视图模型，处理历史记录查询。
- **SettingsViewModel**：设置页面的视图模型，处理主题切换和版本信息获取。
- **LogService**：日志服务，负责日志的存储和查询。

### 关键代码片段
#### 文件上传
```csharp
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
```

#### 密码修改
```csharp
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
    int failCount = 0;

    StatusText = $"执行状态: 完成0，剩余{totalSwitches}";
    ProgressValue = 0;
    var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Min(8, totalSwitches) };

    await Task.Run(() =>
    {
        Parallel.ForEach(SwitchList, options, switchInfo =>
        {
            bool success = false;
            try
            {
                success = ChangePassword(switchInfo);
            }
            catch (Exception ex)
            {
                switchInfo.CompletionStatus = $"错误: {ex.Message}";
                logging($"未知错误: {switchInfo.IPAddress} - {ex.Message}");
            }

            if (success)
            {
                switchInfo.CompletionStatus = "已完成";
                Interlocked.Increment(ref successCount);
            }
            else
            {
                switchInfo.CompletionStatus = "失败";
                Interlocked.Increment(ref failCount);
            }

            int currentCompleted = Interlocked.Increment(ref completedSwitches);

            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusText = $"执行状态: 成功{successCount}, 失败{failCount}";
                ProgressValue = (currentCompleted / (double)totalSwitches) * 100;
            });
        });
    });

    StatusText = $"处理完成: 成功 {successCount} 台, 失败 {failCount} 台";
    StartText = "开始";
}
```

#### 历史记录查询
```csharp
[RelayCommand]
public void SearchHistory()
{
    RetrieveDialogs();
}

private void RetrieveDialogs()
{
    Logs.Clear();
    foreach (var dialog in _logService.GetAllDialogContents(_dateStart, _dateEnd))
    {
        Logs.Add(dialog);
    }
}
```

#### 主题切换
```csharp
[RelayCommand]
private void OnChangeTheme(string parameter)
{
    switch (parameter)
    {
        case "theme_light":
            if (CurrentTheme == ApplicationTheme.Light)
                break;

            ApplicationThemeManager.Apply(ApplicationTheme.Light);
            CurrentTheme = ApplicationTheme.Light;

            break;

        default:
            if (CurrentTheme == ApplicationTheme.Dark)
                break;

            ApplicationThemeManager.Apply(ApplicationTheme.Dark);
            CurrentTheme = ApplicationTheme.Dark;

            break;
    }
}
```

## 贡献与反馈
如果你对项目有任何建议或发现了 bug，请在项目的 GitHub 仓库中提交 issue 或 pull request。

## 许可证
本项目采用 [MIT 许可证](https://opensource.org/licenses/MIT)。
