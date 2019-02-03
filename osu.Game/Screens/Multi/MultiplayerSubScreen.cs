// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Screens.Multi
{
    public abstract class MultiplayerSubScreen : CompositeDrawable, IMultiplayerSubScreen, IKeyBindingHandler<GlobalAction>
    {
        public virtual bool AllowBeatmapRulesetChange => true;
        public bool AllowExternalScreenChange => true;
        public bool CursorVisible => true;

        public bool HideOverlaysOnEnter => false;
        public OverlayActivation InitialOverlayActivationMode => OverlayActivation.All;

        public float BackgroundParallaxAmount => 1;

        public bool ValidForResume { get; set; } = true;
        public bool ValidForPush { get; set; } = true;

        public override bool RemoveWhenNotAlive => false;

        public abstract string Title { get; }
        public virtual string ShortTitle => Title;

        [Resolved]
        protected IBindableBeatmap Beatmap { get; private set; }

        [Resolved(CanBeNull = true)]
        protected OsuGame Game { get; private set; }

        [Resolved(CanBeNull = true)]
        protected IRoomManager Manager { get; private set; }

        protected MultiplayerSubScreen()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        public virtual void OnEntering(IScreen last)
        {
            this.FadeInFromZero(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            this.FadeInFromZero(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(200).MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
        }

        public virtual bool OnExiting(IScreen next)
        {
            this.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(200, WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);

            return false;
        }

        public virtual void OnResuming(IScreen last)
        {
            this.FadeIn(WaveContainer.APPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(0, WaveContainer.APPEAR_DURATION, Easing.OutQuint);
        }

        public virtual void OnSuspending(IScreen next)
        {
            this.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
            this.MoveToX(-200, WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);
        }

        public virtual bool OnPressed(GlobalAction action)
        {
            if (!this.IsCurrentScreen()) return false;

            if (action == GlobalAction.Back)
            {
                this.Exit();
                return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => action == GlobalAction.Back;

        public override string ToString() => Title;
    }
}
