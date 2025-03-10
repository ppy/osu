// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// Represents a component responsible for displaying
    /// the appropriate catcher trails when requested to.
    /// </summary>
    public partial class CatcherTrailDisplay : PooledDrawableWithLifetimeContainer<CatcherTrailEntry, CatcherTrail>
    {
        /// <summary>
        /// The most recent time a dash trail was added to this container.
        /// Only alive (not faded out) trails are considered.
        /// Returns <see cref="double.NegativeInfinity"/> if no dash trail is alive.
        /// </summary>
        public double LastDashTrailTime => getLastDashTrailTime();

        public Color4 HyperDashTrailsColour => hyperDashTrails.Colour;

        public Color4 HyperDashAfterImageColour => hyperDashAfterImages.Colour;

        protected override bool RemoveRewoundEntry => true;

        private readonly DrawablePool<CatcherTrail> trailPool;

        private readonly Container<CatcherTrail> dashTrails;
        private readonly Container<CatcherTrail> hyperDashTrails;
        private readonly Container<CatcherTrail> hyperDashAfterImages;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        public CatcherTrailDisplay()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                trailPool = new DrawablePool<CatcherTrail>(30),
                dashTrails = new Container<CatcherTrail> { RelativeSizeAxes = Axes.Both },
                hyperDashTrails = new Container<CatcherTrail> { RelativeSizeAxes = Axes.Both, Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR },
                hyperDashAfterImages = new Container<CatcherTrail> { RelativeSizeAxes = Axes.Both, Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR },
            };
        }

        /// <summary>
        /// Update the scale of all trails.
        /// </summary>
        /// <param name="scale">The new body scale of the Catcher</param>
        public void UpdateCatcherTrailsScale(Vector2 scale)
        {
            var oldEntries = Entries.ToList();

            Clear();

            foreach (var oldEntry in oldEntries)
            {
                // use magnitude of the new scale while preserving the sign of the old one in the X direction.
                // the end effect is preserving the direction in which the trail sprites face, which is important.
                var targetScale = new Vector2(Math.Abs(scale.X) * Math.Sign(oldEntry.Scale.X), Math.Abs(scale.Y));
                Add(new CatcherTrailEntry(oldEntry.LifetimeStart, oldEntry.CatcherState, oldEntry.Position, targetScale, oldEntry.Animation));
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            skin.SourceChanged += skinSourceChanged;
            skinSourceChanged();
        }

        private void skinSourceChanged()
        {
            hyperDashTrails.Colour = skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash)?.Value ?? Catcher.DEFAULT_HYPER_DASH_COLOUR;
            hyperDashAfterImages.Colour = skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashAfterImage)?.Value ?? hyperDashTrails.Colour;
        }

        protected override void AddDrawable(CatcherTrailEntry entry, CatcherTrail drawable)
        {
            switch (entry.Animation)
            {
                case CatcherTrailAnimation.Dashing:
                    dashTrails.Add(drawable);
                    break;

                case CatcherTrailAnimation.HyperDashing:
                    hyperDashTrails.Add(drawable);
                    break;

                case CatcherTrailAnimation.HyperDashAfterImage:
                    hyperDashAfterImages.Add(drawable);
                    break;
            }
        }

        protected override void RemoveDrawable(CatcherTrailEntry entry, CatcherTrail drawable)
        {
            switch (entry.Animation)
            {
                case CatcherTrailAnimation.Dashing:
                    dashTrails.Remove(drawable, false);
                    break;

                case CatcherTrailAnimation.HyperDashing:
                    hyperDashTrails.Remove(drawable, false);
                    break;

                case CatcherTrailAnimation.HyperDashAfterImage:
                    hyperDashAfterImages.Remove(drawable, false);
                    break;
            }
        }

        protected override CatcherTrail GetDrawable(CatcherTrailEntry entry)
        {
            CatcherTrail trail = trailPool.Get();
            trail.Apply(entry);
            return trail;
        }

        private double getLastDashTrailTime()
        {
            double maxTime = double.NegativeInfinity;

            foreach (var trail in dashTrails)
                maxTime = Math.Max(maxTime, trail.LifetimeStart);

            foreach (var trail in hyperDashTrails)
                maxTime = Math.Max(maxTime, trail.LifetimeStart);

            return maxTime;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skin.IsNotNull())
                skin.SourceChanged -= skinSourceChanged;
        }
    }
}
