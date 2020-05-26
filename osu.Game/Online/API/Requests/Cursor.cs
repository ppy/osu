// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace osu.Game.Online.API.Requests
{
    /// <summary>
    /// A collection of parameters which should be passed to the search endpoint to fetch the next page.
    /// </summary>
    public class Cursor
    {
        [UsedImplicitly]
        [JsonExtensionData]
        public IDictionary<string, JToken> Properties;
    }
}
