// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineHitObjectBlueprint : SelectionBlueprint
    {
        private readonly Circle circle;

        private readonly Container extensionBar;

        [UsedImplicitly]
        private readonly Bindable<double> startTime;

        public const float THICKNESS = 3;

        private const float circle_size = 16;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => base.ReceivePositionalInputAt(screenSpacePos) || circle.ReceivePositionalInputAt(screenSpacePos);

        public TimelineHitObjectBlueprint(HitObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            startTime = hitObject.StartTimeBindable.GetBoundCopy();
            startTime.BindValueChanged(time => X = (float)time.NewValue, true);

            RelativePositionAxes = Axes.X;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            if (hitObject is IHasEndTime)
            {
                AddInternal(extensionBar = new Container
                {
                    CornerRadius = 2,
                    Masking = true,
                    Size = new Vector2(1, THICKNESS),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativePositionAxes = Axes.X,
                    RelativeSizeAxes = Axes.X,
                    Colour = Color4.Black,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                });
            }

            AddInternal(circle = new Circle
            {
                Size = new Vector2(circle_size),
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.X,
                AlwaysPresent = true,
                Colour = Color4.White,
                BorderColour = Color4.Black,
                BorderThickness = THICKNESS,
            });
        }

        protected override void Update()
        {
            base.Update();

            // no bindable so we perform this every update
            Width = (float)(HitObject.GetEndTime() - HitObject.StartTime);
        }

        protected override void OnSelected()
        {
            circle.BorderColour = Color4.Orange;
            if (extensionBar != null)
                extensionBar.Colour = Color4.Orange;
        }

        protected override void OnDeselected()
        {
            circle.BorderColour = Color4.Black;
            if (extensionBar != null)
                extensionBar.Colour = Color4.Black;
        }

        public override Quad SelectionQuad
        {
            get
            {
                // correctly include the circle in the selection quad region, as it is usually outside the blueprint itself.
                var circleQuad = circle.ScreenSpaceDrawQuad;
                var actualQuad = ScreenSpaceDrawQuad;

                return new Quad(circleQuad.TopLeft, Vector2.ComponentMax(actualQuad.TopRight, circleQuad.TopRight),
                    circleQuad.BottomLeft, Vector2.ComponentMax(actualQuad.BottomRight, circleQuad.BottomRight));
            }
        }

        public override Vector2 SelectionPoint => ScreenSpaceDrawQuad.TopLeft;
    }
}