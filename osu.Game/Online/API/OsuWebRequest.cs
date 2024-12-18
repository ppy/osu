// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    public class OsuWebRequest : WebRequest
    {
        public OsuWebRequest(string uri)
            : base(uri)
        {
        }

        public OsuWebRequest()
        {
        }

        protected override string UserAgent => "osu!";
    }
}
