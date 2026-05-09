// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
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
            InfoText = Value.UserCount > 0
                ? ChangelogStrings.BuildsUsersOnline(Value.UserCount.ToLocalisableString()).ToQuantity(Value.UserCount)
                : default(LocalisableString);
        }

        protected override Color4 GetBarColour(OsuColour colours) => Value.Colour;
    }
}
