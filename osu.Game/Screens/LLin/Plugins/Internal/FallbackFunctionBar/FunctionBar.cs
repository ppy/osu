using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.LLin.Plugins.Types;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.LLin.Plugins.Internal.FallbackFunctionBar
{
    public class FunctionBar : LLinPlugin, IFunctionBarProvider
    {
        public float GetSafeAreaPadding() => Height;

        public bool OkForHide() => IsHovered;

        private readonly FillFlowContainer<SimpleBarButton> contentContainer;

        private readonly List<IPluginFunctionProvider> pluginButtons = new List<IPluginFunctionProvider>();
        private readonly Box idleIndicator;
        private readonly Box hideIndicator;

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
                    Colour = Color4Extensions.FromHex("#2d353f")
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
                },
                idleIndicator = new Box
                {
                    Width = 5,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Colour = Color4.Gold
                },
                hideIndicator = new Box
                {
                    Width = 5,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Colour = Color4Extensions.FromHex("#365960"),
                    Margin = new MarginPadding { Right = 5 }
                }
            };
        }

        protected override Drawable CreateContent()
        {
            throw new NotImplementedException();
        }

        protected override bool OnContentLoaded(Drawable content) => true;

        protected override bool PostInit() => true;

        public override int Version => 0;

        [BackgroundDependencyLoader]
        private void load()
        {
            LLin.OnIdle += onIdle;
            LLin.OnActive += resumeFromIdle;
        }

        private void resumeFromIdle()
        {
            hideIndicator.FadeOut(300, Easing.OutQuint);
        }

        private void onIdle()
        {
            hideIndicator.FadeIn(300, Easing.OutQuint);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (LLin != null)
            {
                LLin.OnIdle -= onIdle;
                LLin.OnActive -= resumeFromIdle;
            }

            base.Dispose(isDisposing);
        }

        public bool AddFunctionControl(IFunctionProvider provider)
        {
            SimpleBarButton button;

            if (provider is IToggleableFunctionProvider toggleableFunctionProvider)
                button = new ToggleableBarButton(toggleableFunctionProvider);
            else
                button = new SimpleBarButton(provider);

            switch (provider.Type)
            {
                case FunctionType.ProgressDisplay:
                    button.Dispose();
                    contentContainer.Add(new SongProgressButton((IToggleableFunctionProvider)provider));
                    break;

                default:
                    contentContainer.Add(button);
                    break;
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
            idleIndicator.FlashColour(Color4.Green, 500);
        }

        public List<IPluginFunctionProvider> GetAllPluginFunctionButton() => pluginButtons;

        public Action OnDisable { get; set; }

        public override bool Disable()
        {
            OnDisable?.Invoke();
            return base.Disable();
        }

        public class ButtonNotFoundException : Exception
        {
            public ButtonNotFoundException(IFunctionProvider provider)
                : base($"无法找到与{provider.ToString()}对应的按钮")
            {
            }
        }
    }
}
