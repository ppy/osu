// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Rulesets.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Edit.Tools
{
    public abstract class PlacementVisualiser : CompositeDrawable, IRequireHighFrequencyMousePosition
    {
        public event Action<HitObject> PlacementFinished;

        protected PlacementVisualiser()
        {
            Origin = Anchor.Centre;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;
        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args) => true;
        protected override bool OnClick(InputState state) => true;
        protected override bool OnDragStart(InputState state) => true;
        protected override bool OnDrag(InputState state) => true;
        protected override bool OnDragEnd(InputState state) => true;

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => Parent.ReceiveMouseInputAt(screenSpacePos);

        protected void FinishPlacement(HitObject hitObject) => PlacementFinished?.Invoke(hitObject);

        protected override bool OnMouseMove(InputState state)
        {
            Position = state.Mouse.Position;
            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            PlacementFinished = null;
        }
    }
}
