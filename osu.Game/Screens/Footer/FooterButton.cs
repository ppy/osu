// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Footer
{
    public partial class FooterButton : ShearedButton
    {
        public FooterButton(float? width)
            : base(width)
        {
        }

        public void Appear()
        {
            FinishTransforms();

            Content.MoveToY(150f)
                   .FadeOut()
                   .MoveToY(0f, 240, Easing.OutCubic)
                   .FadeIn(240, Easing.OutCubic);
        }

        public void Disappear(bool expire)
        {
            FinishTransforms();

            Content.FadeOut(240, Easing.InOutCubic)
                   .MoveToY(150f, 240, Easing.InOutCubic);

            if (expire)
                this.Delay(Content.LatestTransformEndTime - Time.Current).Expire();
        }
    }
}
