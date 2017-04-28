using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play
{
    public class SongProgressInfo : Container
    {
        private InfoText timeCurrent;
        private InfoText timeLeft;
        private InfoText progress;

        private const int margin = 10;

        public string TimeCurrent
        {
            set
            {
                timeCurrent.Text = value;
            }
        }

        public string TimeLeft
        {
            set
            {
                timeLeft.Text = @"-" + value;
            }
        }
        public string Progress
        {
            set
            {
                progress.Text = value + @"%";
            }
        }

        public SongProgressInfo()
        {
            Children = new Drawable[]
            {
                timeCurrent = new InfoText
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Margin = new MarginPadding
                    {
                        Left = margin,
                    },
                },
                progress = new InfoText
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                },
                timeLeft = new InfoText
                {
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    Margin = new MarginPadding
                    {
                        Right = margin,
                    }
                }
            };
        }

        private class InfoText : OsuSpriteText
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = colours.BlueLighter;
                Font = @"Venera";
                EdgeEffect = new EdgeEffect
                {
                    Colour = colours.BlueDarker,
                    Type = EdgeEffectType.Glow,
                    Radius = 5,
                };
            }
        }
    }
}
