// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneExternalLinkButton : OsuTestScene
    {
        public TestSceneExternalLinkButton()
        {
            Child = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new ExternalLinkButton("https://osu.ppy.sh/home")
                {
                    Size = new Vector2(50),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }
    }
}
