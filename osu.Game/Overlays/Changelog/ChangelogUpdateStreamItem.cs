// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public partial class ChangelogUpdateStreamItem : OverlayStreamItem<APIUpdateStream>
    {
        public ChangelogUpdateStreamItem(APIUpdateStream stream)
            : base(stream)
        {
            if (stream.IsFeatured)
                Width *= 2;

            MainText = Value.DisplayName;
            AdditionalText = Value.LatestBuild.DisplayVersion;
            InfoText = Value.UserCount > 0 ? $"{"user".ToQuantity(Value.UserCount, "N0")} online" : default(LocalisableString);
        }

        protected override Color4 GetBarColour(OsuColour colours) => Value.Colour;
    }
}
