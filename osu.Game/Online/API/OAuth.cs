// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using Newtonsoft.Json;
using osu.Framework.Bindables;

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

        internal void AuthenticateWithLogin(string username, string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(username);
            ArgumentException.ThrowIfNullOrEmpty(password);

            var accessTokenRequest = new AccessTokenRequestPassword(username, password)
            {
                Url = $@"{endpoint}/oauth/token",
                Method = HttpMethod.Post,
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            using (accessTokenRequest)
            {
                try
                {
                    accessTokenRequest.Perform();
                }
                catch (Exception ex)
                {
                    Token.Value = null;

                    var throwableException = ex;

                    try
                    {
                        // attempt to decode a displayable error string.
                        var error = JsonConvert.DeserializeObject<OAuthError>(accessTokenRequest.GetResponseString() ?? string.Empty);
                        if (error != null)
                            throwableException = new APIException(error.UserDisplayableError, ex);
                    }
                    catch
                    {
                    }

                    throw throwableException;
                }

                Token.Value = accessTokenRequest.ResponseObject;
            }
        }

        internal bool AuthenticateWithRefresh(string refresh)
        {
            try
            {
                var refreshRequest = new AccessTokenRequestRefresh(refresh)
                {
                    Url = $@"{endpoint}/oauth/token",
                    Method = HttpMethod.Post,
                    ClientId = clientId,
                    ClientSecret = clientSecret
                };

                using (refreshRequest)
                {
                    refreshRequest.Perform();

                    Token.Value = refreshRequest.ResponseObject;
                    return true;
                }
            }
            catch (SocketException)
            {
                // Network failure.
                return false;
            }
            catch (HttpRequestException)
            {
                // Network failure.
                return false;
            }
            catch
            {
                // Force a full re-authentication.
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

            // if not, let's try using our refresh token to request a new access token.
            if (!string.IsNullOrEmpty(Token.Value?.RefreshToken))
                // ReSharper disable once PossibleNullReferenceException
                AuthenticateWithRefresh(Token.Value.RefreshToken);

            return accessTokenValid;
        }

        private bool accessTokenValid => Token.Value?.IsValid ?? false;

        internal bool HasValidAccessToken => RequestAccessToken() != null;

        internal string RequestAccessToken()
        {
            lock (access_token_retrieval_lock)
            {
                if (!ensureAccessToken()) return null;

                return Token.Value.AccessToken;
            }
        }

        internal void Clear()
        {
            lock (access_token_retrieval_lock)
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

        private class AccessTokenRequest : OsuJsonWebRequest<OAuthToken>
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

        private class OAuthError
        {
            public string UserDisplayableError => !string.IsNullOrEmpty(Hint) ? Hint : ErrorIdentifier;

            [JsonProperty("error")]
            public string ErrorIdentifier { get; set; }

            [JsonProperty("hint")]
            public string Hint { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }
    }
}
