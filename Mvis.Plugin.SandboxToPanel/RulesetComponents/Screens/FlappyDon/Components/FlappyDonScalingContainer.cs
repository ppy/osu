using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.FlappyDon.Components
{
    public partial class FlappyDonScalingContainer : Container
    {
        protected override Container<Drawable> Content => content;

        private readonly Container content;
        private readonly Vector2 size;

        public FlappyDonScalingContainer(Vector2 size)
        {
            this.size = size;

            RelativeSizeAxes = Axes.Both;
            InternalChild = content = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                Size = size
            };
        }

        protected override void Update()
        {
            base.Update();
            content.Scale = DrawWidth / DrawHeight < size.X / size.Y ? new Vector2(DrawWidth / size.X) : new Vector2(DrawHeight / size.Y);
        }
    }
}
