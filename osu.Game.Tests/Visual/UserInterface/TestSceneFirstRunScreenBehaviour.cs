// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Screens;
using osu.Game.Overlays.FirstRunSetup;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneFirstRunScreenBehaviour : OsuManualInputManagerTestScene
    {
        public TestSceneFirstRunScreenBehaviour()
        {
            AddStep("load screen", () =>
            {
                Child = new ScreenStack(new ScreenBehaviour());
            });
        }
    }
}
