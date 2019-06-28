// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;

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

        /// <summary>
        /// Whether to show a place holder on null country.
        /// </summary>
        public bool ShowPlaceholderOnNull = true;

        public UpdateableFlag(Country country = null, bool hideImmediately = false)
        {
            TransformImmediately = hideImmediately;
            Country = country;
        }

        protected override TransformSequence<Drawable> ApplyHideTransforms(Drawable drawable) => TransformImmediately ? drawable?.FadeOut() : base.ApplyHideTransforms(drawable);

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
