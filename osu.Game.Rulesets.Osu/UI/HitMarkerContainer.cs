// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class HitMarkerContainer : Container, IRequireHighFrequencyMousePosition, IKeyBindingHandler<OsuAction>
    {
        private Vector2 lastMousePosition;

        public Bindable<bool> HitMarkerEnabled = new BindableBool();
        public Bindable<bool> AimMarkersEnabled = new BindableBool();

        public override bool ReceivePositionalInputAt(Vector2 _) => true;

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            if (HitMarkerEnabled.Value && (e.Action == OsuAction.LeftButton || e.Action == OsuAction.RightButton))
            {
                AddMarker(e.Action);
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e) { }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            lastMousePosition = e.MousePosition;

            if (AimMarkersEnabled.Value)
            {
                AddMarker(null);
            }

            return base.OnMouseMove(e);
        }

        private void AddMarker(OsuAction? action)
        {
            Add(new HitMarkerDrawable(action) { Position = lastMousePosition });
        }

        private partial class HitMarkerDrawable : CompositeDrawable
        {
            private const double lifetime_duration = 1000;
            private const double fade_out_time = 400;

            public override bool RemoveWhenNotAlive => true;

            public HitMarkerDrawable(OsuAction? action)
            {
                var colour = Colour4.Gray.Opacity(0.5F);
                var length = 8;
                var depth = float.MaxValue;
                switch (action)
                {
                    case OsuAction.LeftButton:
                        colour = Colour4.Orange;
                        length = 20;
                        depth = float.MinValue;
                        break;
                    case OsuAction.RightButton:
                        colour = Colour4.LightGreen;
                        length = 20;
                        depth = float.MinValue;
                        break;
                }

                this.Depth = depth;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(3, length),
                        Rotation = 45,
                        Colour = Colour4.Black.Opacity(0.5F)
                    },
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(3, length),
                        Rotation = 135,
                        Colour = Colour4.Black.Opacity(0.5F)
                    },
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(1, length),
                        Rotation = 45,
                        Colour = colour
                    },
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(1, length),
                        Rotation = 135,
                        Colour = colour
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                LifetimeStart = Time.Current;
                LifetimeEnd = LifetimeStart + lifetime_duration;

                Scheduler.AddDelayed(() => 
                {
                    this.FadeOut(fade_out_time);
                }, lifetime_duration - fade_out_time);
            }
        }
    }
}