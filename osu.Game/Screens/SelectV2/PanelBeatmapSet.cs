// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelBeatmapSet : Panel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT * 1.6f;

        private Box chevronBackground = null!;
        private PanelSetBackground setBackground = null!;

        private OsuSpriteText titleText = null!;
        private OsuSpriteText artistText = null!;
        private Drawable chevronIcon = null!;
        private PanelUpdateBeatmapButton updateButton = null!;
        private BeatmapSetOnlineStatusPill statusPill = null!;
        private DifficultySpectrumDisplay difficultiesDisplay = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private BeatmapSetOverlay? beatmapOverlay { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private ISongSelect? songSelect { get; set; }

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        private GroupedBeatmapSet groupedBeatmapSet
        {
            get
            {
                Debug.Assert(Item != null);
                return (GroupedBeatmapSet)Item!.Model;
            }
        }

        public PanelBeatmapSet()
        {
            PanelXOffset = 20f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = HEIGHT;

            Icon = chevronIcon = new Container
            {
                Size = new Vector2(0, 22),
                Child = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.ChevronRight,
                    Size = new Vector2(8),
                    X = 1f,
                    Colour = colourProvider.Background5,
                },
            };

            Background = chevronBackground = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
                Alpha = 0f,
            };

            Content.Children = new Drawable[]
            {
                setBackground = new PanelSetBackground(),
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding { Top = 7.5f, Left = 15, Bottom = 13 },
                    Children = new Drawable[]
                    {
                        titleText = new OsuSpriteText
                        {
                            Font = OsuFont.Style.Heading1.With(typeface: Typeface.TorusAlternate),
                        },
                        artistText = new OsuSpriteText
                        {
                            Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                        },
                        new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Top = 4f },
                            Children = new Drawable[]
                            {
                                updateButton = new PanelUpdateBeatmapButton
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Margin = new MarginPadding { Right = 5f, Top = -2f },
                                },
                                statusPill = new BeatmapSetOnlineStatusPill
                                {
                                    Origin = Anchor.CentreLeft,
                                    Anchor = Anchor.CentreLeft,
                                    TextSize = OsuFont.Style.Caption2.Size,
                                    Margin = new MarginPadding { Right = 5f },
                                    Animated = false,
                                },
                                difficultiesDisplay = new DifficultySpectrumDisplay
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                            },
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(_ => onExpanded(), true);
            KeyboardSelected.BindValueChanged(k => KeyboardSelected.Value = k.NewValue, true);
        }

        private void onExpanded()
        {
            if (Expanded.Value)
            {
                chevronBackground.FadeIn(DURATION / 2, Easing.OutQuint);
                chevronIcon.ResizeWidthTo(18, DURATION * 1.5f, Easing.OutElasticQuarter);
                chevronIcon.FadeTo(1f, DURATION, Easing.OutQuint);
            }
            else
            {
                chevronBackground.FadeOut(DURATION, Easing.OutQuint);
                chevronIcon.ResizeWidthTo(0f, DURATION, Easing.OutQuint);
                chevronIcon.FadeTo(0f, DURATION, Easing.OutQuint);
            }
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            var beatmapSet = groupedBeatmapSet.BeatmapSet;

            // Choice of background image matches BSS implementation (always uses the lowest `beatmap_id` from the set).
            setBackground.Beatmap = beatmaps.GetWorkingBeatmap(beatmapSet.Beatmaps.MinBy(b => b.OnlineID));

            titleText.Text = new RomanisableString(beatmapSet.Metadata.TitleUnicode, beatmapSet.Metadata.Title);
            artistText.Text = new RomanisableString(beatmapSet.Metadata.ArtistUnicode, beatmapSet.Metadata.Artist);
            updateButton.BeatmapSet = beatmapSet;
            statusPill.Status = beatmapSet.Status;
            difficultiesDisplay.BeatmapSet = beatmapSet;
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            setBackground.Beatmap = null;
            updateButton.BeatmapSet = null;
            difficultiesDisplay.BeatmapSet = null;
        }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        public override MenuItem[] ContextMenuItems
        {
            get
            {
                if (Item == null)
                    return Array.Empty<MenuItem>();

                var beatmapSet = groupedBeatmapSet.BeatmapSet;

                List<MenuItem> items = new List<MenuItem>();

                if (Expanded.Value)
                {
                    if (songSelect is SoloSongSelect soloSongSelect)
                    {
                        // Assume the current set has one of its beatmaps selected since it is expanded.
                        items.Add(new OsuMenuItem(ButtonSystemStrings.Edit.ToSentence(), MenuItemType.Standard, () => soloSongSelect.Edit(soloSongSelect.Beatmap.Value.BeatmapInfo))
                        {
                            Icon = FontAwesome.Solid.PencilAlt
                        });
                        items.Add(new OsuMenuItemSpacer());
                    }
                }
                else
                {
                    items.Add(new OsuMenuItem(WebCommonStrings.ButtonsExpand.ToSentence(), MenuItemType.Highlighted, () => TriggerClick()));
                    items.Add(new OsuMenuItemSpacer());
                }

                if (beatmapSet.OnlineID > 0)
                {
                    items.Add(new OsuMenuItem(CommonStrings.Details, MenuItemType.Standard, () => beatmapOverlay?.FetchAndShowBeatmapSet(beatmapSet.OnlineID)));

                    if (beatmapSet.GetOnlineURL(api, ruleset.Value) is string url)
                        items.Add(new OsuMenuItem(CommonStrings.CopyLink, MenuItemType.Standard, () => game?.CopyToClipboard(url)));

                    items.Add(new OsuMenuItemSpacer());
                }

                var collectionItems = realm.Realm.All<BeatmapCollection>()
                                           .OrderBy(c => c.Name)
                                           .AsEnumerable()
                                           .Select(createCollectionMenuItem)
                                           .ToList();

                if (manageCollectionsDialog != null)
                    collectionItems.Add(new OsuMenuItem(CommonStrings.Manage, MenuItemType.Standard, manageCollectionsDialog.Show));

                items.Add(new OsuMenuItem(CommonStrings.Collections) { Items = collectionItems });

                if (beatmapSet.Beatmaps.Any(b => b.Hidden))
                    items.Add(new OsuMenuItem(SongSelectStrings.RestoreAllHidden, MenuItemType.Standard, () => songSelect?.RestoreAllHidden(beatmapSet)));

                items.Add(new OsuMenuItem(SongSelectStrings.DeleteBeatmap, MenuItemType.Destructive, () => songSelect?.Delete(beatmapSet)));
                return items.ToArray();
            }
        }

        private MenuItem createCollectionMenuItem(BeatmapCollection collection)
        {
            var beatmapSet = groupedBeatmapSet.BeatmapSet;

            TernaryState state;

            int countExisting = beatmapSet.Beatmaps.Count(b => collection.BeatmapMD5Hashes.Contains(b.MD5Hash));

            if (countExisting == beatmapSet.Beatmaps.Count)
                state = TernaryState.True;
            else if (countExisting > 0)
                state = TernaryState.Indeterminate;
            else
                state = TernaryState.False;

            var liveCollection = collection.ToLive(realm);

            return new TernaryStateToggleMenuItem(collection.Name, MenuItemType.Standard, s =>
            {
                liveCollection.PerformWrite(c =>
                {
                    foreach (var b in beatmapSet.Beatmaps)
                    {
                        switch (s)
                        {
                            case TernaryState.True:
                                if (c.BeatmapMD5Hashes.Contains(b.MD5Hash))
                                    continue;

                                c.BeatmapMD5Hashes.Add(b.MD5Hash);
                                break;

                            case TernaryState.False:
                                c.BeatmapMD5Hashes.Remove(b.MD5Hash);
                                break;
                        }
                    }
                });
            })
            {
                State = { Value = state }
            };
        }
    }
}
