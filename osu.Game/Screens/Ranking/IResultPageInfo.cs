// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Ranking
{
    public interface IResultPageInfo
    {
        IconUsage Icon { get; }

        string Name { get; }

        ResultsPage CreatePage();
    }
}
