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
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Game.Resources.Localisation.Web;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Profile.Sections.Kudosu
{
    public class KudosuInfo : Container
    {
        private readonly Bindable<APIUser> user = new Bindable<APIUser>();

        public KudosuInfo(Bindable<APIUser> user)
        {
            this.user.BindTo(user);
            CountSection total;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 3;
            Child = total = new CountTotal();

            this.user.ValueChanged += u => total.Count = u.NewValue?.Kudosu.Total ?? 0;
        }

        protected override bool OnClick(ClickEvent e) => true;

        private class CountTotal : CountSection
        {
            public CountTotal()
                : base(UsersStrings.ShowExtraKudosuTotal)
            {
                DescriptionText.AddText("Based on how much of a contribution the user has made to beatmap moderation. See ");
                DescriptionText.AddLink("this page", "https://osu.ppy.sh/wiki/Kudosu");
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
                set => valueText.Text = value.ToLocalisableString("N0");
            }

            public CountSection(LocalisableString header)
            {
                RelativeSizeAxes = Axes.X;
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
                            Height = 2,
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
            private void load(OverlayColourProvider colourProvider)
            {
                lineBackground.Colour = colourProvider.Highlight1;
            }
        }
    }
}
