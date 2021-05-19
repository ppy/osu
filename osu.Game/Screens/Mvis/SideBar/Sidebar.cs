using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.SideBar.Header;
using osu.Game.Screens.Mvis.Skinning;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.SideBar
{
    internal class Sidebar : VisibilityContainer
    {
        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        public readonly List<ISidebarContent> Components = new List<ISidebarContent>();
        private readonly TabHeader header;
        private const float duration = 400;
        private HeaderTabItem prevTab;

        [CanBeNull]
        private Box sidebarBg;

        public BindableBool IsVisible = new BindableBool();
        public Bindable<Drawable> CurrentDisplay = new Bindable<Drawable>();

        private readonly Container<Drawable> contentContainer;
        protected override Container<Drawable> Content => contentContainer;

        private Sample sampleToggle;
        private Sample samplePopIn;
        private Sample samplePopOut;

        private bool startFromHiddenState;
        private readonly Container content;
        private bool isFirstHide = true;

        public Sidebar()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new ClickToCloseBox
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.7f),
                    Action = () =>
                    {
                        if (!content.IsHovered)
                            Hide();
                    }
                },
                content = new BlockClickContainer
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.3f, 1f),
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                        Colour = Color4.Black.Opacity(0.5f)
                    },
                    Children = new Drawable[]
                    {
                        header = new TabHeader
                        {
                            Depth = float.MinValue
                        },
                        contentContainer = new Container
                        {
                            Name = "Content",
                            RelativeSizeAxes = Axes.Both
                        },
                        new Footer.Footer
                        {
                            Depth = float.MinValue
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleToggle = audio.Samples.Get("UI/overlay-pop-in");
            samplePopIn = audio.Samples.Get("UI/overlay-pop-in");
            samplePopOut = audio.Samples.Get("UI/overlay-pop-out");

            content.Add(new SkinnableComponent(
                "MSidebar-background",
                confineMode: ConfineMode.ScaleToFill,
                defaultImplementation: _ => sidebarBg = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background6,
                    Depth = float.MaxValue
                })
            {
                Name = "侧边栏背景",
                Depth = float.MaxValue,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                ChildAnchor = Anchor.BottomRight,
                ChildOrigin = Anchor.BottomRight,
                RelativeSizeAxes = Axes.Both,
                CentreComponent = false,
                OverrideChildAnchor = true,
            });

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                sidebarBg?.FadeColour(colourProvider.Background6);
            }, true);
        }

        protected override void LoadComplete()
        {
            CurrentDisplay.BindValueChanged(onCurrentDisplayChanged);
            base.LoadComplete();
        }

        private void onCurrentDisplayChanged(ValueChangedEvent<Drawable> v)
        {
            if (!(v.NewValue is ISidebarContent sc)) return;

            prevTab?.MakeInActive();

            foreach (var t in header.Tabs)
            {
                if (t.Value == sc)
                {
                    t.MakeActive();
                    prevTab = t;
                    break;
                }
            }

            if (!startFromHiddenState)
                sampleToggle?.Play();
        }

        protected override void UpdateAfterChildren()
        {
            contentContainer.Padding = new MarginPadding
            {
                Top = header.Height + header.DrawPosition.Y,
                Bottom = mvisScreen.BottombarHeight
            };

            base.UpdateAfterChildren();
        }

        public void ShowComponent(Drawable d)
        {
            if (!(d is ISidebarContent c))
                throw new InvalidOperationException($"{d}不是{typeof(ISidebarContent)}");

            if (!Components.Contains(c))
                throw new InvalidOperationException($"组成部分中不包含{c}");

            if (c.ResizeWidth < 0.3f || c.ResizeHeight < 0.3f)
                throw new InvalidOperationException("组件过小, 缩放大小不能小于30%(0.3)");

            startFromHiddenState = State.Value == Visibility.Hidden;

            Show();

            //如果要显示的是当前正在显示的内容，则中断
            if (CurrentDisplay.Value == d)
            {
                return;
            }

            var resizeDuration = startFromHiddenState ? 0 : duration;

            var lastDisplay = CurrentDisplay.Value;
            lastDisplay?.FadeOut(resizeDuration / 2, Easing.OutQuint)
                       .OnComplete(_ => contentContainer.Remove(lastDisplay));

            if (!contentContainer.Contains(d))
                contentContainer.Add(d);

            //如果某一个侧边栏元素在较短的时间内切换，那么FadeTo(0.01f)可以打断上面的OnComplete
            d.FadeTo(0.01f).FadeTo(0).Then()
             .Delay(resizeDuration / 2).FadeIn(resizeDuration / 2);

            CurrentDisplay.Value = d;

            content.ResizeTo(new Vector2(c.ResizeWidth, c.ResizeHeight), resizeDuration, Easing.OutQuint);
        }

        private void addDrawableToList(Drawable d)
        {
            if (d is ISidebarContent s)
            {
                d.Alpha = 0;
                Components.Add(s);
                header.Tabs.Add(new HeaderTabItem(s)
                {
                    Action = () => ShowComponent(d)
                });
            }
        }

        public override void Add(Drawable drawable) => addDrawableToList(drawable);

        public override void Clear(bool disposeChildren)
        {
            header.Tabs.Clear(disposeChildren);
            contentContainer.Clear(disposeChildren);
        }

        public override bool Remove(Drawable drawable)
        {
            if (drawable is ISidebarContent sc)
            {
                foreach (var t in header.Tabs)
                {
                    if (t.Value == sc)
                    {
                        header.Tabs.Remove(t);
                        drawable.Expire();
                        return true;
                    }
                }
            }

            return base.Remove(drawable);
        }

        protected override void PopOut()
        {
            if (!isFirstHide)
                samplePopOut?.Play();
            else
                isFirstHide = false;

            content.MoveToX(100, duration + 100, Easing.OutQuint);
            this.FadeOut(duration + 100, Easing.OutQuint);

            contentContainer.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);

            IsVisible.Value = false;
        }

        protected override void PopIn()
        {
            samplePopIn?.Play();

            content.MoveToX(0, duration + 100, Easing.OutQuint);
            this.FadeIn(duration + 100, Easing.OutQuint);

            contentContainer.FadeIn(WaveContainer.APPEAR_DURATION, Easing.OutQuint);

            IsVisible.Value = true;
        }

        private class ClickToCloseBox : Box
        {
            public Action Action;

            protected override bool OnClick(ClickEvent e)
            {
                Action?.Invoke();
                return base.OnClick(e);
            }
        }

        private class BlockClickContainer : Container
        {
            protected override bool OnClick(ClickEvent e) => true;
            protected override bool OnMouseDown(MouseDownEvent e) => true;
        }
    }
}
