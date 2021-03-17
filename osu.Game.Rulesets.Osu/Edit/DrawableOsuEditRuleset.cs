// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class DrawableOsuEditRuleset : DrawableOsuRuleset
    {
        public DrawableOsuEditRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new OsuEditPlayfield();

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { Size = Vector2.One };

        private class OsuEditPlayfield : OsuPlayfield
        {
            protected override GameplayCursorContainer CreateCursor() => null;

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
                if (state == ArmedState.Idle)
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
                              .FadeOutFromOne(editor_hit_object_fade_out_extension)
                              .Expire();
                        break;
                }

                // Get the existing fade out transform
                var existing = hitObject.Transforms.LastOrDefault(t => t.TargetMember == nameof(Alpha));

                if (existing == null)
                    return;

                hitObject.RemoveTransform(existing);

                using (hitObject.BeginAbsoluteSequence(existing.StartTime))
                    hitObject.FadeOut(editor_hit_object_fade_out_extension).Expire();
            }
        }
    }
}
