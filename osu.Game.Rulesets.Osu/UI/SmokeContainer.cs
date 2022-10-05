// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
    public class SmokeContainer : Container, IRequireHighFrequencyMousePosition, IKeyBindingHandler<OsuAction>
    {
        public Vector2 LastMousePosition;

        private SkinnableDrawable? currentSegment;

        public override bool ReceivePositionalInputAt(Vector2 _) => true;

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            if (e.Action == OsuAction.Smoke)
            {
                AddInternal(currentSegment = new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.SmokeTrail), _ => new DefaultSmokeSegment()));
                addPosition(LastMousePosition, Time.Current);
                return true;
            }

            return false;
        }

        private void addPosition(Vector2 position, double timeCurrent) => (currentSegment?.Drawable as SmokeSegment)?.AddPosition(position, timeCurrent);

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
        {
            if (e.Action == OsuAction.Smoke)
            {
                (currentSegment?.Drawable as SmokeSegment)?.FinishDrawing(Time.Current);
                currentSegment = null;

                foreach (Drawable child in Children)
                {
                    var skinnable = (SkinnableDrawable)child;
                    skinnable.LifetimeEnd = skinnable.Drawable.LifetimeEnd;
                }
            }
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (currentSegment != null)
                addPosition(e.MousePosition, Time.Current);

            LastMousePosition = e.MousePosition;
            return base.OnMouseMove(e);
        }
    }
}
