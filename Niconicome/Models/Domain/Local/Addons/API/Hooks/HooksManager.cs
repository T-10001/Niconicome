﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ClearScript;
using Niconicome.Models.Domain.Niconico.Video.Infomations;
using Niconicome.Models.Domain.Utils;
using Niconicome.Models.Helper.Result;

namespace Niconicome.Models.Domain.Local.Addons.API.Hooks
{
    public interface IHooksManager
    {
        /// <summary>
        /// 視聴ページを解析する
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        IAttemptResult<dynamic> ParseWatchPage(string page);

        /// <summary>
        /// セッションを確立する
        /// </summary>
        /// <param name="dmcInfo"></param>
        /// <returns></returns>
        Task<IAttemptResult<dynamic>> EnsureSessionAsync(dynamic dmcInfo);

        /// <summary>
        /// 関数を登録する
        /// </summary>
        /// <param name="function"></param>
        /// <param name="type"></param>
        void Register(ScriptObject function, HookType type);

        /// <summary>
        /// 指定したタイプの関数が登録されているかどうかチェックする
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsRegistered(HookType type);
    }

    public class HooksManager : IHooksManager
    {
        public HooksManager(ILogger logger)
        {
            this.logger = logger;
        }

        #region field

        private readonly ILogger logger;

        private Dictionary<HookType, ScriptObject> hooks = new();

        #endregion

        #region Method

        public IAttemptResult<dynamic> ParseWatchPage(string page)
        {
            this.hooks.TryGetValue(HookType.WatchPageParser, out ScriptObject? function);

            if (function is null)
            {
                return new AttemptResult<dynamic>() { Message = "視聴ページ解析関数が登録されていません。" };
            }

            dynamic returnVal;
            try
            {
                returnVal = function.Invoke(false, page);
            }
            catch (Exception e)
            {
                this.logger.Error("視聴ページ解析に失敗しました。", e);
                return new AttemptResult<dynamic>() { Message = "視聴ページ解析に失敗しました。", Exception = e };
            }

            return new AttemptResult<dynamic>() { IsSucceeded = true, Data = returnVal };
        
        }

        public async Task<IAttemptResult<dynamic>> EnsureSessionAsync(dynamic dmcInfo)
        {
            this.hooks.TryGetValue(HookType.SessionEnsuring, out ScriptObject? function);

            if (function is null)
            {
                return new AttemptResult<dynamic>() { Message = "セッション確立関数が登録されていません。" };
            }

            dynamic returnVal;

            try
            {
                returnVal = await function.Invoke(false, dmcInfo);
            }
            catch (Exception e)
            {
                this.logger.Error("セッションの確立に失敗しました。", e);
                return new AttemptResult<dynamic>() { Message = "セッションの確立に失敗しました。", Exception = e };
            }

            return new AttemptResult<dynamic>() { IsSucceeded = true, Data = returnVal };
        }


        public void Register(ScriptObject function, HookType type)
        {
            if (this.hooks.ContainsKey(type))
            {
                this.hooks[type] = function;
            }
            else
            {
                this.hooks.Add(type, function);
            }
        }

        public bool IsRegistered(HookType type)
        {
            return this.hooks.ContainsKey(type) && this.hooks[type] is not null;
        }


        #endregion
    }

    public enum HookType
    {
        WatchPageParser,
        SessionEnsuring,
    }
}
