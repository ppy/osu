﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    internal class OAuth
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string endpoint;

        public OAuthToken Token;

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
            var req = new AccessTokenRequestPassword(username, password)
            {
                Url = $@"{endpoint}/oauth/token",
                Method = HttpMethod.POST,
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            try
            {
                req.BlockingPerform();
            }
            catch
            {
                return false;
            }

            Token = req.ResponseObject;
            return true;
        }

        internal bool AuthenticateWithRefresh(string refresh)
        {
            try
            {
                var req = new AccessTokenRequestRefresh(refresh)
                {
                    Url = $@"{endpoint}/oauth/token",
                    Method = HttpMethod.POST,
                    ClientId = clientId,
                    ClientSecret = clientSecret
                };
                req.BlockingPerform();

                Token = req.ResponseObject;
                return true;
            }
            catch
            {
                //todo: potentially only kill the refresh token on certain exception types.
                Token = null;
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
                if (!string.IsNullOrEmpty(Token?.RefreshToken))
                    AuthenticateWithRefresh(Token.RefreshToken);

                return accessTokenValid;
            }
        }

        private bool accessTokenValid => Token?.IsValid ?? false;

        internal bool HasValidAccessToken => RequestAccessToken() != null;

        internal string RequestAccessToken()
        {
            if (!ensureAccessToken()) return null;

            return Token.AccessToken;
        }

        internal void Clear()
        {
            Token = null;
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
                Parameters[@"refresh_token"] = RefreshToken;
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
                Parameters[@"username"] = Username;
                Parameters[@"password"] = Password;
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
                Parameters[@"grant_type"] = GrantType;
                Parameters[@"client_id"] = ClientId;
                Parameters[@"client_secret"] = ClientSecret;
                base.PrePerform();
            }
        }
    }
}
