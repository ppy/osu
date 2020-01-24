// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Menu;
using osuTK;

namespace osu.Game.Overlays.AccountCreation
{
    public class ScreenWelcome : AccountCreationScreen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Padding = new MarginPadding(20),
                Spacing = new Vector2(0, 5),
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 150,
                        Child = new OsuLogo
                        {
                            Scale = new Vector2(0.1f),
                            Anchor = Anchor.Centre,
                            Triangles = false,
                        },
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = OsuFont.GetFont(size: 24, weight: FontWeight.Light),
                        Text = "New Player Registration",
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = OsuFont.GetFont(size: 12),
                        Text = "let's get you started",
                    },
                    new SettingsButton
                    {
                        Text = "Let's create an account!",
                        Margin = new MarginPadding { Vertical = 120 },
                        Action = () => this.Push(new ScreenWarning())
                    }
                }
            };
        }
    }
}
