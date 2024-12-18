// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Rulesets.UI.Scrolling
{
    /// <summary>
    /// Denotes a <see cref="IDrawableScrollingRuleset"/> which supports toggling constant algorithm for better display in the editor.
    /// </summary>
    public interface ISupportConstantAlgorithmToggle : IDrawableScrollingRuleset
    {
        public BindableBool ShowSpeedChanges { get; }
    }
}
