// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;
using osu.Framework.Allocation;
using osuTK.Graphics;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsScopeSelector : GradientLineTabControl<RankingsScope>
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AccentColour = LineColour = Color4.Black;
        }
    }

    public enum RankingsScope
    {
        Performance,
        Spotlights,
        Score,
        Country
    }
}
