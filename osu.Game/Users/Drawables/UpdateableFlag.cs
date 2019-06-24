// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Users.Drawables
{
    public class UpdateableFlag : ModelBackedDrawable<Country>
    {
        public Country Country
        {
            get => Model;
            set => Model = value;
        }

        protected override bool TransformImmediately { get; }

        protected override double TransformDuration { get; } = 1000;

        /// <summary>
        /// Whether to show a place holder on null country.
        /// </summary>
        public bool ShowPlaceholderOnNull = true;

        public UpdateableFlag(Country country = null, bool transformImmediately = false)
        {
            Country = country;

            if (transformImmediately) {
                TransformDuration = 0;
                TransformImmediately = true;
            }
        }

        protected override Drawable CreateDrawable(Country country)
        {
            if (country == null && !ShowPlaceholderOnNull)
                return null;

            return new DrawableFlag(country)
            {
                RelativeSizeAxes = Axes.Both,
            };
        }
    }
}
