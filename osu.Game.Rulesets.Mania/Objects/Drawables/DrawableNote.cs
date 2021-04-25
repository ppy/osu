// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="Note"/> hit object.
    /// </summary>
    public class DrawableNote : DrawableManiaHitObject<Note>, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private OsuColour colours { get; set; }

        private readonly Bindable<bool> configColourCodedNotes = new Bindable<bool>();

        protected virtual ManiaSkinComponents Component => ManiaSkinComponents.Note;

        private readonly Drawable headPiece;

        private readonly Bindable<int> snap = new Bindable<int>();

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

        [BackgroundDependencyLoader(true)]
        private void load(ManiaRulesetConfigManager rulesetConfig, BeatDivisorFinder beatDivisorFinder)
        {
            if (beatDivisorFinder != null)
            {
                rulesetConfig?.BindWith(ManiaRulesetSetting.ColourCodedNotes, configColourCodedNotes);

                HitObject.StartTimeBindable.BindValueChanged(_ => snap.Value = beatDivisorFinder.FindDivisor(HitObject), true);

                snap.BindValueChanged(_ => updateSnapColour(), true);
                configColourCodedNotes.BindValueChanged(_ => updateSnapColour());
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

        private void updateSnapColour()
        {
            Colour = configColourCodedNotes.Value
                ? BindableBeatDivisor.GetColourFor(snap.Value, colours)
                : Color4.White;
        }
    }
}
