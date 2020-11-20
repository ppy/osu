// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// A skinnable judgement element which supports playing an animation from the current point in time.
    /// </summary>
    public interface IAnimatableJudgement : IDrawable
    {
        /// <summary>
        /// Start the animation for this judgement from the current point in time.
        /// </summary>
        void PlayAnimation();

        /// <summary>
        /// Get proxied content which should be displayed above all hitobjects.
        /// </summary>
        [CanBeNull]
        Drawable GetAboveHitObjectsProxiedContent();
    }
}
