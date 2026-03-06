// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card
{
    public partial class RankedPlayCardContent
    {
        private partial class AttributeListing(APIBeatmap beatmap) : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(RulesetStore rulesets)
            {
                var rulesetInfo = rulesets.GetRuleset(beatmap.RulesetID);
                Debug.Assert(rulesetInfo != null);
                var ruleset = rulesetInfo.CreateInstance();

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Padding = new MarginPadding(7),
                    Children =
                    [
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children =
                            [
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Spacing = new Vector2(4),
                                    Children =
                                    [
                                        new OsuSpriteText
                                        {
                                            Text = "Length",
                                            Font = OsuFont.GetFont(size: 9, weight: FontWeight.Medium),
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            UseFullGlyphHeight = false,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = beatmap.HitLength.ToFormattedDuration(),
                                            Font = OsuFont.GetFont(size: 9, weight: FontWeight.SemiBold),
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            UseFullGlyphHeight = false,
                                        },
                                    ]
                                },

                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Spacing = new Vector2(4),
                                    Children =
                                    [
                                        new OsuSpriteText
                                        {
                                            Text = "BPM",
                                            Font = OsuFont.GetFont(size: 9, weight: FontWeight.Medium),
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            UseFullGlyphHeight = false,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = ((int)beatmap.BPM).ToString(),
                                            Font = OsuFont.GetFont(size: 9, weight: FontWeight.SemiBold),
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            UseFullGlyphHeight = false,
                                        },
                                    ]
                                },
                            ]
                        },
                        ..ruleset.GetBeatmapAttributesForDisplay(beatmap, [])
                                 .Select(attribute => new AttributeRow(attribute))
                    ]
                };
            }
        }

        private partial class AttributeRow(RulesetBeatmapAttribute attribute) : CompositeDrawable
        {
            private float normalizedValue => float.Clamp(attribute.AdjustedValue / attribute.MaxValue, 0, 1);

            [BackgroundDependencyLoader]
            private void load(CardColours colours)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                InternalChildren =
                [
                    new OsuSpriteText
                    {
                        Text = attribute.Label,
                        Font = OsuFont.GetFont(size: 9, weight: FontWeight.Medium),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        UseFullGlyphHeight = false,
                    },
                    new OsuSpriteText
                    {
                        RelativePositionAxes = Axes.X,
                        Text = attribute.AdjustedValue.ToStandardFormattedString(maxDecimalDigits: 1),
                        Font = OsuFont.GetFont(size: 9, weight: FontWeight.SemiBold),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreRight,
                        UseFullGlyphHeight = false,
                        X = 0.65f,
                        Padding = new MarginPadding { Right = 2 },
                        Colour = colours.OnBackground,
                    },
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Width = 0.35f,
                        Height = 2,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Masking = true,
                        Children =
                        [
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colours.BackgroundLightest,
                            },
                            new CircularContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = normalizedValue,
                                Masking = true,
                                Children =
                                [
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colours.PrimaryWithContrastToBackground,
                                    },
                                ]
                            }
                        ]
                    }
                ];
            }
        }
    }
}
