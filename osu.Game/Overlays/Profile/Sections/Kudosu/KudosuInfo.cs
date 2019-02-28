// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections.Kudosu
{
    public class KudosuInfo : Container
    {
        private readonly Bindable<User> user = new Bindable<User>();

        public KudosuInfo(Bindable<User> user)
        {
            this.user.BindTo(user);

            CountSection total;
            CountSection avaliable;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 3;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(0f, 1f),
                Radius = 3f,
                Colour = Color4.Black.Opacity(0.2f),
            };
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f)
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new[]
                    {
                        total = new CountSection(
                            "Total Kudosu Earned",
                            "Based on how much of a contribution the user has made to beatmap moderation. See this link for more information."
                        ),
                        avaliable = new CountSection(
                            "Kudosu Avaliable",
                            "Kudosu can be traded for kudosu stars, which will help your beatmap get more attention. This is the number of kudosu you haven't traded in yet."
                        ),
                    }
                }
            };

            this.user.ValueChanged += u =>
            {
                total.Count = u.NewValue?.Kudosu.Total ?? 0;
                avaliable.Count = u.NewValue?.Kudosu.Available ?? 0;
            };
        }

        protected override bool OnClick(ClickEvent e) => true;

        private class CountSection : Container
        {
            private readonly OsuSpriteText valueText;

            public new int Count
            {
                set => valueText.Text = value.ToString();
            }

            public CountSection(string header, string description)
            {
                RelativeSizeAxes = Axes.X;
                Width = 0.5f;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Horizontal = 10, Top = 10, Bottom = 20 };
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
                                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular, italics: true)
                                },
                                valueText = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Text = "0",
                                    Font = OsuFont.GetFont(size: 40, weight: FontWeight.Regular, italics: true),
                                    UseFullGlyphHeight = false,
                                }
                            }
                        },
                        new OsuTextFlowContainer(t => t.Font = t.Font.With(size: 19))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = description
                        }
                    }
                };
            }
        }
    }
}
