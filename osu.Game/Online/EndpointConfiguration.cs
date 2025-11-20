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
        /// The OAuth client secret.
        /// </summary>
        public string APIClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// The OAuth client ID.
        /// </summary>
        public string APIClientID { get; set; } = string.Empty;

        /// <summary>
        /// The base URL for the website. Does not include a trailing slash.
        /// </summary>
        public string WebsiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint for the main (osu-web) API. Does not include a trailing slash.
        /// </summary>
        public string APIUrl { get; set; } = string.Empty;

        /// <summary>
        /// The root URL for the service handling beatmap submission. Does not include a trailing slash.
        /// </summary>
        public string? BeatmapSubmissionServiceUrl { get; set; }

        /// <summary>
        /// The endpoint for the SignalR spectator server.
        /// </summary>
        public string SpectatorUrl { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint for the SignalR multiplayer server.
        /// </summary>
        public string MultiplayerUrl { get; set; } = string.Empty;

        /// <summary>
        /// The endpoint for the SignalR metadata server.
        /// </summary>
        public string MetadataUrl { get; set; } = string.Empty;

        /// <summary>
        /// The URL to a separate endpoint that serves as a "liveness probe" for online services, indicating any potential active outages.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>The liveness probe's presence is optional. If this is <see langword="null"/>, the entire mechanism predicated on it will be turned off.</item>
        /// <item>
        /// The liveness probe only has any effect if it is reachable and actively returns a response that indicates an ongoing outage.
        /// Failing to reach the liveness probe has no effect as it is indistinguishable from a problem that is local to the machine the client is running on.
        /// </item>
        /// </list>
        /// </remarks>
        public string? LivenessProbeUrl { get; set; }
    }
}
