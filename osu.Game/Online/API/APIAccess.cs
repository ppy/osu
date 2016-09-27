//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using osu.Framework;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Online.API.Requests;

namespace osu.Game.Online.API
{
    public class APIAccess : IUpdateable
    {
        private OAuth authentication;

        public string Endpoint = @"https://new.ppy.sh";
        const string ClientId = @"daNBnfdv7SppRVc61z0XuOI13y6Hroiz";
        const string ClientSecret = @"d6fgZuZeQ0eSXkEj5igdqQX6ztdtS6Ow";

        ConcurrentQueue<APIRequest> queue = new ConcurrentQueue<APIRequest>();

        public Scheduler Scheduler = new Scheduler();

        public string Username;

        private SecurePassword password;

        public string Password
        {
            set
            {
                password = string.IsNullOrEmpty(value) ? null : new SecurePassword(value);
            }
        }

        public string Token
        {
            get { return authentication.Token?.ToString(); }

            set
            {

                if (string.IsNullOrEmpty(value))
                    authentication.Token = null;
                else
                    authentication.Token = OAuthToken.Parse(value);
            }
        }

        protected bool HasLogin => Token != null || (!string.IsNullOrEmpty(Username) && password != null);

        private Thread thread;

        Logger log;

        public APIAccess()
        {
            authentication = new OAuth(ClientId, ClientSecret, Endpoint);
            log = Logger.GetLogger(LoggingTarget.Network);

            thread = new Thread(run) { IsBackground = true };
            thread.Start();
        }

        public string AccessToken => authentication.RequestAccessToken();

        /// <summary>
        /// Number of consecutive requests which failed due to network issues.
        /// </summary>
        int failureCount = 0;

        private void run()
        {
            while (true)
            {
                switch (State)
                {
                    case APIState.Failing:
                        //todo: replace this with a ping request.
                        log.Add($@"In a failing state, waiting a bit before we try again...");
                        Thread.Sleep(5000);
                        if (queue.Count == 0)
                        {
                            log.Add($@"Queueing a ping request");
                            Queue(new ListChannelsRequest() { Timeout = 5000 });
                        }
                        break;
                    case APIState.Offline:
                        //work to restore a connection...
                        if (!HasLogin)
                        {
                            //OsuGame.Scheduler.Add(() => { OsuGame.ShowLogin(); });

                            State = APIState.Offline;
                            Thread.Sleep(500);
                            continue;
                        }

                        if (State < APIState.Connecting)
                            State = APIState.Connecting;

                        if (!authentication.HasValidAccessToken && !authentication.AuthenticateWithLogin(Username, password.Get(Representation.Raw)))
                        {
                            //todo: this fails even on network-related issues. we should probably handle those differently.
                            //NotificationManager.ShowMessage("Login failed!");
                            log.Add(@"Login failed!");
                            ClearCredentials();
                            continue;
                        }

                        //we're connected!
                        State = APIState.Online;
                        failureCount = 0;
                        break;
                }

                //hard bail if we can't get a valid access token.
                if (authentication.RequestAccessToken() == null)
                {
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

        private void ClearCredentials()
        {
            Username = null;
            password = null;
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
                req.Perform(this);

                State = APIState.Online;
                failureCount = 0;
                return true;
            }
            catch (WebException we)
            {
                HttpStatusCode statusCode = (we.Response as HttpWebResponse)?.StatusCode ?? HttpStatusCode.RequestTimeout;

                switch (statusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        State = APIState.Offline;
                        return true;
                    case HttpStatusCode.RequestTimeout:
                        failureCount++;
                        log.Add($@"API failure count is now {failureCount}");

                        if (failureCount < 3)
                            //we might try again at an api level.
                            return false;

                        State = APIState.Failing;
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
            set
            {
                APIState oldState = state;
                APIState newState = value;

                state = value;

                switch (state)
                {
                    case APIState.Failing:
                    case APIState.Offline:
                        flushQueue();
                        break;
                }

                if (oldState != newState)
                {
                    //OsuGame.Scheduler.Add(delegate
                    {
                        //NotificationManager.ShowMessage($@"We just went {newState}!", newState == APIState.Online ? Color4.YellowGreen : Color4.OrangeRed, 5000);
                        log.Add($@"We just went {newState}!");
                        OnStateChange?.Invoke(oldState, newState);
                    }
                }
            }
        }

        public void Queue(APIRequest request)
        {
            queue.Enqueue(request);
        }

        public event StateChangeDelegate OnStateChange;

        public delegate void StateChangeDelegate(APIState oldState, APIState newState);

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

        private void flushQueue(bool failOldRequests = true)
        {
            var oldQueue = queue;

            //flush the queue.
            queue = new ConcurrentQueue<APIRequest>();

            if (failOldRequests)
            {
                APIRequest req;
                while (queue.TryDequeue(out req))
                    req.Fail(new Exception(@"Disconnected from server"));
            }
        }

        public void Logout()
        {
            authentication.Clear();
            State = APIState.Offline;
        }

        public void Update()
        {
            Scheduler.Update();
        }
    }
}
