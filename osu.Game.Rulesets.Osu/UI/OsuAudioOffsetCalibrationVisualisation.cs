// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuAudioOffsetCalibrationVisualisation : AudioOffsetCalibrationVisualisation
    {
        public OsuAudioOffsetCalibrationVisualisation(RulesetInfo ruleset)
        {
            AddInternal(new OsuInputManager(ruleset)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new TapTarget { Tapped = () => Tapped?.Invoke() },
            });
        }

        protected override void RecreateHitObject(int beat)
        {
            var hit = new HitCircle { StartTime = 0, IndexInCurrentCombo = beat };
            hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 5 });
            hit.TimePreempt = BeatLength * 0.75;
            hit.TimeFadeIn = Math.Min(150, hit.TimePreempt);

            HitObjectContainer.Child = new CalibrationHitCircle(hit)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(0.8f),
            };
        }

        private partial class CalibrationHitCircle : DrawableHitCircle
        {
            public CalibrationHitCircle(HitCircle hit)
                : base(hit)
            {
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                // Calibration only: no scoring, misses or hitsounds.
            }
        }

        private partial class TapTarget : CompositeDrawable, IKeyBindingHandler<OsuAction>
        {
            public override bool HandlePositionalInput => true;

            public required Action Tapped { get; init; }

            public TapTarget()
            {
                RelativeSizeAxes = Axes.Both;
            }

            public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
            {
                // Keep clicks on the dialog's controls out of the tap sample.
                if (!IsHovered || e.Action is not (OsuAction.LeftButton or OsuAction.RightButton))
                    return false;

                Tapped();
                return true;
            }

            public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
            {
            }
        }
    }
}
