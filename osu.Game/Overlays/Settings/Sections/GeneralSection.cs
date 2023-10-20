// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings.Sections.General;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class GeneralSection : SettingsSection
    {
        [Resolved(CanBeNull = true)]
        private FirstRunSetupOverlay? firstRunSetupOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private OsuGame? game { get; set; }

        public override LocalisableString Header => CommonStrings.General;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Cog
        };

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = GeneralSettingsStrings.RunSetupWizard,
                    Keywords = new[] { @"first run", @"initial", @"getting started", @"import", @"tutorial", @"recommended beatmaps" },
                    TooltipText = FirstRunSetupOverlayStrings.FirstRunSetupDescription,
                    Action = () => firstRunSetupOverlay?.Show(),
                },
                new SettingsButton
                {
                    Text = GeneralSettingsStrings.LearnMoreAboutLazer,
                    TooltipText = GeneralSettingsStrings.LearnMoreAboutLazerTooltip,
                    BackgroundColour = colours.YellowDark,
                    Action = () => game?.ShowWiki(@"Help_centre/Upgrading_to_lazer")
                },
                new LanguageSettings(),
                new UpdateSettings(),
            };
        }
    }
}
