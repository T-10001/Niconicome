﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niconicome.Models.Auth;
using Niconicome.Models.Local;
using Niconicome.ViewModels.Mainpage.Utils;
using WS = Niconicome.Workspaces;

namespace Niconicome.ViewModels.Setting.Pages
{
    class GeneralSettingsPageViewModel : SettingaBase
    {
        public GeneralSettingsPageViewModel()
        {
            this.isAutologinEnableField = WS::SettingPage.SettingHandler.GetBoolSetting(Settings.AutologinEnable);

            var normal = new AutoLoginTypes(AutoLoginTypeString.Normal, "パスワードログイン");
            var wv2 = new AutoLoginTypes(AutoLoginTypeString.Webview2, "Webview2とCookieを共有");

            this.SelectableAutoLoginTypes = new List<AutoLoginTypes>() { normal, wv2 };
            this.isSkippingSSLVerificationEnableField = WS::SettingPage.SettingHandler.GetBoolSetting(Settings.SkipSSLVerification);

            var type = WS::SettingPage.SettingHandler.GetStringSetting(Settings.AutologinMode);
            this.selectedAutoLoginTypeField = type switch
            {
                AutoLoginTypeString.Normal => normal,
                AutoLoginTypeString.Webview2 => wv2,
                _ => normal
            };

            var n1 = new ComboboxItem<int>(1, "1");
            var n2 = new ComboboxItem<int>(2, "2");
            var n3 = new ComboboxItem<int>(3, "3");
            var n4 = new ComboboxItem<int>(4, "4");
            var n5 = new ComboboxItem<int>(5, "5");

            this.SelectablefetchSleepInterval = new List<ComboboxItem<int>> { n1, n2, n3, n4, n5 };
            this.SelectableMaxParallelFetch = new List<ComboboxItem<int>>() { n1, n2, n3, n4 };


            var maxFetchParallelCount = WS::SettingPage.SettingHandler.GetIntSetting(Settings.MaxFetchCount);
            var fetchSleepInterval = WS::SettingPage.SettingHandler.GetIntSetting(Settings.FetchSleepInterval);

            this.maxFetchParallelCountField = maxFetchParallelCount switch
            {
                1 => n1,
                2 => n2,
                3 => n3,
                4 => n4,
                _ => n4,
            };

            this.fetchSleepIntervalFIeld = fetchSleepInterval switch
            {
                1 => n1,
                2 => n2,
                3 => n3,
                4 => n4,
                5 => n5,
                _ => n4,
            };
        }

        private bool isAutologinEnableField;

        private bool isSkippingSSLVerificationEnableField;

        private string empty = string.Empty;

        private ComboboxItem<int> maxFetchParallelCountField;

        private ComboboxItem<int> fetchSleepIntervalFIeld;

        private AutoLoginTypes selectedAutoLoginTypeField;

        /// <summary>
        /// 選択可能な自動ログインの種別
        /// </summary>
        public List<AutoLoginTypes> SelectableAutoLoginTypes { get; init; }

        /// <summary>
        /// 自動ログインの種別
        /// </summary>
        public AutoLoginTypes SelectedAutoLoginType
        {
            get => this.selectedAutoLoginTypeField; set
            {
                this.Savesetting(ref this.empty, value.Value, Settings.AutologinMode);
                this.SetProperty(ref this.selectedAutoLoginTypeField, value);
            }
        }

        /// <summary>
        /// 同時取得数
        /// </summary>
        public List<ComboboxItem<int>> SelectableMaxParallelFetch { get; init; }

        /// <summary>
        /// スリープ間隔
        /// </summary>
        public List<ComboboxItem<int>> SelectablefetchSleepInterval { get; init; }

        /// <summary>
        /// 自動ログイン
        /// </summary>
        public bool IsAutologinEnable { get => this.isAutologinEnableField; set => this.Savesetting(ref this.isAutologinEnableField, value, Settings.AutologinEnable); }

        /// <summary>
        /// 動画情報最大並列取得数
        /// </summary>
        public ComboboxItem<int> MaxFetchParallelCount
        {
            get => this.maxFetchParallelCountField;
            set => this.Savesetting(ref this.maxFetchParallelCountField, value, Settings.MaxFetchCount);
        }

        /// <summary>
        /// 待機間隔
        /// </summary>
        public ComboboxItem<int> FetchSleepInterval { get => this.fetchSleepIntervalFIeld; set => this.Savesetting(ref this.fetchSleepIntervalFIeld, value, Settings.FetchSleepInterval); }

        /// <summary>
        /// SSL証明書の検証をスキップする
        /// </summary>
        public bool IsSkippingSSLVerificationEnable { get => this.isSkippingSSLVerificationEnableField; set => this.Savesetting(ref this.isSkippingSSLVerificationEnableField, value, Settings.SkipSSLVerification); }

    }

    class GeneralSettingsPageViewModelD
    {

        public GeneralSettingsPageViewModelD()
        {
            var n1 = new ComboboxItem<int>(1, "1");
            var n2 = new ComboboxItem<int>(2, "2");
            var n3 = new ComboboxItem<int>(3, "3");
            var n4 = new ComboboxItem<int>(4, "4");
            var n5 = new ComboboxItem<int>(5, "5");

            this.SelectablefetchSleepInterval = new List<ComboboxItem<int>> { n1, n2, n3, n4, n5 };
            this.SelectableMaxParallelFetch = new List<ComboboxItem<int>>() { n1, n2, n3, n4 };

            this.MaxFetchParallelCount = n3;
            this.FetchSleepInterval = n5;
        }

        public bool IsAutologinEnable { set; get; } = true;

        public bool IsSkippingSSLVerificationEnable { get; set; } = false;

        public List<AutoLoginTypes> SelectableAutoLoginTypes { get; init; } = new();

        public AutoLoginTypes SelectedAutoLoginType { get; init; } = new AutoLoginTypes(AutoLoginTypeString.Normal, "パスワード");

        public List<ComboboxItem<int>> SelectableMaxParallelFetch { get; init; }

        public List<ComboboxItem<int>> SelectablefetchSleepInterval { get; init; }

        public ComboboxItem<int> MaxFetchParallelCount { get; set; }

        public ComboboxItem<int> FetchSleepInterval { get; set; }

    }

    class AutoLoginTypes
    {
        public AutoLoginTypes(string value, string displayvalue)
        {
            this.Value = value;
            this.DisplayValue = displayvalue;

            var n1 = new ComboboxItem<int>(1, "1");
            var n2 = new ComboboxItem<int>(2, "2");
            var n3 = new ComboboxItem<int>(3, "3");
            var n4 = new ComboboxItem<int>(4, "4");
            var n5 = new ComboboxItem<int>(5, "5");

            this.SelectablefetchSleepInterval = new List<ComboboxItem<int>> { n1, n2, n3, n4, n5 };
            this.SelectableMaxParallelFetch = new List<ComboboxItem<int>>() { n1, n2, n3, n4 };
        }

        public string Value { get; init; }

        public string DisplayValue { get; init; }

        public List<ComboboxItem<int>> SelectableMaxParallelFetch { get; init; }

        public List<ComboboxItem<int>> SelectablefetchSleepInterval { get; init; }


    }
}