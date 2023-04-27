// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Framework.Extensions;

namespace osu.Game.Users.Drawables
{
    public partial class UpdateableCountryText : ModelBackedDrawable<CountryCode>
    {
        public CountryCode CountryCode
        {
            get => Model;
            set => Model = value;
        }
        
        public bool ShowPlaceholderOnUnknown = true;

        public Action? Action;

        protected override Drawable? CreateDrawable(CountryCode countryCode)
        {
            if (countryCode == CountryCode.Unknown && !ShowPlaceholderOnUnknown)
                return null;

            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 14f, weight: FontWeight.Regular),
                        Margin = new MarginPadding { Left = 5 },
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Text = countryCode.GetDescription(),
                    },
                    new HoverClickSounds()
                }
            };
        }

        [Resolved]
        private RankingsOverlay? rankingsOverlay { get; set; }

        public UpdateableCountryText(CountryCode countryCode = CountryCode.Unknown)
        {
            CountryCode = countryCode;
        }
        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            rankingsOverlay?.ShowCountry(CountryCode);
            return true;
        }
    }
    
}