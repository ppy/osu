// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play
{
    public partial class ScrollingMessage : CompositeDrawable
    {
        private readonly Drawable messageContent;

        public ScrollingMessage(Drawable messageContent)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = this.messageContent = messageContent;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.FadeInFromZero(2000, Easing.OutQuint);
            resetMessagePosition();
        }

        protected override void Update()
        {
            base.Update();

            if (messageContent.X + messageContent.DrawWidth > 0)
                messageContent.X -= (float)Clock.ElapsedFrameTime * 0.05f;
            else
                resetMessagePosition();
        }

        private void resetMessagePosition()
        {
            messageContent.X = DrawWidth + 10;
        }
    }
}
