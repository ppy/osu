﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
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
        /// Returns whether the local user is logged in.
        /// </summary>
        bool IsLoggedIn { get; }

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
    }
}
