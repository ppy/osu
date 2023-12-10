// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
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
    public partial class DrawableNote : DrawableManiaHitObject<Note>, IKeyBindingHandler<ManiaAction>, IHasSnapInformation
    {
        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved(canBeNull: true)]
        private IBeatmap beatmap { get; set; }

        private readonly Bindable<bool> configTimingBasedNoteColouring = new Bindable<bool>();

        protected virtual ManiaSkinComponents Component => ManiaSkinComponents.Note;

        private Drawable headPiece;

        private DrawableNotePerfectBonus perfectBonus;

        protected ISkinSource skin { get; private set; }
        public int SnapIndex { get; set; }


        public DrawableNote()
            : this(null)
        {
        }

        public DrawableNote(Note hitObject)
            : base(hitObject)
        {
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader(true)]
        private void load(ManiaRulesetConfigManager rulesetConfig, ISkinSource skinSource)
        {
            rulesetConfig?.BindWith(ManiaRulesetSetting.TimingBasedNoteColouring, configTimingBasedNoteColouring);

            AddInternal(headPiece = new SkinnableDrawable(new ManiaSkinComponentLookup(Component), _ => new DefaultNotePiece())
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y
            });
            skin = skinSource;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            configTimingBasedNoteColouring.BindValueChanged(_ => updateSnapColour());
            StartTimeBindable.BindValueChanged(_ => updateSnapColour(), true);
        }

        protected override void OnApply()
        {
            base.OnApply();
            updateSnapColour();
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
                {
                    perfectBonus.TriggerResult(false);
                    ApplyResult(r => r.Type = r.Judgement.MinResult);
                }

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            result = GetCappedResult(result);

            perfectBonus.TriggerResult(result == HitResult.Perfect);
            ApplyResult(r => r.Type = result);
        }

        public override void MissForcefully()
        {
            perfectBonus.TriggerResult(false);
            base.MissForcefully();
        }

        /// <summary>
        /// Some objects in mania may want to limit the max result.
        /// </summary>
        protected virtual HitResult GetCappedResult(HitResult result) => result;

        public virtual bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (e.Action != Action.Value)
                return false;

            if (CheckHittable?.Invoke(this, Time.Current) == false)
                return false;

            return UpdateResult(true);
        }

        public virtual void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableNotePerfectBonus bonus:
                    AddInternal(perfectBonus = bonus);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            RemoveInternal(perfectBonus, false);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case NotePerfectBonus bonus:
                    return new DrawableNotePerfectBonus(bonus);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        private void updateSnapColour()
        {
            if (beatmap == null || HitObject == null) return;

            int snapDivisor = beatmap.ControlPointInfo.GetClosestBeatDivisor(HitObject.StartTime);


            if (configTimingBasedNoteColouring.Value) {
                switch (snapDivisor) {
                    case 1:
                        SnapIndex = 1;
                        break;

                    case 2:
                        SnapIndex = 2;
                        break;

                    case 3:
                        SnapIndex = 3;
                        break;

                    case 4:
                        SnapIndex = 4;
                        break;

                    case 5:
                        SnapIndex = 5;
                        break;

                    case 6:
                        SnapIndex = 6;
                        break;

                    case 8:
                        SnapIndex = 7;
                        break;

                    case 12:
                        SnapIndex = 8;
                        break;

                    case 16:
                        SnapIndex = 9;
                        break;

                    case 20:
                        SnapIndex = 10;
                        break;

                    case 24:
                        SnapIndex = 11;
                        break;

                    default:
                        SnapIndex = 0;
                        break;
                }
                Colour = ((IHasSnapInformation)this).GetSnapColour(skin);
            }
        }
    }
}
