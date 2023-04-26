// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class FPSCounterTooltip : CompositeDrawable, ITooltip
    {
        private OsuTextFlowContainer textFlow = null!;

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Both;

            CornerRadius = 15;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Gray1,
                    Alpha = 1,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuTextFlowContainer(cp =>
                {
                    cp.Font = OsuFont.Default.With(weight: FontWeight.SemiBold);
                })
                {
                    AutoSizeAxes = Axes.Both,
                    TextAnchor = Anchor.TopRight,
                    Margin = new MarginPadding { Left = 5, Vertical = 10 },
                    Text = string.Join('\n', gameHost.Threads.Select(t => t.Name))
                },
                textFlow = new OsuTextFlowContainer(cp =>
                {
                    cp.Font = OsuFont.Default.With(fixedWidth: true, weight: FontWeight.Regular);
                    cp.Spacing = new Vector2(-1);
                })
                {
                    Width = 190,
                    Margin = new MarginPadding { Left = 35, Right = 10, Vertical = 10 },
                    AutoSizeAxes = Axes.Y,
                    TextAnchor = Anchor.TopRight,
                },
            };
        }

        private int lastUpdate;

        protected override void Update()
        {
            int currentSecond = (int)(Clock.CurrentTime / 100);

            if (currentSecond != lastUpdate)
            {
                lastUpdate = currentSecond;

                textFlow.Clear();

                foreach (var thread in gameHost.Threads)
                {
                    var clock = thread.Clock;

                    string maximum = clock.Throttling
                        ? $"/{(clock.MaximumUpdateHz > 0 && clock.MaximumUpdateHz < 10000 ? clock.MaximumUpdateHz.ToString("0") : "∞"),4}"
                        : string.Empty;

                    textFlow.AddParagraph($"{clock.FramesPerSecond:0}{maximum}fps ({clock.ElapsedFrameTime:0.00}ms)");
                }
            }
        }

        public void SetContent(object content)
        {
        }

        public void Move(Vector2 pos)
        {
            Position = pos;
        }
    }
}
