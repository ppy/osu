using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Mvis.BottomBar.Buttons;
using osuTK;

namespace osu.Game.Screens.Mvis.BottomBar
{
    internal class BottomBarContainer : Container
    {
        public Drawable[] LeftContent;
        public Drawable[] CentreContent;
        public Drawable[] RightContent;
        public FillFlowContainer PluginEntriesFillFlow;

        public BottomBarContainer()
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AutoSizeDuration = 300;
            AutoSizeEasing = Easing.OutQuint;
            Margin = new MarginPadding { Bottom = 10 };
        }

        [BackgroundDependencyLoader]
        private void load(MvisScreen mvisScreen)
        {
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    Name = "Left Container",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding { Left = 5 },
                    Children = LeftContent
                },
                new FillFlowContainer
                {
                    Name = "Centre Container",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(5),
                    Children = CentreContent
                },
                new FillFlowContainer
                {
                    Name = "Right Container",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding { Right = 5 },
                    Children = RightContent
                },
                PluginEntriesFillFlow = new FillFlowContainer
                {
                    Name = "Plugin Entries Container",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    AutoSizeAxes = Axes.Both,
                    AutoSizeDuration = 300,
                    AutoSizeEasing = Easing.OutQuint,
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding { Bottom = 35 }
                }
            };

            mvisScreen.OnIdle += Hide;
            mvisScreen.OnResumeFromIdle += Show;
        }

        public override void Show() =>
            this.MoveToY(0, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);

        public override void Hide() =>
            this.MoveToY(40, 300, Easing.OutQuint);

        public void CentreBotton(BottomBarButton btn) =>
            PluginEntriesFillFlow.SetLayoutPosition(btn, (float)Math.Floor(PluginEntriesFillFlow.Count / 2f));
    }
}
