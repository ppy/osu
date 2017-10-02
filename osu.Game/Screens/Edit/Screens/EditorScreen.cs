// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;

namespace osu.Game.Screens.Edit.Screens
{
    public class EditorScreen : Container
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public EditorScreen()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;

            InternalChild = content = new Container { RelativeSizeAxes = Axes.Both };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(0.5f).FadeTo(0)
                .Then()
                .ScaleTo(1f, 500, Easing.OutQuint).FadeTo(1f, 250, Easing.OutQuint);
        }

        public void Exit()
        {
            this.ScaleTo(1.5f, 500).FadeOut(250).Expire();
        }
    }
}
