// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Models;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament
{
    public class TournamentGame : TournamentGameBase
    {
        public static ColourInfo GetTeamColour(TeamColour teamColour) => teamColour == TeamColour.Red ? COLOUR_RED : COLOUR_BLUE;

        public static readonly Color4 COLOUR_RED = Color4Extensions.FromHex("#AA1414");
        public static readonly Color4 COLOUR_BLUE = Color4Extensions.FromHex("#1462AA");

        public static readonly Color4 ELEMENT_BACKGROUND_COLOUR = Color4Extensions.FromHex("#fff");
        public static readonly Color4 ELEMENT_FOREGROUND_COLOUR = Color4Extensions.FromHex("#000");

        public static readonly Color4 TEXT_COLOUR = Color4Extensions.FromHex("#fff");
        private Drawable heightWarning;
        private Bindable<Size> windowSize;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            windowSize = frameworkConfig.GetBindable<Size>(FrameworkSetting.WindowedSize);
            windowSize.BindValueChanged(size => ScheduleAfterChildren(() =>
            {
                var minWidth = (int)(size.NewValue.Height / 768f * TournamentSceneManager.REQUIRED_WIDTH) - 1;

                heightWarning.Alpha = size.NewValue.Width < minWidth ? 1 : 0;
            }), true);

            AddRange(new[]
            {
                new Container
                {
                    CornerRadius = 10,
                    Depth = float.MinValue,
                    Position = new Vector2(5),
                    Masking = true,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = OsuColour.Gray(0.2f),
                            RelativeSizeAxes = Axes.Both,
                        },
                        new TourneyButton
                        {
                            Text = "Save Changes",
                            Width = 140,
                            Height = 50,
                            Padding = new MarginPadding
                            {
                                Top = 10,
                                Left = 10,
                            },
                            Margin = new MarginPadding
                            {
                                Right = 10,
                                Bottom = 10,
                            },
                            Action = SaveChanges,
                        },
                    }
                },
                heightWarning = new Container
                {
                    Masking = true,
                    CornerRadius = 5,
                    Depth = float.MinValue,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Red,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new TournamentSpriteText
                        {
                            Text = "Please make the window wider",
                            Font = OsuFont.Torus.With(weight: FontWeight.Bold),
                            Colour = Color4.White,
                            Padding = new MarginPadding(20)
                        }
                    }
                },
                new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new TournamentSceneManager()
                }
            });
        }
    }
}
