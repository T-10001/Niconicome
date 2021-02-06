﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Niconicome.Extensions.System;
using Niconicome.Extensions.System.List;
using Niconicome.Models.Local;
using WS = Niconicome.Workspaces;
using MD = MaterialDesignThemes.Wpf;
using Niconicome.Extensions;

namespace Niconicome.ViewModels.Setting.Pages
{
    class RestorePageViewModel : BindableBase
    {
        public RestorePageViewModel(Func<string, string, MessageBoxButton, MessageBoxImage, MessageBoxResult> showMessage)
        {
            this.showMessage = showMessage;
            this.Backups = new ObservableCollection<IBackupData>();
            this.Backups.Addrange(WS::SettingPage.Restore.GetAllBackups());
            this.SnackbarMessageQueue = WS::SettingPage.SnackbarMessageQueue;

            this.CreatebackupCommand = new CommandBase<object>(_ => true, _ =>
            {
                if (this.BackupName.IsNullOrEmpty()) return;
                var result = WS::SettingPage.Restore.TryCreateBackup(this.BackupName);
                if (result)
                {
                    this.Backups.Clear();
                    this.Backups.Addrange(WS::SettingPage.Restore.GetAllBackups());
                    this.SnackbarMessageQueue.Enqueue($"バックアップ「{this.BackupName}」を作成しました。");
                    this.BackupName = string.Empty;
                }
                else
                {
                    this.SnackbarMessageQueue.Enqueue("バックアップの作成に失敗しました。");
                }
            });

            this.ResetSettingsCommand = new CommandBase<object>(_ => true, _ =>
            {
                var confirm = this.showMessage("本当に設定を削除しますか？この操作は元に戻すことができません。", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;
                WS::SettingPage.Restore.ResetSettings();
                this.SnackbarMessageQueue.Enqueue("設定をリセットしました。");
            });

            this.RemovebackupCommand = new CommandBase<IBackupData>(_ => true, arg =>
            {
                if (arg is null) return;
                if (arg.AsNullable<IBackupData>() is not IBackupData backup || backup is null) return;

                bool result = WS::SettingPage.Restore.TryRemoveBackup(backup.GUID);

                if (result)
                {
                    this.SnackbarMessageQueue.Enqueue($"バックアップを削除しました。");
                    this.Backups.Clear();
                    this.Backups.Addrange(WS::SettingPage.Restore.GetAllBackups());
                }
                else
                {
                    this.SnackbarMessageQueue.Enqueue($"バックアップ「{backup.Name}」の削除に失敗しました。");
                }

            });

            this.ApplyBackupCommand = new CommandBase<IBackupData>(_ => true, arg =>
            {

                if (arg is null) return;
                if (arg.AsNullable<IBackupData>() is not IBackupData backup || backup is null) return;

                var confirm = this.showMessage("本当にこのバックアップを適用しますか？現在の設定は全て削除され、操作は元に戻すことができません。", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                bool result = WS::SettingPage.Restore.TryApplyBackup(backup.GUID);

                if (result)
                {
                    this.SnackbarMessageQueue.Enqueue($"バックアップを適用しました。");
                }
                else
                {
                    this.SnackbarMessageQueue.Enqueue($"バックアップの適用に失敗しました。");
                }
            });

            this.ResetDataCommand = new CommandBase<object>(_ => true,_=>
            {

                var confirm = this.showMessage("本当に全ての動画・プレイリストを削除しますか？この操作は元に戻すことができません。", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;
                WS::SettingPage.Restore.DeleteAllVideosAndPlaylists();
                this.SnackbarMessageQueue.Enqueue("データをリセットしました。");
                WS::SettingPage.PlaylistTreeHandler.Refresh();
                WS::SettingPage.PlaylistTreeHandler.Refresh();
            });

            this.LoadSavedFiles = new CommandBase<object>(_ => true, _ =>
            {
                WS::SettingPage.Restore.JustifySavedFilePaths();
            });
        }


        public RestorePageViewModel() : this((message, title, button, image) => MessageBox.Show(message, title, button, image))
        {
        }

        private string backupNameField = string.Empty;

        /// <summary>
        /// バックアップ名
        /// </summary>
        public string BackupName { get => this.backupNameField; set => this.SetProperty(ref this.backupNameField, value); }

        /// <summary>
        /// バックアップを作成する
        /// </summary>
        public CommandBase<object> CreatebackupCommand { get; init; }

        /// <summary>
        /// 設定をリセット
        /// </summary>
        public CommandBase<object> ResetSettingsCommand { get; init; }

        /// <summary>
        /// バックアップ一覧
        /// </summary>
        public ObservableCollection<IBackupData> Backups { get; init; }

        /// <summary>
        /// バックアップを削除
        /// </summary>
        public CommandBase<IBackupData> RemovebackupCommand { get; init; }

        /// <summary>
        /// バックアップを適用
        /// </summary>
        public CommandBase<IBackupData> ApplyBackupCommand { get; init; }

        /// <summary>
        /// データをリセット
        /// </summary>
        public CommandBase<object> ResetDataCommand { get; init; }

        /// <summary>
        /// 保存したファイルを再読み込み
        /// </summary>
        public CommandBase<object> LoadSavedFiles { get; init; }

        /// <summary>
        /// メッセージキュー
        /// </summary>
        public MD::ISnackbarMessageQueue SnackbarMessageQueue { get; init; }

        /// <summary>
        /// メッセージボックス
        /// </summary>
        private readonly Func<string, string, MessageBoxButton, MessageBoxImage, MessageBoxResult> showMessage;
    }
}
