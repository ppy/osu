using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class HitMarkerClick : HitMarker
    {
        public HitMarkerClick()
        {
            const float length = 20;
            const float border_size = 3;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(border_size, length + border_size),
                    Colour = Colour4.Black.Opacity(0.5F)
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(border_size, length + border_size),
                    Rotation = 90,
                    Colour = Colour4.Black.Opacity(0.5F)
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(1, length),
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(1, length),
                    Rotation = 90,
                }
            };
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected override void OnApply(AnalysisFrameEntry entry)
        {
            base.OnApply(entry);

            switch (entry.Action)
            {
                case OsuAction.LeftButton:
                    Colour = colours.BlueLight;
                    break;

                case OsuAction.RightButton:
                    Colour = colours.YellowLight;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
