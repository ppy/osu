// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using System.Net.Http;

namespace osu.Game.Online.API.Requests
{
    public class PostBeatmapFavouriteRequest : APIRequest
    {
        public readonly BeatmapFavouriteAction Action;

        private readonly int id;

        public PostBeatmapFavouriteRequest(int id, BeatmapFavouriteAction action)
        {
            this.id = id;
            Action = action;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            req.AddParameter(@"action", Action.ToString().ToLowerInvariant());
            return req;
        }

        protected override string Target => $@"beatmapsets/{id}/favourites";
    }

    public enum BeatmapFavouriteAction
    {
        Favourite,
        UnFavourite
    }
}
