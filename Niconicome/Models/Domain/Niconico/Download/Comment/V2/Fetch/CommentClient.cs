﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Niconicome.Models.Domain.Network;
using Niconicome.Models.Domain.Niconico.Net.Json;
using Niconicome.Models.Domain.Niconico.Video.Infomations;
using Niconicome.Models.Domain.Utils;
using Niconicome.Models.Helper.Result;
using Niconicome.Models.Network.Download;
using Windows.Devices.Display.Core;
using Converter = Niconicome.Models.Domain.Niconico.Download.Comment.V2.Core.Converter;
using Core = Niconicome.Models.Domain.Niconico.Download.Comment.V2.Core;
using Fetch = Niconicome.Models.Domain.Niconico.Download.Comment.V2.Fetch;
using Response = Niconicome.Models.Domain.Niconico.Net.Json.API.Comment.V2.Response;

namespace Niconicome.Models.Domain.Niconico.Download.Comment.V2.Fetch
{
    public interface ICommentClient
    {
        /// <summary>
        /// 非同期にコメントをダウンロードする
        /// </summary>
        /// <param name="dmcInfo"></param>
        /// <param name="settings"></param>
        /// <param name="option">オプション</param>
        /// <param name="context">コンテクスト</param>
        /// <param name="token">トークン</param>
        /// <returns></returns>
        Task<IAttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>> DownloadCommentAsync(IDmcInfo dmcInfo, IDownloadSettings settings, ICommentClientOption option, IDownloadContext context, CancellationToken token);
    }

    public class CommentClient : ICommentClient
    {
        public CommentClient(ICommentRequestBuilder requestBuilder, Converter::INetCommentConverter converter, ILogger logger, INicoHttp http, INetWorkHelper helper)
        {
            this._requestBuilder = requestBuilder;
            this._converter = converter;
            this._logger = logger;
            this._helper = helper;
            this._http = http;
        }

        #region field

        /// <summary>
        /// key:スレッドID val:{ index:fork item1: 開始コメ番 item2: コメント数 }
        /// </summary>
        private readonly Dictionary<string, (int, int)[]> _lastFetchedInfo = new();

        private readonly ICommentRequestBuilder _requestBuilder;

        private readonly Converter::INetCommentConverter _converter;

        private readonly ILogger _logger;

        private readonly INicoHttp _http;

        private readonly INetWorkHelper _helper;

        private Core::IThreadInfo? threadInfo;

        #endregion

        #region Method

        public async Task<IAttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>> DownloadCommentAsync(IDmcInfo dmcInfo, IDownloadSettings settings, ICommentClientOption option, IDownloadContext context, CancellationToken token)
        {
            IAttemptResult<(Core::ICommentCollection, Core::IThreadInfo)> result;

            try
            {
                result = await this.DownloadCommentAsyncInternal(dmcInfo, settings, option, context, token);
            }
            catch (Exception ex)
            {
                this._logger.Error("コメント取得でエラーが発生しました。", ex);
                return AttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>.Fail($"コメント取得でエラーが発生しました。（詳細:{ex.Message}）");
            }

            return result;
        }


        #endregion

        #region private

        /// <summary>
        /// 非同期にコメントを取得
        /// </summary>
        /// <param name="dmcInfo"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private async Task<IAttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>> DownloadCommentAsyncInternal(IDmcInfo dmcInfo, IDownloadSettings settings, ICommentClientOption clientOption, IDownloadContext context, CancellationToken token)
        {
            //コレクションを作成
            var collection = new Core::CommentCollection(dmcInfo.CommentCount, settings.CommentCountPerBlock);

            //リクエストビルダーをリセット
            var key = this._requestBuilder.ResetState();

            //変数定義
            var loopIndex = 0;
            var fetchedCommentCountOfDefaultThread = new List<int>();
            Core::IComment? firstComment = null;
            IThread? defaultThread = dmcInfo.CommentThreads.FirstOrDefault(t => t.IsDefaultPostTarget);
            this.threadInfo = null;

            //デフォルトスレッドが存在しない場合はエラー
            if (defaultThread is null)
            {
                return AttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>.Fail("デフォルトスレッドを取得できませんでした。");
            }

            string defaultThreadID = defaultThread.ID.ToString();
            int defaultThreadFork = defaultThread.Fork;
            long lastWhen = 0;

            while (firstComment?.No is null or > 1)
            {

                //メッセージを送信
                if (loopIndex > 0)
                {
                    if (settings.CommentFetchWaitSpan > 0)
                    {
                        try
                        {
                            await Task.Delay(settings.CommentFetchWaitSpan, token);
                        }
                        catch (TaskCanceledException)
                        {
                            return AttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>.Fail("ダウンロード処理がキャンセルされました。");
                        }
                    }

                    context.SendMessage($"過去ログを取得中（{loopIndex + 1}件目・{collection.Count}コメ取得済み）");
                }

                //過去ログの起点を取得
                long when = firstComment is null ? 0 : firstComment.Date - 1;

                //過去ログの起点が前回のループと同じ場合は、ループを終了
                if (loopIndex > 0 && when == lastWhen)
                {
                    break;
                }
                else
                {
                    lastWhen = when;
                }

                //コメント取得の起点に達した場合、ループを終了
                if (loopIndex > 0 && clientOption.IsOriginationSpecified && when < new DateTimeOffset(clientOption.Origination).ToUnixTimeSeconds()) break;

                //リクエストのオプションを定義
                var option = new CommentFetchOption(settings.DownloadOwner, settings.DownloadEasy, settings.DownloadLog && loopIndex > 0, when);

                //コメントを取得
                IAttemptResult<IEnumerable<Core::IComment>> fResult = await this.FetchCommentAsync(dmcInfo, option, key);

                if (!fResult.IsSucceeded || fResult.Data is null)
                {
                    return AttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>.Fail(fResult.Message);
                }

                //キャンセル処理
                token.ThrowIfCancellationRequested();

                var converted = fResult.Data!;

                //取得したコメントのうち、デフォルトスレッドに投稿されたものの数を記録
                fetchedCommentCountOfDefaultThread.Add(this.GetDefaultThreadCommentCount(defaultThreadID, defaultThreadFork, converted));

                //コメントをコレクションに追加
                foreach (var c in converted) collection.Add(c);

                //過去ログをDLしない場合、ループを終了
                if (!settings.DownloadLog && !clientOption.IsOriginationSpecified)
                {
                    break;
                }
                else if (loopIndex == 0)
                {
                    context.SendMessage("過去ログの取得を開始します。");
                }

                //最初のコメントを取得
                IAttemptResult<Core::IComment> fiResult = collection.GetFirstComment(defaultThreadID, defaultThreadFork);
                if (!fiResult.IsSucceeded || fiResult.Data is null)
                {
                    return AttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>.Fail(fResult.Message);
                }
                firstComment = fiResult.Data;

                //変数を更新
                loopIndex++;
            }

            //過去ログをダウンロードしない場合は終了
            if (!settings.DownloadLog && !clientOption.IsOriginationSpecified)
            {
                return AttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>.Succeeded(new(collection, threadInfo!));
            }

            //取得できなかったコメントを再取得
            var averageFetchCount = (int)Math.Floor(fetchedCommentCountOfDefaultThread.Average());
            var unfetchedRange = collection.GetUnFilledRange().OrderByDescending(r => r.Start.No).ToList();

            //取得情報を初期化
            this.SetupLastFetchedInfo(dmcInfo);
            loopIndex = 0;

            //メッセージを送信
            context.SendMessage("取得できなかったコメントを再取得します。");

            while (true)
            {
                //メッセージを送信
                if (loopIndex > 0)
                {
                    if (settings.CommentFetchWaitSpan > 0)
                    {
                        try
                        {
                            await Task.Delay(settings.CommentFetchWaitSpan, token);
                        }
                        catch (TaskCanceledException)
                        {
                            return AttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>.Fail("ダウンロード処理がキャンセルされました。");
                        }
                    }

                    context.SendMessage($"コメントを再取得中（{loopIndex + 1}件目・{collection.Count}コメ取得済み）");
                }

                //取得できなかったコメントのうち、一番新しいものの情報（すでに取得したものは除外、取得起点より古いものは除外）
                var unfetched = collection.GetUnFilledRange().OrderByDescending(r => r.Start.No).FirstOrDefault(r => this._lastFetchedInfo[r.Thread][r.Fork].Item1 == 0 || r.Start.No < this._lastFetchedInfo[r.Thread][r.Fork].Item1 - this._lastFetchedInfo[r.Thread][r.Fork].Item2 && (!clientOption.IsOriginationSpecified || DateTimeOffset.FromUnixTimeSeconds(r.Start.Date).ToLocalTime().DateTime > clientOption.Origination));

                //取得できなかったコメントが存在しないので終了
                if (unfetched is null)
                {
                    return AttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>.Succeeded(new(collection, threadInfo!));
                }

                //取得
                var option = new CommentFetchOption(settings.DownloadOwner, settings.DownloadEasy, true, unfetched.Start.Date - 1);
                IAttemptResult<IEnumerable<Core::IComment>> fResult = await this.FetchCommentAsync(dmcInfo, option, key);

                if (!fResult.IsSucceeded || fResult.Data is null)
                {
                    return AttemptResult<(Core::ICommentCollection, Core::IThreadInfo)>.Fail(fResult.Message);
                }

                //キャンセル処理
                token.ThrowIfCancellationRequested();

                //コメントを追加
                foreach (var c in fResult.Data!) collection.Add(c);

                //変数を更新
                this._lastFetchedInfo[unfetched.Thread][unfetched.Fork] = new(unfetched.Start.No - 1, settings.CommentCountPerBlock);// this.GetDefaultThreadCommentCount(defaultThreadID, defaultThreadFork, fResult.Data));
                loopIndex++;
            }



        }

        /// <summary>
        /// 与えられたコメントのうち、デフォルトスレッドに投稿された数を取得
        /// </summary>
        /// <param name="threadID"></param>
        /// <param name="fork"></param>
        /// <param name="comments"></param>
        /// <returns></returns>
        private int GetDefaultThreadCommentCount(string threadID, int fork, IEnumerable<Core::IComment> comments)
        {
            return comments.Where(c => c.Thread == threadID && c.Fork == fork).Count();
        }

        /// <summary>
        /// コメント取得情報を初期化
        /// </summary>
        /// <param name="dmcInfo"></param>
        private void SetupLastFetchedInfo(IDmcInfo dmcInfo)
        {
            if (dmcInfo.CommentThreads.Count == 0) return;

            //forkの最大値を求める
            var max = dmcInfo.CommentThreads.Select(t => t.Fork).Max();

            //リセット
            this._lastFetchedInfo.Clear();

            //threadを追加
            foreach (var threadID in dmcInfo.CommentThreads.Select(t => t.ID.ToString()))
            {
                if (this._lastFetchedInfo.ContainsKey(threadID)) continue;
                this._lastFetchedInfo.Add(threadID, new (int, int)[max + 1]);
            }

        }

        /// <summary>
        /// コメント取得の実処理（リクエスト構築=>DL=>デシリアライズ=>抽象コメント化）
        /// </summary>
        /// <param name="dmcInfo"></param>
        /// <param name="option"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task<IAttemptResult<IEnumerable<Core::IComment>>> FetchCommentAsync(IDmcInfo dmcInfo, ICommentFetchOption option, string key)
        {
            //リクエストを構築
            IAttemptResult<string> rResult = await this._requestBuilder.BuildRequestAsync(dmcInfo, option, key);
            if (!rResult.IsSucceeded || rResult.Data is null)
            {
                return AttemptResult<IEnumerable<Core::IComment>>.Fail(rResult.Message);
            }

            string url = dmcInfo.CommentServer.EndsWith(".json") ? dmcInfo.CommentServer : dmcInfo.CommentServer[0..^1] + ".json";

            //コメントをダウンロード
            HttpResponseMessage res = await this._http.PostAsync(new Uri(url), new StringContent(rResult.Data));

            if (!res.IsSuccessStatusCode)
            {
                string status = this._helper.GetHttpStatusForLog(res);
                this._logger.Error($"コメントサーバーへのリクエストに失敗しました。（{status}）");
                return AttemptResult<IEnumerable<Core::IComment>>.Fail($"コメントサーバーへのリクエストに失敗しました。（{ status}）");
            }

            IReadOnlyList<Response::ResponseRoot> data;

            try
            {
                string content = await res.Content.ReadAsStringAsync();
                data = JsonParser.DeSerialize<IReadOnlyList<Response::ResponseRoot>>(content);
            }
            catch (Exception ex)
            {
                this._logger.Error("コメントの解析に失敗しました。", ex);
                return AttemptResult<IEnumerable<Core::IComment>>.Fail($"コメントの解析に失敗しました。（詳細:{ex.Message}）");
            }

            //ダウンロードしたコメントを変換
            var converted = data.Where(c => c.Chat is not null).Select(c => this._converter.ConvertNetCommentToCoreComment(c.Chat!));

            if (this.threadInfo is null)
            {
                Response::Thread? responseThread = data.FirstOrDefault(r => r.Thread is not null)?.Thread;

                if (responseThread is null)
                {
                    return AttemptResult<IEnumerable<Core::IComment>>.Fail("スレッド情報の取得に失敗しました。");
                }

                threadInfo = this._converter.ConvertNetThreadToCoreThreadInfo(responseThread);
            }


            return AttemptResult<IEnumerable<Core::IComment>>.Succeeded(converted);

        }

        #endregion
    }
}