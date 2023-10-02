// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class DefaultReverseArrow : BeatSyncedContainer
    {
        [Resolved]
        private DrawableHitObject drawableRepeat { get; set; } = null!;

        public DefaultReverseArrow()
        {
            Divisor = 2;
            MinimumBeatLength = 200;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Size = OsuHitObject.OBJECT_DIMENSIONS;

            Child = new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Blending = BlendingParameters.Additive,
                Icon = FontAwesome.Solid.ChevronRight,
                Size = new Vector2(0.35f),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!drawableRepeat.Judged)
                Child.ScaleTo(1.3f).ScaleTo(1f, timingPoint.BeatLength, Easing.Out);
        }
    }
}
