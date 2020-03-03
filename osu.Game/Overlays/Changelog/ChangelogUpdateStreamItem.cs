// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogUpdateStreamItem : OverlayUpdateStreamItem<APIUpdateStream>
    {
        public ChangelogUpdateStreamItem(APIUpdateStream stream)
            : base(stream)
        {
            if (stream.IsFeatured)
                Width *= 2; 
        }

        protected override string GetMainText() => Value.DisplayName;

        protected override string GetAdditionalText() => Value.LatestBuild.DisplayVersion;

        protected override string GetInfoText() => Value.LatestBuild.Users > 0 ? $"{"user".ToQuantity(Value.LatestBuild.Users, "N0")} online" : null;

        protected override Color4 GetBarColour(OsuColour colours) => Value.Colour;
    }
}
