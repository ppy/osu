// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Game.Resources.Localisation.Web;
using osu.Framework.Localisation;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Profile.Sections.Kudosu
{
    public partial class KudosuInfo : Container
    {
        private readonly Bindable<UserProfileData?> user = new Bindable<UserProfileData?>();

        public KudosuInfo(Bindable<UserProfileData?> user)
        {
            this.user.BindTo(user);
            CountSection total;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 3;
            Child = total = new CountTotal();

            this.user.ValueChanged += u => total.Count = u.NewValue?.User.Kudosu.Total ?? 0;
        }

        protected override bool OnClick(ClickEvent e) => true;

        private partial class CountTotal : CountSection
        {
            public CountTotal()
                : base(UsersStrings.ShowExtraKudosuTotal)
            {
                DescriptionText.AddText("Based on how much of a contribution the user has made to beatmap moderation. See ");
                DescriptionText.AddLink("this page", LinkAction.OpenWiki, @"Modding/Kudosu");
                DescriptionText.AddText(" for more information.");
            }
        }

        private partial class CountSection : Container
        {
            private readonly OsuSpriteText valueText;
            protected readonly LinkFlowContainer DescriptionText;

            public new int Count
            {
                set => valueText.Text = value.ToLocalisableString("N0");
            }

            protected CountSection(LocalisableString header)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Bottom = 20 };
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = header,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold)
                        },
                        valueText = new OsuSpriteText
                        {
                            Text = "0",
                            Font = OsuFont.GetFont(size: 40, weight: FontWeight.Light),
                        },
                        DescriptionText = new LinkFlowContainer(t => t.Font = t.Font.With(size: 14))
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
