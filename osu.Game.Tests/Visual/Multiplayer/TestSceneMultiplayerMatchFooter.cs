// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerMatchFooter : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create footer", () =>
            {
                Child = new PopoverContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Child = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        Height = 50,
                        Child = new MultiplayerMatchFooter()
                    }
                };
            });
        }
    }
}
