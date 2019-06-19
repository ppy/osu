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

        public UpdateableFlag(Country country = null)
        {
            Country = country;
        }

        protected override Drawable CreateDrawable(Country country) => new DrawableFlag(country)
        {
            RelativeSizeAxes = Axes.Both,
        };
    }
}
