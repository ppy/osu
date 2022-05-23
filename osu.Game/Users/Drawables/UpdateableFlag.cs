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
    public class UpdateableFlag : ModelBackedDrawable<Country>
    {
        public Country Country
        {
            get => Model;
            set => Model = value;
        }

        /// <summary>
        /// Whether to show a place holder on null country.
        /// </summary>
        public bool ShowPlaceholderOnNull = true;

        /// <summary>
        /// Perform an action in addition to showing the country ranking.
        /// This should be used to perform auxiliary tasks and not as a primary action for clicking a flag (to maintain a consistent UX).
        /// </summary>
        public Action Action;

        public UpdateableFlag(Country country = null)
        {
            Country = country;
        }

        protected override Drawable CreateDrawable(Country country)
        {
            if (country == null && !ShowPlaceholderOnNull)
                return null;

            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new DrawableFlag(country)
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new HoverClickSounds(HoverSampleSet.Submit)
                }
            };
        }

        [Resolved(canBeNull: true)]
        private RankingsOverlay rankingsOverlay { get; set; }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            rankingsOverlay?.ShowCountry(Country);
            return true;
        }
    }
}
