// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
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

        public string Endpoint => @"https://osu.ppy.sh";
        private const string client_id = @"5";
        private const string client_secret = @"FGc9GAtyHzeQDshWP5Ah7dega8hJACAJpQtw6OXk";

        private readonly Queue<APIRequest> queue = new Queue<APIRequest>();

        /// <summary>
        /// The username/email provided by the user when initiating a login.
        /// </summary>
        public string ProvidedUsername { get; private set; }

        private string password;

        public Bindable<User> LocalUser { get; } = new Bindable<User>(createGuestUser());

        public Bindable<UserActivity> Activity { get; } = new Bindable<UserActivity>();

        protected bool HasLogin => authentication.Token.Value != null || (!string.IsNullOrEmpty(ProvidedUsername) && !string.IsNullOrEmpty(password));

        private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

        private readonly Logger log;

        public APIAccess(OsuConfigManager config)
        {
            this.config = config;

            authentication = new OAuth(client_id, client_secret, Endpoint);
            log = Logger.GetLogger(LoggingTarget.Network);

            ProvidedUsername = config.Get<string>(OsuSetting.Username);

            authentication.TokenString = config.Get<string>(OsuSetting.Token);
            authentication.Token.ValueChanged += onTokenChanged;

            LocalUser.BindValueChanged(u =>
            {
                u.OldValue?.Activity.UnbindFrom(Activity);
                u.NewValue.Activity.BindTo(Activity);
            }, true);

            var thread = new Thread(run)
            {
                Name = "APIAccess",
                IsBackground = true
            };

            thread.Start();
        }

        private void onTokenChanged(ValueChangedEvent<OAuthToken> e) => config.Set(OsuSetting.Token, config.Get<bool>(OsuSetting.SavePassword) ? authentication.TokenString : string.Empty);

        private readonly List<IOnlineComponent> components = new List<IOnlineComponent>();

        internal new void Schedule(Action action) => base.Schedule(action);

        /// <summary>
        /// Register a component to receive API events.
        /// Fires <see cref="IOnlineComponent.APIStateChanged"/> once immediately to ensure a correct state.
        /// </summary>
        /// <param name="component"></param>
        public void Register(IOnlineComponent component)
        {
            Schedule(() => components.Add(component));
            component.APIStateChanged(this, state);
        }

        public void Unregister(IOnlineComponent component)
        {
            Schedule(() => components.Remove(component));
        }

        public string AccessToken => authentication.RequestAccessToken();

        /// <summary>
        /// Number of consecutive requests which failed due to network issues.
        /// </summary>
        private int failureCount;

        private void run()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                switch (State)
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
                        //work to restore a connection...
                        if (!HasLogin)
                        {
                            State = APIState.Offline;
                            Thread.Sleep(50);
                            continue;
                        }

                        State = APIState.Connecting;

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
                            LocalUser.Value = u;
                            failureCount = 0;

                            //we're connected!
                            State = APIState.Online;
                        };

                        if (!handleRequest(userReq))
                        {
                            if (State == APIState.Connecting)
                                State = APIState.Failing;
                            continue;
                        }

                        // The Success callback event is fired on the main thread, so we should wait for that to run before proceeding.
                        // Without this, we will end up circulating this Connecting loop multiple times and queueing up many web requests
                        // before actually going online.
                        while (State > APIState.Offline && State < APIState.Online)
                            Thread.Sleep(500);

                        break;
                }

                //hard bail if we can't get a valid access token.
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
        }

        public void Login(string username, string password)
        {
            Debug.Assert(State == APIState.Offline);

            ProvidedUsername = username;
            this.password = password;
        }

        public RegistrationRequest.RegistrationRequestErrors CreateAccount(string email, string username, string password)
        {
            Debug.Assert(State == APIState.Offline);

            var req = new RegistrationRequest
            {
                Url = $@"{Endpoint}/users",
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
                    return JObject.Parse(req.ResponseString).SelectToken("form_error", true).ToObject<RegistrationRequest.RegistrationRequestErrors>();
                }
                catch
                {
                    // if we couldn't deserialize the error message let's throw the original exception outwards.
                    throw e;
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

                //we could still be in initialisation, at which point we don't want to say we're Online yet.
                if (IsLoggedIn) State = APIState.Online;

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

        private APIState state;

        public APIState State
        {
            get => state;
            private set
            {
                if (state == value)
                    return;

                APIState oldState = state;
                state = value;

                log.Add($@"We just went {state}!");
                Schedule(() =>
                {
                    components.ForEach(c => c.APIStateChanged(this, state));
                    OnStateChange?.Invoke(oldState, state);
                });
            }
        }

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
                        //we might try again at an api level.
                        return false;

                    if (State == APIState.Online)
                    {
                        State = APIState.Failing;
                        flushQueue();
                    }

                    return true;
            }

            return true;
        }

        public bool IsLoggedIn => LocalUser.Value.Id > 1;

        public void Queue(APIRequest request)
        {
            lock (queue) queue.Enqueue(request);
        }

        public event StateChangeDelegate OnStateChange;

        public delegate void StateChangeDelegate(APIState oldState, APIState newState);

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

            // Scheduled prior to state change such that the state changed event is invoked with the correct user present
            Schedule(() => LocalUser.Value = createGuestUser());

            State = APIState.Offline;
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
