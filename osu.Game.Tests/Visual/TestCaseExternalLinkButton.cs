using System;
using System.Collections.Generic;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    public class TestCaseExternalLinkButton : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(ExternalLinkButton) };

        public TestCaseExternalLinkButton()
        {
            Child = new ExternalLinkButton("https://osu.ppy.sh/home")
            {
                Size = new Vector2(50)
            };
        }
    }
}
