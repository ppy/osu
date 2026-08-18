// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class KeyBindingPanel : SettingsSubPanel
    {
        protected override Drawable CreateHeader() => new SettingsHeader(InputSettingsStrings.KeyBindingPanelHeader, InputSettingsStrings.KeyBindingPanelDescription);

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(RulesetStore rulesets)
        {
            AddSection(new GlobalKeyBindingsSection(FontAwesome.Solid.Globe, InputSettingsStrings.GlobalKeyBindingHeader)
            {
                Children = new[]
                {
                    new GlobalKeyBindingsSubsection(CommonStrings.General, GlobalActionCategory.General),
                    new GlobalKeyBindingsSubsection(InputSettingsStrings.AudioSection, GlobalActionCategory.AudioControl),
                    new GlobalKeyBindingsSubsection(InputSettingsStrings.OverlaysSection, GlobalActionCategory.Overlays),
                }
            });

            AddSection(new GlobalKeyBindingsSection(OsuIcon.GameplayC, InputSettingsStrings.InGameSection)
            {
                Children = new[]
                {
                    new GlobalKeyBindingsSubsection(CommonStrings.General, GlobalActionCategory.InGame),
                    new GlobalKeyBindingsSubsection(InputSettingsStrings.ReplaySection, GlobalActionCategory.Replay),
                }
            });

            AddSection(new GlobalKeyBindingsSection(OsuIcon.Beatmap, InputSettingsStrings.SongSelectSection)
            {
                Children = new[]
                {
                    new GlobalKeyBindingsSubsection(CommonStrings.General, GlobalActionCategory.SongSelect),
                }
            });

            AddSection(new GlobalKeyBindingsSection(OsuIcon.EditorSelect, InputSettingsStrings.EditorSection)
            {
                Children = new[]
                {
                    new GlobalKeyBindingsSubsection(CommonStrings.General, GlobalActionCategory.Editor),
                    new GlobalKeyBindingsSubsection(InputSettingsStrings.EditorTestPlaySection, GlobalActionCategory.EditorTestPlay),
                }
            });

            foreach (var ruleset in rulesets.AvailableRulesets)
                AddSection(new RulesetBindingsSection(ruleset));
        }
    }
}
