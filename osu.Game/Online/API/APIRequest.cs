﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.IO.Network;
using osu.Framework.Logging;

namespace osu.Game.Online.API
{
    /// <summary>
    /// An API request with a well-defined response type.
    /// </summary>
    /// <typeparam name="T">Type of the response (used for deserialisation).</typeparam>
    public abstract class APIRequest<T> : APIRequest
    {
        protected override WebRequest CreateWebRequest() => new JsonWebRequest<T>(Uri);

        public T Result => ((JsonWebRequest<T>)WebRequest).ResponseObject;

        protected APIRequest()
        {
            base.Success += onSuccess;
        }

        private void onSuccess() => Success?.Invoke(Result);

        /// <summary>
        /// Invoked on successful completion of an API request.
        /// This will be scheduled to the API's internal scheduler (run on update thread automatically).
        /// </summary>
        public new event APISuccessHandler<T> Success;
    }

    /// <summary>
    /// AN API request with no specified response type.
    /// </summary>
    public abstract class APIRequest
    {
        protected abstract string Target { get; }

        protected virtual WebRequest CreateWebRequest() => new WebRequest(Uri);

        protected virtual string Uri => $@"{API.Endpoint}/api/v2/{Target}";

        protected APIAccess API;
        protected WebRequest WebRequest;

        /// <summary>
        /// Invoked on successful completion of an API request.
        /// This will be scheduled to the API's internal scheduler (run on update thread automatically).
        /// </summary>
        public event APISuccessHandler Success;

        /// <summary>
        /// Invoked on failure to complete an API request.
        /// This will be scheduled to the API's internal scheduler (run on update thread automatically).
        /// </summary>
        public event APIFailureHandler Failure;

        private bool cancelled;

        private Action pendingFailure;

        public void Perform(APIAccess api)
        {
            API = api;

            if (checkAndScheduleFailure())
                return;

            WebRequest = CreateWebRequest();
            WebRequest.Failed += Fail;
            WebRequest.AllowRetryOnTimeout = false;
            WebRequest.AddHeader("Authorization", $"Bearer {api.AccessToken}");

            if (checkAndScheduleFailure())
                return;

            if (!WebRequest.Aborted) //could have been aborted by a Cancel() call
            {
                Logger.Log($@"Performing request {this}", LoggingTarget.Network);
                WebRequest.Perform();
            }

            if (checkAndScheduleFailure())
                return;

            api.Schedule(delegate { Success?.Invoke(); });
        }

        public void Cancel() => Fail(new OperationCanceledException(@"Request cancelled"));

        public void Fail(Exception e)
        {
            if (WebRequest?.Completed == true)
                return;

            if (cancelled)
                return;

            cancelled = true;
            WebRequest?.Abort();

            Logger.Log($@"Failing request {this} ({e})", LoggingTarget.Network);
            pendingFailure = () => Failure?.Invoke(e);
            checkAndScheduleFailure();
        }

        /// <summary>
        /// Checked for cancellation or error. Also queues up the Failed event if we can.
        /// </summary>
        /// <returns>Whether we are in a failed or cancelled state.</returns>
        private bool checkAndScheduleFailure()
        {
            if (API == null || pendingFailure == null) return cancelled;

            API.Schedule(pendingFailure);
            pendingFailure = null;
            return true;
        }
    }

    public delegate void APIFailureHandler(Exception e);
    public delegate void APISuccessHandler();
    public delegate void APIProgressHandler(long current, long total);
    public delegate void APISuccessHandler<in T>(T content);
}
