// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    public class OsuJsonWebRequest<T> : JsonWebRequest<T>
    {
        public OsuJsonWebRequest(string uri)
            : base(uri)
        {
        }

        public OsuJsonWebRequest()
        {
        }

        protected override string UserAgent => "osu!";
    }
}
