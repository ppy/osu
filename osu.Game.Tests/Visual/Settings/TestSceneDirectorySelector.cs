// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Tests.Visual.Settings
{
    public class TestSceneDirectorySelector : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new DirectorySelector { RelativeSizeAxes = Axes.Both });
        }
    }
}
