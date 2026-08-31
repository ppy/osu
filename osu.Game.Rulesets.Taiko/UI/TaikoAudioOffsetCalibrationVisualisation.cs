// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class TaikoAudioOffsetCalibrationVisualisation : AudioOffsetCalibrationVisualisation
    {
        private DrawableHit? hit;

        public TaikoAudioOffsetCalibrationVisualisation(RulesetInfo ruleset)
        {
            AddInternal(new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.Centre,
                Position = new Vector2(65, 0),
                Size = new Vector2(100),
                Child = new TaikoHitTarget(),
            });
            AddInternal(new TaikoInputManager(ruleset)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new TapTarget { Tapped = () => Tapped?.Invoke() },
            });
        }

        protected override void RecreateHitObject(int beat)
        {
            var obj = new Hit { StartTime = 0, Type = HitType.Centre };
            obj.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            HitObjectContainer.Child = hit = new CalibrationHit(obj)
            {
                RelativeSizeAxes = Axes.None,
                Size = new Vector2(100 * TaikoHitObject.DEFAULT_SIZE),
            };
        }

        protected override void UpdateHitObject(double time)
        {
            if (hit != null)
                hit.X = 65 + (float)(-time / (BeatLength * 0.75)) * (DrawWidth - 65);
        }

        private partial class CalibrationHit : DrawableHit
        {
            public CalibrationHit(Hit hit)
                : base(hit)
            {
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                // Calibration only: no scoring, misses or hitsounds.
            }

            protected override void RecreatePieces()
            {
                base.RecreatePieces();
                RelativeSizeAxes = Axes.None;
                Size = new Vector2(100 * TaikoHitObject.DEFAULT_SIZE);
            }
        }

        private partial class TapTarget : CompositeDrawable, IKeyBindingHandler<TaikoAction>
        {
            public override bool HandlePositionalInput => true;

            public required Action Tapped { get; init; }

            public TapTarget()
            {
                RelativeSizeAxes = Axes.Both;
            }

            public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
            {
                if (!IsHovered || e.Action is not (TaikoAction.LeftCentre or TaikoAction.RightCentre or TaikoAction.LeftRim or TaikoAction.RightRim))
                    return false;

                Tapped();
                return true;
            }

            public void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
            {
            }
        }
    }
}
