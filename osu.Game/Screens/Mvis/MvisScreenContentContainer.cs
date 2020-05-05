using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Mvis
{
    public class MvisScreenContentContainer : Container
    {
        public Func<float> GetBottombarHeight;

        public MvisScreenContentContainer()
        {
            Name = "Content Container";
            RelativeSizeAxes = Axes.Both;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            Padding = new MarginPadding { Bottom = GetBottombarHeight?.Invoke() ?? 0 };
        }
    }
}