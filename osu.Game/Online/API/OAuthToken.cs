// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using Newtonsoft.Json;

namespace osu.Game.Online.API
{
    [Serializable]
    public class OAuthToken
    {
        /// <summary>
        /// OAuth 2.0 access token.
        /// </summary>
        [JsonProperty(@"access_token")]
        public string AccessToken;

        [JsonProperty(@"expires_in")]
        public long ExpiresIn
        {
            get => AccessTokenExpiry - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            set => AccessTokenExpiry = DateTimeOffset.Now.AddSeconds(value).ToUnixTimeSeconds();
        }

        public bool IsValid => !string.IsNullOrEmpty(AccessToken) && ExpiresIn > 30;

        public long AccessTokenExpiry;

        /// <summary>
        /// OAuth 2.0 refresh token.
        /// </summary>
        [JsonProperty(@"refresh_token")]
        public string RefreshToken;

        public override string ToString() => $@"{AccessToken}|{AccessTokenExpiry.ToString(NumberFormatInfo.InvariantInfo)}|{RefreshToken}";

        public static OAuthToken Parse(string value)
        {
            try
            {
                string[] parts = value.Split('|');
                return new OAuthToken
                {
                    AccessToken = parts[0],
                    AccessTokenExpiry = long.Parse(parts[1], NumberFormatInfo.InvariantInfo),
                    RefreshToken = parts[2]
                };
            }
            catch
            {
            }

            return null;
        }
    }
}
