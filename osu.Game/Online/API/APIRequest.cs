// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    /// <summary>
    /// An API request with a well-defined response type.
    /// </summary>
    /// <typeparam name="T">Type of the response (used for deserialisation).</typeparam>
    public abstract class APIRequest<T> : APIRequest where T : class
    {
        protected override WebRequest CreateWebRequest() => new OsuJsonWebRequest<T>(Uri);

        public T Result { get; private set; }

        /// <summary>
        /// Invoked on successful completion of an API request.
        /// This will be scheduled to the API's internal scheduler (run on update thread automatically).
        /// </summary>
        public new event APISuccessHandler<T> Success;

        protected override void PostProcess()
        {
            base.PostProcess();
            Result = ((OsuJsonWebRequest<T>)WebRequest)?.ResponseObject;
        }

        internal void TriggerSuccess(T result)
        {
            if (Result != null)
                throw new InvalidOperationException("Attempted to trigger success more than once");

            Result = result;

            TriggerSuccess();
        }

        internal override void TriggerSuccess()
        {
            base.TriggerSuccess();
            Success?.Invoke(Result);
        }
    }

    /// <summary>
    /// AN API request with no specified response type.
    /// </summary>
    public abstract class APIRequest
    {
        protected abstract string Target { get; }

        protected virtual WebRequest CreateWebRequest() => new OsuWebRequest(Uri);

        protected virtual string Uri => $@"{API.APIEndpointUrl}/api/v2/{Target}";

        protected APIAccess API;
        protected WebRequest WebRequest;

        /// <summary>
        /// The currently logged in user. Note that this will only be populated during <see cref="Perform"/>.
        /// </summary>
        protected User User { get; private set; }

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

        public void Perform(IAPIProvider api)
        {
            if (!(api is APIAccess apiAccess))
            {
                Fail(new NotSupportedException($"A {nameof(APIAccess)} is required to perform requests."));
                return;
            }

            API = apiAccess;
            User = apiAccess.LocalUser.Value;

            if (checkAndScheduleFailure())
                return;

            WebRequest = CreateWebRequest();
            WebRequest.Failed += Fail;
            WebRequest.AllowRetryOnTimeout = false;
            WebRequest.AddHeader("Authorization", $"Bearer {API.AccessToken}");

            if (checkAndScheduleFailure())
                return;

            if (!WebRequest.Aborted) // could have been aborted by a Cancel() call
            {
                Logger.Log($@"Performing request {this}", LoggingTarget.Network);
                WebRequest.Perform();
            }

            if (checkAndScheduleFailure())
                return;

            PostProcess();

            API.Schedule(delegate
            {
                if (cancelled) return;

                TriggerSuccess();
            });
        }

        /// <summary>
        /// Perform any post-processing actions after a successful request.
        /// </summary>
        protected virtual void PostProcess()
        {
        }

        internal virtual void TriggerSuccess()
        {
            Success?.Invoke();
        }

        internal void TriggerFailure(Exception e)
        {
            Failure?.Invoke(e);
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

            string responseString = WebRequest?.GetResponseString();

            if (!string.IsNullOrEmpty(responseString))
            {
                try
                {
                    // attempt to decode a displayable error string.
                    var error = JsonConvert.DeserializeObject<DisplayableError>(responseString);
                    if (error != null)
                        e = new APIException(error.ErrorMessage, e);
                }
                catch
                {
                }
            }

            Logger.Log($@"Failing request {this} ({e})", LoggingTarget.Network);
            pendingFailure = () => TriggerFailure(e);
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

        private class DisplayableError
        {
            [JsonProperty("error")]
            public string ErrorMessage { get; set; }
        }
    }

    public class APIException : InvalidOperationException
    {
        public APIException(string messsage, Exception innerException)
            : base(messsage, innerException)
        {
        }
    }

    public delegate void APIFailureHandler(Exception e);

    public delegate void APISuccessHandler();

    public delegate void APIProgressHandler(long current, long total);

    public delegate void APISuccessHandler<in T>(T content);
}
