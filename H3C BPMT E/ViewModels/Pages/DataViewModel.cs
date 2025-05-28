using System.Windows.Media;
using H3C_BPMT_E.Models;
using Wpf.Ui.Abstractions.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Security.Authentication;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
using Org.BouncyCastle.Utilities;

namespace H3C_BPMT_E.ViewModels.Pages
{
    public partial class DataViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        public DateTime _dateStart = DateTime.Today;

        [ObservableProperty]
        public DateTime _dateEnd = DateTime.Today;

        [ObservableProperty]
        public ObservableCollection<Log>? _logs = new();

        [ObservableProperty]
        private string _searchHistoryText = "搜索历史";

        // 对话日志数据库服务
        private readonly LogService _logService = new();

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom() { }

        private void InitializeViewModel()
        {
            // 加载对话内容
            RetrieveDialogs();
            // System.Windows.MessageBox.Show("DataViewModel initialized.");

            // 标记为已初始化
            _isInitialized = true;
        }

        [RelayCommand]
        public void SearchHistory()
        {
            RetrieveDialogs();
        }

        /// <summary>
        /// 检索对话日志
        /// </summary>
        private void RetrieveDialogs()
        {
            Logs.Clear();
            foreach (var dialog in _logService.GetAllDialogContents(_dateStart, _dateEnd))
            {
                Logs.Add(dialog);
            }
        }

    }
}