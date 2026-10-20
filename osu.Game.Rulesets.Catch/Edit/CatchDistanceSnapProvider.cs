// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Components.TernaryButtons;

namespace osu.Game.Rulesets.Catch.Edit
{
    public partial class CatchDistanceSnapProvider : ComposerDistanceSnapProvider
    {
        public override double ReadCurrentDistanceSnap(HitObject before, HitObject after)
        {
            // osu!catch's distance snap implementation is limited, in that a custom spacing cannot be specified.
            // Therefore this functionality is not currently used.
            //
            // The implementation below is probably correct but should be checked if/when exposed via controls.

            float expectedDistance = DurationToDistance(after.StartTime - before.GetEndTime(), before.StartTime);

            float previousEndX = (before as JuiceStream)?.EndX ?? ((CatchHitObject)before).EffectiveX;
            float actualDistance = Math.Abs(previousEndX - ((CatchHitObject)after).EffectiveX);

            return actualDistance / expectedDistance;
        }

        public override IEnumerable<DrawableTernaryButton> CreateTernaryButtons() => new[]
        {
            new DrawableTernaryButton<CatchAction>
            {
                Current = DistanceSnapToggle,
                Description = "Distance Snap",
                CreateIcon = () => new SpriteIcon { Icon = OsuIcon.EditorDistanceSnap },
                Action = CatchAction.EditorToggleDistanceSnap,
                Hotkey = new Hotkey(CatchRuleset.SHORT_NAME, Ruleset.EDITOR_VARIANT, (int)CatchAction.EditorToggleDistanceSnap),
            }
        };
    }
}
