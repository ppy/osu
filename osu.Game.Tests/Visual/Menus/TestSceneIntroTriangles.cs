// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Screens;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Menus
{
    public class TestSceneIntroTriangles : IntroTestScene
    {
        protected override IScreen CreateScreen() => new IntroTriangles();
    }
}
