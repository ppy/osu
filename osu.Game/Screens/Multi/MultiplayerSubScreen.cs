// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Multi
{
    public abstract class MultiplayerSubScreen : OsuScreen, IMultiplayerSubScreen
    {
        protected virtual Drawable TransitionContent => Content;

        public virtual string ShortTitle => Title;

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Content.FadeInFromZero(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            TransitionContent.FadeInFromZero(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            TransitionContent.MoveToX(200).MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
        }

        protected override bool OnExiting(Screen next)
        {
            Content.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
            TransitionContent.MoveToX(200, WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);

            return base.OnExiting(next);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            Content.FadeIn(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            TransitionContent.MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);

            Content.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
            TransitionContent.MoveToX(-200, WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
        }
    }
}
