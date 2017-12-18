// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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

        private DragSelector selector;

        protected override bool OnDragStart(InputState state)
        {
            // Hide the previous drag box - we won't be working with it any longer
            selector?.Hide();

            AddInternal(selector = new DragSelector(ToLocalSpace(state.Mouse.NativeState.Position))
            {
                CapturableObjects = playfield.HitObjects.Objects,
            });

            Selection.BindTo(selector.Selection);

            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            selector.DragEndPosition = ToLocalSpace(state.Mouse.NativeState.Position);
            selector.BeginCapture();
            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            selector.FinishCapture();
            return true;
        }

        protected override bool OnClick(InputState state)
        {
            selector?.Hide();
            return true;
        }
    }
}
