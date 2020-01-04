// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osu.Framework.Allocation;

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
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5, 0),
                    Children = new[]
                    {
                        total = new CountTotal(),
                        avaliable = new CountAvailable()
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

        private class CountAvailable : CountSection
        {
            public CountAvailable()
                : base("Kudosu Avaliable")
            {
                DescriptionText.Text = "Kudosu can be traded for kudosu stars, which will help your beatmap get more attention. This is the number of kudosu you haven't traded in yet.";
            }
        }

        private class CountTotal : CountSection
        {
            public CountTotal()
                : base("Total Kudosu Earned")
            {
                DescriptionText.AddText("Based on how much of a contribution the user has made to beatmap moderation. See ");
                DescriptionText.AddLink("this link", "https://osu.ppy.sh/wiki/Kudosu");
                DescriptionText.AddText(" for more information.");
            }
        }

        private class CountSection : Container
        {
            private readonly OsuSpriteText valueText;
            protected readonly LinkFlowContainer DescriptionText;
            private readonly Box lineBackground;

            public new int Count
            {
                set => valueText.Text = value.ToString();
            }

            public CountSection(string header)
            {
                RelativeSizeAxes = Axes.X;
                Width = 0.5f;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Top = 10, Bottom = 20 };
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        new CircularContainer
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.X,
                            Height = 5,
                            Child = lineBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        },
                        new OsuSpriteText
                        {
                            Text = header,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold)
                        },
                        valueText = new OsuSpriteText
                        {
                            Text = "0",
                            Font = OsuFont.GetFont(size: 40, weight: FontWeight.Light),
                            UseFullGlyphHeight = false,
                        },
                        DescriptionText = new LinkFlowContainer(t => t.Font = t.Font.With(size: 14))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                lineBackground.Colour = colours.Yellow;
                DescriptionText.Colour = colours.GreySeafoamLighter;
            }
        }
    }
}
