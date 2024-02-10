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
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class HitMarkerContainer : Container, IRequireHighFrequencyMousePosition, IKeyBindingHandler<OsuAction>
    {
        private Vector2 lastMousePosition;
        private Vector2? lastlastMousePosition;
        private double? timePreempt;
        private double TimePreempt
        {
            get => timePreempt ?? default_time_preempt;
            set => timePreempt = value;
        }

        public Bindable<bool> HitMarkerEnabled = new BindableBool();
        public Bindable<bool> AimMarkersEnabled = new BindableBool();
        public Bindable<bool> AimLinesEnabled = new BindableBool();

        private const double default_time_preempt = 1000;

        private readonly HitObjectContainer hitObjectContainer;

        public override bool ReceivePositionalInputAt(Vector2 _) => true;

        public HitMarkerContainer(HitObjectContainer hitObjectContainer)
        {
            this.hitObjectContainer = hitObjectContainer;
        }

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            if (HitMarkerEnabled.Value && (e.Action == OsuAction.LeftButton || e.Action == OsuAction.RightButton))
            {
                updateTimePreempt();
                AddMarker(e.Action);
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
        {
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            lastlastMousePosition = lastMousePosition;
            lastMousePosition = e.MousePosition;

            if (AimMarkersEnabled.Value)
            {
                updateTimePreempt();
                AddMarker(null);
            }

            if (AimLinesEnabled.Value && lastlastMousePosition != null && lastlastMousePosition != lastMousePosition)
            {
                if (!AimMarkersEnabled.Value)
                    updateTimePreempt();
                Add(new AimLineDrawable((Vector2)lastlastMousePosition, lastMousePosition, TimePreempt));
            }

            return base.OnMouseMove(e);
        }

        private void AddMarker(OsuAction? action)
        {
            var component = OsuSkinComponents.AimMarker;
            switch(action)
            {
                case OsuAction.LeftButton:
                    component = OsuSkinComponents.HitMarkerLeft;
                    break;
                case OsuAction.RightButton:
                    component = OsuSkinComponents.HitMarkerRight;
                    break;
            }

            Add(new HitMarkerDrawable(action, component, TimePreempt)
            {
                Position = lastMousePosition,
                Origin = Anchor.Centre,
                Depth = action == null ? float.MaxValue : float.MinValue
            });
        }

        private void updateTimePreempt()
        {
            var hitObject = getHitObject();
            if (hitObject == null)
                return;

            TimePreempt = hitObject.TimePreempt;
        }

        private OsuHitObject? getHitObject()
        {
            foreach (var dho in hitObjectContainer.Objects)
                return (dho as DrawableOsuHitObject)?.HitObject;
            return null;
        }

        private partial class HitMarkerDrawable : SkinnableDrawable
        {
            private readonly double lifetimeDuration;
            private readonly double fadeOutTime;

            public override bool RemoveWhenNotAlive => true;

            public HitMarkerDrawable(OsuAction? action, OsuSkinComponents componenet, double timePreempt)
                : base(new OsuSkinComponentLookup(componenet), _ => new DefaultHitMarker(action))
            {
                fadeOutTime = timePreempt / 2;
                lifetimeDuration = timePreempt + fadeOutTime;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                LifetimeStart = Time.Current;
                LifetimeEnd = LifetimeStart + lifetimeDuration;

                Scheduler.AddDelayed(() => 
                {
                    this.FadeOut(fadeOutTime);
                }, lifetimeDuration - fadeOutTime);
            }
        }

        private partial class AimLineDrawable : CompositeDrawable
        {
            private readonly double lifetimeDuration;
            private readonly double fadeOutTime;

            public override bool RemoveWhenNotAlive => true;

            public AimLineDrawable(Vector2 fromP, Vector2 toP, double timePreempt)
            {
                fadeOutTime = timePreempt / 2;
                lifetimeDuration = timePreempt + fadeOutTime;

                float distance = Vector2.Distance(fromP, toP);
                Vector2 direction = (toP - fromP);
                InternalChild = new Box
                {
                    Position = fromP + (direction / 2),
                    Size = new Vector2(distance, 1),
                    Rotation = (float)(Math.Atan(direction.Y / direction.X) * (180 / Math.PI)),
                    Origin = Anchor.Centre
                };
            }

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                var color = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.ReplayAimLine)?.Value ?? Color4.White;
                color.A = 127;
                InternalChild.Colour = color;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                LifetimeStart = Time.Current;
                LifetimeEnd = LifetimeStart + lifetimeDuration;

                Scheduler.AddDelayed(() => 
                {
                    this.FadeOut(fadeOutTime);
                }, lifetimeDuration - fadeOutTime);
            }
        }
    }
}