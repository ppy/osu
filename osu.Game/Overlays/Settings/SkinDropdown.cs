// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Input.StateChanges;

namespace osu.Game.Overlays.Settings
{
    public partial class SkinDropdown : SettingsDropdown<Live<SkinInfo>>
    {
        protected override OsuDropdown<Live<SkinInfo>> CreateDropdown() => new SkinDropdownControl();

        private partial class SkinDropdownControl : DropdownControl
        {
            private SkinDropdownMenu? skinMenu;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (skinMenu != null)
                    skinMenu.StateChanged += onMenuStateChanged;
            }

            protected override void Dispose(bool isDisposing)
            {
                if (skinMenu != null)
                    skinMenu.StateChanged -= onMenuStateChanged;

                base.Dispose(isDisposing);
            }

            private void onMenuStateChanged(MenuState state)
            {
                if (state == MenuState.Closed)
                    skinMenu?.CommitFavouriteChanges();
            }

            protected override DropdownMenu CreateMenu() => skinMenu = new SkinDropdownMenu();

            public partial class SkinDropdownMenu : OsuDropdownMenu
            {
                [BackgroundDependencyLoader(true)]
                private void load(OverlayColourProvider? colourProvider, OsuColour colours)
                {
                    BackgroundColour = colourProvider?.Background5 ?? Color4.Black;
                    HoverColour = colourProvider?.Light4 ?? colours.PinkDarker;
                    SelectionColour = colourProvider?.Background3 ?? colours.PinkDarker.Opacity(0.5f);

                    MaxHeight = 200;
                }

                [Resolved]
                private RealmAccess realm { get; set; } = null!;

                private readonly Dictionary<Guid, bool> pendingFavouriteChanges = new();

                public void TrackFavouriteChange(Guid skinID, bool isFavourite)
                {
                    pendingFavouriteChanges[skinID] = isFavourite;
                }

                public void CommitFavouriteChanges()
                {
                    realm.Write(r =>
                    {
                        foreach (var (skinID, isFavourite) in pendingFavouriteChanges)
                        {
                            var skin = r.All<SkinInfo>().FirstOrDefault(s => s.ID == skinID);
                            if (skin != null)
                                skin.IsFavourite = isFavourite;
                        }
                    });

                    pendingFavouriteChanges.Clear();
                }

                protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item) => new DrawableSkinDropdownMenuItem(item)
                {
                    BackgroundColourHover = HoverColour,
                    BackgroundColourSelected = SelectionColour
                };

                public partial class DrawableSkinDropdownMenuItem : DrawableOsuDropdownMenuItem
                {
                    [BackgroundDependencyLoader(true)]
                    private void load(OverlayColourProvider? colourProvider)
                    {
                        this.colourProvider = colourProvider;
                    }

                    private Content? content;

                    public bool IsFavourite;

                    public SkinInfo? SkinData;

                    private SkinDropdownMenu? menu;

                    private bool favouriteStarVisible = false; // Rename to favouriteStarButtonVisible. favouriteStar relates more to the indicator

                    private OverlayColourProvider? colourProvider;

                    private ClickableContainer starContainer = null!;

                    private SpriteIcon starIcon = null!;

                    private Box starBackground = null!;

                    public DrawableSkinDropdownMenuItem(MenuItem item)
                        : base(item)
                    {
                        AddInternal(starContainer = new NoFocusChangeClickableContainer
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 0,
                            Depth = float.MaxValue,
                            Alpha = 0,
                            Action = () =>
                            {
                                starBackground
                                    .FadeColour(Colour4.Gold, 150, Easing.OutQuint)
                                    .Then()
                                    .FadeColour(Colour4.FromHex(@"#edae00"), 250, Easing.OutQuint);

                                starIcon
                                    .FadeColour(Colour4.White, 150, Easing.OutQuint)
                                    .ScaleTo(0.8f, 150, Easing.OutQuint)
                                    .Then()
                                    .FadeColour(Colour4.White.Opacity(0.7f), 250, Easing.OutQuint)
                                    .ScaleTo(1.0f, 250, Easing.OutQuint);

                                TriggerFavouriteChange();
                            },
                            Children = [
                                starBackground = new Box(){
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Colour4.FromHex(@"#edae00"),
                                    Depth = 2
                                },
                                starIcon = new SpriteIcon(){
                                    Icon = FontAwesome.Solid.Star,
                                    Size = new Vector2(10),
                                    BypassAutoSizeAxes = Axes.Y,
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Colour = Colour4.White.Opacity(0.7f),
                                    Depth = 1,
                                }
                            ]
                        });
                        Foreground.Padding = new MarginPadding(2);
                        Foreground.AutoSizeAxes = Axes.Y;
                        Foreground.RelativeSizeAxes = Axes.X;

                        Masking = true;
                        CornerRadius = 5;

                        if (item is DropdownMenuItem<Live<SkinInfo>> skinItem)
                        {
                            skinItem.Value.PerformRead(skin =>
                            {
                                if (Foreground.Children.FirstOrDefault() is Content content)
                                    SkinData = skin;
                            });
                        }
                    }

                    private partial class NoFocusChangeClickableContainer : ClickableContainer
                    {
                        public override bool ChangeFocusOnClick => false;
                    }

                    protected override bool OnDragStart(DragStartEvent e)
                    {
                        if (e.CurrentState.Mouse.LastSource is ISourcedFromTouch)
                        {
                            toggleSlideStar(true, 0);
                            bool mostlyHorizontal = Math.Abs(e.Delta.X) > Math.Abs(e.Delta.Y);

                            return mostlyHorizontal;
                        }

                        return base.OnDragStart(e);
                    }

                    protected override void OnDrag(DragEvent e)
                    {
                        adjustOnDrag(e.Delta);

                        base.OnDrag(e);
                    }

                    private float dragDelta;

                    private void adjustOnDrag(Vector2 delta)
                    {
                        dragDelta += delta.X / 2;

                        if (Math.Abs(dragDelta) < 0.01) return;

                        dragDelta = Math.Clamp(dragDelta, 0, 100);
                        Background.MoveToX(dragDelta, 100, Easing.OutQuint);
                        Foreground.MoveToX(dragDelta, 100, Easing.OutQuint);
                        starContainer.ResizeWidthTo(dragDelta, 100, Easing.OutQuint);
                    }

                    private void toggleSlideStar(bool newValue, int itemOffset = 20) // Rename to something better (toggleStarButton? )
                    {
                        if (!newValue && !favouriteStarVisible)
                        {
                            return;
                        }
                        else if (!newValue)
                        {
                            itemOffset = 0;
                            Background.MoveToX(itemOffset, 250, Easing.OutQuint);
                            Foreground.MoveToX(itemOffset, 250, Easing.OutQuint);
                            starContainer.FadeOutFromOne(250, Easing.OutQuint);
                            starContainer.ResizeWidthTo(itemOffset - 3, 250, Easing.OutQuint).Then().ResizeWidthTo(itemOffset, 250, Easing.OutQuint);
                        }
                        else
                        {
                            starContainer.FadeInFromZero(100, Easing.OutQuint);
                            Background.MoveToX(itemOffset, 100, Easing.OutQuint);
                            Foreground.MoveToX(itemOffset, 100, Easing.OutQuint);
                            starContainer.ResizeWidthTo(dragDelta, 100, Easing.OutQuint);
                        }
                        favouriteStarVisible = newValue;
                    }

                    private int itemOffset = 0;
                    protected override void OnDragEnd(DragEndEvent e)
                    {
                        toggleSlideStar(!favouriteStarVisible);

                        if (dragDelta >= 50)
                            TriggerFavouriteChange();

                        dragDelta = 0;
                        itemOffset = 0;

                        base.OnDragEnd(e);
                    }

                    private int hoverSlideThreshold = 25;

                    protected override bool OnMouseMove(MouseMoveEvent e)
                    {
                        if (e.CurrentState.Mouse.LastSource is ISourcedFromTouch)
                            return base.OnMouseMove(e);

                        if (e.MousePosition.X < hoverSlideThreshold && !favouriteStarVisible)
                        {
                            adjustOnDrag(new Vector2(40, 0));
                            toggleSlideStar(true);
                        }
                        else if (e.MousePosition.X >= hoverSlideThreshold && favouriteStarVisible)
                        {
                            adjustOnDrag(new Vector2(-40, 0));
                            toggleSlideStar(false);
                        }

                        return base.OnMouseMove(e);
                    }

                    protected override void OnHoverLost(HoverLostEvent e)
                    {
                        adjustOnDrag(new Vector2(-40, 0));
                        toggleSlideStar(false);

                        base.OnHoverLost(e);
                    }

                    public override bool ChangeFocusOnClick => false;

                    protected override bool OnClick(ClickEvent e)
                    {
                        if (e.AltPressed)
                        {
                            TriggerFavouriteChange();
                            return true;
                        }

                        return base.OnClick(e);
                    }

                    public bool TriggerFavouriteChange()
                    {
                        IsFavourite = !IsFavourite;
                        content?.FavouriteIndicator.ChangeFavouriteIndicatorState(IsFavourite);
                        if (SkinData != null)
                        {
                            menu?.TrackFavouriteChange(SkinData.ID, IsFavourite);
                        }
                        return true;
                    }

                    public partial class FavouriteIndicator : SpriteIcon
                    {
                        private bool isStarHovered { get; set; }

                        public bool IsStarHovered => isStarHovered;

                        private DrawableSkinDropdownMenuItem skinItem;

                        public FavouriteIndicator(DrawableSkinDropdownMenuItem skinItemInstance)
                        {
                            skinItem = skinItemInstance;
                        }

                        public override bool ChangeFocusOnClick => false;

                        private OverlayColourProvider? colourProvider;

                        [BackgroundDependencyLoader(true)]
                        private void load(OverlayColourProvider? colourProvider, RealmAccess realm)
                        {
                            this.colourProvider = colourProvider;
                            if (skinItem.SkinData == null) return;
                            Alpha = skinItem.IsFavourite ? 1 : 0;

                            var skin = realm.Run(r => r.All<SkinInfo>().FirstOrDefault(s => s.ID == skinItem.SkinData.ID));
                            if (skin != null)
                            {
                                skinItem.IsFavourite = skin.IsFavourite;
                                changeFavouriteIndicatorState(skinItem.IsFavourite);
                            }
                        }

                        public void ChangeFavouriteIndicatorState(bool currentState)
                        {
                            changeFavouriteIndicatorState(currentState);
                        }

                        private void changeFavouriteIndicatorState(bool currentState)
                        {
                            this.ScaleTo(1.35f, 250, Easing.OutQuint).Then().ScaleTo(1.0f, 250, Easing.OutQuint);
                            if (currentState)
                                this.FadeInFromZero(250, Easing.OutQuint);
                            else
                                this.Delay(250).FadeOutFromOne(250, Easing.OutQuint);
                        }

                        protected override void LoadComplete()
                        {
                            skinItem.menu = this.FindClosestParent<SkinDropdownMenu>();
                            base.LoadComplete();
                        }
                    }

                    protected override Drawable CreateContent()
                    {
                        content = new Content(this, colourProvider);
                        return content;
                    }

                    protected new partial class Content : CompositeDrawable, IHasText
                    {

                        public LocalisableString Text
                        {
                            get => Label.Text;
                            set => Label.Text = value;
                        }

                        public readonly OsuSpriteText Label;

                        private readonly DrawableSkinDropdownMenuItem skinItem;

                        public readonly FavouriteIndicator FavouriteIndicator;

                        public Content(DrawableSkinDropdownMenuItem skinItem, OverlayColourProvider? colourProvider)
                        {
                            this.skinItem = skinItem;

                            RelativeSizeAxes = Axes.X;
                            AutoSizeAxes = Axes.Y;

                            InternalChildren = new Drawable[]
                            {
                                FavouriteIndicator = new FavouriteIndicator(skinItem)
                                {
                                    Icon = FontAwesome.Solid.Star,
                                    Colour = Colour4.Gold,
                                    Size = new Vector2(10),
                                    BypassAutoSizeAxes = Axes.Y,
                                    X = 6,
                                    Y = 0,
                                    Margin = new MarginPadding { Horizontal = 2 },
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreLeft,
                                },
                                Label = new TruncatingSpriteText
                                {
                                    Padding = new MarginPadding { Left = 16 },
                                    Origin = Anchor.CentreLeft,
                                    Anchor = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.X,
                                },
                            };
                        }
                    }
                }
            }
        }
    }
}
