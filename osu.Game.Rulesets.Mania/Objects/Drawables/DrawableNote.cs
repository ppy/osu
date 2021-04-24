// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Mania.Utils;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="Note"/> hit object.
    /// </summary>
    public class DrawableNote : DrawableManiaHitObject<Note>, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private Bindable<bool> configColourCodedNotes { get; set; }

        [Resolved(canBeNull: true)]
        private SnapFinder snapFinder { get; set; }

        protected virtual ManiaSkinComponents Component => ManiaSkinComponents.Note;

        private readonly Drawable headPiece;

        public readonly Bindable<int> SnapBindable = new Bindable<int>();

        public int Snap
        {
            get => SnapBindable.Value;
            set => SnapBindable.Value = value;
        }

        public DrawableNote(Note hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddInternal(headPiece = new SkinnableDrawable(new ManiaSkinComponent(Component, hitObject.Column), _ => new DefaultNotePiece())
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (snapFinder != null)
            {
                HitObject.StartTimeBindable.BindValueChanged(_ => Snap = snapFinder.FindSnap(HitObject), true);

                SnapBindable.BindValueChanged(snap => updateSnapColour(configColourCodedNotes.Value, snap.NewValue), true);
                configColourCodedNotes.BindValueChanged(colourCode => updateSnapColour(colourCode.NewValue, Snap));
            }
        }

        protected override void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> e)
        {
            base.OnDirectionChanged(e);

            headPiece.Anchor = headPiece.Origin = e.NewValue == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = r.Judgement.MinResult);
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            ApplyResult(r => r.Type = result);
        }

        public virtual bool OnPressed(ManiaAction action)
        {
            if (action != Action.Value)
                return false;

            if (CheckHittable?.Invoke(this, Time.Current) == false)
                return false;

            return UpdateResult(true);
        }

        public virtual void OnReleased(ManiaAction action)
        {
        }

        private void updateSnapColour(bool colourCode, int snap)
        {
            if (colourCode)
            {
                Colour = BindableBeatDivisor.GetColourFor(Snap, colours);
            }
            else
            {
                Colour = Colour4.White;
            }
        }
    }
}