using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Mvis.Plugins.Internal.BottomBar.Buttons;
using osu.Game.Screens.Mvis.Plugins.Types;
using osuTK;

namespace osu.Game.Screens.Mvis.Plugins.Internal.BottomBar
{
    internal class LegacyBottomBar : MvisPlugin, IFunctionBarProvider
    {
        protected override Drawable CreateContent()
        {
            throw new NotImplementedException();
        }

        protected override bool OnContentLoaded(Drawable content) => true;

        protected override bool PostInit() => true;

        public override int Version => 0;

        private readonly FillFlowContainer leftContent;
        private readonly FillFlowContainer centreContent;
        private readonly FillFlowContainer rightContent;
        private readonly FillFlowContainer pluginEntriesFillFlow;

        public LegacyBottomBar()
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AutoSizeDuration = 300;
            AutoSizeEasing = Easing.OutQuint;
            Margin = new MarginPadding { Bottom = 10 };

            InternalChildren = new Drawable[]
            {
                leftContent = new FillFlowContainer
                {
                    Name = "Left Container",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding { Left = 5 }
                },
                centreContent = new FillFlowContainer
                {
                    Name = "Centre Container",
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(5)
                },
                rightContent = new FillFlowContainer
                {
                    Name = "Right Container",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(5),
                    Margin = new MarginPadding { Right = 5 }
                },
                pluginEntriesFillFlow = new FillFlowContainer
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
        }

        [BackgroundDependencyLoader]
        private void load(MvisScreen mvisScreen)
        {
            mvisScreen.OnIdle += Hide;
            mvisScreen.OnResumeFromIdle += Show;
        }

        public float GetSafeAreaPadding() => Height - Y + 10 + 5;

        public override void Show() =>
            this.MoveToY(0, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);

        public override void Hide() =>
            this.MoveToY(40, 300, Easing.OutQuint);

        public bool OkForHide() => IsHovered;

        private void checkForPluginControls(IFunctionProvider provider)
        {
            if (provider is IPluginFunctionProvider pluginFunctionProvider)
                pluginButtons.Add(pluginFunctionProvider);
        }

        public bool AddFunctionControl(IFunctionProvider provider)
        {
            checkForPluginControls(provider);

            var button = provider is IToggleableFunctionProvider
                ? new BottomBarSwitchButton((IToggleableFunctionProvider)provider)
                : new BottomBarButton(provider);

            switch (provider.Type)
            {
                case FunctionType.Audio:
                    centreContent.Add(button);
                    break;

                case FunctionType.Base:
                    leftContent.Add(button);
                    break;

                case FunctionType.Misc:
                    rightContent.Add(button);
                    break;

                case FunctionType.Plugin:
                    pluginEntriesFillFlow.Add(button);
                    break;

                case FunctionType.ProgressDisplay:
                    button.Dispose();
                    centreContent.Add(new SongProgressButton(provider));
                    break;

                default:
                    throw new InvalidOperationException("???");
            }

            return true;
        }

        public bool AddFunctionControls(List<IFunctionProvider> providers)
        {
            foreach (var provider in providers)
            {
                AddFunctionControl(provider);
            }

            return true;
        }

        public bool SetFunctionControls(List<IFunctionProvider> providers)
        {
            leftContent.Clear();
            centreContent.Clear();
            rightContent.Clear();
            pluginEntriesFillFlow.Clear();

            return AddFunctionControls(providers);
        }

        private readonly List<IPluginFunctionProvider> pluginButtons = new List<IPluginFunctionProvider>();

        public void OnRemove(IFunctionBarProvider provider)
        {
            throw new NotImplementedException();
        }

        public void OnRemove(IPluginFunctionProvider provider)
        {
            pluginButtons.Remove(provider);
        }

        public void ShowFunctionControlTemporary() => pluginEntriesFillFlow.FadeIn(500, Easing.OutQuint).Then().Delay(2000).FadeOut(500, Easing.OutQuint);

        public List<IPluginFunctionProvider> GetAllPluginFunctionButton() => pluginButtons;
    }
}
