// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    public class OAuth
    {
        private readonly IAPIProvider api;
        private readonly string clientId;
        private readonly string clientSecret;

        public readonly Bindable<OAuthToken> Token = new Bindable<OAuthToken>();

        public string TokenString
        {
            get => Token.Value?.ToString();
            set => Token.Value = string.IsNullOrEmpty(value) ? null : OAuthToken.Parse(value);
        }

        internal OAuth(IAPIProvider api, string clientId, string clientSecret)
        {
            Debug.Assert(clientId != null);
            Debug.Assert(clientSecret != null);

            this.api = api;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        internal void AuthenticateWithLogin(string username, string password)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Missing username.");
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Missing password.");

            var accessTokenRequest = new AccessTokenRequestPassword(username, password)
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            TaskCompletionSource<OAuthToken> response = new TaskCompletionSource<OAuthToken>();

            accessTokenRequest.Success += token => response.SetResult(token);
            accessTokenRequest.Failure += e =>
            {
                try
                {
                    // attempt to decode a displayable error string.
                    var error = JsonConvert.DeserializeObject<OAuthError>(accessTokenRequest.ResponseString ?? string.Empty);
                    if (error != null)
                        response.SetException(new APIException(error.UserDisplayableError, e));
                }
                catch
                {
                    response.SetException(e);
                }
            };

            try
            {
                accessTokenRequest.Perform(api);
                Token.Value = response.Task.GetResultSafely();
            }
            catch (AggregateException e)
            {
                Token.Value = null;
                throw e.GetBaseException();
            }
            catch
            {
                Token.Value = null;
                throw;
            }
        }

        internal bool AuthenticateWithRefresh(string refresh)
        {
            try
            {
                var refreshRequest = new AccessTokenRequestRefresh(refresh)
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                };

                refreshRequest.Perform(api);

                Token.Value = refreshRequest.Response;
                return true;
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

            protected override WebRequest CreateWebRequest()
            {
                var req = base.CreateWebRequest();

                req.AddParameter("refresh_token", RefreshToken);

                return req;
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

            protected override WebRequest CreateWebRequest()
            {
                var req = base.CreateWebRequest();

                req.AddParameter("username", Username);
                req.AddParameter("password", Password);

                return req;
            }
        }

        private abstract class AccessTokenRequest : APIRequest<OAuthToken>
        {
            [CanBeNull]
            public string ResponseString => WebRequest?.GetResponseString();

            protected string GrantType;

            internal string ClientId;
            internal string ClientSecret;

            protected override string Target => "oauth/token";

            // Override Uri directly because this is not an /api/v2/ request.
            protected override string Uri => $"{API.APIEndpointUrl}/{Target}";

            protected override WebRequest CreateWebRequest()
            {
                var req = base.CreateWebRequest();

                req.Method = HttpMethod.Post;
                req.AddParameter("grant_type", GrantType);
                req.AddParameter("client_id", ClientId);
                req.AddParameter("client_secret", ClientSecret);
                req.AddParameter("scope", "*");

                return req;
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
