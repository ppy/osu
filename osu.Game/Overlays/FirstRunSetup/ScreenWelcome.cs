// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.OnlinePlay.Match.Components;

namespace osu.Game.Overlays.FirstRunSetup
{
    public class ScreenWelcome : FirstRunSetupScreen
    {
        public ScreenWelcome()
        {
            Content.Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 20))
                        {
                            Text =
                                "Welcome to the first-run setup guide!\n\nosu! is a very configurable game, and diving straight into the settings can sometimes be overwhelming. This guide will help you get the important choices out of the way to ensure a great first experience!",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                    }
                },
                new PurpleTriangleButton
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Margin = new MarginPadding(10),
                    Text = "Get started",
                    Action = () => this.Push(new ScreenSetupUIScale()),
                }
            };
        }
    }
}
