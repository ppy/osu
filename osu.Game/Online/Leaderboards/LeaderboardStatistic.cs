// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Online.Leaderboards
{
    public partial class LeaderboardStatistic : Container
    {
        private readonly LocalisableString name;
        private readonly LocalisableString value;

        private readonly bool perfect;

        private Direction direction;

        public Direction Direction
        {
            get => direction;
            set
            {
                direction = value;

                if (IsLoaded)
                    recreateText();
            }
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private readonly Container content;
        private readonly OsuTextFlowContainer textFlow;

        public override bool Contains(Vector2 screenSpacePos) => content.Contains(screenSpacePos);

        public LeaderboardStatistic(LocalisableString name, LocalisableString value, bool perfect, float? minWidth = null)
        {
            this.name = name;
            this.value = value;
            this.perfect = perfect;

            AutoSizeAxes = Axes.Both;
            Child = content = new Container
            {
                AutoSizeAxes = Axes.Both,
                Child = textFlow = new OsuTextFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    ParagraphSpacing = 0.1f,
                },
            };

            if (minWidth != null)
                Add(Empty().With(d => d.Width = minWidth.Value));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            recreateText();
        }

        private void recreateText()
        {
            textFlow.Clear();

            textFlow.AddText($"{name}{(direction == Direction.Horizontal ? "\n" : "")}", t =>
            {
                t.Font = OsuFont.Style.Caption2.With(weight: FontWeight.SemiBold);
                t.Colour = colourProvider.Content2;
            });

            textFlow.AddText(value, t =>
            {
                t.Font = OsuFont.Style.Body;
                t.Colour = perfect ? colours.Lime1 : Color4.White;
                t.Padding = new MarginPadding { Left = direction == Direction.Horizontal ? 0 : 5 };
            });
        }
    }
}
