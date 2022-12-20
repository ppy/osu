using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.UI
{
    public partial class InteractiveContainer : Container
    {
        private const float border_thickness = 4f;

        protected override Container<Drawable> Content => content;

        private readonly Container content;
        protected readonly Container ScalableContent;
        private readonly Container boundsContainer;

        public InteractiveContainer()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = ScalableContent = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    boundsContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderColour = Color4.White,
                        BorderThickness = border_thickness,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                }
            };
        }

        private float zoom = 1f;

        public void Reset()
        {
            Clear();

            boundsContainer.BorderThickness = border_thickness;
            ScalableContent.ClearTransforms();
            ScalableContent.Anchor = Anchor.Centre;
            ScalableContent.Origin = Anchor.Centre;
            ScalableContent.Position = Vector2.Zero;
            ScalableContent.Size = Vector2.Zero;
            ScalableContent.Scale = Vector2.One;
            zoom = 1f;
        }

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);
            ScalableContent.Position += e.Delta;
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            base.OnScroll(e);

            ScalableContent.OriginPosition = ToSpaceOfOtherDrawable(e.MousePosition, ScalableContent);
            ScalableContent.Anchor = Anchor.TopLeft;
            ScalableContent.Position = e.MousePosition;

            zoom += (e.ScrollDelta.Y > 0 ? 1 : -1) * zoom * 0.1f;
            ScalableContent.ScaleTo(zoom, 200, Easing.OutQuint);
            boundsContainer.BorderThickness = border_thickness / zoom;

            return true;
        }
    }
}
