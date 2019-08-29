// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Edit
{
    public abstract class DrawableEditRuleset : CompositeDrawable
    {
        /// <summary>
        /// The <see cref="Playfield"/> contained by this <see cref="DrawableEditRuleset"/>.
        /// </summary>
        public abstract Playfield Playfield { get; }

        public abstract PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer();

        internal DrawableEditRuleset()
        {
            RelativeSizeAxes = Axes.Both;
        }
    }

    public class DrawableEditRuleset<TObject> : DrawableEditRuleset
        where TObject : HitObject
    {
        public override Playfield Playfield => drawableRuleset.Playfield;

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => drawableRuleset.CreatePlayfieldAdjustmentContainer();

        private readonly DrawableRuleset<TObject> drawableRuleset;

        [Resolved]
        private IEditorBeatmap<TObject> beatmap { get; set; }

        public DrawableEditRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            this.drawableRuleset = drawableRuleset;

            InternalChild = drawableRuleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            drawableRuleset.FrameStablePlayback = false;
            Playfield.DisplayJudgements.Value = false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.HitObjectAdded += addHitObject;
            beatmap.HitObjectRemoved += removeHitObject;
        }

        private void addHitObject(HitObject hitObject)
        {
            var drawableObject = drawableRuleset.CreateDrawableRepresentation((TObject)hitObject);

            drawableRuleset.Playfield.Add(drawableObject);
            drawableRuleset.Playfield.PostProcess();
        }

        private void removeHitObject(HitObject hitObject)
        {
            var drawableObject = Playfield.AllHitObjects.Single(d => d.HitObject == hitObject);

            drawableRuleset.Playfield.Remove(drawableObject);
            drawableRuleset.Playfield.PostProcess();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmap != null)
            {
                beatmap.HitObjectAdded -= addHitObject;
                beatmap.HitObjectRemoved -= removeHitObject;
            }
        }
    }
}
