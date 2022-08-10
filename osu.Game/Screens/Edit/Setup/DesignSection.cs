// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    internal class DesignSection : SetupSection
    {
        protected LabelledSwitchButton EnableCountdown;

        protected FillFlowContainer CountdownSettings;
        protected LabelledEnumDropdown<CountdownType> CountdownSpeed;
        protected LabelledNumberBox CountdownOffset;

        private LabelledSwitchButton widescreenSupport;
        private LabelledSwitchButton epilepsyWarning;
        private LabelledSwitchButton letterboxDuringBreaks;
        private LabelledSwitchButton samplesMatchPlaybackRate;

        public override LocalisableString Title => EditorSetupDesignStrings.Design;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new[]
            {
                EnableCountdown = new LabelledSwitchButton
                {
                    Label = EditorSetupDesignStrings.EnableCountdown,
                    Current = { Value = Beatmap.BeatmapInfo.Countdown != CountdownType.None },
                    Description = EditorSetupDesignStrings.CountdownDescription
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
                            Label = EditorSetupDesignStrings.CountdownSpeed,
                            Current = { Value = Beatmap.BeatmapInfo.Countdown != CountdownType.None ? Beatmap.BeatmapInfo.Countdown : CountdownType.Normal },
                            Items = Enum.GetValues(typeof(CountdownType)).Cast<CountdownType>().Where(type => type != CountdownType.None)
                        },
                        CountdownOffset = new LabelledNumberBox
                        {
                            Label = EditorSetupDesignStrings.CountdownOffset,
                            Current = { Value = Beatmap.BeatmapInfo.CountdownOffset.ToString() },
                            Description = EditorSetupDesignStrings.CountdownOffsetDescription,
                        }
                    }
                },
                Empty(),
                widescreenSupport = new LabelledSwitchButton
                {
                    Label = EditorSetupDesignStrings.WidescreenSupport,
                    Description = EditorSetupDesignStrings.WidescreenSupportDescription,
                    Current = { Value = Beatmap.BeatmapInfo.WidescreenStoryboard }
                },
                epilepsyWarning = new LabelledSwitchButton
                {
                    Label = EditorSetupDesignStrings.EpilepsyWarning,
                    Description = EditorSetupDesignStrings.EpilepsyWarningDescription,
                    Current = { Value = Beatmap.BeatmapInfo.EpilepsyWarning }
                },
                letterboxDuringBreaks = new LabelledSwitchButton
                {
                    Label = EditorSetupDesignStrings.LetterboxDuringBreaks,
                    Description = EditorSetupDesignStrings.LetterboxDuringBreaksDescription,
                    Current = { Value = Beatmap.BeatmapInfo.LetterboxInBreaks }
                },
                samplesMatchPlaybackRate = new LabelledSwitchButton
                {
                    Label = EditorSetupDesignStrings.SamplesMatchPlaybackRate,
                    Description = EditorSetupDesignStrings.SamplesMatchPlaybackRateDescription,
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
        }
    }
}
