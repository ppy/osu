// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneExternalLinkButton : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(ExternalLinkButton) };

        public TestSceneExternalLinkButton()
        {
            Child = new ExternalLinkButton("https://osu.ppy.sh/home")
            {
                Size = new Vector2(50)
            };
        }
    }
}
