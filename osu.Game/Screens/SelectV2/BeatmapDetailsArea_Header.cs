// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens.Select.Leaderboards;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapDetailsArea
    {
        public partial class Header : CompositeDrawable
        {
            private WedgeSelector<Selection> tabControl = null!;
            private FillFlowContainer leaderboardControls = null!;

            private ShearedDropdown<BeatmapLeaderboardScope> scopeDropdown = null!;
            private ShearedToggleButton selectedModsToggle = null!;

            public IBindable<Selection> Type => tabControl.Current;

            public IBindable<BeatmapLeaderboardScope> Scope => scopeDropdown.Current;

            public IBindable<bool> FilterBySelectedMods => selectedModsToggle.Active;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 20f },
                        Children = new Drawable[]
                        {
                            tabControl = new WedgeSelector<Selection>(20f)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Width = 200,
                                Height = 22,
                                Margin = new MarginPadding { Top = 2f },
                            },
                            leaderboardControls = new FillFlowContainer
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                AutoSizeAxes = Axes.Both,
                                Spacing = new Vector2(5f, 0f),
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Size = new Vector2(128f, 30f),
                                        Child = selectedModsToggle = new ShearedToggleButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = @"Selected Mods",
                                            Height = 30,
                                        },
                                    },
                                    // new Container
                                    // {
                                    //     Anchor = Anchor.CentreRight,
                                    //     Origin = Anchor.CentreRight,
                                    //     Size = new Vector2(150f, 33f),
                                    //     Child = new ShearedDropdown<RankingsSort>(@"Sort")
                                    //     {
                                    //         Width = 150f,
                                    //         Items = Enum.GetValues<RankingsSort>(),
                                    //     },
                                    // },
                                    new Container
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        Size = new Vector2(160f, 32f),
                                        Child = scopeDropdown = new ScopeDropdown
                                        {
                                            Width = 160f,
                                            Current = { Value = BeatmapLeaderboardScope.Global },
                                        },
                                    },
                                },
                            },
                        },
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                tabControl.Current.BindValueChanged(v =>
                {
                    leaderboardControls.FadeTo(v.NewValue == Selection.Ranking ? 1 : 0, 300, Easing.OutQuint);
                }, true);
            }

            public enum Selection
            {
                Details,
                Ranking,
            }

            // public enum RankingsSort
            // {
            //     Score,
            //     Accuracy,
            //     Combo,
            //     Misses,
            //     Date,
            // }

            private partial class ScopeDropdown : ShearedDropdown<BeatmapLeaderboardScope>
            {
                public ScopeDropdown()
                    : base("Scope")
                {
                    Items = Enum.GetValues<BeatmapLeaderboardScope>();
                }

                protected override LocalisableString GenerateItemText(BeatmapLeaderboardScope item) => item.ToString();
            }
        }
    }
}
