// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Types;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    /// <summary>
    /// A box which surrounds <see cref="DrawableHitObject"/>s and provides interactive handles, context menus etc.
    /// </summary>
    public class SelectionBox : VisibilityContainer
    {
        private readonly IReadOnlyList<HitObjectMask> overlays;

        public const float BORDER_RADIUS = 2;

        public SelectionBox(IReadOnlyList<HitObjectMask> overlays)
        {
            this.overlays = overlays;

            Masking = true;
            BorderThickness = BORDER_RADIUS;

            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                AlwaysPresent = true,
                Alpha = 0
            };

            State = Visibility.Visible;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BorderColour = colours.Yellow;
        }

        protected override void Update()
        {
            base.Update();

            // Todo: We might need to optimise this

            // Move the rectangle to cover the hitobjects
            var topLeft = new Vector2(float.MaxValue, float.MaxValue);
            var bottomRight = new Vector2(float.MinValue, float.MinValue);

            foreach (var obj in overlays)
            {
                topLeft = Vector2.ComponentMin(topLeft, Parent.ToLocalSpace(obj.HitObject.SelectionQuad.TopLeft));
                bottomRight = Vector2.ComponentMax(bottomRight, Parent.ToLocalSpace(obj.HitObject.SelectionQuad.BottomRight));
            }

            topLeft -= new Vector2(5);
            bottomRight += new Vector2(5);

            Size = bottomRight - topLeft;
            Position = topLeft;
        }

        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => overlays.Any(o => o.ReceiveMouseInputAt(screenSpacePos));

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            // Todo: Various forms of snapping
            foreach (var hitObject in overlays.Select(o => o.HitObject.HitObject))
            {
                switch (hitObject)
                {
                    case IHasEditablePosition editablePosition:
                        editablePosition.OffsetPosition(state.Mouse.Delta);
                        break;
                }
            }
            return true;
        }

        protected override bool OnDragEnd(InputState state) => true;

        public override bool DisposeOnDeathRemoval => true;

        protected override void PopIn() => this.FadeIn();
        protected override void PopOut() => this.FadeOut();
    }
}
