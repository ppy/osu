// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    /// <summary>
    /// Provides a visual state of a <see cref="PalpableCatchHitObject"/>.
    /// </summary>
    public interface IHasCatchObjectState
    {
        PalpableCatchHitObject HitObject { get; }
        Bindable<Color4> AccentColour { get; }
        Bindable<bool> HyperDash { get; }
        Bindable<int> IndexInBeatmap { get; }
        double DisplayStartTime { get; }
        Vector2 DisplayPosition { get; }
        Vector2 DisplaySize { get; }
        float DisplayRotation { get; }

        void RestoreState(CatchObjectState state);
    }

    public static class HasCatchObjectStateExtensions
    {
        public static CatchObjectState SaveState(this IHasCatchObjectState target) => new CatchObjectState(
            target.HitObject,
            target.AccentColour.Value,
            target.HyperDash.Value,
            target.IndexInBeatmap.Value,
            target.DisplayPosition,
            target.DisplaySize,
            target.DisplayRotation);
    }

    public readonly record struct CatchObjectState(
        PalpableCatchHitObject HitObject,
        Color4 AccentColour,
        bool HyperDash,
        int IndexInBeatmap,
        Vector2 DisplayPosition,
        Vector2 DisplaySize,
        float DisplayRotation);
}
