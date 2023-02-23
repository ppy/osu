// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Sections
{
    public partial class AboutSection : ProfileSection
    {
        public override LocalisableString Title => UsersStrings.ShowExtraMeTitle;

        public override string Identifier => @"me";
    }
}
