// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Configuration;
using osu.Game.Screens.Ranking.Expanded;
using osuTK;
using osuTK.Graphics;
using osu.Game.Rulesets;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Displays beatmap metadata inside <see cref="PlayerLoader"/>
    /// </summary>
    public class BeatmapMetadataDisplay : Container
    {
        private readonly WorkingBeatmap beatmap;
        private readonly Bindable<IReadOnlyList<Mod>> mods;
        private readonly Drawable logoFacade;
        private LoadingSpinner loading;
        private SelectedRulesetIcon ruleseticon;

        public IBindable<IReadOnlyList<Mod>> Mods => mods;

        public bool Loading
        {
            set
            {
                if (value)
                {
                    loading.Show();
                    ruleseticon.Hide();
                }
                else
                {
                    loading.Hide();

                    if (optui.Value)
                    {
                        ruleseticon.Loaded = true;
                        ruleseticon.Delay(250).Then()
                                   .ScaleTo(1.5f).FadeOut().Then()
                                   .FadeIn(500, Easing.OutQuint).ScaleTo(1f, 500, Easing.OutQuint);
                    }
                    else
                        ruleseticon.Hide();
                }
            }
        }

        public BeatmapMetadataDisplay(WorkingBeatmap beatmap, Bindable<IReadOnlyList<Mod>> mods, Drawable logoFacade)
        {
            this.beatmap = beatmap;
            this.logoFacade = logoFacade;

            this.mods = new Bindable<IReadOnlyList<Mod>>();
            this.mods.BindTo(mods);
        }

        private Container basePanel;
        private Container bg;
        private readonly Bindable<bool> optui = new Bindable<bool>();

        private IBindable<StarDifficulty?> starDifficulty;

        private FillFlowContainer versionFlow;
        private StarRatingDisplay starRatingDisplay;

        [BackgroundDependencyLoader]
        private void load(BeatmapDifficultyCache difficultyCache, MConfigManager config)
        {
            var metadata = beatmap.BeatmapInfo.Metadata;

            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                basePanel = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Masking = false,
                    Children = new Drawable[]
                    {
                        bg = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            CornerRadius = 20,
                            CornerExponent = 2.5f,
                            Masking = true,
                            BorderColour = Color4.Black,
                            BorderThickness = 3f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.5f,
                                    Colour = Color4.Black,
                                }
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Direction = FillDirection.Vertical,
                            Children = new[]
                            {
                                logoFacade.With(d =>
                                {
                                    d.Anchor = Anchor.TopCentre;
                                    d.Origin = Anchor.TopCentre;
                                }),
                                new OsuSpriteText
                                {
                                    Text = new RomanisableString(metadata.TitleUnicode, metadata.Title),
                                    Font = OsuFont.GetFont(size: 36, italics: true),
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Margin = new MarginPadding { Top = 15 }
                                },
                                new OsuSpriteText
                                {
                                    Text = new RomanisableString(metadata.ArtistUnicode, metadata.Artist),
                                    Font = OsuFont.GetFont(size: 26, italics: true),
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre
                                },
                                new Container
                                {
                                    Size = new Vector2(300, 60),
                                    Margin = new MarginPadding(10),
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    CornerRadius = 10,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Sprite
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Texture = beatmap?.Background,
                                            Origin = Anchor.Centre,
                                            Anchor = Anchor.Centre,
                                            FillMode = FillMode.Fill,
                                        },
                                        loading = new LoadingLayer(true),
                                        ruleseticon = new SelectedRulesetIcon(),
                                    }
                                },
                                versionFlow = new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(5f),
                                    Margin = new MarginPadding { Bottom = 40 },
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = beatmap?.BeatmapInfo?.Version,
                                            Font = OsuFont.GetFont(size: 26, italics: true),
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                        },
                                        starRatingDisplay = new StarRatingDisplay(default)
                                        {
                                            Alpha = 0f,
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                        }
                                    }
                                },
                                new GridContainer
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    AutoSizeAxes = Axes.Both,
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.AutoSize),
                                    },
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.AutoSize),
                                    },
                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            new MetadataLineLabel("来源"),
                                            new MetadataLineInfo(metadata.Source)
                                        },
                                        new Drawable[]
                                        {
                                            new MetadataLineLabel("谱师"),
                                            new MetadataLineInfo(metadata.AuthorString)
                                        }
                                    }
                                },
                                new ModDisplay
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Top = 20 },
                                    Current = mods
                                }
                            },
                        },
                    },
                }
            };

            starDifficulty = difficultyCache.GetBindableDifficulty(beatmap.BeatmapInfo);

            Loading = true;

            config.BindWith(MSetting.OptUI, optui);
            optui.BindValueChanged(updateVisualEffects);

            entryAnimation();
        }

        private void updateVisualEffects(ValueChangedEvent<bool> v)
        {
            switch (v.NewValue)
            {
                case true:
                    ruleseticon.Delay(500).Schedule(() => ruleseticon.AddRulesetSprite());
                    ruleseticon.ScaleTo(1, 500, Easing.OutQuint).FadeIn(500, Easing.OutQuint);
                    bg.ScaleTo(1.2f, 500, Easing.OutQuint).FadeIn(500, Easing.OutQuint);
                    return;

                case false:
                    ruleseticon.ScaleTo(0.9f, 500, Easing.OutQuint).FadeOut(500, Easing.OutQuint);
                    bg.FadeOut(500, Easing.OutQuint).ScaleTo(1.1f, 500, Easing.OutQuint);
                    return;
            }
        }

        private void entryAnimation()
        {
            switch (optui.Value)
            {
                case true:
                    bg.ScaleTo(1.2f);
                    basePanel.ScaleTo(1.5f).Then()
                             .Delay(750).FadeIn(500, Easing.OutQuint)
                             .ScaleTo(1f, 500, Easing.OutQuint);
                    return;

                case false:
                    ruleseticon.ScaleTo(0.9f).FadeOut();
                    bg.ScaleTo(1.1f).FadeOut();
                    return;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (starDifficulty.Value != null)
            {
                starRatingDisplay.Current.Value = starDifficulty.Value.Value;
                starRatingDisplay.Show();
            }
            else
            {
                starRatingDisplay.Hide();

                starDifficulty.ValueChanged += d =>
                {
                    Debug.Assert(d.NewValue != null);

                    starRatingDisplay.Current.Value = d.NewValue.Value;

                    versionFlow.AutoSizeDuration = 300;
                    versionFlow.AutoSizeEasing = Easing.OutQuint;

                    starRatingDisplay.FadeIn(300, Easing.InQuint);
                };
            }
        }

        private class MetadataLineLabel : OsuSpriteText
        {
            public MetadataLineLabel(string text)
            {
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;
                Margin = new MarginPadding { Right = 5 };
                Colour = OsuColour.Gray(0.8f);
                Text = text;
            }
        }

        private class MetadataLineInfo : OsuSpriteText
        {
            public MetadataLineInfo(string text)
            {
                Margin = new MarginPadding { Left = 5 };
                Text = string.IsNullOrEmpty(text) ? @"-" : text;
            }
        }

        private class SelectedRulesetIcon : Container
        {
            public FillFlowContainer ContentFillFlow;
            private OsuSpriteText rulesetSpriteText;

            [Resolved]
            private IBindable<RulesetInfo> rulesetInfo { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(200, 40);
                Origin = Anchor.Centre;
                Anchor = Anchor.Centre;
                CornerRadius = 10;
                Masking = true;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.8f,
                        Colour = Color4.Black,
                    },
                    ContentFillFlow = new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        RelativeSizeAxes = Axes.Both,
                        Spacing = new Vector2(10),
                        LayoutDuration = 500,
                        LayoutEasing = Easing.OutQuint,
                        Child = new ConstrainedIconContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Icon = rulesetInfo.Value.CreateInstance().CreateIcon(),
                            Colour = Color4.White,
                            Size = new Vector2(20),
                        }
                    },
                };

                Loaded = false;
            }

            private bool loadAnimationDone;

            public bool Loaded
            {
                set
                {
                    if (value)
                    {
                        this.Delay(750).Schedule(AddRulesetSprite);
                    }
                }
            }

            public void AddRulesetSprite()
            {
                if (!loadAnimationDone)
                {
                    ContentFillFlow.Add(
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = rulesetSpriteText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = $"{rulesetInfo.Value.CreateInstance().Description}",
                            }
                        });
                    rulesetSpriteText.FadeOut().Then().FadeIn(500, Easing.OutQuint);

                    loadAnimationDone = true;
                }
            }
        }

    }
}
