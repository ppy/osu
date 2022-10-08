// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings.Sections.General;

namespace osu.Game.Overlays.Settings.Sections
{
    public class GeneralSection : SettingsSection
    {
        [Resolved(CanBeNull = true)]
        private FirstRunSetupOverlay firstRunSetupOverlay { get; set; }

        public override LocalisableString Header => GeneralSettingsStrings.GeneralSectionHeader;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Cog
        };

        public GeneralSection()
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = GeneralSettingsStrings.RunSetupWizard,
                    Action = () => firstRunSetupOverlay?.Show(),
                },
                new LanguageSettings(),
                new UpdateSettings(),
            };
        }
    }
}
