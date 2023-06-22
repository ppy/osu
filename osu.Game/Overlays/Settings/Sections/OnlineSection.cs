// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings.Sections.Online;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class OnlineSection : SettingsSection
    {
        public override LocalisableString Header => OnlineSettingsStrings.OnlineSectionHeader;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.GlobeAsia
        };

        public OnlineSection()
        {
            Children = new Drawable[]
            {
                new WebSettings(),
                new AlertsAndPrivacySettings(),
                new IntegrationSettings()
            };
        }
    }
}
