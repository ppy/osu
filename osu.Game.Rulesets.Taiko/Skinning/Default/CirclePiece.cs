// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Default
{
    /// <summary>
    /// A circle piece which is used uniformly through osu!taiko to visualise hitobjects.
    /// <para>
    /// Note that this can actually be non-circle if the width is changed. See <see cref="ElongatedCirclePiece"/>
    /// for a usage example.
    /// </para>
    /// </summary>
    public abstract partial class CirclePiece : BeatSyncedContainer, IHasAccentColour
    {
        public const float SYMBOL_SIZE = TaikoHitObject.DEFAULT_SIZE;
        public const float SYMBOL_BORDER = 8;

        private const double pre_beat_transition_time = 80;

        private const float kiai_flash_opacity = 0.15f;

        [Resolved]
        private DrawableHitObject drawableHitObject { get; set; } = null!;

        private Color4 accentColour;

        /// <summary>
        /// The colour of the inner circle and outer glows.
        /// </summary>
        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;

                background.Colour = AccentColour;

                resetEdgeEffects();
            }
        }

        private bool kiaiMode;

        /// <summary>
        /// Whether Kiai mode effects are enabled for this circle piece.
        /// </summary>
        public bool KiaiMode
        {
            get => kiaiMode;
            set
            {
                kiaiMode = value;

                resetEdgeEffects();
            }
        }

        protected override Container<Drawable> Content => content;

        private readonly Container content;

        private readonly Container background;

        private readonly Box flashBox;

        protected CirclePiece()
        {
            RelativeSizeAxes = Axes.Both;

            EarlyActivationMilliseconds = pre_beat_transition_time;

            AddRangeInternal(new Drawable[]
            {
                background = new CircularContainer
                {
                    Name = "Background",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Triangles
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            ColourLight = Color4.White,
                            ColourDark = Color4.White.Darken(0.1f)
                        }
                    }
                },
                new CircularContainer
                {
                    Name = "Ring",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    BorderThickness = 8,
                    BorderColour = Color4.White,
                    Masking = true,
                    Children = new[]
                    {
                        flashBox = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Blending = BlendingParameters.Additive,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
                content = new Container
                {
                    Name = "Content",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            drawableHitObject.ApplyCustomUpdateState += updateStateTransforms;
            updateStateTransforms(drawableHitObject, drawableHitObject.State.Value);
        }

        private void updateStateTransforms(DrawableHitObject h, ArmedState state)
        {
            if (h.HitObject is not Hit)
                return;

            switch (state)
            {
                case ArmedState.Hit:
                    using (BeginAbsoluteSequence(h.HitStateUpdateTime))
                        flashBox.FadeTo(0.9f).FadeOut(300);
                    break;
            }
        }

        private const float edge_alpha_kiai = 0.5f;

        private void resetEdgeEffects()
        {
            background.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = AccentColour.Opacity(KiaiMode ? edge_alpha_kiai : 1f),
                Radius = KiaiMode ? 32 : 8
            };
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode)
                return;

            if (drawableHitObject.State.Value == ArmedState.Idle)
            {
                flashBox
                    .FadeTo(kiai_flash_opacity)
                    .Then()
                    .FadeOut(timingPoint.BeatLength * 0.75, Easing.OutSine);
            }

            if (beatIndex % timingPoint.TimeSignature.Numerator != 0)
                return;

            double duration = timingPoint.BeatLength * 2;

            background
                .FadeEdgeEffectTo(1, pre_beat_transition_time, Easing.OutQuint)
                .Then()
                .FadeEdgeEffectTo(edge_alpha_kiai, duration, Easing.OutQuint);
        }
    }
}
