﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niconicome.Models.Helper.Result;

namespace Niconicome.Models.Domain.Local.IO.V2
{
    public interface INiconicomeDirectoryIO
    {
        /// <summary>
        /// ディレクトリを作成
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IAttemptResult CreateDirectory(string path);

        /// <summary>
        /// ディレクトリを削除
        /// </summary>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        IAttemptResult Delete(string path, bool recursive = true);

        /// <summary>
        /// ディレクトリの存在を確認
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool Exists(string path);
    }
}
