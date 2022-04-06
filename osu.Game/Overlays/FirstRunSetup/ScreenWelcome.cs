// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osuTK;

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
                        new OsuTextFlowContainer
                        {
                            Text = "Welcome to the first-run setup guide!\n\nThis will help you get osu! setup in a way that suits you.",
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
                    Action = () => this.Push(new ScreenWelcome()),
                }
            };
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            Overlay.MoveDisplayTo(new Vector2(RNG.NextSingle(), RNG.NextSingle()));
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            Overlay.MoveDisplayTo(new Vector2(RNG.NextSingle(), RNG.NextSingle()));
        }
    }
}
