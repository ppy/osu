using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.Plugins.Types;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.Plugins.Internal.FallbackFunctionBar
{
    public class FunctionBar : Container, IFunctionBarProvider, IHasTooltip
    {
        public float GetSafeAreaPadding() => Height;

        public bool OkForHide() => IsHovered;

        private readonly FillFlowContainer<SimpleBarButton> contentContainer;

        private readonly List<IPluginFunctionProvider> pluginButtons = new List<IPluginFunctionProvider>();

        public FunctionBar()
        {
            Height = 40;
            RelativeSizeAxes = Axes.X;

            Anchor = Origin = Anchor.BottomCentre;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Green
                },
                new OsuScrollContainer(Direction.Horizontal)
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = contentContainer = new FillFlowContainer<SimpleBarButton>
                    {
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Spacing = new Vector2(5)
                    }
                }
            };
        }

        public bool AddFunctionControl(IFunctionProvider provider)
        {
            if (provider is IToggleableFunctionProvider toggleableFunctionProvider)
                contentContainer.Add(new ToggleableBarButton(toggleableFunctionProvider));
            else
                contentContainer.Add(new SimpleBarButton(provider));

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
            contentContainer.Clear();

            return AddFunctionControls(providers);
        }

        public void Remove(IFunctionProvider provider)
        {
            var target = contentContainer.FirstOrDefault(b => b.Provider == provider);

            if (target != null)
            {
                target.Expire();

                if (provider is IPluginFunctionProvider pluginFunctionProvider)
                    pluginButtons.Remove(pluginFunctionProvider);
            }
            else
                throw new ButtonNotFoundException(provider);
        }

        public void ShowFunctionControlTemporary()
        {
            this.FadeIn().Delay(300).FadeTo(0.8f);
        }

        public List<IPluginFunctionProvider> GetAllPluginFunctionButton() => pluginButtons;

        public class ButtonNotFoundException : Exception
        {
            public ButtonNotFoundException(IFunctionProvider provider)
                : base($"无法找到与{provider.ToString()}对应的按钮")
            {
            }
        }

        public LocalisableString TooltipText => "这是一个后备功能条，如果你没在开发功能条插件，那么这很可能是个错误。\n请检查你的插件配置";
    }
}
