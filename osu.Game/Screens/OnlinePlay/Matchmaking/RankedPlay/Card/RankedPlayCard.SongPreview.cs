// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Threading;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card
{
    public partial class RankedPlayCard
    {
        public partial class SongPreviewContainer : Container, IBeatSyncProvider
        {
            private const double minimum_beat_length = 800;

            public readonly Bindable<bool> Enabled = new BindableBool(true);

            public readonly Bindable<bool> CardHovered = new BindableBool(true);

            public bool TrackLoaded => previewTrack?.TrackLoaded ?? false;

            public bool IsRunning => previewTrack?.IsRunning ?? false;

            protected override Container<Drawable> Content { get; }

            private readonly Bindable<bool> trackRunning = new BindableBool();

            private readonly Container overlayLayer;

            private bool shouldBePlaying => Enabled.Value && CardHovered.Value;

            [Resolved]
            private PreviewTrackManager previewTrackManager { get; set; } = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public SongPreviewContainer()
            {
                InternalChildren =
                [
                    new PulseContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children =
                        [
                            Content = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            overlayLayer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        ]
                    },
                ];
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Enabled.BindValueChanged(enabled =>
                {
                    if (!enabled.NewValue)
                    {
                        stopPreviewIfAvailable();
                        return;
                    }

                    if (shouldBePlaying)
                    {
                        startPreviewIfAvailable();
                    }
                });

                CardHovered.BindValueChanged(selected =>
                {
                    if (selected.NewValue && shouldBePlaying)
                    {
                        startPreviewIfAvailable();
                    }
                });
            }

            private PreviewTrack? previewTrack;

            public void LoadPreview(APIBeatmap beatmap)
            {
                Debug.Assert(previewTrack == null);

                LoadComponentAsync(previewTrack = previewTrackManager.Get(beatmap.BeatmapSet!), track =>
                {
                    AddInternal(track);

                    track.Looping = true;
                    track.Started += () => Schedule(() => trackRunning.Value = true);
                    track.Stopped += () => Schedule(() => trackRunning.Value = false);

                    setupBeatSyncProvider(track, beatmap);

                    var cardColours = new RankedPlayCardContent.CardColours(beatmap, colours);

                    overlayLayer.Add(new RippleVisualization(cardColours.Border)
                    {
                        TrackRunning = { BindTarget = trackRunning }
                    });

                    if (shouldBePlaying)
                        startPreviewIfAvailable();
                });
            }

            // The following weirdness is a workaround for single-threaded crashes when
            // attempting to start a track before it's fully loaded.
            //
            // See https://github.com/ppy/osu-framework/pull/6727
            //     https://github.com/ppy/osu/pull/37473
            private ScheduledDelegate? trackStartStopAction;

            private void startPreviewIfAvailable()
            {
                if (previewTrack == null)
                    return;

                trackStartStopAction?.Cancel();

                if (!previewTrack.TrackLoaded)
                {
                    trackStartStopAction = Schedule(startPreviewIfAvailable);
                    return;
                }

                previewTrack?.Start();
            }

            private void stopPreviewIfAvailable()
            {
                if (previewTrack == null)
                    return;

                trackStartStopAction?.Cancel();

                if (!previewTrack.TrackLoaded)
                {
                    trackStartStopAction = Schedule(stopPreviewIfAvailable);
                    return;
                }

                previewTrack?.Stop();
            }

            #region IBeatSyncProvider implementation

            private readonly PreviewTrackClock beatSyncClock = new PreviewTrackClock();
            private readonly ControlPointInfo controlPoints = new ControlPointInfo();

            ChannelAmplitudes IHasAmplitudes.CurrentAmplitudes => ChannelAmplitudes.Empty;
            ControlPointInfo IBeatSyncProvider.ControlPoints => controlPoints;
            IClock IBeatSyncProvider.Clock => beatSyncClock;

            private void setupBeatSyncProvider(PreviewTrack track, APIBeatmap beatmap)
            {
                beatSyncClock.Track = track;

                controlPoints.Add(0, new TimingControlPoint
                {
                    BeatLength = beatmap.BPM > 0 ? 60_000 / beatmap.BPM : TimingControlPoint.DEFAULT_BEAT_LENGTH
                });
            }

            private class PreviewTrackClock : IClock
            {
                public PreviewTrack? Track { get; set; }

                public double CurrentTime => Track?.CurrentTime ?? 0;
                public double Rate => 1;
                public bool IsRunning => Track?.IsRunning ?? false;
            }

            #endregion

            private partial class PulseContainer : BeatSyncedContainer
            {
                public const double EXPAND_DURATION = 200;

                public PulseContainer()
                {
                    MinimumBeatLength = minimum_beat_length;
                }

                protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
                {
                    if (!IsBeatSyncedWithTrack)
                        return;

                    double beatLength = TimeUntilNextBeat;

                    this.ScaleTo(1.02f, EXPAND_DURATION, Easing.In)
                        .Then()
                        .ScaleTo(1f, Math.Max(0, beatLength - EXPAND_DURATION), new CubicBezierEasingFunction(easeIn: 0.1f, easeOut: 1f));
                }
            }

            private partial class RippleVisualization : BeatSyncedContainer
            {
                [Resolved]
                private SongPreviewParticleContainer? particleContainer { get; set; }

                public readonly IBindable<bool> TrackRunning = new Bindable<bool>();

                private readonly Color4 accentColour;
                private readonly Container rippleContainer;

                public RippleVisualization(Color4 accentColour)
                {
                    this.accentColour = accentColour;

                    MinimumBeatLength = minimum_beat_length;

                    RelativeSizeAxes = Axes.Both;

                    InternalChildren =
                    [
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = CORNER_RADIUS + 1.5f,
                            Blending = BlendingParameters.Additive,
                            BorderThickness = 2f,
                            BorderColour = this.accentColour.Opacity(0.5f),
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Colour = this.accentColour.Opacity(0.1f),
                                Type = EdgeEffectType.Glow,
                                Radius = 25f,
                                Hollow = true,
                            },
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                                EdgeSmoothness = new Vector2(3),
                            },
                        },
                        rippleContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    ];
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    TrackRunning.BindValueChanged(e =>
                    {
                        if (e.NewValue)
                        {
                            rippleContainer.Clear();
                            this.FadeIn(100);
                        }
                        else
                        {
                            this.FadeOut(200);
                        }
                    }, true);
                    FinishTransforms();
                }

                protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
                {
                    if (!IsBeatSyncedWithTrack)
                        return;

                    var ripple = new Container
                    {
                        Size = DrawSize,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        CornerRadius = CORNER_RADIUS,
                        BorderThickness = 2,
                        BorderColour = accentColour,
                        Blending = BlendingParameters.Additive,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                        },
                        Alpha = 0,
                    };

                    rippleContainer.Add(ripple);

                    const float expansion = 20;

                    // The animation here is delayed to be in sync with the pulse-container's expansion animation.
                    // Since the pulse container expands with ease-out, the animation starts a tiny bit
                    // earlier, so it looks like it's maintaining the momentum of the pulse container's expansion
                    using (BeginDelayedSequence(PulseContainer.EXPAND_DURATION - 50))
                    {
                        ripple.FadeIn(200)
                              .Then()
                              .FadeOut(1000);

                        ripple.ResizeTo(DrawSize + new Vector2(expansion), 1000, Easing.OutQuart)
                              .TransformTo(nameof(CornerRadius), CORNER_RADIUS + expansion / 2, 1000, Easing.OutQuart)
                              .TransformTo(nameof(BorderThickness), 0.5f, 1000, Easing.In)
                              .Expire();

                        Schedule(() => particleContainer?.AddParticles(this, accentColour));
                    }
                }
            }
        }
    }
}
