﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Niconicome.Models.Domain.Niconico.Watch;
using Const = Niconicome.Models.Const;

namespace Niconicome.Models.Domain.Niconico.Video.Infomations
{
    public interface IDmcInfo
    {
        string Title { get; set; }
        string Id { get; set; }
        string Owner { get; set; }
        int OwnerID { get; set; }
        string Userkey { get; }
        string UserId { get; }
        string ChannelID { get; }
        string ChannelName { get; }
        string Description { get; }
        string CommentServer { get; }
        string Threadkey { get; }
        string CommentLanguage { get; }
        int ViewCount { get; }
        int CommentCount { get; }
        int MylistCount { get; }
        int LikeCount { get; }
        int Duration { get; }
        IReadOnlyList<ITag> Tags { get; }
        bool IsDownloadable { get; set; }
        bool IsEncrypted { get; set; }
        bool IsOfficial { get; set; }
        bool IsPremium { get; set; }
        bool IsPeakTime { get; set; }
        bool IsEnonomy { get; }
        DateTime UploadedOn { get; set; }
        DateTime DownloadStartedOn { get; set; }
        IThumbInfo ThumbInfo { get; set; }
        ISessionInfo SessionInfo { get; }
        IReadOnlyCollection<ITarget> CommentTargets { get; }
        List<IThread> CommentThreads { get; }
    }
    public interface IThread
    {
        long ID { get; }
        int Fork { get; }
        bool IsLeafRequired { get; }
        bool IsOwnerThread { get; }
        bool IsThreadkeyRequired { get; }
        bool IsActive { get; }
        bool IsDefaultPostTarget { get; }
        bool IsEasyCommentPostTarget { get; }
        string? Threadkey { get; }
        bool Is184Forced { get; }
        bool HasNicoscript { get; }
        string Label { get; }
        string ForkLabel { get; }
        int PostkeyStatus { get; }
        string Server { get; }
        string VideoID { get; }
    }

    public interface ITarget
    {
        string Id { get; }

        string Fork { get; }
    }

    public interface ITag
    {
        string Name { get; }

        bool IsNicodicExist { get; }
    }


    /// <summary>
    /// APi等へのアクセスに必要な情報を格納する
    /// </summary>
    public class DmcInfo : IDmcInfo
    {
        public string Title { get; set; } = string.Empty;

        public string Id { get; set; } = string.Empty;

        public string Owner { get; set; } = string.Empty;

        public string Userkey { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string ChannelID { get; set; } = string.Empty;

        public string ChannelName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string CommentServer { get; set; } = string.Empty;

        public string Threadkey { get; set; } = string.Empty;

        public string CommentLanguage { get; set; } = string.Empty;

        public int ViewCount { get; set; }

        public int CommentCount { get; set; }

        public int LikeCount { get; set; }

        public int MylistCount { get; set; }

        public int Duration { get; set; }

        public int OwnerID { get; set; }

        public IReadOnlyList<ITag> Tags { get; set; } = new List<ITag>();

        public bool IsDownloadable { get; set; } = true;

        public bool IsEncrypted { get; set; }

        public bool IsOfficial { get; set; }

        public bool IsPremium { get; set; }

        public bool IsPeakTime { get; set; }

        public bool IsEnonomy => !this.IsPremium && this.IsPeakTime && this.ViewCount >= Const::NetConstant.EconomyAvoidableViewCount;

        /// <summary>
        /// 投稿日時
        /// </summary>
        public DateTime UploadedOn { get; set; }

        /// <summary>
        /// DL開始日時
        /// </summary>
        public DateTime DownloadStartedOn { get; set; }


        /// <summary>
        /// サムネイル
        /// </summary>
        public IThumbInfo ThumbInfo { get; set; } = new ThumbInfo();

        /// <summary>
        /// セッション情報
        /// </summary>
        public ISessionInfo SessionInfo { get; init; } = new SessionInfo();

        /// <summary>
        /// コメントターゲット
        /// </summary>
        public IReadOnlyCollection<ITarget> CommentTargets { get; set; } = (new List<ITarget>()).AsReadOnly();

        /// <summary>
        /// コメントスレッド
        /// </summary>
        public List<IThread> CommentThreads { get; set; } = new();
    }


    public class Thread : IThread
    {
        public long ID { get; init; }

        public int Fork { get; init; }

        public bool IsActive { get; init; }

        public bool IsDefaultPostTarget { get; init; }

        public bool IsEasyCommentPostTarget { get; init; }

        public bool IsLeafRequired { get; init; }

        public bool IsOwnerThread { get; init; }

        public bool IsThreadkeyRequired { get; init; }

        public string? Threadkey { get; init; }

        public bool Is184Forced { get; init; }

        public bool HasNicoscript { get; init; }

        public int PostkeyStatus { get; init; }

        public string Server { get; init; } = string.Empty;

        public string Label { get; init; } = string.Empty;

        public string ForkLabel { get; init; } = string.Empty;

        public string VideoID { get; init; } = string.Empty;

    }

    public class Target : ITarget
    {
        public string Id { get; set; } = string.Empty;

        public string Fork { get; set; }= string.Empty;
    }

    public class Tag : ITag
    {
        public string Name { get; init; } = string.Empty;

        public bool IsNicodicExist { get; init; }
    }
}
