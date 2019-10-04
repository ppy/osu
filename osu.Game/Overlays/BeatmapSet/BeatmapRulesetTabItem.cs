// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapRulesetTabItem : TabItem<RulesetInfo>
    {
        private readonly OsuSpriteText name, count;
        private readonly Box bar;

        public override bool PropagatePositionalInputSubTree => Enabled.Value && !Active.Value && base.PropagatePositionalInputSubTree;

        public BeatmapRulesetTabItem(RulesetInfo value)
            : base(value)
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;

            FillFlowContainer nameContainer;

            Children = new Drawable[]
            {
                nameContainer = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Bottom = 7.5f },
                    Spacing = new Vector2(2.5f),
                    Children = new Drawable[]
                    {
                        name = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = value.Name,
                            Font = OsuFont.Default.With(size: 18),
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 4f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black.Opacity(0.5f),
                                },
                                count = new OsuSpriteText
                                {
                                    Alpha = 0,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Margin = new MarginPadding { Horizontal = 5f },
                                    Font = OsuFont.Default.With(weight: FontWeight.SemiBold),
                                }
                            }
                        }
                    }
                },
                bar = new Box
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 4f,
                },
                new HoverClickSounds(),
            };

            Enabled.BindValueChanged(v => nameContainer.Alpha = v.NewValue ? 1f : 0.5f, true);
        }

        [Resolved]
        private OsuColour colour { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            count.Colour = colour.Gray9;
            bar.Colour = colour.Blue;

            updateState();
        }

        public void SetBeatmaps(List<BeatmapInfo> beatmaps)
        {
            Trace.Assert(beatmaps?.TrueForAll(b => b.Ruleset.Equals(Value)) ?? true, "A beatmap has a ruleset not of this tab value");

            count.Text = beatmaps?.Count.ToString();

            var hasBeatmaps = (beatmaps?.Count ?? 0) > 0;
            count.Alpha = hasBeatmaps ? 1f : 0f;
            Enabled.Value = hasBeatmaps;
        }

        private void updateState()
        {
            var isHoveredOrActive = IsHovered || Active.Value;
            name.Colour = isHoveredOrActive ? colour.GrayE : colour.GrayC;
            bar.MoveToY(isHoveredOrActive ? 0f : bar.Height, 120);

            name.Font = name.Font.With(weight: Active.Value ? FontWeight.Bold : FontWeight.Regular);
        }

        #region Hovering and activation logic

        protected override void OnActivated() => updateState();

        protected override void OnDeactivated() => updateState();

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
        }

        #endregion
    }
}
