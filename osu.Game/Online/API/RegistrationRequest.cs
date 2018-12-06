// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
