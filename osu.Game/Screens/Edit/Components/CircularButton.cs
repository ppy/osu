// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.Components
{
    public class CircularButton : OsuButton
    {
        private const float width = 125;
        private const float height = 30;

        public CircularButton()
        {
            Size = new Vector2(width, height);
        }

        protected override void Update()
        {
            base.Update();
            Content.CornerRadius = DrawHeight / 2f;
        }
    }
}
