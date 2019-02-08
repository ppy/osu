// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Backgrounds;
using osuTK.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Audio.Track;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces
{
    /// <summary>
    /// A circle piece which is used uniformly through osu!taiko to visualise hitobjects.
    /// <para>
    /// Note that this can actually be non-circle if the width is changed. See <see cref="ElongatedCirclePiece"/>
    /// for a usage example.
    /// </para>
    /// </summary>
    public class CirclePiece : TaikoPiece
    {
        public const float SYMBOL_SIZE = 0.45f;
        public const float SYMBOL_BORDER = 8;
        private const double pre_beat_transition_time = 80;

        /// <summary>
        /// The colour of the inner circle and outer glows.
        /// </summary>
        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                base.AccentColour = value;

                background.Colour = AccentColour;

                resetEdgeEffects();
            }
        }

        /// <summary>
        /// Whether Kiai mode effects are enabled for this circle piece.
        /// </summary>
        public override bool KiaiMode
        {
            get { return base.KiaiMode; }
            set
            {
                base.KiaiMode = value;

                resetEdgeEffects();
            }
        }

        protected override Container<Drawable> Content => content;

        private readonly Container content;

        private readonly Container background;

        public Box FlashBox;

        public CirclePiece()
        {
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
                        FlashBox = new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Blending = BlendingMode.Additive,
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

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode)
                return;

            if (beatIndex % (int)timingPoint.TimeSignature != 0)
                return;

            double duration = timingPoint.BeatLength * 2;

            background
                .FadeEdgeEffectTo(1, pre_beat_transition_time, Easing.OutQuint)
                .Then()
                .FadeEdgeEffectTo(edge_alpha_kiai, duration, Easing.OutQuint);
        }
    }
}
