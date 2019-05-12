// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.Sprites;

namespace osu.Game.Tests.Visual
{
    public class TestCaseCharLookup : OsuTestCase
    {
        public TestCaseCharLookup()
        {
            AddStep("null", () => { });
            AddStep("display acharacter", () => Add(new OsuSpriteText { Text = "振込申請" }));
        }
    }
}
