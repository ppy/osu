// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    /// <summary>
    /// Holds configuration for API endpoints.
    /// </summary>
    public class EndpointConfiguration
    {
        /// <summary>
        /// The endpoint for the main (osu-web) API.
        /// </summary>
        public string APIEndpoint { get; set; }

        /// <summary>
        /// The OAuth client secret.
        /// </summary>
        public string APIClientSecret { get; set; }

        /// <summary>
        /// The OAuth client ID.
        /// </summary>
        public string APIClientID { get; set; }

        public string SpectatorEndpoint { get; set; }

        public string MultiplayerEndpoint { get; set; }
    }
}
