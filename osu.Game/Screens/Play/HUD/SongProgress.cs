// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class SongProgress : OverlayContainer, ISerialisableDrawable
    {
        // Some implementations of this element allow seeking during gameplay playback.
        // Set a sane default of never handling input to override the behaviour provided by OverlayContainer.
        public override bool HandleNonPositionalInput => Interactive.Value;
        public override bool HandlePositionalInput => Interactive.Value;

        protected override bool BlockScrollInput => false;

        /// <summary>
        /// Whether interaction should be allowed (ie. seeking). If <c>false</c>, interaction controls will not be displayed.
        /// </summary>
        /// <remarks>
        /// By default, this will be automatically decided based on the gameplay state.
        /// </remarks>
        public readonly Bindable<bool> Interactive = new Bindable<bool>();

        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        protected IGameplayClock GameplayClock { get; private set; } = null!;

        [Resolved]
        private IFrameStableClock? frameStableClock { get; set; }

        /// <summary>
        /// The reference clock is used to accurately tell the current playfield's time (including catch-up lag).
        /// However, if none is available (i.e. used in tests), we fall back to the gameplay clock.
        /// </summary>
        protected IClock FrameStableClock => frameStableClock ?? GameplayClock;

        private IEnumerable<HitObject>? objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;

                (FirstHitTime, LastHitTime) = BeatmapExtensions.CalculatePlayableBounds(objects);

                UpdateObjects(objects);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Show();
        }

        protected double FirstHitTime { get; private set; }

        protected double LastHitTime { get; private set; }

        protected abstract void UpdateProgress(double progress, bool isIntro);
        protected virtual void UpdateObjects(IEnumerable<HitObject> objects) { }

        [BackgroundDependencyLoader]
        private void load(DrawableRuleset? drawableRuleset, Player? player)
        {
            if (drawableRuleset != null)
            {
                if (player?.Configuration.AllowUserInteraction == true)
                    ((IBindable<bool>)Interactive).BindTo(drawableRuleset.HasReplayLoaded);

                Objects = drawableRuleset.Objects;
            }
        }

        protected override void PopIn() => this.FadeIn(500, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(100);

        protected override void Update()
        {
            base.Update();

            if (objects == null)
                return;

            double currentTime = FrameStableClock.CurrentTime;

            bool isInIntro = currentTime < FirstHitTime;

            if (isInIntro)
            {
                double introStartTime = GameplayClock.StartTime;

                double introOffsetCurrent = currentTime - introStartTime;
                double introDuration = FirstHitTime - introStartTime;

                UpdateProgress(introOffsetCurrent / introDuration, true);
            }
            else
            {
                double objectOffsetCurrent = currentTime - FirstHitTime;

                double objectDuration = LastHitTime - FirstHitTime;
                if (objectDuration == 0)
                    UpdateProgress(0, false);
                else
                    UpdateProgress(objectOffsetCurrent / objectDuration, false);
            }
        }
    }
}
