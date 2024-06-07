// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class GlobalKeyBindingsSection : SettingsSection
    {
        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Globe
        };

        public override LocalisableString Header => InputSettingsStrings.GlobalKeyBindingHeader;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(new[]
            {
                new GlobalKeyBindingsSubsection(string.Empty, GlobalActionCategory.General),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.OverlaysSection, GlobalActionCategory.Overlays),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.AudioSection, GlobalActionCategory.AudioControl),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.SongSelectSection, GlobalActionCategory.SongSelect),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.InGameSection, GlobalActionCategory.InGame),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.ReplaySection, GlobalActionCategory.Replay),
                new GlobalKeyBindingsSubsection(InputSettingsStrings.EditorSection, GlobalActionCategory.Editor),
            });
        }
    }
}
