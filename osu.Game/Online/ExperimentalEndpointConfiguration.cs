// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online
{
    public class ExperimentalEndpointConfiguration : EndpointConfiguration
    {
        public ExperimentalEndpointConfiguration()
        {
            WebsiteRootUrl = @"https://osu.ppy.sh";
            APIEndpointUrl = @"https://lazer.ppy.sh";
            APIClientSecret = @"FGc9GAtyHzeQDshWP5Ah7dega8hJACAJpQtw6OXk";
            APIClientID = "5";
            SpectatorEndpointUrl = "https://spectator.ppy.sh/spectator";
            MultiplayerEndpointUrl = "https://spectator.ppy.sh/multiplayer";
            MetadataEndpointUrl = "https://spectator.ppy.sh/metadata";
        }
    }
}
