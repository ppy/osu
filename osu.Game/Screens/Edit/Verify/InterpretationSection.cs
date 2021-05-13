// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Edit.Verify
{
    internal class InterpretationSection : EditorRoundedScreenSettingsSection
    {
        [Resolved]
        private VerifyScreen verify { get; set; }

        protected override string Header => "Interpretation";

        private Bindable<DifficultyRating> interpretedDifficulty;

        [BackgroundDependencyLoader]
        private void load()
        {
            interpretedDifficulty = verify.InterpretedDifficulty.GetBoundCopy();

            var dropdown = new SettingsEnumDropdown<DifficultyRating>
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                TooltipText = "Affects checks that depend on difficulty level"
            };

            dropdown.Current.BindTo(interpretedDifficulty);

            Flow.Add(dropdown);
        }
    }
}
