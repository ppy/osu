// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseLoadingAnimation : TestCase
    {
        public override string Description => @"Show the Loading Animation";

        private LoadingAnimation loading;

        public override void Reset()
        {
            base.Reset();

            Add(loading = new LoadingAnimation());

            // Make it bigger
            loading.ScaleTo(5);

            AddStep(@"Toggle Visibility", loading.ToggleVisibility);
            loading.ToggleVisibility();
        }
    }
}
