// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class GlobalKeyBindingsSection : SettingsSection
    {
        private readonly IconUsage icon;

        public GlobalKeyBindingsSection(IconUsage icon, LocalisableString header)
        {
            this.icon = icon;
            Header = header;
        }

        public override Drawable CreateIcon() => new SpriteIcon { Icon = icon };

        public override LocalisableString Header { get; }
    }
}
