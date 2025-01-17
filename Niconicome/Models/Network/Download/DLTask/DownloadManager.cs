﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Niconicome.Models.Const;
using Niconicome.Models.Domain.Local.Settings;
using Niconicome.Models.Domain.Utils;
using Niconicome.Models.Helper.Result;
using Niconicome.Models.Local.Settings;
using Niconicome.Models.Local.State;
using Niconicome.Models.Playlist.V2;
using Niconicome.Models.Playlist.VideoList;
using Niconicome.Models.Utils;
using Reactive.Bindings;

namespace Niconicome.Models.Network.Download.DLTask
{
    public interface IDownloadManager
    {
        /// <summary>
        /// ダウンロードタスク
        /// </summary>
        ReadOnlyObservableCollection<IDownloadTask> Queue { get; init; }

        /// <summary>
        /// ステージ済みタスク
        /// </summary>
        ReadOnlyObservableCollection<IDownloadTask> Staged { get; init; }

        /// <summary>
        /// キャンセル済みを表示
        /// </summary>
        ReactiveProperty<bool> DisplayCanceled { get; }

        /// <summary>
        /// 完了済みを表示
        /// </summary>
        ReactiveProperty<bool> DisplayCompleted { get; }

        /// <summary>
        /// ダウンロード中
        /// </summary>
        ReadOnlyReactiveProperty<bool> IsProcessing { get; }

        /// <summary>
        /// 動画をステージ
        /// </summary>
        void StageVIdeo();

        /// <summary>
        /// ステージ済みタスクをクリア
        /// </summary>
        void ClearStaged();

        /// <summary>
        /// 指定したタスクをステージ済みから削除
        /// </summary>
        /// <param name="task"></param>
        void RemoveFromStaged(IDownloadTask task);

        /// <summary>
        /// ダウンロードをキャンセル
        /// </summary>
        void CancelDownload();

        /// <summary>
        /// ダウンロードを開始する
        /// </summary>
        /// <returns></returns>
        Task StartDownloadAsync(Action<string> onMessage, Action<string> onMessageVerbose);
    }

    public class DownloadManager : IDownloadManager
    {

        public DownloadManager(ISettingsContainer settingsContainer, IPlaylistVideoContainer videoListContainer, IDownloadSettingsHandler settingHandler, ILogger logger, ICurrent current)
        {
            this.Queue = this._queuePool.Tasks;
            this.Staged = this._stagedPool.Tasks;
            this.DisplayCanceled = this._queuePool.DisplayCanceled;
            this.DisplayCompleted = this._queuePool.DisplayCompleted;
            this.IsProcessing = this._isProcessingSource.ToReadOnlyReactiveProperty();
            this._settingsContainer = settingsContainer;
            this._videoListContainer = videoListContainer;
            this._settingsHandler = settingHandler;
            this._logger = logger;
            this._current = current;

            this.RegisterParallelTasksHandler();

        }

        #region field

        private readonly ILogger _logger;

        private readonly ISettingsContainer _settingsContainer;

        private readonly ICurrent _current;

        private readonly IDownloadSettingsHandler _settingsHandler;

        private readonly IPlaylistVideoContainer _videoListContainer;

        private readonly List<IDownloadTask> _processingTasks = new();

        private readonly ReactiveProperty<bool> _isProcessingSource = new();

        private ReactiveProperty<int>? _maxParallelDL;

        private ReactiveProperty<int>? _sleepInterval;

        private ParallelTasksHandler<IDownloadTask>? _tasksHandler;

        private readonly IDownloadTaskPool _queuePool = new DownloadTaskPool();

        private readonly IDownloadTaskPool _stagedPool = new DownloadTaskPool();

        private CancellationTokenSource? _cts;

        #endregion

        #region Props

        public ReadOnlyObservableCollection<IDownloadTask> Queue { get; init; }

        public ReadOnlyObservableCollection<IDownloadTask> Staged { get; init; }

        public ReactiveProperty<bool> DisplayCanceled { get; init; }

        public ReactiveProperty<bool> DisplayCompleted { get; init; }

        public ReadOnlyReactiveProperty<bool> IsProcessing { get; init; }

        #endregion

        #region Method


        public void StageVIdeo()
        {

            foreach (var video in this._videoListContainer.Videos.Where(v => v.IsSelected.Value).ToList())
            {

                DownloadSettings settings = this._settingsHandler.CreateDownloadSettings();

                //動画固有の情報を設定
                settings.NiconicoId = video.NiconicoId;
                settings.IsEconomy = video.IsEconomy;
                settings.FilePath = video.FilePath;

                //タスクを作成
                IDownloadTask task = DIFactory.Provider.GetRequiredService<IDownloadTask>();
                task.Initialize(video, settings);

                this._stagedPool.AddTask(task);
            }
        }

        public void ClearStaged()
        {
            this._stagedPool.Clear();
        }

        public void RemoveFromStaged(IDownloadTask task)
        {
            this._stagedPool.RemoveTask(task);
        }



        public async Task StartDownloadAsync(Action<string> onMessage, Action<string> onMessageVerbose)
        {
            void Finalize()
            {
                this._isProcessingSource.Value = false;
                this._cts = null;
                this._processingTasks.Clear();
            }

            //ダウンロード中ならキャンセル
            if (this.IsProcessing.Value) return;

            //初期化
            this._processingTasks.Clear();

            //ステージ済みをキューに移動
            this.MoveStagedToQueue();
            var videoCount = this._tasksHandler!.PallarelTasks.Count;
            this._processingTasks.AddRange(this._tasksHandler.PallarelTasks);

            //タスクが0なら中止
            if (videoCount == 0) return;

            //DL開始
            this._isProcessingSource.Value = true;
            onMessageVerbose($"動画のダウンロードを開始します。({videoCount}件)");
            onMessage($"動画のダウンロードを開始します。({videoCount}件)");

            //トークン生成
            this._cts = new CancellationTokenSource();

            try
            {
                await this._tasksHandler.ProcessTasksAsync(ct: this._cts.Token);
            }
            catch (Exception e)
            {
                this._logger.Error("ダウンロード中にエラーが発生しました", e);
                onMessageVerbose($"ダウンロード中にエラーが発生しました。(詳細: {e.Message})");
                onMessage($"ダウンロード中にエラーが発生しました。");
                Finalize();
                return;
            }

            //結果判定
            int succeededCount = this._processingTasks.Select(t => t.IsSuceeded).Count();

            if (succeededCount == 0)
            //1件もできなかった
            {
                onMessageVerbose("動画を1件もダウンロード出来ませんでした。");
                onMessage("動画を1件もダウンロード出来ませんでした。");
            }
            else
            {
                string niconicoID = this._processingTasks.First().NiconicoID;

                if (succeededCount > 1)
                //2件以上DLできた
                {
                    onMessageVerbose($"{niconicoID}ほか{succeededCount - 1}件の動画をダウンロードしました。");
                    onMessage($"{niconicoID}ほか{succeededCount - 1}件の動画をダウンロードしました。");

                    if (succeededCount < videoCount)
                    {
                        onMessageVerbose($"{videoCount - succeededCount}件の動画のダウンロードに失敗しました。");
                    }
                }
                else if (succeededCount == 1)
                //1件だけDLできた
                {
                    onMessageVerbose($"{niconicoID}をダウンロードしました。");
                    onMessage($"{niconicoID}をダウンロードしました。");
                }
            }

            Finalize();
        }

        public void CancelDownload()
        {
            //並列タスクハンドラ用のトークンをキャンセル
            this._cts?.Cancel();
            this._cts = null;

            //ダウンロード中のタスクをキャンセル
            foreach (var task in this._processingTasks)
            {
                task.Cancel();
            }

            this._tasksHandler!.CancellAllTasks();
            this._isProcessingSource.Value = false;
        }

        #endregion

        #region private

        /// <summary>
        /// ステージング済みタスクをキューに移動
        /// </summary>
        private void MoveStagedToQueue()
        {
            bool downloadFromAnotherPlaylist = this._settingsContainer.GetSetting(SettingNames.DownloadAllWhenPushDLButton, false).Data?.Value ?? false;
            int playlistID = this._current.SelectedPlaylist.Value?.Id ?? -1;

            if (downloadFromAnotherPlaylist || playlistID == -1)
            {
                foreach (var t in this._stagedPool.Tasks)
                {
                    this._queuePool.AddTask(t);
                    this._tasksHandler!.AddTaskToQueue(t);
                }
                this._stagedPool.Clear();

            }
            else
            {
                foreach (var t in this._stagedPool.Tasks.Where(t => t.PlaylistID == playlistID).ToList())
                {
                    this._queuePool.AddTask(t);
                    this._tasksHandler!.AddTaskToQueue(t);
                    this._stagedPool.RemoveTask(t);
                }
            }
        }


        /// <summary>
        /// 並行タスクハンドラを設定
        /// </summary>
        private void RegisterParallelTasksHandler()
        {
            IAttemptResult<ISettingInfo<int>> parallelResult = this._settingsContainer.GetSetting(SettingNames.MaxParallelDownloadCount, NetConstant.DefaultMaxParallelDownloadCount);

            IAttemptResult<ISettingInfo<int>> sleepResult = this._settingsContainer.GetSetting(SettingNames.FetchSleepInterval, NetConstant.DefaultFetchWaitInterval);

            if (this.CheckWhetherGetSettingSucceededOrNot(parallelResult, sleepResult))
            {
                this._tasksHandler = new ParallelTasksHandler<IDownloadTask>(parallelResult.Data!.Value, sleepResult.Data!.Value, 15, untilEmpty: true);

                this._sleepInterval = sleepResult.Data.ReactiveValue!;
                this._maxParallelDL = parallelResult.Data.ReactiveValue!;

                Observable.Merge(this._maxParallelDL, this._sleepInterval).Subscribe(_ =>
                {
                    if (this._tasksHandler.IsProcessing) return;
                    this._tasksHandler = new ParallelTasksHandler<IDownloadTask>(this._maxParallelDL.Value, this._sleepInterval.Value, 15, untilEmpty: true);
                });
            }

        }

        private bool CheckWhetherGetSettingSucceededOrNot(IAttemptResult<ISettingInfo<int>> parallelResult, IAttemptResult<ISettingInfo<int>> sleepResult)
        {
            if (!parallelResult.IsSucceeded)
            {
                return false;
            }

            if (parallelResult.Data is null)
            {
                return false;
            }

            if (!sleepResult.IsSucceeded)
            {
                return false;
            }

            if (sleepResult.Data is null)
            {
                return false;
            }

            return true;
        }

        #endregion


    }
}
