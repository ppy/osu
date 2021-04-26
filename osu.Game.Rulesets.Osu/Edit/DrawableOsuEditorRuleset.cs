// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class DrawableOsuEditorRuleset : DrawableOsuRuleset
    {
        public DrawableOsuEditorRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new OsuEditorPlayfield();

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { Size = Vector2.One };

        private class OsuEditorPlayfield : OsuPlayfield
        {
            private Bindable<bool> hitAnimations;

            protected override GameplayCursorContainer CreateCursor() => null;

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config)
            {
                hitAnimations = config.GetBindable<bool>(OsuSetting.EditorHitAnimations);
            }

            protected override void OnNewDrawableHitObject(DrawableHitObject d)
            {
                d.ApplyCustomUpdateState += updateState;
            }

            /// <summary>
            /// Hit objects are intentionally made to fade out at a constant slower rate than in gameplay.
            /// This allows a mapper to gain better historical context and use recent hitobjects as reference / snap points.
            /// </summary>
            private const double editor_hit_object_fade_out_extension = 700;

            private void updateState(DrawableHitObject hitObject, ArmedState state)
            {
                if (state == ArmedState.Idle || hitAnimations.Value)
                    return;

                // adjust the visuals of certain object types to make them stay on screen for longer than usual.
                switch (hitObject)
                {
                    default:
                        // there are quite a few drawable hit types we don't want to extend (spinners, ticks etc.)
                        return;

                    case DrawableSlider _:
                        // no specifics to sliders but let them fade slower below.
                        break;

                    case DrawableHitCircle circle: // also handles slider heads
                        circle.ApproachCircle
                              .FadeOutFromOne(editor_hit_object_fade_out_extension * 4)
                              .Expire();

                        circle.ApproachCircle.ScaleTo(1.1f, 300, Easing.OutQuint);

                        var circlePieceDrawable = circle.CirclePiece.Drawable;

                        // clear any explode animation logic.
                        circlePieceDrawable.ApplyTransformsAt(circle.HitStateUpdateTime, true);
                        circlePieceDrawable.ClearTransformsAfter(circle.HitStateUpdateTime, true);

                        break;
                }

                // Get the existing fade out transform
                var existing = hitObject.Transforms.LastOrDefault(t => t.TargetMember == nameof(Alpha));

                if (existing == null)
                    return;

                hitObject.RemoveTransform(existing);

                using (hitObject.BeginAbsoluteSequence(hitObject.HitStateUpdateTime))
                    hitObject.FadeOut(editor_hit_object_fade_out_extension).Expire();
            }
        }
    }
}
