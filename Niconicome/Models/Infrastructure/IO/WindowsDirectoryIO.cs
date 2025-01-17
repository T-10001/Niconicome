﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niconicome.Models.Domain.Local.IO.V2;
using Niconicome.Models.Domain.Utils.Error;
using Niconicome.Models.Helper.Result;

namespace Niconicome.Models.Infrastructure.IO
{
    public class WindowsDirectoryIO : INiconicomeDirectoryIO
    {
        public WindowsDirectoryIO(IErrorHandler errorHandler)
        {
            this._errorHandler = errorHandler;
        }

        #region field

        private readonly IErrorHandler _errorHandler;

        #endregion

        #region Method

        public IAttemptResult CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                this._errorHandler.HandleError(WindowsDirectoryIOError.FailedToCreateDirectory, ex);
                return AttemptResult.Fail(this._errorHandler.GetMessageForResult(WindowsDirectoryIOError.FailedToCreateDirectory, ex));
            }

            return AttemptResult.Succeeded();
        }

        public IAttemptResult Delete(string path, bool recursive = true)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch (Exception ex)
            {
                this._errorHandler.HandleError(WindowsDirectoryIOError.FailedToDeleteDirectory, ex, path);
                return AttemptResult.Fail(this._errorHandler.GetMessageForResult(WindowsDirectoryIOError.FailedToDeleteDirectory, ex, path));
            }

            return AttemptResult.Succeeded();
        }


        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }


        #endregion
    }
}
