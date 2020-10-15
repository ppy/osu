using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Mvis.Modules
{
    //TODO: 当做Mvis播放器内部的悬浮窗使用
    public class CollectionSelectScreen : OsuScreen
    {
        [Resolved]
        private CollectionManager collectionManager { get; set; }

        [Cached]
        public readonly Bindable<BeatmapCollection> CurrentCollection = new Bindable<BeatmapCollection>();

        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue1);
        private FillFlowContainer collectionsFillFlow;
        private OsuSpriteText titleText;
        private Container contentContainer;

        public CollectionSelectScreen()
        {
            InternalChildren = new Drawable[]
            {
                contentContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Name = "Basic Container",
                    CornerRadius = 25f,
                    Masking = true,
                    Size = new Vector2(0.6f, 0.8f),

                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new Dimension[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Distributed),
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Name = "Header",
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = colourProvider.Background4,
                                            },
                                            titleText = new OsuSpriteText
                                            {
                                                Text = "选择收藏夹",
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Margin = new MarginPadding(15),
                                                Font = OsuFont.GetFont(size: 30)
                                            }
                                        }
                                    },
                                },
                                new Drawable[]
                                {
                                    new OsuScrollContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Child = collectionsFillFlow = new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                            Spacing = new Vector2(10),
                                            Padding = new MarginPadding(10)
                                        }
                                    }
                                },
                                new[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = colourProvider.Background4,
                                            },
                                            new TriangleButton()
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Width = 0.5f,
                                                Margin = new MarginPadding(15),
                                                Text = "选中该收藏夹",
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Action = () =>
                                                {
                                                    CurrentCollection.TriggerChange();
                                                    this.Exit();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            AddCollections();

            CurrentCollection.BindValueChanged(v =>
            {
                titleText.Text = string.IsNullOrEmpty(v.NewValue.Name.Value) ? "选择收藏夹" : v.NewValue.Name.Value;
            });
        }

        private void AddCollections()
        {
            collectionsFillFlow.Clear();

            foreach (var collection in collectionManager.Collections)
            {
                collectionsFillFlow.Add(new CollectionPill(collection));
            }
        }

        protected class CollectionPill : CircularContainer
        {
            private Box flashBox;
            private BeatmapCollection collection;

            [Resolved]
            private Bindable<BeatmapCollection> currentCollection { get; set; }

            public CollectionPill(BeatmapCollection collection)
            {
                RelativeSizeAxes = Axes.X;
                Height = 35;
                Masking = true;
                this.collection = collection;

                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Colour4.Black.Opacity(0.6f),
                        RelativeSizeAxes = Axes.Both
                    },
                    new OsuSpriteText
                    {
                        Text = collection.Name.Value,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Margin = new MarginPadding{Left = 20}
                    },
                    flashBox = new Box
                    {
                        Colour = Colour4.White.Opacity(0.1f),
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0
                    }
                };
            }

            protected override bool OnClick(ClickEvent e)
            {
                currentCollection.Value = collection;
                return base.OnClick(e);
            }

            protected override bool OnHover(HoverEvent e)
            {
                flashBox.FadeIn(300);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                flashBox.FadeOut(300);
                base.OnHoverLost(e);
            }
        }
    
        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            contentContainer.FadeOut().Then().ScaleTo(0.8f).RotateTo(-15).MoveToX(300)
                            .Then()
                            .ScaleTo(1, 1500, Easing.OutElastic)
                            .FadeIn(500)
                            .MoveToX(0, 500, Easing.OutQuint)
                            .RotateTo(0, 500, Easing.OutQuint);
        }

        public override bool OnExiting(IScreen next)
        {
            contentContainer.ScaleTo(0.8f, 500, Easing.OutExpo).RotateTo(-15, 500, Easing.OutExpo).MoveToX(300, 500, Easing.OutQuint).FadeOut(500);
            this.FadeOut(500, Easing.OutExpo);

            return base.OnExiting(next);
        }

    }
}