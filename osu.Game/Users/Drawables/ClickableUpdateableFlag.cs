// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Users.Drawables
{
    public partial class ClickableUpdateableFlag : ModelBackedDrawable<CountryCode>
    {
        public CountryCode CountryCode
        {
            get => Model;
            set => Model = value;
        }

        /// <summary>
        /// Whether to show a place holder on unknown country.
        /// </summary>
        public bool ShowPlaceholderOnUnknown = true;

        /// <summary>
        /// Perform an action in addition to showing the country ranking.
        /// This should be used to perform auxiliary tasks and not as a primary action for clicking a flag (to maintain a consistent UX).
        /// </summary>
        public Action? Action;

        public ClickableUpdateableFlag(CountryCode countryCode = CountryCode.Unknown)
        {
            CountryCode = countryCode;
        }

        protected override Drawable? CreateDrawable(CountryCode countryCode)
        {
            if (countryCode == CountryCode.Unknown && !ShowPlaceholderOnUnknown)
                return null;

            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new DrawableFlag(countryCode)
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new HoverClickSounds()
                }
            };
        }

        [Resolved]
        private RankingsOverlay? rankingsOverlay { get; set; }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            rankingsOverlay?.ShowCountry(CountryCode);
            return true;
        }
    }
}
