// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public interface ISkinnableSwell
    {
        void AnimateSwellProgress(DrawableTaikoHitObject<Swell> swell, int numHits, SkinnableDrawable mainPiece);

        void AnimateSwellCompletion(ArmedState state, SkinnableDrawable mainPiece);

        void AnimateSwellStart(DrawableTaikoHitObject<Swell> swell, SkinnableDrawable mainPiece);
    }
}
