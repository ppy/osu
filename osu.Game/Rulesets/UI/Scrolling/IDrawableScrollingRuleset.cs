// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;

namespace osu.Game.Rulesets.UI.Scrolling
{
    /// <summary>
    /// An interface for scrolling-based <see cref="DrawableRuleset{TObject}"/>s.
    /// </summary>
    public interface IDrawableScrollingRuleset
    {
        ScrollVisualisationMethod VisualisationMethod { get; }

        IScrollingInfo ScrollingInfo { get; }
    }
}
