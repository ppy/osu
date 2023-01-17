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
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit.Setup
{
    internal partial class DesignSection : SetupSection
    {
        protected LabelledSwitchButton EnableCountdown = null!;

        protected FillFlowContainer CountdownSettings = null!;
        protected LabelledEnumDropdown<CountdownType> CountdownSpeed = null!;
        protected LabelledNumberBox CountdownOffset = null!;

        private LabelledSwitchButton widescreenSupport = null!;
        private LabelledSwitchButton epilepsyWarning = null!;
        private LabelledSwitchButton letterboxDuringBreaks = null!;
        private LabelledSwitchButton samplesMatchPlaybackRate = null!;

        public override LocalisableString Title => EditorSetupStrings.DesignHeader;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new[]
            {
                EnableCountdown = new LabelledSwitchButton
                {
                    Label = EditorSetupStrings.EnableCountdown,
                    Current = { Value = Beatmap.BeatmapInfo.Countdown != CountdownType.None },
                    Description = EditorSetupStrings.CountdownDescription
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
                            Label = EditorSetupStrings.CountdownSpeed,
                            Current = { Value = Beatmap.BeatmapInfo.Countdown != CountdownType.None ? Beatmap.BeatmapInfo.Countdown : CountdownType.Normal },
                            Items = Enum.GetValues<CountdownType>().Where(type => type != CountdownType.None)
                        },
                        CountdownOffset = new LabelledNumberBox
                        {
                            Label = EditorSetupStrings.CountdownOffset,
                            Current = { Value = Beatmap.BeatmapInfo.CountdownOffset.ToString() },
                            Description = EditorSetupStrings.CountdownOffsetDescription,
                        }
                    }
                },
                Empty(),
                widescreenSupport = new LabelledSwitchButton
                {
                    Label = EditorSetupStrings.WidescreenSupport,
                    Description = EditorSetupStrings.WidescreenSupportDescription,
                    Current = { Value = Beatmap.BeatmapInfo.WidescreenStoryboard }
                },
                epilepsyWarning = new LabelledSwitchButton
                {
                    Label = EditorSetupStrings.EpilepsyWarning,
                    Description = EditorSetupStrings.EpilepsyWarningDescription,
                    Current = { Value = Beatmap.BeatmapInfo.EpilepsyWarning }
                },
                letterboxDuringBreaks = new LabelledSwitchButton
                {
                    Label = EditorSetupStrings.LetterboxDuringBreaks,
                    Description = EditorSetupStrings.LetterboxDuringBreaksDescription,
                    Current = { Value = Beatmap.BeatmapInfo.LetterboxInBreaks }
                },
                samplesMatchPlaybackRate = new LabelledSwitchButton
                {
                    Label = EditorSetupStrings.SamplesMatchPlaybackRate,
                    Description = EditorSetupStrings.SamplesMatchPlaybackRateDescription,
                    Current = { Value = Beatmap.BeatmapInfo.SamplesMatchPlaybackRate }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            EnableCountdown.Current.BindValueChanged(_ => updateCountdownSettingsVisibility(), true);

            EnableCountdown.Current.BindValueChanged(_ => updateBeatmap());
            CountdownSpeed.Current.BindValueChanged(_ => updateBeatmap());
            CountdownOffset.OnCommit += (_, _) => onOffsetCommitted();

            widescreenSupport.Current.BindValueChanged(_ => updateBeatmap());
            epilepsyWarning.Current.BindValueChanged(_ => updateBeatmap());
            letterboxDuringBreaks.Current.BindValueChanged(_ => updateBeatmap());
            samplesMatchPlaybackRate.Current.BindValueChanged(_ => updateBeatmap());
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
            Beatmap.BeatmapInfo.SamplesMatchPlaybackRate = samplesMatchPlaybackRate.Current.Value;

            Beatmap.SaveState();
        }
    }
}
