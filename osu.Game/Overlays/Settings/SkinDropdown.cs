// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
                    public DrawableSkinDropdownMenuItem(MenuItem item)
                        : base(item)
                    {
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
                                    content.Star.SkinData = skin;
                            });
                        }
                    }

                    public override bool ChangeFocusOnClick => false;

                    protected override bool OnClick(ClickEvent e)
                    {
                        if (e.AltPressed && Foreground.Children.FirstOrDefault() is Content content)
                        {
                            content.Star.TriggerFavouriteChange();
                            return true;
                        }

                        return base.OnClick(e);
                    }

                    protected override void UpdateForegroundColour()
                    {
                        base.UpdateForegroundColour();

                        if (Foreground.Children.FirstOrDefault() is Content content)
                            content.Hovering = IsHovered;
                    }

                    public partial class StarButton : SpriteIcon
                    {
                        public bool IsFavourite;
                        public SkinInfo? SkinData;

                        private SkinDropdownMenu? menu;
                        private bool isStarHovered { get; set; }
                        public bool IsStarHovered => isStarHovered;
                        private OverlayColourProvider? colourProvider;

                        public override bool ChangeFocusOnClick => false;

                        [BackgroundDependencyLoader(true)]
                        private void load(OverlayColourProvider? colourProvider, RealmAccess realm)
                        {
                            this.colourProvider = colourProvider;

                            if (SkinData == null) return;

                            var skin = realm.Run(r => r.All<SkinInfo>().FirstOrDefault(s => s.ID == SkinData.ID));
                            if (skin != null)
                            {
                                IsFavourite = skin.IsFavourite;
                                Alpha = IsFavourite ? 1 : 0;
                                X = Content.StarOffset;

                                changeStarButtonState(IsFavourite);
                            }
                        }

                        protected override void LoadComplete()
                        {
                            menu = this.FindClosestParent<SkinDropdownMenu>();
                            base.LoadComplete();
                        }

                        private void changeStarButtonState(bool currentState)
                        {
                            if (currentState)
                            {
                                this.FadeColour(Colour4.Gold, 250, Easing.OutQuint);
                            }
                            else
                            {
                                Colour4 NewColor = colourProvider?.Background5 ?? Color4.Black;

                                this.FadeColour(NewColor, 250, Easing.OutQuint);
                            }

                            this.ScaleTo(1.35f, 250, Easing.OutQuint);
                            this.DelayUntilTransformsFinished().ScaleTo(1.0f, 250, Easing.OutQuint);
                        }

                        protected override bool OnClick(ClickEvent e)
                        {
                            if (!e.AltPressed && !isStarHovered)
                                return false;

                            TriggerFavouriteChange();
                            return true;
                        }

                        protected override bool OnHover(HoverEvent e)
                        {
                            isStarHovered = true;

                            if (IsFavourite)
                            {
                                this.FadeColour(Colour4.Gold.Lighten(0.3f), 250, Easing.OutQuint);
                                this.ScaleTo(1.25f, 250, Easing.OutQuint);
                            }
                            else
                            {
                                this.FadeColour(Colour4.Gold, 250, Easing.In);
                                this.ScaleTo(1.15f, 250, Easing.In);
                            }
                            return true;
                        }

                        protected override void OnHoverLost(HoverLostEvent e)
                        {
                            isStarHovered = false;

                            if (IsFavourite)
                                this.FadeColour(Colour4.Gold, 250, Easing.OutQuint);
                            else
                            {
                                this.FadeColour(colourProvider?.Background5 ?? Color4.Black, 550, Easing.OutQuint);
                            }

                            this.ScaleTo(1.0f, 350, Easing.OutQuint);
                        }

                        public bool TriggerFavouriteChange()
                        {
                            IsFavourite = !IsFavourite;
                            changeStarButtonState(IsFavourite);
                            if (SkinData != null)
                            {
                                menu?.TrackFavouriteChange(SkinData.ID, IsFavourite);
                            }
                            return true;
                        }
                    }

                    protected override Drawable CreateContent() => new Content();

                    protected new partial class Content : CompositeDrawable, IHasText
                    {
                        [BackgroundDependencyLoader(true)]
                        private void load(OverlayColourProvider? colourProvider)
                        {
                            Star.Colour = colourProvider?.Background5 ?? Color4.Black;
                        }

                        public LocalisableString Text
                        {
                            get => Label.Text;
                            set => Label.Text = value;
                        }

                        public readonly OsuSpriteText Label;
                        public readonly StarButton Star;
                        public static float StarOffset = 6;

                        private bool hovering;

                        public bool Hovering
                        {
                            get => hovering;
                            set
                            {
                                if (value == hovering)
                                    return;

                                hovering = value;

                                if (hovering || Star.IsFavourite)
                                {
                                    Star.FadeIn(400, Easing.OutQuint);
                                    Star.MoveToX(StarOffset, 200, Easing.In);
                                }
                                else if (!hovering && !Star.IsFavourite && !Star.IsStarHovered)
                                {
                                    Star.FadeOut(200);
                                    Star.MoveToX(3, 400, Easing.OutQuint);
                                }
                            }
                        }

                        public Content()
                        {
                            RelativeSizeAxes = Axes.X;
                            AutoSizeAxes = Axes.Y;

                            InternalChildren = new Drawable[]
                            {
                                Star = new StarButton()
                                {
                                    Icon = FontAwesome.Solid.Star,
                                    Size = new Vector2(10),
                                    BypassAutoSizeAxes = Axes.Y,
                                    Alpha = 0,
                                    X = 3,
                                    Y = 1,
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
