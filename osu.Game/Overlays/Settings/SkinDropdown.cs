// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;
using osuTK.Input;
using osu.Framework.Input.StateChanges;

namespace osu.Game.Overlays.Settings
{
    public partial class SkinDropdown : FormDropdown<Live<SkinInfo>>
    {
        protected override LocalisableString GenerateItemText(Live<SkinInfo> item) => item.ToString() ?? string.Empty;

        protected override DropdownMenu CreateMenu() => new SkinDropdownMenu();

        public partial class SkinDropdownMenu : OsuDropdownMenu
        {
            [BackgroundDependencyLoader(true)]
            private void load()
            {
                MaxHeight = 200;
            }

            protected override DrawableDropdownMenuItem CreateDrawableDropdownMenuItem(MenuItem item)
            {
                if (item is DropdownMenuItem<Live<SkinInfo>> skinItem)
                {
                    bool isProtected = false;
                    bool isRandomSkin = false;
                    skinItem.Value.PerformRead(skin =>
                    {
                        isProtected = skin.Protected;
                        isRandomSkin = skin.ID == SkinInfo.RANDOM_SKIN;
                    });

                    if (isProtected || isRandomSkin)
                    {
                        return new DrawableOsuDropdownMenuItem(item)
                        {
                            BackgroundColourHover = HoverColour,
                            BackgroundColourSelected = SelectionColour
                        };
                    }
                }

                return new DrawableSkinDropdownMenuItem(item)
                {
                    BackgroundColourHover = HoverColour,
                    BackgroundColourSelected = SelectionColour
                };
            }

            public partial class DrawableSkinDropdownMenuItem : DrawableOsuDropdownMenuItem
            {
                private bool isFavourite;

                private float dragDelta;

                private Content content = null!;

                public SkinInfo SkinData = null!;

                private Sample sampleShow = null!;

                private Sample sampleHide = null!;

                private readonly Box starBackground;

                private readonly SpriteIcon starIcon;

                private const int favourite_drag_end_threshold = 50;

                private readonly ClickableContainer starContainer;

                public override bool ChangeFocusOnClick => false;

                [Resolved]
                private RealmAccess realm { get; set; } = null!;

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
                            starButtonFlash();
                            triggerFavouriteChange();
                        },
                        Children =
                        [
                            starBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.FromHex(@"#edae00"),
                                Depth = 1
                            },
                            starIcon = new SpriteIcon
                            {
                                Size = new Vector2(10),
                                BypassAutoSizeAxes = Axes.Y,
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                Colour = Colour4.White.Opacity(0.7f),
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
                        skinItem.Value.PerformRead(skin => SkinData = skin);
                    }
                }

                [BackgroundDependencyLoader(true)]
                private void load(AudioManager audio)
                {
                    updateIcon();

                    sampleShow = audio.Samples.Get(@"UI/check-on");
                    sampleHide = audio.Samples.Get(@"UI/check-off");
                }

                protected override bool OnDragStart(DragStartEvent e)
                {
                    if (e.CurrentState.Mouse.LastSource is ISourcedFromTouch)
                    {
                        bool mostlyHorizontal = Math.Abs(e.Delta.X) > Math.Abs(e.Delta.Y);

                        if (mostlyHorizontal)
                        {
                            starContainer.FadeTo(1, 100, Easing.OutQuint);
                            if (isFavourite)
                                content.AnimateFavouriteOut();
                        }

                        return mostlyHorizontal;
                    }

                    return base.OnDragStart(e);
                }

                private bool stateChanged;

                protected override void OnDrag(DragEvent e)
                {
                    dragDelta += e.Delta.X / 2;

                    if (Math.Abs(dragDelta) < 0.01) return;

                    if (dragDelta >= favourite_drag_end_threshold && !stateChanged)
                    {
                        stateChanged = true;
                        triggerFavouriteChange(false);
                    }

                    dragDelta = Math.Clamp(dragDelta, 0, 100);
                    Background.MoveToX(dragDelta, 100, Easing.OutQuint);
                    Foreground.MoveToX(dragDelta, 100, Easing.OutQuint);
                    starContainer.ResizeWidthTo(dragDelta, 100, Easing.OutQuint);

                    base.OnDrag(e);
                }

                protected override void OnDragEnd(DragEndEvent e)
                {
                    const int offset = 0;
                    const int duration = 250;
                    const Easing easing = Easing.OutQuint;

                    Background.MoveToX(offset, duration, easing);
                    Foreground.MoveToX(offset, duration, easing);
                    starContainer.FadeTo(0, duration, easing).ResizeWidthTo(offset, duration, easing);

                    if (stateChanged)
                    {
                        realm.Write(r =>
                        {
                            var skin = r.All<SkinInfo>().FirstOrDefault(s => s.ID == SkinData.ID);

                            if (skin.IsNotNull())
                                skin.Favourite = isFavourite;
                        });
                    }

                    if (isFavourite)
                        content.AnimateFavouriteIn();

                    stateChanged = false;
                    dragDelta = 0;

                    base.OnDragEnd(e);
                }

                protected override bool OnHover(HoverEvent e)
                {
                    if (!isFavourite && e.CurrentState.Mouse.LastSource is not ISourcedFromTouch)
                        content.AnimateFavouriteIn();

                    return base.OnHover(e);
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    if (e.CurrentState.Mouse.LastSource is ISourcedFromTouch)
                    {
                        // Do nothing
                    }
                    else if (!isFavourite)
                        content.AnimateFavouriteOut();

                    base.OnHoverLost(e);
                }

                protected override bool OnMouseDown(MouseDownEvent e)
                {
                    if (e.Button == MouseButton.Right)
                        return triggerFavouriteChange();

                    return base.OnMouseDown(e);
                }

                private bool triggerFavouriteChange(bool writeToRealm = true)
                {
                    if (isFavourite)
                        sampleHide.Play();
                    else
                        sampleShow.Play();

                    isFavourite = !isFavourite;
                    updateIcon();

                    content.ChangeFavourite();
                    starButtonFlash();

                    if (writeToRealm)
                    {
                        realm.Write(r =>
                        {
                            var skin = r.All<SkinInfo>().FirstOrDefault(s => s.ID == SkinData.ID);

                            if (skin.IsNotNull())
                                skin.Favourite = isFavourite;
                        });
                    }

                    return true;
                }

                private void updateIcon()
                {
                    starIcon.Icon = isFavourite ? FontAwesome.Solid.Star : FontAwesome.Regular.Star;
                }

                private void starButtonFlash()
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
                }

                public partial class FavouriteIndicator : SpriteIcon
                {
                    private readonly DrawableSkinDropdownMenuItem skinItem;

                    public readonly Colour4 ActiveColour = Colour4.Gold;

                    private readonly Colour4 inactiveColour = Colour4.Black.Opacity(0.4f);

                    public FavouriteIndicator(DrawableSkinDropdownMenuItem skinItemInstance)
                    {
                        skinItem = skinItemInstance;
                    }

                    public override bool ChangeFocusOnClick => false;

                    [BackgroundDependencyLoader(true)]
                    private void load(RealmAccess realm)
                    {
                        Alpha = skinItem.isFavourite ? 1 : 0;

                        var skin = realm.Run(r => r.All<SkinInfo>().FirstOrDefault(s => s.ID == skinItem.SkinData.ID));

                        if (skin != null)
                        {
                            skinItem.isFavourite = skin.Favourite;
                            if (skinItem.isFavourite)
                                ShowFavouriteIndicator(true);
                        }
                    }

                    protected override bool OnClick(ClickEvent e)
                    {
                        if (e.CurrentState.Mouse.LastSource is ISourcedFromTouch)
                            return true;

                        ChangeFavouriteIndicatorState();
                        skinItem.triggerFavouriteChange();

                        return true;
                    }

                    protected override bool OnHover(HoverEvent e)
                    {
                        if (e.CurrentState.Mouse.LastSource is ISourcedFromTouch)
                            return base.OnHover(e);

                        this.ScaleTo(1.25f, 250, Easing.OutQuint);

                        return base.OnHover(e);
                    }

                    protected override void OnHoverLost(HoverLostEvent e)
                    {
                        if (e.CurrentState.Mouse.LastSource is not ISourcedFromTouch)
                            this.ScaleTo(1.0f, 250, Easing.OutQuint);
                    }

                    public void ShowFavouriteIndicator(bool show, int delay = 250, int duration = 250)
                    {
                        Colour = skinItem.isFavourite ? ActiveColour : inactiveColour;

                        if (show)
                        {
                            this.ScaleTo(1.25f, duration, Easing.OutQuint).Then().ScaleTo(1.0f, duration, Easing.OutQuint);
                            this.FadeInFromZero(duration, Easing.OutQuint);
                        }
                        else
                        {
                            this.Delay(delay).ScaleTo(0.8f, duration, Easing.OutQuint);
                            this.Delay(delay).FadeOutFromOne(duration, Easing.OutQuint);
                        }
                    }

                    public void ChangeFavouriteIndicatorState()
                    {
                        this.ScaleTo(1.35f, 250, Easing.OutQuint).Then().ScaleTo(1.0f, 250, Easing.OutQuint);
                        if (skinItem.isFavourite)
                            this.FadeColour(ActiveColour, 250, Easing.OutQuint);
                        else
                            this.FadeColour(inactiveColour, 250, Easing.OutQuint);
                    }
                }

                protected override Drawable CreateContent()
                {
                    content = new Content(this);
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

                    public readonly FavouriteIndicator FavouriteIndicator;

                    public void AnimateFavouriteIn() => FavouriteIndicator.ShowFavouriteIndicator(true);

                    public void AnimateFavouriteOut() => FavouriteIndicator.ShowFavouriteIndicator(false, 100, 350);

                    public void ChangeFavourite() => FavouriteIndicator.ChangeFavouriteIndicatorState();

                    public Content(DrawableSkinDropdownMenuItem skinItem)
                    {
                        RelativeSizeAxes = Axes.X;
                        AutoSizeAxes = Axes.Y;

                        InternalChildren = new Drawable[]
                        {
                            FavouriteIndicator = new FavouriteIndicator(skinItem)
                            {
                                Icon = FontAwesome.Solid.Star,
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

    public partial class NoFocusChangeClickableContainer : ClickableContainer
    {
        public override bool ChangeFocusOnClick => false;
    }
}
