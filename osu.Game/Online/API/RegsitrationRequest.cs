// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    internal class RegsitrationRequest : JsonWebRequest<OAuthToken>
    {
        internal string Username;
        internal string Email;
        internal string Password;

        protected override void PrePerform()
        {
            AddParameter("user", Username);
            AddParameter("user_email", Email);
            AddParameter("password", Password);

            base.PrePerform();
        }
    }
}
