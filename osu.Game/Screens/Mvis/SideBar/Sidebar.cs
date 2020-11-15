using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.Modules;
using osu.Game.Screens.Mvis.SideBar.Header;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar
{
    public class Sidebar : VisibilityContainer
    {
        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private readonly List<Drawable> components = new List<Drawable>();
        private readonly WaveContainer waveContainer;
        private readonly TabHeader header;
        private const float duration = 400;
        private Box sidebarBg;

        public bool IsHidden = true;
        public bool Hiding;
        public Bindable<Drawable> CurrentDisplay = new Bindable<Drawable>();

        private readonly Container<Drawable> contentContainer;
        protected override Container<Drawable> Content => contentContainer;

        public Sidebar()
        {
            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.3f, 1f);

            InternalChild = waveContainer = new WaveContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    header = new TabHeader(),
                    contentContainer = new Container
                    {
                        Name = "Content",
                        RelativeSizeAxes = Axes.Both
                    },
                    new Footer.Footer()
                }
            };
        }

        private bool headerShown = true;

        [BackgroundDependencyLoader]
        private void load()
        {
            contentContainer.AddRange(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Depth = float.MaxValue,
                    Child = new SkinnableSprite("MSidebar-background", confineMode: ConfineMode.ScaleToFill)
                    {
                        Name = "侧边栏背景图",
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        ChildAnchor = Anchor.BottomRight,
                        ChildOrigin = Anchor.BottomRight,
                        RelativeSizeAxes = Axes.Both,
                        CentreComponent = false,
                        OverrideChildAnchor = true,
                    }
                },
                new ShowTabsButton
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Depth = float.MinValue,
                    Action = () =>
                    {
                        if (headerShown)
                        {
                            header.MoveToY(-header.DrawHeight, 300, Easing.OutQuint).FadeOut(300);
                            headerShown = false;
                        }
                        else
                        {
                            header.FadeIn().MoveToY(0, 300, Easing.OutQuint);
                            headerShown = true;
                        }
                    },
                    Margin = new MarginPadding(30)
                }
            });

            waveContainer.Add(sidebarBg = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Background5,
                Alpha = 0.5f,
                Depth = float.MaxValue
            });

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                updateWaves();
                sidebarBg.Colour = colourProvider.Background5;
            }, true);
        }

        protected override void LoadComplete()
        {
            CurrentDisplay.BindValueChanged(onCurrentDisplayChanged);
            base.LoadComplete();
        }

        private HeaderTabItem prevTab;

        private void onCurrentDisplayChanged(ValueChangedEvent<Drawable> v)
        {
            if (!(v.NewValue is ISidebarContent)) return;

            var sc = (ISidebarContent)v.NewValue;
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
        }

        private void updateWaves()
        {
            //与其他Overlay保持一致
            waveContainer.FirstWaveColour = colourProvider.Light4;
            waveContainer.SecondWaveColour = colourProvider.Light3;
            waveContainer.ThirdWaveColour = colourProvider.Dark4;
            waveContainer.FourthWaveColour = colourProvider.Dark3;
        }

        protected override void UpdateAfterChildren()
        {
            contentContainer.Padding = new MarginPadding { Top = header.Height + header.DrawPosition.Y, Bottom = 50 };
            base.UpdateAfterChildren();
        }

        public void ResizeFor(Drawable d)
        {
            if (!(d is ISidebarContent) || !components.Contains(d)) return;

            var c = (ISidebarContent)d;
            Show();

            //如果要显示的是当前正在显示的内容，则中断
            if (CurrentDisplay.Value == d)
            {
                IsHidden = false;
                return;
            }

            var resizeDuration = IsHidden ? 0 : duration;

            CurrentDisplay.Value?.FadeOut(resizeDuration / 2, Easing.OutQuint);

            CurrentDisplay.Value = d;

            d.Delay(resizeDuration / 2).FadeIn(resizeDuration / 2);

            this.ResizeTo(new Vector2(c.ResizeWidth, c.ResizeHeight), resizeDuration, Easing.OutQuint);
            IsHidden = false;
        }

        private void addDrawableToList(Drawable d)
        {
            if (d is ISidebarContent s)
            {
                d.Alpha = 0;
                components.Add(d);
                contentContainer.Add(d);
                header.Tabs.Add(new HeaderTabItem(s)
                {
                    Action = () => ResizeFor(d)
                });
            }
        }

        public override void Add(Drawable drawable) => addDrawableToList(drawable);

        protected override void PopOut()
        {
            waveContainer.Hide();
            Hiding = true;
            this.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.InExpo).OnComplete(_ => IsHidden = true);
        }

        protected override void PopIn()
        {
            waveContainer.Show();
            Hiding = false;
            this.FadeIn(200);
        }
    }
}
