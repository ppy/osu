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
        public Playfield Playfield => DrawableRuleset.Playfield;

        public readonly DrawableRuleset<TObject> DrawableRuleset;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        public DrawableEditorRulesetWrapper(DrawableRuleset<TObject> drawableRuleset)
        {
            DrawableRuleset = drawableRuleset;

            RelativeSizeAxes = Axes.Both;

            InternalChild = drawableRuleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            DrawableRuleset.FrameStablePlayback = false;
            Playfield.DisplayJudgements.Value = false;
        }

        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.HitObjectAdded += addHitObject;
            beatmap.HitObjectRemoved += removeHitObject;

            if (changeHandler != null)
            {
                // for now only regenerate replay on a finalised state change, not HitObjectUpdated.
                changeHandler.OnStateChange += () => Scheduler.AddOnce(regenerateAutoplay);
            }
            else
            {
                beatmap.HitObjectUpdated += _ => Scheduler.AddOnce(regenerateAutoplay);
            }

            Scheduler.AddOnce(regenerateAutoplay);
        }

        private void regenerateAutoplay()
        {
            var autoplayMod = DrawableRuleset.Mods.OfType<ModAutoplay>().Single();
            DrawableRuleset.SetReplayScore(autoplayMod.CreateScoreFromReplayData(DrawableRuleset.Beatmap, DrawableRuleset.Mods));
        }

        private void addHitObject(HitObject hitObject)
        {
            DrawableRuleset.AddHitObject((TObject)hitObject);
            DrawableRuleset.Playfield.PostProcess();
        }

        private void removeHitObject(HitObject hitObject)
        {
            DrawableRuleset.RemoveHitObject((TObject)hitObject);
            DrawableRuleset.Playfield.PostProcess();
        }

        public override bool PropagatePositionalInputSubTree => false;

        public override bool PropagateNonPositionalInputSubTree => false;

        public PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => DrawableRuleset.CreatePlayfieldAdjustmentContainer();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmap.IsNotNull())
            {
                beatmap.HitObjectAdded -= addHitObject;
                beatmap.HitObjectRemoved -= removeHitObject;
            }
        }
    }
}
