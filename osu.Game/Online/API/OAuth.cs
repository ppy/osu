// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Net.Http;
using osu.Framework.Bindables;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    public class OAuth
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string endpoint;

        public readonly Bindable<OAuthToken> Token = new Bindable<OAuthToken>();

        public string TokenString
        {
            get => Token.Value?.ToString();
            set => Token.Value = string.IsNullOrEmpty(value) ? null : OAuthToken.Parse(value);
        }

        internal OAuth(string clientId, string clientSecret, string endpoint)
        {
            Debug.Assert(clientId != null);
            Debug.Assert(clientSecret != null);
            Debug.Assert(endpoint != null);

            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.endpoint = endpoint;
        }

        internal bool AuthenticateWithLogin(string username, string password)
        {
            if (string.IsNullOrEmpty(username)) return false;
            if (string.IsNullOrEmpty(password)) return false;

            using (var req = new AccessTokenRequestPassword(username, password)
            {
                Url = $@"{endpoint}/oauth/token",
                Method = HttpMethod.Post,
                ClientId = clientId,
                ClientSecret = clientSecret
            })
            {
                try
                {
                    req.Perform();
                }
                catch
                {
                    return false;
                }

                Token.Value = req.ResponseObject;
                return true;
            }
        }

        internal bool AuthenticateWithRefresh(string refresh)
        {
            try
            {
                using (var req = new AccessTokenRequestRefresh(refresh)
                {
                    Url = $@"{endpoint}/oauth/token",
                    Method = HttpMethod.Post,
                    ClientId = clientId,
                    ClientSecret = clientSecret
                })
                {
                    req.Perform();

                    Token.Value = req.ResponseObject;
                    return true;
                }
            }
            catch
            {
                //todo: potentially only kill the refresh token on certain exception types.
                Token.Value = null;
                return false;
            }
        }

        private static readonly object access_token_retrieval_lock = new object();

        /// <summary>
        /// Should be run before any API request to make sure we have a valid key.
        /// </summary>
        private bool ensureAccessToken()
        {
            // if we already have a valid access token, let's use it.
            if (accessTokenValid) return true;

            // we want to ensure only a single authentication update is happening at once.
            lock (access_token_retrieval_lock)
            {
                // re-check if valid, in case another request completed and revalidated our access.
                if (accessTokenValid) return true;

                // if not, let's try using our refresh token to request a new access token.
                if (!string.IsNullOrEmpty(Token.Value?.RefreshToken))
                    // ReSharper disable once PossibleNullReferenceException
                    AuthenticateWithRefresh(Token.Value.RefreshToken);

                return accessTokenValid;
            }
        }

        private bool accessTokenValid => Token.Value?.IsValid ?? false;

        internal bool HasValidAccessToken => RequestAccessToken() != null;

        internal string RequestAccessToken()
        {
            if (!ensureAccessToken()) return null;

            return Token.Value.AccessToken;
        }

        internal void Clear()
        {
            Token.Value = null;
        }

        private class AccessTokenRequestRefresh : AccessTokenRequest
        {
            internal readonly string RefreshToken;

            internal AccessTokenRequestRefresh(string refreshToken)
            {
                RefreshToken = refreshToken;
                GrantType = @"refresh_token";
            }

            protected override void PrePerform()
            {
                AddParameter("refresh_token", RefreshToken);

                base.PrePerform();
            }
        }

        private class AccessTokenRequestPassword : AccessTokenRequest
        {
            internal readonly string Username;
            internal readonly string Password;

            internal AccessTokenRequestPassword(string username, string password)
            {
                Username = username;
                Password = password;
                GrantType = @"password";
            }

            protected override void PrePerform()
            {
                AddParameter("username", Username);
                AddParameter("password", Password);

                base.PrePerform();
            }
        }

        private class AccessTokenRequest : JsonWebRequest<OAuthToken>
        {
            protected string GrantType;

            internal string ClientId;
            internal string ClientSecret;

            protected override void PrePerform()
            {
                AddParameter("grant_type", GrantType);
                AddParameter("client_id", ClientId);
                AddParameter("client_secret", ClientSecret);
                AddParameter("scope", "*");

                base.PrePerform();
            }
        }
    }
}
