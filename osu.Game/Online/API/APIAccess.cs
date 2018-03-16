// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using osu.Framework.Configuration;
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

        public string Endpoint = @"https://osu.ppy.sh";
        private const string client_id = @"5";
        private const string client_secret = @"FGc9GAtyHzeQDshWP5Ah7dega8hJACAJpQtw6OXk";

        private ConcurrentQueue<APIRequest> queue = new ConcurrentQueue<APIRequest>();

        /// <summary>
        /// The username/email provided by the user when initiating a login.
        /// </summary>
        public string ProvidedUsername { get; private set; }

        private string password;

        public Bindable<User> LocalUser { get; } = new Bindable<User>(createGuestUser());

        public string Token
        {
            get { return authentication.Token?.ToString(); }
            set { authentication.Token = string.IsNullOrEmpty(value) ? null : OAuthToken.Parse(value); }
        }

        protected bool HasLogin => Token != null || !string.IsNullOrEmpty(ProvidedUsername) && !string.IsNullOrEmpty(password);

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable (should dispose of this or at very least keep a reference).
        private readonly Thread thread;

        private readonly Logger log;

        public APIAccess(OsuConfigManager config)
        {
            this.config = config;

            authentication = new OAuth(client_id, client_secret, Endpoint);
            log = Logger.GetLogger(LoggingTarget.Network);

            ProvidedUsername = config.Get<string>(OsuSetting.Username);
            Token = config.Get<string>(OsuSetting.Token);

            thread = new Thread(run) { IsBackground = true };
            thread.Start();
        }

        private readonly List<IOnlineComponent> components = new List<IOnlineComponent>();

        internal void Schedule(Action action) => base.Schedule(action);

        public void Register(IOnlineComponent component)
        {
            Scheduler.Add(delegate
            {
                components.Add(component);
                component.APIStateChanged(this, state);
            });
        }

        public void Unregister(IOnlineComponent component)
        {
            Scheduler.Add(delegate
            {
                components.Remove(component);
            });
        }

        public string AccessToken => authentication.RequestAccessToken();

        /// <summary>
        /// Number of consecutive requests which failed due to network issues.
        /// </summary>
        private int failureCount;

        private void run()
        {
            while (thread.IsAlive)
            {
                switch (State)
                {
                    case APIState.Failing:
                        //todo: replace this with a ping request.
                        log.Add(@"In a failing state, waiting a bit before we try again...");
                        Thread.Sleep(5000);
                        if (queue.Count == 0)
                        {
                            log.Add(@"Queueing a ping request");
                            Queue(new ListChannelsRequest { Timeout = 5000 });
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
                            Thread.Sleep(500);
                            continue;
                        }

                        // The Success callback event is fired on the main thread, so we should wait for that to run before proceeding.
                        // Without this, we will end up circulating this Connecting loop multiple times and queueing up many web requests
                        // before actually going online.
                        while (State != APIState.Online)
                            Thread.Sleep(500);

                        break;
                }

                //hard bail if we can't get a valid access token.
                if (authentication.RequestAccessToken() == null)
                {
                    Logout(false);
                    State = APIState.Offline;
                    continue;
                }

                //process the request queue.
                APIRequest req;
                while (queue.TryPeek(out req))
                {
                    if (handleRequest(req))
                    {
                        //we have succeeded, so let's unqueue.
                        queue.TryDequeue(out req);
                    }
                }

                Thread.Sleep(1);
            }
        }

        public void Login(string username, string password)
        {
            Debug.Assert(State == APIState.Offline);

            ProvidedUsername = username;
            this.password = password;
        }

        /// <summary>
        /// Handle a single API request.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <returns>true if we should remove this request from the queue.</returns>
        private bool handleRequest(APIRequest req)
        {
            try
            {
                Logger.Log($@"Performing request {req}", LoggingTarget.Network);
                req.Perform(this);

                //we could still be in initialisation, at which point we don't want to say we're Online yet.
                if (IsLoggedIn)
                    State = APIState.Online;

                failureCount = 0;
                return true;
            }
            catch (WebException we)
            {
                HttpStatusCode statusCode = (we.Response as HttpWebResponse)?.StatusCode ?? (we.Status == WebExceptionStatus.UnknownError ? HttpStatusCode.NotAcceptable : HttpStatusCode.RequestTimeout);

                switch (statusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        Logout(false);
                        return true;
                    case HttpStatusCode.RequestTimeout:
                        failureCount++;
                        log.Add($@"API failure count is now {failureCount}");

                        if (failureCount < 3)
                            //we might try again at an api level.
                            return false;

                        State = APIState.Failing;
                        flushQueue();
                        return true;
                }

                req.Fail(we);
                return true;
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                    log.Add(@"API level timeout exception was hit");

                req.Fail(e);
                return true;
            }
        }

        private APIState state;
        public APIState State
        {
            get { return state; }
            private set
            {
                APIState oldState = state;
                APIState newState = value;

                state = value;

                if (oldState != newState)
                {
                    log.Add($@"We just went {newState}!");
                    Scheduler.Add(delegate
                    {
                        components.ForEach(c => c.APIStateChanged(this, newState));
                        OnStateChange?.Invoke(oldState, newState);
                    });
                }
            }
        }

        public bool IsLoggedIn => LocalUser.Value.Id > 1;

        public void Queue(APIRequest request)
        {
            queue.Enqueue(request);
        }

        public event StateChangeDelegate OnStateChange;

        public delegate void StateChangeDelegate(APIState oldState, APIState newState);

        private void flushQueue(bool failOldRequests = true)
        {
            var oldQueue = queue;

            //flush the queue.
            queue = new ConcurrentQueue<APIRequest>();

            if (failOldRequests)
            {
                APIRequest req;
                while (oldQueue.TryDequeue(out req))
                    req.Fail(new WebException(@"Disconnected from server"));
            }
        }

        public void Logout(bool clearUsername = true)
        {
            flushQueue();
            if (clearUsername) ProvidedUsername = null;
            password = null;
            authentication.Clear();
            LocalUser.Value = createGuestUser();
        }

        private static User createGuestUser() => new User
        {
            Username = @"Guest",
            Id = 1,
        };

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            config.Set(OsuSetting.Token, config.Get<bool>(OsuSetting.SavePassword) ? Token : string.Empty);
            config.Save();
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
