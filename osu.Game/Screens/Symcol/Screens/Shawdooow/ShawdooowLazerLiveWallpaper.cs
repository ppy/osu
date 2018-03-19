using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Screens.Symcol.Pieces;
using osu.Framework.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Symcol.Screens.Shawdooow
{
    class ShawdooowLazerLiveWallpaper : OsuScreen
    {
        private SymcolBackground background;
        private CircularContainer visualizer;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Children = new Drawable[]
            {
                background = new SymcolBackground
                {
                    Depth = 0,
                    Colour = Color4.Yellow,
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                },
                new Box
                {
                    Depth = -1,
                    Position = new Vector2(0 , -140),
                    Colour = Color4.Black,
                    Height = 20,
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomCentre,
                },
                visualizer = new CircularContainer
                {
                    Masking = true,
                    Depth = -2,
                    Position = new Vector2(200 , -140),
                    Colour = Color4.Black,
                    Size = new Vector2(60),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomLeft,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                },
                new SpriteText
                {
                    Colour = Color4.Black,
                    Position = new Vector2(10),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Text = "CAUTION:",
                    TextSize = 100,
                },
                new SpriteText
                {
                    Colour = Color4.Black,
                    Position = new Vector2(10 , 120),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Text = "CLASS 05Z2 LAZER RADIATION",
                    TextSize = 80,
                },
                new SpriteText
                {
                    Colour = Color4.Black,
                    Position = new Vector2(10 , 210),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Text = "July 31, 2017",
                    TextSize = 60,
                },
            };
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}
