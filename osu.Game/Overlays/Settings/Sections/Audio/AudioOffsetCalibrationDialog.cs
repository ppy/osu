// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Mixing;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public partial class AudioOffsetCalibrationDialog : PopupDialog
    {
        /// <summary>
        /// An unbound copy of the saved offset. Only Save writes back to the configuration.
        /// </summary>
        public Bindable<double> PreviewOffset { get; }

        public bool IsPlaying => track?.IsRunning == true;

        public readonly BindableBool SlowerTempo = new BindableBool(true);
        public readonly Bindable<RulesetInfo> PreviewRuleset = new Bindable<RulesetInfo>();
        public readonly AudioOffsetTapEstimator TapEstimator = new AudioOffsetTapEstimator();

        private Container displayContainer = null!;
        private AudioOffsetCalibrationBeatDisplay display = null!;
        private OsuTextFlowContainer tapStatus = null!;
        private FormButton.Button applyEstimate = null!;
        private Track? normalTrack;
        private Track? slowTrack;

        private double beatLength => SlowerTempo.Value ? AudioOffsetCalibrationTrackStore.SLOW_BEAT_LENGTH : AudioOffsetCalibrationTrackStore.BEAT_LENGTH;

        private readonly BindableDouble muteMusic = new BindableDouble();

        private AudioOffsetCalibrationTrackStore? resources;
        private AudioMixer? mixer;
        private ITrackStore? trackStore;
        private Track? track;
        private ScheduledDelegate? pendingStart;
        private bool musicMuted;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        public AudioOffsetCalibrationDialog(Bindable<double> savedOffset)
        {
            PreviewOffset = savedOffset.GetUnboundCopy();

            HeaderText = AudioSettingsStrings.OffsetWizard;
            BodyText = AudioSettingsStrings.CalibrationInstructions;
            Icon = FontAwesome.Solid.Music;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = WebCommonStrings.ButtonsSave,
                    Action = () =>
                    {
                        if (!savedOffset.Disabled)
                            savedOffset.Value = PreviewOffset.Value;
                    },
                },
                new PopupDialogCancelButton
                {
                    Text = WebCommonStrings.ButtonsCancel,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets, IBindable<RulesetInfo> currentRuleset)
        {
            resources = new AudioOffsetCalibrationTrackStore();
            // DialogOverlay low-pass filters the background track mixer. The reference click must stay unfiltered.
            mixer = audio.CreateAudioMixer("Audio offset calibration");
            trackStore = audio.GetTrackStore(resources, mixer);
            normalTrack = trackStore.Get(AudioOffsetCalibrationTrackStore.TRACK_NAME);
            slowTrack = trackStore.Get(AudioOffsetCalibrationTrackStore.SLOW_TRACK_NAME);
            normalTrack.Looping = slowTrack.Looping = true;
            track = SlowerTempo.Value ? slowTrack : normalTrack;

            var availableRulesets = rulesets.AvailableRulesets.Where(r => r.ShortName is "osu" or "taiko").ToArray();
            PreviewRuleset.Value = availableRulesets.FirstOrDefault(r => r.Equals(currentRuleset.Value)) ?? availableRulesets.First();

            MainContent.Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Padding = new MarginPadding { Horizontal = 25 },
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new FormDropdown<RulesetInfo>
                            {
                                Caption = AudioSettingsStrings.CalibrationGameplay,
                                Items = availableRulesets,
                                Current = PreviewRuleset,
                                Width = 0.6f,
                            },
                            new TempoDropdown
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Caption = AudioSettingsStrings.CalibrationTempo,
                                Items = new[] { true, false },
                                Current = SlowerTempo,
                                Width = 0.38f,
                            },
                        },
                    },
                    displayContainer = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                    new FormSliderBar<double>
                    {
                        Caption = AudioSettingsStrings.AudioOffset,
                        Current = PreviewOffset,
                        KeyboardStep = 1,
                        LabelFormat = v => $"{v:N0} ms",
                        TooltipFormat = BeatmapOffsetControl.GetOffsetExplanatoryText,
                        PlaySamplesOnAdjust = false,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            tapStatus = new OsuTextFlowContainer(t => t.Font = t.Font.With(size: 16))
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Width = 0.6f,
                            },
                            applyEstimate = new FormButton.Button
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.X,
                                Width = 0.38f,
                                Text = AudioSettingsStrings.CalibrationUseEstimate,
                                Action = ApplyTapEstimate,
                            },
                        },
                    },
                },
            };

            createDisplay();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SlowerTempo.BindValueChanged(_ =>
            {
                track?.Stop();
                track = SlowerTempo.Value ? slowTrack : normalTrack;
                createDisplay();
                if (musicMuted && State.Value == Visibility.Visible)
                    track?.Restart();
            });
            PreviewRuleset.BindValueChanged(_ => createDisplay());
            PreviewOffset.BindValueChanged(_ => resetTaps());

            // Do not play over the dialog opening sound or start a dialog dismissed during loading.
            if (State.Value == Visibility.Visible)
                pendingStart = Scheduler.AddDelayed(startPlayback, ENTER_DURATION);
        }

        private void createDisplay()
        {
            displayContainer.Child = display = new AudioOffsetCalibrationBeatDisplay(track!, PreviewOffset, beatLength,
                PreviewRuleset.Value.CreateInstance())
            {
                Tapped = recordTap,
            };
            resetTaps();
        }

        private void recordTap()
        {
            if (State.Value != Visibility.Visible || !IsPlaying)
                return;

            if (!TapEstimator.AddTap(display.ReferenceTime, Time.Current, beatLength, out double error))
                return;

            display.AddTap(error);
            updateTapStatus();
        }

        public void ApplyTapEstimate()
        {
            if (!TapEstimator.HasEstimate || PreviewOffset.Disabled)
                return;

            // Positive tap error means the visual cue occurred too early.
            PreviewOffset.Value = getEstimatedOffset();
            resetTaps();
        }

        private void resetTaps()
        {
            TapEstimator.Reset();
            display.ClearTaps();
            updateTapStatus();
        }

        private void updateTapStatus()
        {
            applyEstimate.Enabled.Value = TapEstimator.HasEstimate && !PreviewOffset.Disabled;
            tapStatus.Text = TapEstimator.HasEstimate
                ? AudioSettingsStrings.CalibrationTapEstimate(getEstimatedOffset())
                : AudioSettingsStrings.CalibrationTapProgress(TapEstimator.Count, AudioOffsetTapEstimator.REQUIRED_TAPS);
        }

        private double getEstimatedOffset()
        {
            // Show the same bounded, rounded value that the slider will accept.
            var estimate = PreviewOffset.GetUnboundCopy();
            estimate.Disabled = false;
            estimate.Value -= TapEstimator.MedianError;
            return estimate.Value;
        }

        private partial class TempoDropdown : FormDropdown<bool>
        {
            protected override LocalisableString GenerateItemText(bool slow) => LocalisableString.Interpolate($"{(slow ? 60 : 120)} {SongSelectStrings.BPM}");
        }

        private void startPlayback()
        {
            if (State.Value != Visibility.Visible)
                return;

            beatmaps.BeatmapTrackStore.AddAdjustment(AdjustableProperty.Volume, muteMusic);
            musicMuted = true;
            track?.Restart();
        }

        private void stopPlayback()
        {
            pendingStart?.Cancel();
            pendingStart = null;
            track?.Stop();

            if (musicMuted)
            {
                beatmaps.BeatmapTrackStore.RemoveAdjustment(AdjustableProperty.Volume, muteMusic);
                musicMuted = false;
            }
        }

        protected override void PopOut()
        {
            stopPlayback();
            base.PopOut();
        }

        protected override void Dispose(bool isDisposing)
        {
            stopPlayback();
            normalTrack?.Dispose();
            slowTrack?.Dispose();
            trackStore?.Dispose();
            mixer?.Dispose();
            resources?.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
