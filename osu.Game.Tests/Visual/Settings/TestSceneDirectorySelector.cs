// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tests.Visual.UserInterface;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneDirectorySelector : ThemeComparisonTestScene
    {
        protected override Drawable CreateContent() => new OsuDirectorySelector
        {
            RelativeSizeAxes = Axes.Both
        };
    }
}
