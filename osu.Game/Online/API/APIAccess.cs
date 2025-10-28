// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ExceptionExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Notifications.WebSocket;

namespace osu.Game.Online.API
{
    public partial class APIAccess : CompositeComponent, IAPIProvider
    {
        private readonly OsuGameBase game;
        private readonly OsuConfigManager config;

        private readonly string versionHash;

        private readonly OAuth authentication;

        private readonly Queue<APIRequest> queue = new Queue<APIRequest>();

        public EndpointConfiguration Endpoints { get; }

        /// <summary>
        /// The API response version.
        /// See: https://osu.ppy.sh/docs/index.html#api-versions
        /// </summary>
        public int APIVersion { get; }

        public Exception LastLoginError { get; private set; }

        public string ProvidedUsername { get; private set; }

        public SessionVerificationMethod? SessionVerificationMethod { get; private set; }

        public string SecondFactorCode { get; private set; }

        private string password;

        public IBindable<APIUser> LocalUser => localUserState.User;

        public ILocalUserState LocalUserState => localUserState;
        private readonly LocalUserState localUserState;

        public INotificationsClient NotificationsClient { get; }

        public Language Language => game.CurrentLanguage.Value;

        protected bool HasLogin => authentication.Token.Value != null || (!string.IsNullOrEmpty(ProvidedUsername) && !string.IsNullOrEmpty(password));

        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private readonly Logger log;

        public APIAccess(OsuGameBase game, OsuConfigManager config, EndpointConfiguration endpoints, string versionHash)
        {
            this.game = game;
            this.config = config;
            this.versionHash = versionHash;

            if (game.IsDeployedBuild)
                APIVersion = game.AssemblyVersion.Major * 10000 + game.AssemblyVersion.Minor;
            else
            {
                var now = DateTimeOffset.Now;
                APIVersion = now.Year * 10000 + now.Month * 100 + now.Day;
            }

            Endpoints = endpoints;
            NotificationsClient = setUpNotificationsClient();

            authentication = new OAuth(endpoints.APIClientID, endpoints.APIClientSecret, Endpoints.APIUrl);

            log = Logger.GetLogger(LoggingTarget.Network);
            log.Add($@"API endpoint root: {Endpoints.APIUrl}");
            log.Add($@"API request version: {APIVersion}");

            ProvidedUsername = config.Get<string>(OsuSetting.Username);

            authentication.TokenString = config.Get<string>(OsuSetting.Token);
            authentication.Token.ValueChanged += onTokenChanged;

            AddInternal(localUserState = new LocalUserState(this, config));

            if (HasLogin)
            {
                // Early call to ensure the local user / "logged in" state is correct immediately.
                localUserState.SetPlaceholderLocalUser(ProvidedUsername);

                // This is required so that Queue() requests during startup sequence don't fail due to "not logged in".
                state.Value = APIState.Connecting;
            }

            var thread = new Thread(run)
            {
                Name = "APIAccess",
                IsBackground = true
            };

            thread.Start();
        }

        private WebSocketNotificationsClientConnector setUpNotificationsClient()
        {
            var connector = new WebSocketNotificationsClientConnector(this);

            connector.MessageReceived += msg =>
            {
                switch (msg.Event)
                {
                    case @"verified":
                        if (state.Value == APIState.RequiresSecondFactorAuth)
                            state.Value = APIState.Online;
                        break;

                    case @"logout":
                        if (state.Value == APIState.Online)
                            Logout();

                        break;
                }
            };

            return connector;
        }

        private void onTokenChanged(ValueChangedEvent<OAuthToken> e) => config.SetValue(OsuSetting.Token, config.Get<bool>(OsuSetting.SavePassword) ? authentication.TokenString : string.Empty);

        void IAPIProvider.Schedule(Action action) => base.Schedule(action);

        public string AccessToken => authentication.RequestAccessToken();

        public Guid SessionIdentifier { get; } = Guid.NewGuid();

        /// <summary>
        /// Number of consecutive requests which failed due to network issues.
        /// </summary>
        private int failureCount;

        /// <summary>
        /// The main API thread loop, which will continue to run until the game is shut down.
        /// </summary>
        private void run()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (state.Value == APIState.Failing)
                {
                    // To recover from a failing state, falling through and running the full reconnection process seems safest for now.
                    // This could probably be replaced with a ping-style request if we want to avoid the reconnection overheads.
                    log.Add($@"{nameof(APIAccess)} is in a failing state, waiting a bit before we try again...");
                    Thread.Sleep(5000);
                }

                // Ensure that we have valid credentials.
                // If not, setting the offline state will allow the game to prompt the user to provide new credentials.
                if (!HasLogin)
                {
                    state.Value = APIState.Offline;
                    Thread.Sleep(50);
                    continue;
                }

                Debug.Assert(HasLogin);

                // Ensure that we are in an online state. If not, attempt to connect.
                if (state.Value != APIState.Online)
                {
                    attemptConnect();

                    if (state.Value != APIState.Online)
                    {
                        Thread.Sleep(50);
                        continue;
                    }
                }

                // hard bail if we can't get a valid access token.
                if (authentication.RequestAccessToken() == null)
                {
                    Logout();
                    continue;
                }

                processQueuedRequests();
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Dequeue from the queue and run each request synchronously until the queue is empty.
        /// </summary>
        private void processQueuedRequests()
        {
            while (true)
            {
                APIRequest req;

                lock (queue)
                {
                    if (queue.Count == 0) return;

                    req = queue.Dequeue();
                }

                handleRequest(req);
            }
        }

        /// <summary>
        /// From a non-connected state, perform a full connection flow, obtaining OAuth tokens and populating the local user and friends.
        /// </summary>
        /// <remarks>
        /// This method takes control of <see cref="state"/> and transitions from <see cref="APIState.Connecting"/> to either
        /// - <see cref="APIState.RequiresSecondFactorAuth"/> (pending 2fa)
        /// - <see cref="APIState.Online"/>  (successful connection)
        /// - <see cref="APIState.Failing"/> (failed connection but retrying)
        /// - <see cref="APIState.Offline"/> (failed and can't retry, clear credentials and require user interaction)
        /// </remarks>
        /// <returns>Whether the connection attempt was successful.</returns>
        private void attemptConnect()
        {
            if (LocalUser.IsDefault)
                Scheduler.Add(localUserState.SetPlaceholderLocalUser, ProvidedUsername, false);

            // save the username at this point, if the user requested for it to be.
            config.SetValue(OsuSetting.Username, config.Get<bool>(OsuSetting.SaveUsername) ? ProvidedUsername : string.Empty);

            if (!authentication.HasValidAccessToken && HasLogin)
            {
                state.Value = APIState.Connecting;
                LastLoginError = null;

                try
                {
                    authentication.AuthenticateWithLogin(ProvidedUsername, password);
                }
                catch (WebRequestFlushedException)
                {
                    return;
                }
                catch (Exception e)
                {
                    //todo: this fails even on network-related issues. we should probably handle those differently.
                    LastLoginError = e;
                    log.Add($@"Login failed for username {ProvidedUsername} ({LastLoginError.Message})!");

                    Logout();
                    return;
                }
            }

            switch (state.Value)
            {
                case APIState.RequiresSecondFactorAuth:
                {
                    if (string.IsNullOrEmpty(SecondFactorCode))
                        return;

                    state.Value = APIState.Connecting;
                    LastLoginError = null;

                    var verificationRequest = new VerifySessionRequest(SecondFactorCode);

                    verificationRequest.Success += () => state.Value = APIState.Online;
                    verificationRequest.Failure += ex =>
                    {
                        state.Value = APIState.RequiresSecondFactorAuth;

                        if (verificationRequest.RequiredVerificationMethod != null)
                        {
                            SessionVerificationMethod = verificationRequest.RequiredVerificationMethod;
                            LastLoginError = new APIException($"Must use {SessionVerificationMethod.GetDescription().ToLowerInvariant()} to complete verification.", ex);
                        }
                        else
                        {
                            LastLoginError = ex;
                        }

                        SecondFactorCode = null;
                    };

                    if (!handleRequest(verificationRequest))
                    {
                        state.Value = APIState.Failing;
                        return;
                    }

                    if (state.Value != APIState.Online)
                        return;

                    break;
                }

                default:
                {
                    var userReq = new GetMeRequest();

                    userReq.Failure += ex =>
                    {
                        if (ex is APIException)
                        {
                            LastLoginError = ex;
                            log.Add($@"Login failed for username {ProvidedUsername} on user retrieval ({LastLoginError.Message})!");
                            Logout();
                        }
                        else if (ex is WebException webException && webException.Message == @"Unauthorized")
                        {
                            log.Add(@"Login no longer valid");
                            Logout();
                        }
                        else if (ex is not WebRequestFlushedException)
                        {
                            state.Value = APIState.Failing;
                        }
                    };

                    userReq.Success += me =>
                    {
                        Debug.Assert(ThreadSafety.IsUpdateThread);

                        localUserState.SetLocalUser(me);
                        SessionVerificationMethod = me.SessionVerificationMethod;
                        state.Value = SessionVerificationMethod == null ? APIState.Online : APIState.RequiresSecondFactorAuth;
                        failureCount = 0;
                    };

                    if (!handleRequest(userReq))
                    {
                        state.Value = APIState.Failing;
                        return;
                    }

                    break;
                }
            }

            // The Success callback event is fired on the main thread, so we should wait for that to run before proceeding.
            // Without this, we will end up circulating this Connecting loop multiple times and queueing up many web requests
            // before actually going online.
            while (State.Value == APIState.Connecting && !cancellationToken.IsCancellationRequested)
                Thread.Sleep(500);
        }

        public void Perform(APIRequest request)
        {
            try
            {
                request.AttachAPI(this);
                request.Perform();
            }
            catch (Exception e)
            {
                // todo: fix exception handling
                request.Fail(e);
            }
        }

        public Task PerformAsync(APIRequest request) =>
            Task.Factory.StartNew(() => Perform(request), TaskCreationOptions.LongRunning);

        public void Login(string username, string password)
        {
            Debug.Assert(State.Value == APIState.Offline);

            ProvidedUsername = username;
            this.password = password;
        }

        public void AuthenticateSecondFactor(string code)
        {
            Debug.Assert(State.Value == APIState.RequiresSecondFactorAuth);

            SecondFactorCode = code;
        }

        public IHubClientConnector GetHubConnector(string clientName, string endpoint) =>
            new HubClientConnector(clientName, endpoint, this, versionHash);

        public IChatClient GetChatClient() => new WebSocketChatClient(this);

        public RegistrationRequest.RegistrationRequestErrors CreateAccount(string email, string username, string password)
        {
            Debug.Assert(State.Value == APIState.Offline);

            var req = new RegistrationRequest
            {
                Url = $@"{Endpoints.APIUrl}/users",
                Method = HttpMethod.Post,
                Username = username,
                Email = email,
                Password = password
            };

            try
            {
                req.Perform();
            }
            catch (Exception e)
            {
                try
                {
                    return JObject.Parse(req.GetResponseString().AsNonNull()).SelectToken(@"form_error", true).AsNonNull().ToObject<RegistrationRequest.RegistrationRequestErrors>();
                }
                catch
                {
                    try
                    {
                        // attempt to parse a non-form error message
                        var response = JObject.Parse(req.GetResponseString().AsNonNull());

                        string redirect = (string)response.SelectToken(@"url", true);
                        string message = (string)response.SelectToken(@"error", false);

                        if (!string.IsNullOrEmpty(redirect))
                        {
                            return new RegistrationRequest.RegistrationRequestErrors
                            {
                                Redirect = redirect,
                                Message = message,
                            };
                        }

                        // if we couldn't deserialize the error message let's throw the original exception outwards.
                        e.Rethrow();
                    }
                    catch
                    {
                        // if we couldn't deserialize the error message let's throw the original exception outwards.
                        e.Rethrow();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Handle a single API request.
        /// Ensures all exceptions are caught and dealt with correctly.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <returns>true if the request succeeded.</returns>
        private bool handleRequest(APIRequest req)
        {
            try
            {
                req.AttachAPI(this);
                req.Perform();

                if (req.CompletionState != APIRequestCompletionState.Completed)
                    return false;

                // Reset failure count if this request succeeded.
                failureCount = 0;
                return true;
            }
            catch (HttpRequestException re)
            {
                log.Add($"{nameof(HttpRequestException)} while performing request {req}: {re.Message}");
                handleFailure();
                return false;
            }
            catch (SocketException se)
            {
                log.Add($"{nameof(SocketException)} while performing request {req}: {se.Message}");
                handleFailure();
                return false;
            }
            catch (WebException we)
            {
                log.Add($"{nameof(WebException)} while performing request {req}: {we.Message}");
                handleWebException(we);
                return false;
            }
            catch (WebRequestFlushedException wrf)
            {
                log.Add(wrf.Message);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while handling an API request.");
                return false;
            }
        }

        private readonly Bindable<APIState> state = new Bindable<APIState>();

        /// <summary>
        /// The current connectivity state of the API.
        /// </summary>
        public IBindable<APIState> State => state;

        private void handleWebException(WebException we)
        {
            HttpStatusCode statusCode = (we.Response as HttpWebResponse)?.StatusCode
                                        ?? (we.Status == WebExceptionStatus.UnknownError ? HttpStatusCode.NotAcceptable : HttpStatusCode.RequestTimeout);

            // special cases for un-typed but useful message responses.
            switch (we.Message)
            {
                case "Unauthorized":
                case "Forbidden":
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
            }

            switch (statusCode)
            {
                case HttpStatusCode.Unauthorized:
                    Logout();
                    break;

                case HttpStatusCode.RequestTimeout:
                    handleFailure();
                    break;
            }
        }

        private void handleFailure()
        {
            failureCount++;
            log.Add($@"API failure count is now {failureCount}");

            if (failureCount >= 3)
            {
                state.Value = APIState.Failing;
                flushQueue();
            }
        }

        public bool IsLoggedIn => State.Value > APIState.Offline;

        public void Queue(APIRequest request)
        {
            lock (queue)
            {
                request.AttachAPI(this);

                if (state.Value == APIState.Offline)
                {
                    request.Fail(new WebException(@"User not logged in"));
                    return;
                }

                queue.Enqueue(request);
            }
        }

        private void flushQueue(bool failOldRequests = true)
        {
            lock (queue)
            {
                var oldQueueRequests = queue.ToArray();

                queue.Clear();

                if (failOldRequests)
                {
                    foreach (var req in oldQueueRequests)
                        req.Fail(new WebRequestFlushedException(state.Value));
                }
            }
        }

        public void Logout()
        {
            password = null;
            SecondFactorCode = null;
            authentication.Clear();

            localUserState.ClearLocalUser();

            state.Value = APIState.Offline;
            flushQueue();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            flushQueue();
            cancellationToken.Cancel();
        }

        private class WebRequestFlushedException : Exception
        {
            public WebRequestFlushedException(APIState state)
                : base($@"Request failed from flush operation (state {state})")
            {
            }
        }
    }

    internal class GuestUser : APIUser
    {
        public GuestUser()
        {
            Username = @"Guest";
            Id = SYSTEM_USER_ID;
        }
    }

    public enum APIState
    {
        /// <summary>
        /// We cannot login (not enough credentials).
        /// </summary>
        Offline,

        /// <summary>
        /// We are having connectivity issues.
        /// </summary>
        Failing,

        /// <summary>
        /// Waiting on second factor authentication.
        /// </summary>
        RequiresSecondFactorAuth,

        /// <summary>
        /// We are in the process of (re-)connecting.
        /// </summary>
        Connecting,

        /// <summary>
        /// We are online.
        /// </summary>
        Online
    }
}
