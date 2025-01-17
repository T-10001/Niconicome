﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niconicome.Models.Local.Addon.API.Net.Http.Fetch;
using API = Niconicome.Models.Local.Addon.API;
using Fetch = Niconicome.Models.Local.Addon.API.Net.Http.Fetch;

namespace Niconicome.Models.Domain.Local.Addons.Core.V2.Engine
{
    public record APIObjectConatainer(API::IAPIEntryPoint APIEntryPoint, Func<string, dynamic?, Task<Fetch::Response>> FetchFunc, IFetch Fetch);
}
