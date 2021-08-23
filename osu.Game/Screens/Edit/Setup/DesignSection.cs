// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal class DesignSection : SetupSection
    {
        private LabelledSwitchButton widescreenSupport;
        private LabelledSwitchButton epilepsyWarning;
        private LabelledSwitchButton letterboxDuringBreaks;

        public override LocalisableString Title => "Design";

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new[]
            {
                widescreenSupport = new LabelledSwitchButton
                {
                    Label = "Widescreen support",
                    Description = "Allows storyboards to use the full screen space, rather than be confined to a 4:3 area.",
                    Current = { Value = Beatmap.BeatmapInfo.WidescreenStoryboard }
                },
                epilepsyWarning = new LabelledSwitchButton
                {
                    Label = "Epilepsy warning",
                    Description = "Recommended if the storyboard or video contain scenes with rapidly flashing colours.",
                    Current = { Value = Beatmap.BeatmapInfo.EpilepsyWarning }
                },
                letterboxDuringBreaks = new LabelledSwitchButton
                {
                    Label = "Letterbox during breaks",
                    Description = "Adds horizontal letterboxing to give a cinematic look during breaks.",
                    Current = { Value = Beatmap.BeatmapInfo.LetterboxInBreaks }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            widescreenSupport.Current.BindValueChanged(_ => updateBeatmap());
            epilepsyWarning.Current.BindValueChanged(_ => updateBeatmap());
            letterboxDuringBreaks.Current.BindValueChanged(_ => updateBeatmap());
        }

        private void updateBeatmap()
        {
            Beatmap.BeatmapInfo.WidescreenStoryboard = widescreenSupport.Current.Value;
            Beatmap.BeatmapInfo.EpilepsyWarning = epilepsyWarning.Current.Value;
            Beatmap.BeatmapInfo.LetterboxInBreaks = letterboxDuringBreaks.Current.Value;
        }
    }
}
