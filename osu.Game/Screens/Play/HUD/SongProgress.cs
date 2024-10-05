// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Localisation.HUD;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public enum DifficultyGraphType
    {
        None,
        ObjectDensity,
        MaxStrain,
        TotalStrain
    }

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

        /// <summary>
        /// Type of the difficulty info used in graph.
        /// </summary>
        protected readonly Bindable<DifficultyGraphType> GraphTypeInternal = new Bindable<DifficultyGraphType>(DifficultyGraphType.None);

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Show();
        }

        /// <summary>
        /// Called every update frame with current progress information.
        /// </summary>
        /// <param name="progress">Current (visual) progress through the beatmap (0..1).</param>
        /// <param name="isIntro">If <c>true</c>, progress is (0..1) through the intro.</param>
        protected abstract void UpdateProgress(double progress, bool isIntro);

        [BackgroundDependencyLoader]
        private void load(DrawableRuleset? drawableRuleset, Player? player)
        {
            if (drawableRuleset != null)
            {
                if (player?.Configuration.AllowUserInteraction == true)
                    ((IBindable<bool>)Interactive).BindTo(drawableRuleset.HasReplayLoaded);

                Objects = drawableRuleset.Objects;
            }

            GraphTypeInternal.BindValueChanged(_ => updateBasedOnGraphType(), true);
        }

        private void updateBasedOnGraphType()
        {
            switch (GraphTypeInternal.Value)
            {
                case DifficultyGraphType.None:
                    UpdateFromObjects(Enumerable.Empty<HitObject>());
                    break;

                case DifficultyGraphType.ObjectDensity:
                    if (objects != null) UpdateFromObjects(objects);
                    break;

                case DifficultyGraphType.MaxStrain:
                    if (sectionStrains != null) UpdateFromStrains(getMaxStrains(sectionStrains));
                    else calculateStrains();
                    break;

                case DifficultyGraphType.TotalStrain:
                    if (sectionStrains != null) UpdateFromStrains(getTotalStrains(sectionStrains));
                    else calculateStrains();
                    break;
            }
        }

        protected override void PopIn() => this.FadeIn(500, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(100);

        protected override void Update()
        {
            base.Update();

            if (objects == null)
                return;

            double currentTime = Math.Min(FrameStableClock.CurrentTime, LastHitTime);

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

        protected virtual void UpdateTimeBounds() { }

        #region object density

        protected double FirstHitTime { get; private set; }

        protected double LastHitTime { get; private set; }

        private IEnumerable<HitObject>? objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;

                (FirstHitTime, LastHitTime) = BeatmapExtensions.CalculatePlayableBounds(objects);

                UpdateTimeBounds();
                updateBasedOnGraphType();
            }
        }

        protected virtual void UpdateFromObjects(IEnumerable<HitObject> objects) { }

        #endregion


        #region diffcalc

        private bool strainsCalculationWasStarted = false;

        private List<double[]>? sectionStrains;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private void calculateStrains()
        {
            // No need for another recalc if strains are being recalculated right now;
            if (strainsCalculationWasStarted) return;

            strainsCalculationWasStarted = true;
            difficultyCache.GetSectionDifficultiesAsync(beatmap.Value, ruleset.Value.CreateInstance(), mods.Value.ToArray())
                .ContinueWith(task => Schedule(() =>
                {
                    sectionStrains = task.GetResultSafely();
                    updateBasedOnGraphType();
                }), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private double[] getMaxStrains(List<double[]> allStrains)
        {
            var result = allStrains
                .SelectMany(arr => arr.Select((value, index) => (value, index)))
                .GroupBy(x => x.index)
                .Select(g => g.Max(x => x.value));

            return result.ToArray();
        }

        private double[] getTotalStrains(List<double[]> allStrains)
        {
            var result = allStrains
                .SelectMany(arr => arr.Select((value, index) => (value, index)))
                .GroupBy(x => x.index)
                .Select(g => Math.Sqrt(g.Sum(x => x.value * x.value)));

            return result.ToArray();
        }

        protected virtual void UpdateFromStrains(double[] sectionStrains) { }

        #endregion
    }
}
