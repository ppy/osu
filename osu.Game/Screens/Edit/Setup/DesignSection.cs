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
    public partial class DesignSection : SetupSection
    {
        protected FormCheckBox EnableCountdown = null!;

        protected FillFlowContainer CountdownSettings = null!;
        protected FormEnumDropdown<CountdownType> CountdownSpeed = null!;
        protected FormNumberBox CountdownOffset = null!;

        private FormCheckBox widescreenSupport = null!;
        private FormCheckBox epilepsyWarning = null!;
        private FormCheckBox letterboxDuringBreaks = null!;
        private FormCheckBox samplesMatchPlaybackRate = null!;

        public override LocalisableString Title => EditorSetupStrings.DesignHeader;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                EnableCountdown = new FormCheckBox
                {
                    Caption = EditorSetupStrings.EnableCountdown,
                    HintText = EditorSetupStrings.CountdownDescription,
                    Current = { Value = Beatmap.Countdown != CountdownType.None },
                },
                CountdownSettings = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(5),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        CountdownSpeed = new FormEnumDropdown<CountdownType>
                        {
                            Caption = EditorSetupStrings.CountdownSpeed,
                            Current = { Value = Beatmap.Countdown != CountdownType.None ? Beatmap.Countdown : CountdownType.Normal },
                            Items = Enum.GetValues<CountdownType>().Where(type => type != CountdownType.None)
                        },
                        CountdownOffset = new FormNumberBox
                        {
                            Caption = EditorSetupStrings.CountdownOffset,
                            HintText = EditorSetupStrings.CountdownOffsetDescription,
                            Current = { Value = Beatmap.CountdownOffset.ToString() },
                            TabbableContentContainer = this,
                        }
                    }
                },
                widescreenSupport = new FormCheckBox
                {
                    Caption = EditorSetupStrings.WidescreenSupport,
                    HintText = EditorSetupStrings.WidescreenSupportDescription,
                    Current = { Value = Beatmap.WidescreenStoryboard }
                },
                epilepsyWarning = new FormCheckBox
                {
                    Caption = EditorSetupStrings.EpilepsyWarning,
                    HintText = EditorSetupStrings.EpilepsyWarningDescription,
                    Current = { Value = Beatmap.EpilepsyWarning }
                },
                letterboxDuringBreaks = new FormCheckBox
                {
                    Caption = EditorSetupStrings.LetterboxDuringBreaks,
                    HintText = EditorSetupStrings.LetterboxDuringBreaksDescription,
                    Current = { Value = Beatmap.LetterboxInBreaks }
                },
                samplesMatchPlaybackRate = new FormCheckBox
                {
                    Caption = EditorSetupStrings.SamplesMatchPlaybackRate,
                    HintText = EditorSetupStrings.SamplesMatchPlaybackRateDescription,
                    Current = { Value = Beatmap.SamplesMatchPlaybackRate }
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
            CountdownOffset.Current.Value = Beatmap.CountdownOffset.ToString(CultureInfo.InvariantCulture);
        }

        private void updateBeatmap()
        {
            Beatmap.Countdown = EnableCountdown.Current.Value ? CountdownSpeed.Current.Value : CountdownType.None;
            Beatmap.CountdownOffset = int.TryParse(CountdownOffset.Current.Value, NumberStyles.None, CultureInfo.InvariantCulture, out int offset) ? offset : 0;

            Beatmap.WidescreenStoryboard = widescreenSupport.Current.Value;
            Beatmap.EpilepsyWarning = epilepsyWarning.Current.Value;
            Beatmap.LetterboxInBreaks = letterboxDuringBreaks.Current.Value;
            Beatmap.SamplesMatchPlaybackRate = samplesMatchPlaybackRate.Current.Value;

            Beatmap.SaveState();
        }
    }
}
