﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niconicome.Models.Const
{
    class NetConstant
    {
        /// <summary>
        /// ニコニコ動画視聴ページのURL
        /// </summary>
        public const string NiconicoWatchUrl = "https://nicovideo.jp/watch/";

        /// <summary>
        /// ニコニコの短縮リンク
        /// </summary>
        public const string NiconicoShortUrl = "https://nico.ms/";

        /// <summary>
        /// 削除動画のサムネ
        /// </summary>
        public const string NiconicoDeletedVideothumb = @"https://nicovideo.cdn.nimg.jp/web/img/common/video_deleted.jpg";

        /// <summary>
        /// ニコニコのURL
        /// </summary>
        public const string NiconicoBaseURL = @"https://nicovideo.jp";

        /// <summary>
        /// ニコニコのURL（非SSL）
        /// </summary>
        public const string NiconicoBaseURLNonSSL = @"http://nicovideo.jp";

        /// <summary>
        /// ニコニコのログインページURL
        /// </summary>
        public const string NiconicoLoginPageURL = @"https://account.nicovideo.jp/login";

        /// <summary>
        /// ニコニコのログインURL
        /// </summary>
        public const string NiconicoLoginURL = @"https://secure.nicovideo.jp/secure/login?site=niconico";

        /// <summary>
        /// ニコニコのログアウトURL
        /// </summary>
        public const string NiconicoLogoutURL = @"https://account.nicovideo.jp/logout";

        /// <summary>
        /// ニコニコのドメイン
        /// </summary>
        public const string NiconicoDomain = @"nicovideo.jp";

        /// <summary>
        /// エコノミー判定されない視聴回数
        /// </summary>
        public static int EconomyAvoidableViewCount = 500;

        /// <summary>
        /// 同時取得数の上限（デフォルト）
        /// </summary>
        public static int DefaultMaxParallelFetchCount = 3;

        /// <summary>
        /// 動画取得の待機時間（デフォルト）
        /// </summary>
        public static int DefaultFetchWaitInterval = 5;

        /// <summary>
        /// 最大並列DL数（デフォルト）
        /// </summary>
        public static int DefaultMaxParallelDownloadCount = 2;

        /// <summary>
        /// 最大セグメント並列DL数（デフォルト）
        /// </summary>
        public static int DefaultMaxParallelSegmentDownloadCount = 3;

        /// <summary>
        /// コメントコレクションの1ブロックあたりのコメント数（デフォルト）
        /// </summary>
        public static int DefaultCommentCountPerBlock = 100;

        /// <summary>
        /// ローカルサーバーのデフォルトポート
        /// </summary>
        public static int DefaultServerPort = 2580;
    }
}
