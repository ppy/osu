// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osuTK;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class ReverseArrowPiece : BeatSyncedContainer
    {
        private readonly RepeatPoint repeatPoint;

        public ReverseArrowPiece(RepeatPoint repeatPoint)
        {
            this.repeatPoint = repeatPoint;

            Divisor = 2;
            MinimumBeatLength = 200;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingParameters.Additive;

            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Child = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.ReverseArrow), _ => new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Icon = FontAwesome.Solid.ChevronRight,
                Size = new Vector2(0.35f)
            }, confineMode: ConfineMode.NoScaling)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            if (Clock.CurrentTime < repeatPoint.StartTime)
                Child.ScaleTo(1.3f).ScaleTo(1f, Math.Min(timingPoint.BeatLength, repeatPoint.StartTime - Clock.CurrentTime), Easing.Out);
        }
    }
}
