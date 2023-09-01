// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// An <see cref="IHitPolicy"/> which allows hitobjects to be hit in any order.
    /// </summary>
    public class AnyOrderHitPolicy : IHitPolicy
    {
        public IHitObjectContainer HitObjectContainer { get; set; } = null!;

        public ClickAction CheckHittable(DrawableHitObject hitObject, double time, HitResult result) => ClickAction.Hit;

        public void HandleHit(DrawableHitObject hitObject)
        {
        }
    }
}
