// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOsuDropdown : ThemeComparisonTestScene
    {
        protected override Drawable CreateContent() =>
            new OsuEnumDropdown<BeatmapOnlineStatus>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 150
            };
    }
}
