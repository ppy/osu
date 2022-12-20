using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Main.Components
{
    public partial class SandboxButtonSystem : CompositeDrawable
    {
        public SandboxPanel[] Buttons
        {
            set => buttonsFlow.Children = value;
        }

        private readonly FillFlowContainer<SandboxPanel> buttonsFlow;

        public SandboxButtonSystem()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = new OsuScrollContainer(Direction.Horizontal)
            {
                RelativeSizeAxes = Axes.Both,
                Height = 0.7f,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                ScrollbarVisible = false,
                Masking = false,
                Child = buttonsFlow = new FillFlowContainer<SandboxPanel>
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(20, 0)
                }
            };
        }

        protected override void Update()
        {
            base.Update();
            buttonsFlow.Margin = new MarginPadding { Horizontal = DrawWidth / 2 - SandboxPanel.WIDTH / 2 };
        }
    }
}
