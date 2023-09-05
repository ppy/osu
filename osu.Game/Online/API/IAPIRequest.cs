// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Online.API
{
    /// <summary>
    /// AN API request with no specified response type.
    /// </summary>
    public interface IAPIRequest
    {
        /// <summary>
        /// Invoked on successful completion of an API request.
        /// This will be scheduled to the API's internal scheduler (run on update thread automatically).
        /// </summary>
        event APISuccessHandler? Success;

        /// <summary>
        /// Invoked on failure to complete an API request.
        /// This will be scheduled to the API's internal scheduler (run on update thread automatically).
        /// </summary>
        event APIFailureHandler? Failure;

        /// <summary>
        /// The state of this request, from an outside perspective.
        /// This is used to ensure correct notification events are fired.
        /// </summary>
        APIRequestCompletionState CompletionState { get; }

        /// <summary>
        /// Synchronously performs this request using the supplied <paramref name="api"/>.
        /// </summary>
        void Perform(IAPIProvider api);

        /// <summary>
        /// Cancels the in-flight request.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Fails the request.
        /// </summary>
        /// <param name="e">The exception to throw describing the cause of failure.</param>
        void Fail(Exception e);

        /// <summary>
        /// Successfully completes the request.
        /// Used for testing purposes only.
        /// </summary>
        internal void TriggerSuccess();

        /// <summary>
        /// Successfully completes the request.
        /// Used for testing purposes only.
        /// </summary>
        /// <param name="e">The exception to throw describing the cause of failure.</param>
        internal void TriggerFailure(Exception e);
    }

    /// <summary>
    /// An API request with a well-defined response type.
    /// </summary>
    /// <typeparam name="TResponse">Type of the response (used for deserialisation).</typeparam>
    public interface IAPIRequest<out TResponse> : IAPIRequest
        where TResponse : class
    {
        /// <summary>
        /// The deserialised response object. May be null if the request or deserialisation failed.
        /// </summary>
        TResponse? Response { get; }

        /// <summary>
        /// Invoked on successful completion of an API request.
        /// This will be scheduled to the API's internal scheduler (run on update thread automatically).
        /// </summary>
        new event APISuccessHandler<TResponse>? Success;
    }
}
