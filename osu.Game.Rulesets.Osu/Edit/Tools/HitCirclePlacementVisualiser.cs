// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input.States;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Tools
{
    public class HitCirclePlacementVisualiser : PlacementVisualiser
    {
        public HitCirclePlacementVisualiser(IBeatmap beatmap)
        {
            AutoSizeAxes = Axes.Both;

            Alpha = 0.75f;

            // Apply defaults on a HitCircle to get the correct scale
            var circle = new HitCircle();
            circle.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty);

            InternalChild = new DrawableHitCircle(circle);
        }

        protected override bool OnClick(InputState state)
        {
            FinishPlacement(new HitCircle { Position = Position });
            return true;
        }

        private class DrawableHitCircle : Objects.Drawables.DrawableHitCircle
        {
            public DrawableHitCircle(HitCircle h)
                : base(h)
            {
                Anchor = Anchor.TopLeft;
                Origin = Anchor.TopLeft;

                Position = Vector2.Zero;
                Alpha = 1;
            }

            protected override void UpdatePreemptState()
            {
            }

            protected override void UpdateCurrentState(ArmedState state)
            {
            }
        }
    }
}
