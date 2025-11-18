// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Manages smoke trails generated from user input.
    /// </summary>
    public partial class SmokeContainer : Container, IRequireHighFrequencyMousePosition, IKeyBindingHandler<OsuAction>
    {
        private DrawablePool<SmokeSkinnableDrawable> segmentPool = null!;
        private SmokeSkinnableDrawable? currentSegmentSkinnable;

        private Vector2 lastMousePosition;

        public override bool ReceivePositionalInputAt(Vector2 _) => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(segmentPool = new DrawablePool<SmokeSkinnableDrawable>(10));
        }

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            if (e.Action == OsuAction.Smoke)
            {
                AddInternal(currentSegmentSkinnable = segmentPool.Get(segment => segment.Segment?.StartDrawing(Time.Current)));

                // Add initial position immediately.
                addPosition();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
        {
            if (e.Action == OsuAction.Smoke)
            {
                if (currentSegmentSkinnable?.Drawable is SmokeSegment segment)
                {
                    segment.FinishDrawing(Time.Current);
                    currentSegmentSkinnable = null;
                }
            }
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            lastMousePosition = e.MousePosition;
            addPosition();

            return base.OnMouseMove(e);
        }

        private void addPosition() => currentSegmentSkinnable?.Segment?.AddPosition(lastMousePosition, Time.Current);

        private partial class SmokeSkinnableDrawable : SkinnableDrawable
        {
            public SmokeSegment? Segment => Drawable as SmokeSegment;

            public override bool RemoveWhenNotAlive => true;

            public override double LifetimeStart => Drawable.LifetimeStart;
            public override double LifetimeEnd => Drawable.LifetimeEnd;

            public SmokeSkinnableDrawable()
                : base(new OsuSkinComponentLookup(OsuSkinComponents.CursorSmoke), _ => new DefaultSmokeSegment())
            {
            }
        }
    }
}
