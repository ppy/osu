// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Multi
{
    public abstract class MultiplayerSubScreen : OsuScreen, IMultiplayerSubScreen
    {
        protected virtual Drawable TransitionContent => this;

        public virtual string ShortTitle => Title;

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            this.FadeInFromZero(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            TransitionContent.FadeInFromZero(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            TransitionContent.MoveToX(200).MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
        }

        public override bool OnExiting(IScreen next)
        {
            this.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
            TransitionContent.MoveToX(200, WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);

            return base.OnExiting(next);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            this.FadeIn(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            TransitionContent.MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            this.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
            TransitionContent.MoveToX(-200, WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
        }
    }
}
