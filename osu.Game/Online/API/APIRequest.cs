// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Extensions;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API
{
    /// <summary>
    /// An API request with a well-defined response type.
    /// </summary>
    /// <typeparam name="T">Type of the response (used for deserialisation).</typeparam>
    public class APIRequest<T> : APIRequest
    {
        protected override WebRequest CreateWebRequest() => new JsonWebRequest<T>(Uri);

        public APIRequest()
        {
            base.Success += onSuccess;
        }

        private void onSuccess()
        {
            Success?.Invoke(((JsonWebRequest<T>)WebRequest).ResponseObject);
        }

        public new event APISuccessHandler<T> Success;
    }

    /// <summary>
    /// AN API request with no specified response type.
    /// </summary>
    public class APIRequest
    {
        /// <summary>
        /// The maximum amount of time before this request will fail.
        /// </summary>
        public int Timeout = WebRequest.DEFAULT_TIMEOUT;

        protected virtual string Target => string.Empty;

        protected virtual WebRequest CreateWebRequest() => new WebRequest(Uri);

        protected virtual string Uri => $@"{api.Endpoint}/api/v2/{Target}";

        private double remainingTime => Math.Max(0, Timeout - (DateTime.Now.TotalMilliseconds() - (startTime ?? 0)));

        public bool ExceededTimeout => remainingTime == 0;

        private double? startTime;

        public double StartTime => startTime ?? -1;

        private APIAccess api;
        protected WebRequest WebRequest;

        public event APISuccessHandler Success;
        public event APIFailureHandler Failure;

        private bool cancelled;

        private Action pendingFailure;

        public void Perform(APIAccess api)
        {
            this.api = api;

            if (checkAndProcessFailure())
                return;

            if (startTime == null)
                startTime = DateTime.Now.TotalMilliseconds();

            if (remainingTime <= 0)
                throw new TimeoutException(@"API request timeout hit");

            WebRequest = CreateWebRequest();
            WebRequest.RetryCount = 0;
            WebRequest.Headers[@"Authorization"] = $@"Bearer {api.AccessToken}";

            if (checkAndProcessFailure())
                return;

            if (!WebRequest.Aborted) //could have been aborted by a Cancel() call
                WebRequest.BlockingPerform();

            if (checkAndProcessFailure())
                return;

            api.Scheduler.Add(delegate { Success?.Invoke(); });
        }

        public void Cancel() => Fail(new OperationCanceledException(@"Request cancelled"));

        public void Fail(Exception e)
        {
            cancelled = true;

            WebRequest?.Abort();

            pendingFailure = () => Failure?.Invoke(e);
            checkAndProcessFailure();
        }

        /// <summary>
        /// Checked for cancellation or error. Also queues up the Failed event if we can.
        /// </summary>
        /// <returns>Whether we are in a failed or cancelled state.</returns>
        private bool checkAndProcessFailure()
        {
            if (api == null || pendingFailure == null) return cancelled;

            api.Scheduler.Add(pendingFailure);
            pendingFailure = null;
            return true;
        }
    }

    public delegate void APIFailureHandler(Exception e);
    public delegate void APISuccessHandler();
    public delegate void APISuccessHandler<in T>(T content);
}
