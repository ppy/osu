// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings.Sections.Gameplay;
using osu.Game.Overlays.Settings.Sections.Graphics;
using KiaiSettings = osu.Game.Overlays.Settings.Sections.Gameplay.KiaiSettings;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class GameplaySection : SettingsSection
    {
        public override LocalisableString Header => GameplaySettingsStrings.GameplaySectionHeader;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Regular.DotCircle
        };

        public GameplaySection()
        {
            Children = new Drawable[]
            {
                new GeneralSettings(),
                new AudioSettings(),
                new BeatmapSettings(),
                new BackgroundSettings(),
                new KiaiSettings(),
                new HUDSettings(),
                new InputSettings(),
                new ModsSettings(),
            };
        }
    }
}
