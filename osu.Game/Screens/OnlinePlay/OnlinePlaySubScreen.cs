// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace osu.Game.Screens.OnlinePlay
{
    public abstract partial class OnlinePlaySubScreen : OsuScreen, IOnlinePlaySubScreen
    {
        public override bool DisallowExternalBeatmapRulesetChanges => false;

        public virtual string ShortTitle => Title;

        protected sealed override bool PlayExitSound => false;

        [Resolved]
        protected IRoomManager? RoomManager { get; private set; }

        protected OnlinePlaySubScreen()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        public const double APPEAR_DURATION = 800;

        public const double DISAPPEAR_DURATION = 500;

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this.FadeInFromZero(APPEAR_DURATION, Easing.OutQuint);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            base.OnExiting(e);
            this.FadeOut(DISAPPEAR_DURATION, Easing.OutQuint);
            return false;
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            this.FadeIn(APPEAR_DURATION, Easing.OutQuint);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);
            this.FadeOut(DISAPPEAR_DURATION, Easing.OutQuint);
        }

        public override string ToString() => Title;
    }
}
