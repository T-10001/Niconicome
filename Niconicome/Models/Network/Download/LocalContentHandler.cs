﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niconicome.Models.Domain.Local.Store;
using Niconicome.Models.Domain.Niconico.Watch;
using Niconicome.Models.Domain.Utils;

namespace Niconicome.Models.Network.Download
{
    public interface ILocalContentHandler
    {
        ILocalContentInfo GetLocalContentInfo(string folderPath, string format, IDmcInfo dmcInfo, bool replaceStricted, string videoInfoExt, string ichibaInfoExt);
        IDownloadResult MoveDownloadedFile(string niconicoId, string downloadedFilePath, string destinationPath);
    }

    public interface ILocalContentInfo
    {
        bool CommentExist { get; init; }
        bool ThumbExist { get; init; }
        bool VideoExist { get; init; }
        bool VIdeoExistInOnotherFolder { get; init; }
        bool VideoInfoExist { get; init; }
        bool IchibaInfoExist { get; init; }
        string? LocalPath { get; init; }
    }

    /// <summary>
    /// ローカルデータの処理
    /// </summary>
    public class LocalContentHandler : ILocalContentHandler
    {
        public LocalContentHandler(INiconicoUtils niconicoUtils, IVideoFileStorehandler videoFileStorehandler, ILogger logger)
        {
            this.niconicoUtils = niconicoUtils;
            this.videoFileStorehandler = videoFileStorehandler;
            this.logger = logger;
        }

        private readonly INiconicoUtils niconicoUtils;

        private readonly IVideoFileStorehandler videoFileStorehandler;

        private readonly ILogger logger;

        /// <summary>
        /// ローカルの保存状況を取得する
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="format"></param>
        /// <param name="dmcInfo"></param>
        /// <returns></returns>
        public ILocalContentInfo GetLocalContentInfo(string folderPath, string format, IDmcInfo dmcInfo, bool replaceStricted, string videoInfoExt, string ichibaInfoExt)
        {
            string videoFIlename = this.niconicoUtils.GetFileName(format, dmcInfo, ".mp4", replaceStricted);
            string commentFIlename = this.niconicoUtils.GetFileName(format, dmcInfo, ".xml", replaceStricted);
            string thumbFIlename = this.niconicoUtils.GetFileName(format, dmcInfo, ".jpg", replaceStricted);
            string videoInfoFilename = this.niconicoUtils.GetFileName(format, dmcInfo, videoInfoExt, replaceStricted);
            string ichibaInfoFilename = this.niconicoUtils.GetFileName(format, dmcInfo, ichibaInfoExt, replaceStricted);
            bool videoExist = this.videoFileStorehandler.Exists(dmcInfo.Id);
            string? localPath = null;

            if (videoExist)
            {
                localPath = this.videoFileStorehandler.GetFilePath(dmcInfo.Id);
            }

            return new LocalContentInfo()
            {
                VideoExist = File.Exists(Path.Combine(folderPath, videoFIlename)),
                CommentExist = File.Exists(Path.Combine(folderPath, commentFIlename)),
                ThumbExist = File.Exists(Path.Combine(folderPath, thumbFIlename)),
                VideoInfoExist = File.Exists(Path.Combine(folderPath, videoInfoFilename)),
                VIdeoExistInOnotherFolder = videoExist,
                IchibaInfoExist = File.Exists(Path.Combine(folderPath, ichibaInfoFilename)),
                LocalPath = localPath,
            };
        }

        /// <summary>
        /// ダウンロード済のファイルをコピーする
        /// </summary>
        /// <param name="niconicoId"></param>
        /// <param name="downloadedFilePath"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        public IDownloadResult MoveDownloadedFile(string niconicoId, string downloadedFilePath, string destinationPath)
        {
            if (!File.Exists(downloadedFilePath))
            {
                return new DownloadResult() { Message = "そのようなファイルは存在しません。" };
            }

            if (!Directory.Exists(destinationPath))
            {
                try
                {
                    Directory.CreateDirectory(destinationPath);
                }
                catch (Exception e)
                {
                    this.logger.Error("移動先フォルダーの作成に失敗しました。", e);
                    return new DownloadResult() { Message = "移動先フォルダーの作成に失敗しました。" };
                }
            }

            string filename = Path.GetFileName(downloadedFilePath);
            try
            {
                File.Copy(downloadedFilePath, Path.Combine(destinationPath, filename));
            }
            catch (Exception e)
            {
                this.logger.Error("ファイルのコピーに失敗しました。", e);
                return new DownloadResult() { Message = $"ファイルのコピーに失敗しました。" };
            }

            this.videoFileStorehandler.Add(niconicoId, Path.Combine(destinationPath, filename));

            return new DownloadResult() { IsSucceeded = true };

        }

    }

    /// <summary>
    /// ローカル情報
    /// </summary>
    public class LocalContentInfo : ILocalContentInfo
    {
        public LocalContentInfo(string? localPath)
        {
            this.LocalPath = localPath;
        }

        public LocalContentInfo() : this(null) { }

        public bool VideoExist { get; init; }

        public bool VIdeoExistInOnotherFolder { get; init; }

        public bool CommentExist { get; init; }

        public bool ThumbExist { get; init; }

        public bool VideoInfoExist { get; init; }

        public bool IchibaInfoExist { get; init; }

        public string? LocalPath { get; init; }
    }
}