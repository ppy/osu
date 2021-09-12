// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Setup
{
    internal class DesignSection : SetupSection
    {
        protected LabelledSwitchButton EnableCountdown;

        protected FillFlowContainer CountdownSettings;
        protected LabelledEnumDropdown<CountdownType> CountdownSpeed;
        protected LabelledNumberBox CountdownOffset;

        private LabelledSwitchButton widescreenSupport;
        private LabelledSwitchButton epilepsyWarning;
        private LabelledSwitchButton letterboxDuringBreaks;

        public override LocalisableString Title => "Design";

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new[]
            {
                EnableCountdown = new LabelledSwitchButton
                {
                    Label = "Enable countdown",
                    Current = { Value = Beatmap.BeatmapInfo.Countdown != CountdownType.None },
                    Description = "If enabled, an \"Are you ready? 3, 2, 1, GO!\" countdown will be inserted at the beginning of the beatmap, assuming there is enough time to do so."
                },
                CountdownSettings = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        CountdownSpeed = new LabelledEnumDropdown<CountdownType>
                        {
                            Label = "Countdown speed",
                            Current = { Value = Beatmap.BeatmapInfo.Countdown != CountdownType.None ? Beatmap.BeatmapInfo.Countdown : CountdownType.Normal },
                            Items = Enum.GetValues(typeof(CountdownType)).Cast<CountdownType>().Where(type => type != CountdownType.None)
                        },
                        CountdownOffset = new LabelledNumberBox
                        {
                            Label = "Countdown offset",
                            Current = { Value = Beatmap.BeatmapInfo.CountdownOffset.ToString() },
                            Description = "If the countdown sounds off-time, use this to make it appear one or more beats early.",
                        }
                    }
                },
                Empty(),
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

            EnableCountdown.Current.BindValueChanged(_ => updateCountdownSettingsVisibility(), true);

            EnableCountdown.Current.BindValueChanged(_ => updateBeatmap());
            CountdownSpeed.Current.BindValueChanged(_ => updateBeatmap());
            CountdownOffset.OnCommit += (_, __) => onOffsetCommitted();

            widescreenSupport.Current.BindValueChanged(_ => updateBeatmap());
            epilepsyWarning.Current.BindValueChanged(_ => updateBeatmap());
            letterboxDuringBreaks.Current.BindValueChanged(_ => updateBeatmap());
        }

        private void updateCountdownSettingsVisibility() => CountdownSettings.FadeTo(EnableCountdown.Current.Value ? 1 : 0);

        private void onOffsetCommitted()
        {
            updateBeatmap();
            // update displayed text to ensure parsed value matches display (i.e. if empty string was provided).
            CountdownOffset.Current.Value = Beatmap.BeatmapInfo.CountdownOffset.ToString(CultureInfo.InvariantCulture);
        }

        private void updateBeatmap()
        {
            Beatmap.BeatmapInfo.Countdown = EnableCountdown.Current.Value ? CountdownSpeed.Current.Value : CountdownType.None;
            Beatmap.BeatmapInfo.CountdownOffset = int.TryParse(CountdownOffset.Current.Value, NumberStyles.None, CultureInfo.InvariantCulture, out int offset) ? offset : 0;

            Beatmap.BeatmapInfo.WidescreenStoryboard = widescreenSupport.Current.Value;
            Beatmap.BeatmapInfo.EpilepsyWarning = epilepsyWarning.Current.Value;
            Beatmap.BeatmapInfo.LetterboxInBreaks = letterboxDuringBreaks.Current.Value;
        }
    }
}
