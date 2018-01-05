// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    public class SelectionLayer : CompositeDrawable
    {
        public readonly Bindable<SelectionInfo> Selection = new Bindable<SelectionInfo>();

        private readonly Playfield playfield;

        public SelectionLayer(Playfield playfield)
        {
            this.playfield = playfield;

            RelativeSizeAxes = Axes.Both;
        }

        private HitObjectSelectionBox selectionBoxBox;

        protected override bool OnDragStart(InputState state)
        {
            // Hide the previous drag box - we won't be working with it any longer
            selectionBoxBox?.Hide();

            AddInternal(selectionBoxBox = new HitObjectSelectionBox(ToLocalSpace(state.Mouse.NativeState.Position))
            {
                CapturableObjects = playfield.HitObjects.Objects,
            });

            Selection.BindTo(selectionBoxBox.Selection);

            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            selectionBoxBox.DragEndPosition = ToLocalSpace(state.Mouse.NativeState.Position);
            selectionBoxBox.BeginCapture();
            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            selectionBoxBox.FinishCapture();
            return true;
        }

        protected override bool OnClick(InputState state)
        {
            selectionBoxBox?.Hide();
            return true;
        }
    }
}
