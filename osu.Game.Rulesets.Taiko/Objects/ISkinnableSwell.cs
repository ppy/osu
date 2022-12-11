// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public interface ISkinnableSwell
    {
        void OnUserInput(DrawableTaikoHitObject<Swell> swell, int numHits, SkinnableDrawable mainPiece);

        void OnHitObjectEnd(ArmedState state, SkinnableDrawable mainPiece);

        /// <summary>
        /// Applies passive transforms on HitObject start. Gets called every time DrawableTaikoHitobject
        /// changes state. This happens on creation, and when the object is completed (as in hit or missed).
        /// </summary>
        void ApplyPassiveTransforms(DrawableTaikoHitObject<Swell> swell, SkinnableDrawable mainPiece);
    }
}
