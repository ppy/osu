// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelBeatmapStandalone
    {
        public partial class SpreadDisplay : OsuAnimatedButton
        {
            public Bindable<BeatmapInfo?> Beatmap { get; } = new Bindable<BeatmapInfo?>();
            public Bindable<StarDifficulty> StarDifficulty { get; } = new Bindable<StarDifficulty>();

            private readonly Bindable<BeatmapSetInfo?> scopedBeatmapSet = new Bindable<BeatmapSetInfo?>();
            private readonly Bindable<bool> showConvertedBeatmaps = new Bindable<bool>();

            private const double transition_duration = 200;

            [Resolved]
            private Bindable<RulesetInfo> ruleset { get; set; } = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            private FillFlowContainer preceding = null!;
            public Circle Current { get; private set; } = null!;
            private FillFlowContainer succeeding = null!;

            private OsuSpriteText countText = null!;
            private SpriteIcon icon = null!;

            public SpreadDisplay()
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;
                Content.CornerRadius = 5;
            }

            [BackgroundDependencyLoader]
            private void load(ISongSelect? songSelect, OsuConfigManager configManager)
            {
                Add(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Padding = new MarginPadding { Horizontal = 5 },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(2),
                            Children = new Drawable[]
                            {
                                preceding = new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(1),
                                    Alpha = 0.5f,
                                },
                                Current = new Circle
                                {
                                    Size = new Vector2(7, 12),
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                                succeeding = new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(1),
                                    Alpha = 0.5f,
                                }
                            }
                        },
                        countText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.Style.Caption2,
                        },
                        icon = new SpriteIcon
                        {
                            Size = new Vector2(12),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Icon = FontAwesome.Solid.Eye,
                            Alpha = 0,
                        }
                    }
                });

                if (songSelect != null)
                    scopedBeatmapSet.BindTo(songSelect.ScopedBeatmapSet);

                configManager.BindWith(OsuSetting.ShowConvertedBeatmaps, showConvertedBeatmaps);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Beatmap.BindValueChanged(_ => updateBeatmap());
                StarDifficulty.BindValueChanged(_ => updateBeatmap());
                showConvertedBeatmaps.BindValueChanged(_ => updateBeatmap());
                scopedBeatmapSet.BindValueChanged(_ => updateBeatmap(), true);
                Enabled.BindValueChanged(_ => updateAppearance(), true);
                FinishTransforms(true);
            }

            private void updateBeatmap()
            {
                if (Beatmap.Value == null || scopedBeatmapSet.Value != null)
                {
                    this.FadeOut(transition_duration, Easing.OutQuint);
                    return;
                }

                preceding.Clear();
                succeeding.Clear();

                var otherStarDifficulties = Beatmap.Value.BeatmapSet!.Beatmaps
                                                   .Except([Beatmap.Value])
                                                   .Where(b => b.AllowGameplayWithRuleset(ruleset.Value, showConvertedBeatmaps.Value))
                                                   .OrderBy(b => b.StarRating)
                                                   .Select(b => b.StarRating)
                                                   .ToList();
                this.FadeTo(otherStarDifficulties.Count > 0 ? 1 : 0, transition_duration, Easing.OutQuint);

                if (otherStarDifficulties.Count == 0)
                    return;

                const int max_difficulties_total = 11;

                int startIndex;
                int endIndex;

                if (otherStarDifficulties.Count <= max_difficulties_total)
                {
                    startIndex = 0;
                    endIndex = otherStarDifficulties.Count - 1;
                }
                else
                {
                    startIndex = otherStarDifficulties.BinarySearch(StarDifficulty.Value.Stars);
                    if (startIndex < 0)
                        startIndex = ~startIndex - 1;

                    startIndex = Math.Clamp(startIndex - max_difficulties_total / 2, 0, otherStarDifficulties.Count - 1);
                    endIndex = Math.Clamp(startIndex + max_difficulties_total, 0, otherStarDifficulties.Count - 1);
                }

                for (int i = startIndex; i <= endIndex; i++)
                {
                    double otherStarDifficulty = otherStarDifficulties[i];
                    var target = otherStarDifficulty < StarDifficulty.Value.Stars ? preceding : succeeding;

                    var circle = new Circle
                    {
                        Size = new Vector2(5, 10),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Colour = colours.ForStarDifficulty(otherStarDifficulty)
                    };
                    target.Add(circle);
                    target.SetLayoutPosition(circle, (float)otherStarDifficulty);
                }

                int countNotShown = otherStarDifficulties.Count - (preceding.Count + succeeding.Count);
                countText.Alpha = countNotShown > 0 ? 1 : 0;
                countText.Text = $@"+{countNotShown}";

                if (startIndex > 0)
                {
                    for (int i = 0; i < preceding.Count; ++i)
                    {
                        var dot = preceding[i];
                        dot.Alpha = (1 + 4 * (float)(i + 1) / preceding.Count) / 5;
                    }
                }

                if (endIndex < otherStarDifficulties.Count - 1)
                {
                    for (int i = 0; i < succeeding.Count; ++i)
                    {
                        var dot = succeeding[i];
                        dot.Alpha = (1 + 4 * (float)(succeeding.Count - i) / succeeding.Count) / 5;
                    }
                }

                Action = () => scopedBeatmapSet.Value = Beatmap.Value.BeatmapSet!;
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (!Enabled.Value)
                    return false;

                base.OnMouseDown(e);
                return true;
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (!Enabled.Value)
                    return false;

                return base.OnClick(e);
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateAppearance();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateAppearance();
                base.OnHoverLost(e);
            }

            private void updateAppearance()
            {
                bool isInteractable = Enabled.Value && IsHovered;

                HoverColour = isInteractable ? Colour4.White.Opacity(0.1f) : Colour4.Transparent;
                preceding.FadeTo(isInteractable ? 1 : 0.5f, transition_duration, Easing.OutQuint);
                succeeding.FadeTo(isInteractable ? 1 : 0.5f, transition_duration, Easing.OutQuint);
                icon.FadeTo(isInteractable ? 1 : 0, transition_duration, Easing.OutQuint);
            }
        }
    }
}
