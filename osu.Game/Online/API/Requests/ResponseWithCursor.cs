// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests
{
    public abstract class ResponseWithCursor
    {
        /// <summary>
        /// A collection of parameters which should be passed to the search endpoint to fetch the next page.
        /// </summary>
        [JsonProperty("cursor")]
        public dynamic CursorJson;
    }

    public abstract class ResponseWithCursor<T> : ResponseWithCursor where T : class
    {
        /// <summary>
        /// Cursor deserialized into T class type (cannot implicitly convert type to object using raw Cursor)
        /// </summary>
        [JsonProperty("cursor")]
        public T Cursor;
    }
}
