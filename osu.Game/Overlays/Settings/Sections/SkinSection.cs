// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Screens.Select;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;
using Realms;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class SkinSection : SettingsSection
    {
        private SkinSettingsDropdown skinDropdown;

        public override LocalisableString Header => SkinSettingsStrings.SkinSectionHeader;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = OsuIcon.SkinB
        };

        private static readonly Live<SkinInfo> random_skin_info = new SkinInfo
        {
            ID = SkinInfo.RANDOM_SKIN,
            Name = "<Random Skin>",
        }.ToLiveUnmanaged();

        private readonly List<Live<SkinInfo>> dropdownItems = new List<Live<SkinInfo>>();

        [Resolved]
        private SkinManager skins { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; }

        private IDisposable realmSubscription;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load([CanBeNull] SkinEditorOverlay skinEditor)
        {
            Children = new Drawable[]
            {
                skinDropdown = new SkinSettingsDropdown
                {
                    AlwaysShowSearchBar = true,
                    AllowNonContiguousMatching = true,
                    LabelText = SkinSettingsStrings.CurrentSkin,
                    Current = skins.CurrentSkinInfo,
                    Keywords = new[] { @"skins" },
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5, 0),
                    Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS, Right = SettingsPanel.CONTENT_MARGINS },
                    Children = new Drawable[]
                    {
                        // This is all super-temporary until we move skin settings to their own panel / overlay.
                        new RenameSkinButton { Padding = new MarginPadding(), RelativeSizeAxes = Axes.None, Width = 120 },
                        new ExportSkinButton { Padding = new MarginPadding(), RelativeSizeAxes = Axes.None, Width = 120 },
                        new DeleteSkinButton { Padding = new MarginPadding(), RelativeSizeAxes = Axes.None, Width = 110 },
                    }
                },
                new SettingsButton
                {
                    Text = SkinSettingsStrings.SkinLayoutEditor,
                    Action = () => skinEditor?.ToggleVisibility(),
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realmSubscription = realm.RegisterForNotifications(_ => realm.Realm.All<SkinInfo>()
                                                                         .Where(s => !s.DeletePending)
                                                                         .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase), skinsChanged);

            skinDropdown.Current.BindValueChanged(skin =>
            {
                if (skin.NewValue == random_skin_info)
                {
                    // before selecting random, set the skin back to the previous selection.
                    // this is done because at this point it will be random_skin_info, and would
                    // cause SelectRandomSkin to be unable to skip the previous selection.
                    skins.CurrentSkinInfo.Value = skin.OldValue;
                    skins.SelectRandomSkin();
                }
            });
        }

        private void skinsChanged(IRealmCollection<SkinInfo> sender, ChangeSet changes)
        {
            // This can only mean that realm is recycling, else we would see the protected skins.
            // Because we are using `Live<>` in this class, we don't need to worry about this scenario too much.
            if (!sender.Any())
                return;

            // For simplicity repopulate the full list.
            // In the future we should change this to properly handle ChangeSet events.
            dropdownItems.Clear();

            dropdownItems.Add(sender.Single(s => s.ID == SkinInfo.ARGON_SKIN).ToLive(realm));
            dropdownItems.Add(sender.Single(s => s.ID == SkinInfo.ARGON_PRO_SKIN).ToLive(realm));
            dropdownItems.Add(sender.Single(s => s.ID == SkinInfo.TRIANGLES_SKIN).ToLive(realm));
            dropdownItems.Add(sender.Single(s => s.ID == SkinInfo.CLASSIC_SKIN).ToLive(realm));
            dropdownItems.Add(sender.Single(s => s.ID == SkinInfo.RETRO_SKIN).ToLive(realm));

            dropdownItems.Add(random_skin_info);

            foreach (var skin in sender.Where(s => !s.Protected && s.IsFavourite))
            {
                dropdownItems.Add(skin.ToLive(realm));
            }

            foreach (var skin in sender.Where(s => !s.Protected && !s.IsFavourite))
            {
                dropdownItems.Add(skin.ToLive(realm));
            }

            Schedule(() => skinDropdown.Items = dropdownItems);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            realmSubscription?.Dispose();
        }

        private partial class SkinSettingsDropdown : SettingsDropdown<Live<SkinInfo>>
        {
            protected override OsuDropdown<Live<SkinInfo>> CreateDropdown() => new SkinDropdownControl();

            private partial class SkinDropdownControl : DropdownControl
            {
                protected override DropdownMenu CreateMenu() => new SkinDropdownMenu();

                private partial class SkinDropdownMenu : OsuDropdownMenu
                {
                    [BackgroundDependencyLoader(true)]
                    private void load(OverlayColourProvider? colourProvider, OsuColour colours)
                    {
                        BackgroundColour = colourProvider?.Background5 ?? Color4.Black;
                        HoverColour = colourProvider?.Light4 ?? colours.PinkDarker;
                        SelectionColour = colourProvider?.Background3 ?? colours.PinkDarker.Opacity(0.5f);

                        MaxHeight = 200;
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

                            // This was "corner_radius", but now it's hard-coded here due to the private const in the parent class
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

                        [BackgroundDependencyLoader]
                        private void load()
                        {
                            AddInternal(new HoverSounds());
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
                            private bool isStarHovered;
                            private OverlayColourProvider? colourProvider;

                            [Resolved]
                            private RealmAccess realm { get; set; } = null!;

                            [BackgroundDependencyLoader(true)]
                            private void load(OverlayColourProvider? colourProvider, RealmAccess realm)
                            {
                                this.colourProvider = colourProvider;

                                var skin = realm.Run(r => r.All<SkinInfo>().FirstOrDefault(s => s.ID == SkinData.ID));
                                if (skin != null)
                                {
                                    IsFavourite = skin.IsFavourite;
                                    Alpha = IsFavourite ? 1 : 0;
                                    X = Content.StarOffset;
                                    changeStarButtonState(IsFavourite);
                                }
                            }

                            private void changeStarButtonState(bool currentState)
                            {
                                if (currentState)
                                {
                                    this.FadeColour(Colour4.Gold, 250, Easing.OutQuint);
                                    this.DelayUntilTransformsFinished().Schedule(() =>
                                    {
                                        Alpha = 1;
                                    });
                                }
                                else
                                {
                                    Colour4 NewColor = colourProvider?.Background5 ?? Color4.Black;
                                    this.FadeColour(NewColor, 250, Easing.OutQuint);
                                    this.DelayUntilTransformsFinished().Schedule(() =>
                                    {
                                        Alpha = 0;
                                    });
                                }
                            }

                            // todo:
                            //      Touchscreen/mobile support
                            //          Slide to reveal instead of permanently visible star?
                            //      Realm changes are causing a re-render. This instantly refreshes the list, which at this point I don't even know whether is a good or bad UX. Instant change -> good, but scrolls to the top so kinda bad too.
                            //      Fix warnings

                            public override bool ChangeFocusOnClick => false;

                            protected override bool OnClick(ClickEvent e)
                            {
                                IsFavourite = !IsFavourite;
                                changeStarButtonState(IsFavourite);

                                realm.Write(r =>
                                {
                                    var skin = r.All<SkinInfo>().FirstOrDefault(s => s.ID == SkinData.ID);
                                    if (skin != null)
                                        skin.IsFavourite = IsFavourite;
                                });

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

                            public bool IsStarHovered => isStarHovered;
                        }

                        protected override Drawable CreateContent() => new Content();

                        protected new partial class Content : CompositeDrawable, IHasText
                        {
                            public LocalisableString Text
                            {
                                get => Label.Text;
                                set => Label.Text = value;
                            }

                            public readonly OsuSpriteText Label;
                            public readonly StarButton Star;

                            public static float StarOffset = 6;

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

                            [BackgroundDependencyLoader(true)]
                            private void load(OverlayColourProvider? colourProvider)
                            {
                                Star.Colour = colourProvider?.Background5 ?? Color4.Black;
                            }

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
                        }
                    }
                }

                protected override LocalisableString GenerateItemText(Live<SkinInfo> item) => item.ToString();
            }
        }

        public partial class RenameSkinButton : SettingsButton, IHasPopover
        {
            [Resolved]
            private SkinManager skins { get; set; }

            private Bindable<Skin> currentSkin;

            [BackgroundDependencyLoader]
            private void load()
            {
                Text = CommonStrings.Rename;
                Action = this.ShowPopover;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                currentSkin = skins.CurrentSkin.GetBoundCopy();
                currentSkin.BindValueChanged(_ => updateState());
                currentSkin.BindDisabledChanged(_ => updateState(), true);
            }

            private void updateState() => Enabled.Value = !currentSkin.Disabled && currentSkin.Value.SkinInfo.PerformRead(s => !s.Protected);

            public Popover GetPopover()
            {
                return new RenameSkinPopover();
            }
        }

        public partial class ExportSkinButton : SettingsButton
        {
            [Resolved]
            private SkinManager skins { get; set; }

            private Bindable<Skin> currentSkin;

            [BackgroundDependencyLoader]
            private void load()
            {
                Text = CommonStrings.Export;
                Action = export;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                currentSkin = skins.CurrentSkin.GetBoundCopy();
                currentSkin.BindValueChanged(_ => updateState());
                currentSkin.BindDisabledChanged(_ => updateState(), true);
            }

            private void updateState() => Enabled.Value = !currentSkin.Disabled && currentSkin.Value.SkinInfo.PerformRead(s => !s.Protected);

            private void export()
            {
                try
                {
                    skins.ExportCurrentSkin();
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not export current skin: {e.Message}", level: LogLevel.Error);
                }
            }
        }

        public partial class DeleteSkinButton : DangerousSettingsButton
        {
            [Resolved]
            private SkinManager skins { get; set; }

            [Resolved(CanBeNull = true)]
            private IDialogOverlay dialogOverlay { get; set; }

            private Bindable<Skin> currentSkin;

            [BackgroundDependencyLoader]
            private void load()
            {
                Text = WebCommonStrings.ButtonsDelete;
                Action = delete;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                currentSkin = skins.CurrentSkin.GetBoundCopy();
                currentSkin.BindValueChanged(_ => updateState());
                currentSkin.BindDisabledChanged(_ => updateState(), true);
            }

            private void updateState() => Enabled.Value = !currentSkin.Disabled && currentSkin.Value.SkinInfo.PerformRead(s => !s.Protected);

            private void delete()
            {
                dialogOverlay?.Push(new SkinDeleteDialog(currentSkin.Value));
            }
        }

        public partial class RenameSkinPopover : OsuPopover
        {
            [Resolved]
            private SkinManager skins { get; set; }

            private readonly FocusedTextBox textBox;

            public RenameSkinPopover()
            {
                AutoSizeAxes = Axes.Both;
                Origin = Anchor.TopCentre;

                RoundedButton renameButton;

                Child = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    Width = 250,
                    Spacing = new Vector2(10f),
                    Children = new Drawable[]
                    {
                        textBox = new FocusedTextBox
                        {
                            PlaceholderText = @"Skin name",
                            FontSize = OsuFont.DEFAULT_FONT_SIZE,
                            RelativeSizeAxes = Axes.X,
                            SelectAllOnFocus = true,
                        },
                        renameButton = new RoundedButton
                        {
                            Height = 40,
                            RelativeSizeAxes = Axes.X,
                            MatchingFilter = true,
                            Text = "Save",
                        }
                    }
                };

                renameButton.Action += rename;
                textBox.OnCommit += (_, _) => rename();
            }

            protected override void PopIn()
            {
                textBox.Text = skins.CurrentSkinInfo.Value.Value.Name;
                textBox.TakeFocus();

                base.PopIn();
            }

            private void rename()
            {
                skins.Rename(skins.CurrentSkinInfo.Value, textBox.Text);
                PopOut();
            }
        }
    }
}
