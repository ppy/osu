// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Performance;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Judgements;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public class HitExplosionEntry : LifetimeEntry
    {
        /// <summary>
        /// The judgement result that triggered this explosion.
        /// </summary>
        public Judgement Judgement { get; }

        /// <summary>
        /// The hitobject which triggered this explosion.
        /// </summary>
        public CatchHitObject HitObject => (CatchHitObject)Judgement.HitObject;

        /// <summary>
        /// The accent colour of the object caught.
        /// </summary>
        public Color4 ObjectColour { get; }

        /// <summary>
        /// The position at which the object was caught.
        /// </summary>
        public float Position { get; }

        public HitExplosionEntry(double startTime, Judgement judgement, Color4 objectColour, float position)
        {
            LifetimeStart = startTime;
            Position = position;
            Judgement = judgement;
            ObjectColour = objectColour;
        }
    }
}
