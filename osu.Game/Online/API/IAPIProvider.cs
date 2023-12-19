// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Notifications;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    public interface IAPIProvider
    {
        /// <summary>
        /// The local user.
        /// </summary>
        IBindable<APIUser> LocalUser { get; }

        /// <summary>
        /// The user's friends.
        /// </summary>
        IBindableList<APIUser> Friends { get; }

        /// <summary>
        /// The current user's activity.
        /// </summary>
        IBindable<UserActivity> Activity { get; }

        /// <summary>
        /// The language supplied by this provider to API requests.
        /// </summary>
        Language Language { get; }

        /// <summary>
        /// Retrieve the OAuth access token.
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Returns whether the local user is logged in.
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// The last username provided by the end-user.
        /// May not be authenticated.
        /// </summary>
        string ProvidedUsername { get; }

        /// <summary>
        /// The URL endpoint for this API. Does not include a trailing slash.
        /// </summary>
        string APIEndpointUrl { get; }

        /// <summary>
        /// The root URL of of the website, excluding the trailing slash.
        /// </summary>
        string WebsiteRootUrl { get; }

        /// <summary>
        /// The version of the API.
        /// </summary>
        int APIVersion { get; }

        /// <summary>
        /// The last login error that occurred, if any.
        /// </summary>
        Exception? LastLoginError { get; }

        /// <summary>
        /// The current connection state of the API.
        /// This is not thread-safe and should be scheduled locally if consumed from a drawable component.
        /// </summary>
        IBindable<APIState> State { get; }

        /// <summary>
        /// Queue a new request.
        /// </summary>
        /// <param name="request">The request to perform.</param>
        void Queue(APIRequest request);

        /// <summary>
        /// Perform a request immediately, bypassing any API state checks.
        /// </summary>
        /// <remarks>
        /// Can be used to run requests as a guest user.
        /// </remarks>
        /// <param name="request">The request to perform.</param>
        void Perform(APIRequest request);

        /// <summary>
        /// Perform a request immediately, bypassing any API state checks.
        /// </summary>
        /// <remarks>
        /// Can be used to run requests as a guest user.
        /// </remarks>
        /// <param name="request">The request to perform.</param>
        Task PerformAsync(APIRequest request);

        /// <summary>
        /// Attempt to login using the provided credentials. This is a non-blocking operation.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <param name="password">The user's password.</param>
        void Login(string username, string password);

        /// <summary>
        /// Log out the current user.
        /// </summary>
        void Logout();

        /// <summary>
        /// Constructs a new <see cref="IHubClientConnector"/>. May be null if not supported.
        /// </summary>
        /// <param name="clientName">The name of the client this connector connects for, used for logging.</param>
        /// <param name="endpoint">The endpoint to the hub.</param>
        /// <param name="preferMessagePack">Whether to use MessagePack for serialisation if available on this platform.</param>
        IHubClientConnector? GetHubConnector(string clientName, string endpoint, bool preferMessagePack = true);

        /// <summary>
        /// Constructs a new <see cref="NotificationsClientConnector"/>.
        /// </summary>
        NotificationsClientConnector GetNotificationsConnector();

        /// <summary>
        /// Create a new user account. This is a blocking operation.
        /// </summary>
        /// <param name="email">The email to create the account with.</param>
        /// <param name="username">The username to create the account with.</param>
        /// <param name="password">The password to create the account with.</param>
        /// <returns>Any errors encoutnered during account creation.</returns>
        RegistrationRequest.RegistrationRequestErrors? CreateAccount(string email, string username, string password);
    }
}
