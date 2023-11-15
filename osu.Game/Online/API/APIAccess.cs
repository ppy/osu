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
using osu.Framework.Extensions.ExceptionExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Notifications;
using osu.Game.Online.Notifications.WebSocket;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    public partial class APIAccess : Component, IAPIProvider
    {
        private readonly OsuGameBase game;
        private readonly OsuConfigManager config;

        private readonly string versionHash;

        private readonly OAuth authentication;

        private readonly Queue<APIRequest> queue = new Queue<APIRequest>();

        public string APIEndpointUrl { get; }

        public string WebsiteRootUrl { get; }

        public int APIVersion => 20220705; // We may want to pull this from the game version eventually.

        public Exception LastLoginError { get; private set; }

        public string ProvidedUsername { get; private set; }

        private string password;

        public IBindable<APIUser> LocalUser => localUser;
        public IBindableList<APIUser> Friends => friends;
        public IBindable<UserActivity> Activity => activity;

        public Language Language => game.CurrentLanguage.Value;

        private Bindable<APIUser> localUser { get; } = new Bindable<APIUser>(createGuestUser());

        private BindableList<APIUser> friends { get; } = new BindableList<APIUser>();

        private Bindable<UserActivity> activity { get; } = new Bindable<UserActivity>();

        protected bool HasLogin => authentication.Token.Value != null || (!string.IsNullOrEmpty(ProvidedUsername) && !string.IsNullOrEmpty(password));

        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private readonly Logger log;

        public APIAccess(OsuGameBase game, OsuConfigManager config, EndpointConfiguration endpointConfiguration, string versionHash)
        {
            this.game = game;
            this.config = config;
            this.versionHash = versionHash;

            APIEndpointUrl = endpointConfiguration.APIEndpointUrl;
            WebsiteRootUrl = endpointConfiguration.WebsiteRootUrl;

            authentication = new OAuth(endpointConfiguration.APIClientID, endpointConfiguration.APIClientSecret, APIEndpointUrl);
            log = Logger.GetLogger(LoggingTarget.Network);

            ProvidedUsername = config.Get<string>(OsuSetting.Username);

            authentication.TokenString = config.Get<string>(OsuSetting.Token);
            authentication.Token.ValueChanged += onTokenChanged;

            localUser.BindValueChanged(u =>
            {
                u.OldValue?.Activity.UnbindFrom(activity);
                u.NewValue.Activity.BindTo(activity);
            }, true);

            var thread = new Thread(run)
            {
                Name = "APIAccess",
                IsBackground = true
            };

            thread.Start();
        }

        private void onTokenChanged(ValueChangedEvent<OAuthToken> e) => config.SetValue(OsuSetting.Token, config.Get<bool>(OsuSetting.SavePassword) ? authentication.TokenString : string.Empty);

        internal new void Schedule(Action action) => base.Schedule(action);

        public string AccessToken => authentication.RequestAccessToken();

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

                // Ensure that we are in an online state. If not, attempt a connect.
                if (state.Value != APIState.Online)
                {
                    attemptConnect();

                    if (state.Value != APIState.Online)
                        continue;
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
        /// - <see cref="APIState.Online"/>  (successful connection)
        /// - <see cref="APIState.Failing"/> (failed connection but retrying)
        /// - <see cref="APIState.Offline"/> (failed and can't retry, clear credentials and require user interaction)
        /// </remarks>
        /// <returns>Whether the connection attempt was successful.</returns>
        private void attemptConnect()
        {
            state.Value = APIState.Connecting;

            if (localUser.IsDefault)
            {
                // Show a placeholder user if saved credentials are available.
                // This is useful for storing local scores and showing a placeholder username after starting the game,
                // until a valid connection has been established.
                setLocalUser(new APIUser
                {
                    Username = ProvidedUsername,
                });
            }

            // save the username at this point, if the user requested for it to be.
            config.SetValue(OsuSetting.Username, config.Get<bool>(OsuSetting.SaveUsername) ? ProvidedUsername : string.Empty);

            if (!authentication.HasValidAccessToken)
            {
                LastLoginError = null;

                try
                {
                    authentication.AuthenticateWithLogin(ProvidedUsername, password);
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

            var userReq = new GetUserRequest();
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
                else
                {
                    state.Value = APIState.Failing;
                }
            };
            userReq.Success += user =>
            {
                // todo: save/pull from settings
                user.Status.Value = new UserStatusOnline();

                setLocalUser(user);

                // we're connected!
                state.Value = APIState.Online;
                failureCount = 0;
            };

            if (!handleRequest(userReq))
            {
                state.Value = APIState.Failing;
                return;
            }

            var friendsReq = new GetFriendsRequest();
            friendsReq.Failure += _ => state.Value = APIState.Failing;
            friendsReq.Success += res =>
            {
                friends.Clear();
                friends.AddRange(res);
            };

            if (!handleRequest(friendsReq))
            {
                state.Value = APIState.Failing;
                return;
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
                request.Perform(this);
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

        public IHubClientConnector GetHubConnector(string clientName, string endpoint, bool preferMessagePack) =>
            new HubClientConnector(clientName, endpoint, this, versionHash, preferMessagePack);

        public NotificationsClientConnector GetNotificationsConnector() =>
            new WebSocketNotificationsClientConnector(this);

        public RegistrationRequest.RegistrationRequestErrors CreateAccount(string email, string username, string password)
        {
            Debug.Assert(State.Value == APIState.Offline);

            var req = new RegistrationRequest
            {
                Url = $@"{APIEndpointUrl}/users",
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
                req.Perform(this);

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
                        req.Fail(new WebException($@"Request failed from flush operation (state {state.Value})"));
                }
            }
        }

        public void Logout()
        {
            password = null;
            authentication.Clear();

            // Scheduled prior to state change such that the state changed event is invoked with the correct user and their friends present
            Schedule(() =>
            {
                setLocalUser(createGuestUser());
                friends.Clear();
            });

            state.Value = APIState.Offline;
            flushQueue();
        }

        private static APIUser createGuestUser() => new GuestUser();

        private void setLocalUser(APIUser user) => Scheduler.Add(() => localUser.Value = user, false);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            flushQueue();
            cancellationToken.Cancel();
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
        RequiresAuthentication,

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
