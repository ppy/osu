// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings.Sections.Gameplay;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class GameplaySection : SettingsSection
    {
        public override LocalisableString Header => GameplaySettingsStrings.GameplaySectionHeader;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = OsuIcon.GameplayC
        };

        public GameplaySection()
        {
            Children = new Drawable[]
            {
                new GeneralSettings(),
                new AudioSettings(),
                new BeatmapSettings(),
                new BackgroundSettings(),
                new HUDSettings(),
                new InputSettings(),
                new ModsSettings(),
            };
        }
    }
}
