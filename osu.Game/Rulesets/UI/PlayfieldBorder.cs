// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Provides a border around the playfield.
    /// </summary>
    public class PlayfieldBorder : VisibilityContainer
    {
        private const int fade_duration = 200;

        public PlayfieldBorder()
        {
            RelativeSizeAxes = Axes.Both;

            Masking = true;
            BorderColour = Color4.White;
            BorderThickness = 2;

            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                AlwaysPresent = true
            };
        }

        protected override void PopIn() => this.FadeIn(fade_duration, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(fade_duration, Easing.OutQuint);
    }
}
