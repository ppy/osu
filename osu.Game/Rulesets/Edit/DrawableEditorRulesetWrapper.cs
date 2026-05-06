// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A wrapper for a <see cref="DrawableRuleset{TObject}"/>. Handles adding visual representations of <see cref="HitObject"/>s to the underlying <see cref="DrawableRuleset{TObject}"/>.
    /// </summary>
    internal partial class DrawableEditorRulesetWrapper<TObject> : CompositeDrawable
        where TObject : HitObject
    {
        public Playfield Playfield => drawableRuleset.Playfield;

        private readonly DrawableRuleset<TObject> drawableRuleset;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        public DrawableEditorRulesetWrapper(DrawableRuleset<TObject> drawableRuleset)
        {
            this.drawableRuleset = drawableRuleset;

            RelativeSizeAxes = Axes.Both;

            InternalChild = drawableRuleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            drawableRuleset.FrameStablePlayback = false;
            Playfield.DisplayJudgements.Value = false;
        }

        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.HitObjectAdded += hitObjectAdded;
            beatmap.HitObjectRemoved += hitObjectRemoved;

            if (changeHandler != null)
            {
                // for now only regenerate replay on a finalised state change, not HitObjectUpdated.
                changeHandler.OnStateChange += stateChanged;
            }
            else
            {
                beatmap.HitObjectUpdated += hitObjectUpdated;
            }

            Scheduler.AddOnce(regenerateAutoplay);
        }

        private void regenerateAutoplay()
        {
            var autoplayMod = drawableRuleset.Mods.OfType<ModAutoplay>().Single();
            drawableRuleset.SetReplayScore(autoplayMod.CreateScoreFromReplayData(drawableRuleset.Beatmap, drawableRuleset.Mods));
        }

        private void hitObjectAdded(HitObject hitObject)
        {
            drawableRuleset.AddHitObject((TObject)hitObject);
            drawableRuleset.Playfield.PostProcess();
        }

        private void hitObjectRemoved(HitObject hitObject)
        {
            drawableRuleset.RemoveHitObject((TObject)hitObject);
            drawableRuleset.Playfield.PostProcess();
        }

        private void hitObjectUpdated(HitObject _) => Scheduler.AddOnce(regenerateAutoplay);

        private void stateChanged() => Scheduler.AddOnce(regenerateAutoplay);

        public override bool PropagatePositionalInputSubTree => false;

        public override bool PropagateNonPositionalInputSubTree => false;

        public PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => drawableRuleset.CreatePlayfieldAdjustmentContainer();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmap.IsNotNull())
            {
                beatmap.HitObjectAdded -= hitObjectAdded;
                beatmap.HitObjectRemoved -= hitObjectRemoved;
                beatmap.HitObjectUpdated -= hitObjectUpdated;
            }

            if (changeHandler != null)
                changeHandler.OnStateChange -= stateChanged;
        }
    }
}
