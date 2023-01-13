// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.API
{
    public class RegistrationRequest : OsuWebRequest
    {
        internal string Username = string.Empty;
        internal string Email = string.Empty;
        internal string Password = string.Empty;

        protected override void PrePerform()
        {
            AddParameter("user[username]", Username);
            AddParameter("user[user_email]", Email);
            AddParameter("user[password]", Password);

            base.PrePerform();
        }

        public class RegistrationRequestErrors
        {
            /// <summary>
            /// An optional error message.
            /// </summary>
            public string? Message;

            /// <summary>
            /// An optional URL which the user should be directed towards to complete registration.
            /// </summary>
            public string? Redirect;

            public UserErrors? User;

            public class UserErrors
            {
                [JsonProperty("username")]
                public string[] Username = Array.Empty<string>();

                [JsonProperty("user_email")]
                public string[] Email = Array.Empty<string>();

                [JsonProperty("password")]
                public string[] Password = Array.Empty<string>();
            }
        }
    }
}
