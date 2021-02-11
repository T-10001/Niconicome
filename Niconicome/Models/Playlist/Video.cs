﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niconicome.Models.Domain.Local.Store;

namespace Niconicome.Models.Playlist
{
    public interface IVideoHandler
    {
        int AddVideo(ITreeVideoInfo video, int playlidtId);
        void RemoveVideo(int videoID, int playlistID);
        void Update(ITreeVideoInfo video);
        IEnumerable<ITreeVideoInfo> GetAllVideos();
        ITreeVideoInfo GetVideo(int id);

    }

    public class VideoHandler : IVideoHandler
    {

        public VideoHandler(IVideoStoreHandler storeHandler,IPlaylistStoreHandler playlistStoreHandler,IPlaylistVideoHandler playlistVideoHandler)
        {
            this.videoStoreHandler = storeHandler;
            this.playlistStoreHandler = playlistStoreHandler;
            this.playlistVideoHandler = playlistVideoHandler;
        }

        /// <summary>
        /// DBにデータを保存
        /// </summary>
        private readonly IVideoStoreHandler videoStoreHandler;

        /// <summary>
        /// DB上のプレイリストにアクセスする
        /// </summary>
        private readonly IPlaylistStoreHandler playlistStoreHandler;

        /// <summary>
        /// プレイリストツリーにアクセスする
        /// </summary>
        private readonly IPlaylistVideoHandler playlistVideoHandler;

        /// <summary>
        /// 動画を追加する
        /// </summary>
        /// <param name="video"></param>
        /// <param name="playlidtId"></param>
        /// <returns></returns>
        public int AddVideo(ITreeVideoInfo video, int playlidtId)
        {
            int id = this.playlistStoreHandler.AddVideo(video, playlidtId);
            this.playlistVideoHandler.AddVideo(video, playlidtId);
            return id;
        }

        /// <summary>
        /// 動画を削除する
        /// </summary>
        /// <param name="videoID"></param>
        /// <param name="playlistID"></param>
        public void RemoveVideo(int videoID, int playlistID)
        {
            this.playlistStoreHandler.RemoveVideo(videoID, playlistID);
            this.playlistVideoHandler.RemoveVideo(videoID, playlistID);
        }

        /// <summary>
        /// 動画情報を更新する
        /// </summary>
        /// <param name="video"></param>
        public void Update(ITreeVideoInfo video)
        {
            this.videoStoreHandler.Update(video);
        }


        /// <summary>
        /// 全ての動画を取得する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITreeVideoInfo> GetAllVideos()
        {
            return this.videoStoreHandler.GetAllVideos().Select(v => BindableTreeVideoInfo.ConvertToTreeVideoInfo(v));
        }

        /// <summary>
        /// 指定した動画を取得する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ITreeVideoInfo GetVideo(int id)
        {
            return BindableTreeVideoInfo.ConvertToTreeVideoInfo(this.videoStoreHandler.GetVideo(id));
        }


    }
}
