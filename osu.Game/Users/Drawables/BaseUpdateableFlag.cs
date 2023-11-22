// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Users.Drawables
{
    public partial class BaseUpdateableFlag : ModelBackedDrawable<CountryCode>
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

        public BaseUpdateableFlag(CountryCode countryCode = CountryCode.Unknown)
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
                }
            };
        }
    }
}
