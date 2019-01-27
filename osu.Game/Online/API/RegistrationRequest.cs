// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    public class RegistrationRequest : WebRequest
    {
        internal string Username;
        internal string Email;
        internal string Password;

        protected override void PrePerform()
        {
            AddParameter("user[username]", Username);
            AddParameter("user[user_email]", Email);
            AddParameter("user[password]", Password);

            base.PrePerform();
        }

        public class RegistrationRequestErrors
        {
            public UserErrors User;

            public class UserErrors
            {
                [JsonProperty("username")]
                public string[] Username;

                [JsonProperty("user_email")]
                public string[] Email;

                [JsonProperty("password")]
                public string[] Password;
            }
        }
    }
}
