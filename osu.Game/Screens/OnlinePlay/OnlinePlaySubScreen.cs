// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace osu.Game.Screens.OnlinePlay
{
    public abstract class OnlinePlaySubScreen : OsuScreen, IOnlinePlaySubScreen
    {
        public override bool DisallowExternalBeatmapRulesetChanges => false;

        public virtual string ShortTitle => Title;

        [Resolved(CanBeNull = true)]
        protected IRoomManager RoomManager { get; private set; }

        protected OnlinePlaySubScreen()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        public const float X_SHIFT = 200;

        public const double X_MOVE_DURATION = 800;

        public const double RESUME_TRANSITION_DELAY = DISAPPEAR_DURATION / 2;

        public const double APPEAR_DURATION = 800;

        public const double DISAPPEAR_DURATION = 500;

        public override void OnEntering(IScreen last)
        {
            this.FadeInFromZero(APPEAR_DURATION, Easing.OutQuint);
            this.FadeInFromZero(APPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(X_SHIFT).MoveToX(0, X_MOVE_DURATION, Easing.OutQuint);
        }

        public override bool OnExiting(IScreen next)
        {
            this.FadeOut(DISAPPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(X_SHIFT, X_MOVE_DURATION, Easing.OutQuint);

            return false;
        }

        public override void OnResuming(IScreen last)
        {
            this.Delay(RESUME_TRANSITION_DELAY).FadeIn(APPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(0, X_MOVE_DURATION, Easing.OutQuint);
        }

        public override void OnSuspending(IScreen next)
        {
            this.FadeOut(DISAPPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(-X_SHIFT, X_MOVE_DURATION, Easing.OutQuint);
        }

        public override string ToString() => Title;
    }
}
