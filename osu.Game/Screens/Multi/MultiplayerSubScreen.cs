// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Multi
{
    public abstract class MultiplayerSubScreen : OsuScreen, IMultiplayerSubScreen
    {
        public override bool DisallowExternalBeatmapRulesetChanges => false;

        public virtual string ShortTitle => Title;

        [Resolved(CanBeNull = true)]
        protected IRoomManager RoomManager { get; private set; }

        protected MultiplayerSubScreen()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        public override void OnEntering(IScreen last)
        {
            this.FadeInFromZero(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            this.FadeInFromZero(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(200).MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
        }

        public override bool OnExiting(IScreen next)
        {
            this.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(200, WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);

            return false;
        }

        public override void OnResuming(IScreen last)
        {
            this.FadeIn(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
        }

        public override void OnSuspending(IScreen next)
        {
            this.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(-200, WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
        }

        public override string ToString() => Title;
    }
}
