// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ExceptionExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Online.API.Requests;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    public class APIAccess : Component, IAPIProvider
    {
        private readonly OsuConfigManager config;

        private readonly OAuth authentication;

        private readonly Queue<APIRequest> queue = new Queue<APIRequest>();

        public string APIEndpointUrl { get; }

        public string WebsiteRootUrl { get; }

        /// <summary>
        /// The username/email provided by the user when initiating a login.
        /// </summary>
        public string ProvidedUsername { get; private set; }

        private string password;

        public IBindable<User> LocalUser => localUser;
        public IBindableList<User> Friends => friends;
        public IBindable<UserActivity> Activity => activity;

        private Bindable<User> localUser { get; } = new Bindable<User>(createGuestUser());

        private BindableList<User> friends { get; } = new BindableList<User>();

        private Bindable<UserActivity> activity { get; } = new Bindable<UserActivity>();

        protected bool HasLogin => authentication.Token.Value != null || (!string.IsNullOrEmpty(ProvidedUsername) && !string.IsNullOrEmpty(password));

        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private readonly Logger log;

        public APIAccess(OsuConfigManager config, EndpointConfiguration endpointConfiguration)
        {
            this.config = config;

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

        private void onTokenChanged(ValueChangedEvent<OAuthToken> e) => config.Set(OsuSetting.Token, config.Get<bool>(OsuSetting.SavePassword) ? authentication.TokenString : string.Empty);

        internal new void Schedule(Action action) => base.Schedule(action);

        public string AccessToken => authentication.RequestAccessToken();

        /// <summary>
        /// Number of consecutive requests which failed due to network issues.
        /// </summary>
        private int failureCount;

        private void run()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                switch (State.Value)
                {
                    case APIState.Failing:
                        //todo: replace this with a ping request.
                        log.Add(@"In a failing state, waiting a bit before we try again...");
                        Thread.Sleep(5000);

                        if (!IsLoggedIn) goto case APIState.Connecting;

                        if (queue.Count == 0)
                        {
                            log.Add(@"Queueing a ping request");
                            Queue(new GetUserRequest());
                        }

                        break;

                    case APIState.Offline:
                    case APIState.Connecting:
                        // work to restore a connection...
                        if (!HasLogin)
                        {
                            state.Value = APIState.Offline;
                            Thread.Sleep(50);
                            continue;
                        }

                        state.Value = APIState.Connecting;

                        // save the username at this point, if the user requested for it to be.
                        config.Set(OsuSetting.Username, config.Get<bool>(OsuSetting.SaveUsername) ? ProvidedUsername : string.Empty);

                        if (!authentication.HasValidAccessToken && !authentication.AuthenticateWithLogin(ProvidedUsername, password))
                        {
                            //todo: this fails even on network-related issues. we should probably handle those differently.
                            //NotificationOverlay.ShowMessage("Login failed!");
                            log.Add(@"Login failed!");
                            password = null;
                            authentication.Clear();
                            continue;
                        }

                        var userReq = new GetUserRequest();

                        userReq.Success += u =>
                        {
                            localUser.Value = u;

                            // todo: save/pull from settings
                            localUser.Value.Status.Value = new UserStatusOnline();

                            failureCount = 0;
                        };

                        if (!handleRequest(userReq))
                        {
                            failConnectionProcess();
                            continue;
                        }

                        // getting user's friends is considered part of the connection process.
                        var friendsReq = new GetFriendsRequest();

                        friendsReq.Success += res =>
                        {
                            friends.AddRange(res);

                            //we're connected!
                            state.Value = APIState.Online;
                        };

                        if (!handleRequest(friendsReq))
                        {
                            failConnectionProcess();
                            continue;
                        }

                        // The Success callback event is fired on the main thread, so we should wait for that to run before proceeding.
                        // Without this, we will end up circulating this Connecting loop multiple times and queueing up many web requests
                        // before actually going online.
                        while (State.Value > APIState.Offline && State.Value < APIState.Online)
                            Thread.Sleep(500);

                        break;
                }

                // hard bail if we can't get a valid access token.
                if (authentication.RequestAccessToken() == null)
                {
                    Logout();
                    continue;
                }

                while (true)
                {
                    APIRequest req;

                    lock (queue)
                    {
                        if (queue.Count == 0) break;

                        req = queue.Dequeue();
                    }

                    handleRequest(req);
                }

                Thread.Sleep(50);
            }

            void failConnectionProcess()
            {
                // if something went wrong during the connection process, we want to reset the state (but only if still connecting).
                if (State.Value == APIState.Connecting)
                    state.Value = APIState.Failing;
            }
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
                    return JObject.Parse(req.GetResponseString()).SelectToken("form_error", true).AsNonNull().ToObject<RegistrationRequest.RegistrationRequestErrors>();
                }
                catch
                {
                    // if we couldn't deserialize the error message let's throw the original exception outwards.
                    e.Rethrow();
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

                // we could still be in initialisation, at which point we don't want to say we're Online yet.
                if (IsLoggedIn) state.Value = APIState.Online;

                failureCount = 0;
                return true;
            }
            catch (WebException we)
            {
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

        private bool handleWebException(WebException we)
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
                    return true;

                case HttpStatusCode.RequestTimeout:
                    failureCount++;
                    log.Add($@"API failure count is now {failureCount}");

                    if (failureCount < 3)
                        // we might try again at an api level.
                        return false;

                    if (State.Value == APIState.Online)
                    {
                        state.Value = APIState.Failing;
                        flushQueue();
                    }

                    return true;
            }

            return true;
        }

        public bool IsLoggedIn => localUser.Value.Id > 1;

        public void Queue(APIRequest request)
        {
            lock (queue) queue.Enqueue(request);
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
                        req.Fail(new WebException(@"Disconnected from server"));
                }
            }
        }

        public void Logout()
        {
            flushQueue();

            password = null;
            authentication.Clear();

            // Scheduled prior to state change such that the state changed event is invoked with the correct user and their friends present
            Schedule(() =>
            {
                localUser.Value = createGuestUser();
                friends.Clear();
            });

            state.Value = APIState.Offline;
        }

        private static User createGuestUser() => new GuestUser();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            flushQueue();
            cancellationToken.Cancel();
        }
    }

    internal class GuestUser : User
    {
        public GuestUser()
        {
            Username = @"Guest";
            Id = 1;
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
        /// We are in the process of (re-)connecting.
        /// </summary>
        Connecting,

        /// <summary>
        /// We are online.
        /// </summary>
        Online
    }
}
