// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.IO.Network;
using osu.Framework.Logging;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API
{
    /// <summary>
    /// An API request with a well-defined response type.
    /// </summary>
    /// <typeparam name="T">Type of the response (used for deserialisation).</typeparam>
    public abstract class APIRequest<T> : APIRequest where T : class
    {
        protected override WebRequest CreateWebRequest() => new OsuJsonWebRequest<T>(Uri);

        /// <summary>
        /// The deserialised response object. May be null if the request or deserialisation failed.
        /// </summary>
        [CanBeNull]
        public T Response { get; private set; }

        /// <summary>
        /// Invoked on successful completion of an API request.
        /// This will be scheduled to the API's internal scheduler (run on update thread automatically).
        /// </summary>
        public new event APISuccessHandler<T> Success;

        protected APIRequest()
        {
            base.Success += () => Success?.Invoke(Response);
        }

        protected override void PostProcess()
        {
            base.PostProcess();

            if (WebRequest != null)
            {
                Response = ((OsuJsonWebRequest<T>)WebRequest).ResponseObject;
                Logger.Log($"{GetType()} finished with response size of {WebRequest.ResponseStream.Length:#,0} bytes", LoggingTarget.Network);
            }
        }

        internal void TriggerSuccess(T result)
        {
            if (Response != null)
                throw new InvalidOperationException("Attempted to trigger success more than once");

            Response = result;

            TriggerSuccess();
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
        protected APIUser User { get; private set; }

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

        private readonly object completionStateLock = new object();

        /// <summary>
        /// The state of this request, from an outside perspective.
        /// This is used to ensure correct notification events are fired.
        /// </summary>
        public APIRequestCompletionState CompletionState { get; private set; }

        public void Perform(IAPIProvider api)
        {
            if (!(api is APIAccess apiAccess))
            {
                Fail(new NotSupportedException($"A {nameof(APIAccess)} is required to perform requests."));
                return;
            }

            API = apiAccess;
            User = apiAccess.LocalUser.Value;

            if (isFailing) return;

            WebRequest = CreateWebRequest();
            WebRequest.Failed += Fail;
            WebRequest.AllowRetryOnTimeout = false;
            if (!string.IsNullOrEmpty(API.AccessToken))
                WebRequest.AddHeader("Authorization", $"Bearer {API.AccessToken}");

            if (isFailing) return;

            Logger.Log($@"Performing request {this}", LoggingTarget.Network);
            WebRequest.Perform();

            if (isFailing) return;

            PostProcess();

            TriggerSuccess();
        }

        /// <summary>
        /// Perform any post-processing actions after a successful request.
        /// </summary>
        protected virtual void PostProcess()
        {
        }

        internal void TriggerSuccess()
        {
            lock (completionStateLock)
            {
                if (CompletionState != APIRequestCompletionState.Waiting)
                    return;

                CompletionState = APIRequestCompletionState.Completed;
            }

            if (API == null)
                Success?.Invoke();
            else
                API.Schedule(() => Success?.Invoke());
        }

        internal void TriggerFailure(Exception e)
        {
            lock (completionStateLock)
            {
                if (CompletionState != APIRequestCompletionState.Waiting)
                    return;

                CompletionState = APIRequestCompletionState.Failed;
            }

            if (API == null)
                Failure?.Invoke(e);
            else
                API.Schedule(() => Failure?.Invoke(e));
        }

        public void Cancel() => Fail(new OperationCanceledException(@"Request cancelled"));

        public void Fail(Exception e)
        {
            lock (completionStateLock)
            {
                if (CompletionState != APIRequestCompletionState.Waiting)
                    return;

                WebRequest?.Abort();

                // in the case of a cancellation we don't care about whether there's an error in the response.
                if (!(e is OperationCanceledException))
                {
                    string responseString = WebRequest?.GetResponseString();

                    // naive check whether there's an error in the response to avoid unnecessary JSON deserialisation.
                    if (!string.IsNullOrEmpty(responseString) && responseString.Contains(@"""error"""))
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
                }

                Logger.Log($@"Failing request {this} ({e})", LoggingTarget.Network);
                TriggerFailure(e);
            }
        }

        /// <summary>
        /// Whether this request is in a failing or failed state.
        /// </summary>
        private bool isFailing
        {
            get
            {
                lock (completionStateLock)
                    return CompletionState == APIRequestCompletionState.Failed;
            }
        }

        private class DisplayableError
        {
            [JsonProperty("error")]
            public string ErrorMessage { get; set; }
        }
    }

    public delegate void APIFailureHandler(Exception e);

    public delegate void APISuccessHandler();

    public delegate void APIProgressHandler(long current, long total);

    public delegate void APISuccessHandler<in T>(T content);
}
