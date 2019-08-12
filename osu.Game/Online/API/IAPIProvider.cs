// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Users;

namespace osu.Game.Online.API
{
    public interface IAPIProvider
    {
        /// <summary>
        /// The local user.
        /// </summary>
        Bindable<User> LocalUser { get; }

        /// <summary>
        /// The current user's activity.
        /// </summary>
        Bindable<UserActivity> Activity { get; }

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
        string Endpoint { get; }

        APIState State { get; }

        /// <summary>
        /// Queue a new request.
        /// </summary>
        /// <param name="request">The request to perform.</param>
        void Queue(APIRequest request);

        /// <summary>
        /// Register a component to receive state changes.
        /// </summary>
        /// <param name="component">The component to register.</param>
        void Register(IOnlineComponent component);

        /// <summary>
        /// Unregisters a component to receive state changes.
        /// </summary>
        /// <param name="component">The component to unregister.</param>
        void Unregister(IOnlineComponent component);

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
        /// Create a new user account. This is a blocking operation.
        /// </summary>
        /// <param name="email">The email to create the account with.</param>
        /// <param name="username">The username to create the account with.</param>
        /// <param name="password">The password to create the account with.</param>
        /// <returns>Any errors encoutnered during account creation.</returns>
        RegistrationRequest.RegistrationRequestErrors CreateAccount(string email, string username, string password);
    }
}
