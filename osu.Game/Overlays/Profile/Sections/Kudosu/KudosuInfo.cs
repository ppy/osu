// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections.Kudosu
{
    public class KudosuInfo : Container
    {
        private const int content_text_size = 19;

        protected readonly Bindable<User> User = new Bindable<User>();

        public KudosuInfo(Bindable<User> user)
        {
            User.BindTo(user);

            SubSection total;
            SubSection avaliable;

            RelativeSizeAxes = Axes.X;
            Height = 130;
            Masking = true;
            CornerRadius = 3;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(0f, 1f),
                Radius = 2.5f,
                Colour = Color4.Black.Opacity(0.2f),
            };
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f)
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            total = new SubSection("Total Kudosu Earned"),
                            avaliable = new SubSection("Kudosu Avaliable"),
                        }
                    }
                }
            };

            total.TextFlow.Text = "Based on how much of a contribution the user has made to " +
                "beatmap moderation. See this link for more information.";

            avaliable.TextFlow.Text = "Kudosu can be traded for kudosu stars, which will help your beatmap get " +
                "more attention. This is the number of kudosu you haven't traded in yet.";
            
            User.ValueChanged += newUser =>
            {
                total.KudosuValue = newUser == null ? 0 : newUser.Kudosu.Total;
                avaliable.KudosuValue = newUser == null ? 0 : newUser.Kudosu.Available;
            };
        }

        protected override bool OnClick(InputState state) => true;

        private class SubSection : Container
        {
            public readonly TextFlowContainer TextFlow;

            private readonly OsuSpriteText valueText;

            private int kudosuValue;
            public int KudosuValue
            {
                get { return kudosuValue; }
                set
                {
                    if (kudosuValue == value)
                        return;
                    kudosuValue = value;

                    valueText.Text = kudosuValue.ToString();
                }
            }

            public SubSection(string header)
            {
                RelativeSizeAxes = Axes.Both;
                Padding = new MarginPadding { Horizontal = 10, Top = 10 };
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5, 0),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Text = header + ":",
                                    TextSize = 20,
                                    Font = @"Exo2.0-RegularItalic",
                                },
                                valueText = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Text = "0",
                                    TextSize = 40,
                                    UseFullGlyphHeight = false,
                                    Font = @"Exo2.0-RegularItalic"
                                }
                            }
                        },
                        TextFlow = new TextFlowContainer(t => { t.TextSize = 19; })
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }
                    }
                };
            }
        }
    }
}
