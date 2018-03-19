using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Vitaru.Objects.Characters;
using osu.Game.Rulesets.Vitaru.Objects.Drawables;
using osu.Game.Rulesets.Vitaru.Scoring;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Game.Rulesets.Vitaru.UI;
using Symcol.Core.Networking;

namespace eden.Game.GamePieces
{
    /// <summary>
    /// Assign the values here whatever you need for debugging
    /// </summary>
    public class DebugValueUI : Container
    {
        private DebugUiConfiguration currentConfiguration = VitaruSettings.VitaruConfigManager.GetBindable<DebugUiConfiguration>(VitaruSetting.DebugUIConfiguration);

        //Debug section
        private Container debugContainer;

        private SpriteText value1;
        private SpriteText value2;
        private SpriteText value3;
        private SpriteText value4;
        private SpriteText value5;
        private SpriteText value6;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Size = new Vector2(512, 384);

            Children = new Drawable[]
            {
                debugContainer = new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Size = new Vector2(300 , 260),
                    Masking = true,
                    Depth = 0,
                    Alpha = 1,
                    BorderColour = Color4.White,
                    BorderThickness = 10,
                    CornerRadius = 20,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Depth = 0,
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.DarkBlue , Color4.Blue),
                        },
                        value1 = new SpriteText
                        {
                            Depth = -10,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Position = new Vector2(10 , -100),
                            TextSize = 25,
                            Colour = Color4.Green,
                            Text = "Value 1 Here",
                        },
                        value2 = new SpriteText
                        {
                            Depth = -10,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Position = new Vector2(10 , -60),
                            TextSize = 25,
                            Colour = Color4.Green,
                            Text = "Value 2 Here",
                        },
                        value3 = new SpriteText
                        {
                            Depth = -10,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Position = new Vector2(10 , -20),
                            TextSize = 25,
                            Colour = Color4.Green,
                            Text = "Value 3 Here",
                        },
                        value4 = new SpriteText
                        {
                            Depth = -10,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Position = new Vector2(10 , 20),
                            TextSize = 25,
                            Colour = Color4.Green,
                            Text = "Value 4 Here",
                        },
                        value5 = new SpriteText
                        {
                            Depth = -10,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Position = new Vector2(10 , 60),
                            TextSize = 25,
                            Colour = Color4.Green,
                            Text = "Value 5 Here",
                        },
                        value6 = new SpriteText
                        {
                            Depth = -10,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Position = new Vector2(10 , 100),
                            TextSize = 25,
                            Colour = Color4.Green,
                            Text = "Value 6 Here",
                        }
                    }
                }
            };

            DrawableBullet.BulletCount = 0;
            DrawablePattern.PatternCount = 0;
            Enemy.EnemyCount = 0;
        }

        protected override void Update()
        {
            base.Update();

            if (currentConfiguration == DebugUiConfiguration.PerformanceMetrics)
            {
                value1.Text = "Bullets = " + DrawableBullet.BulletCount.ToString();
                value2.Text = "Patterns = " + DrawablePattern.PatternCount.ToString();
                value3.Text = "Enemies = " + Enemy.EnemyCount.ToString();
                value4.Text = "Energy = " + VitaruPlayer.Energystored.ToString();
                value5.Text = "Fps = " + Clock.FramesPerSecond;
            }
            else if (currentConfiguration == DebugUiConfiguration.PP)
            {
                value1.Text = "PlayerScoreZone = " + VitaruPlayfield.VitaruPlayer.ScoreZone.ToString();


                value4.Text = "CurrentPP = " + VitaruPerformanceCalculator.CurrentPPValue;
                value5.Text = "MaxPP = " + VitaruPerformanceCalculator.MaxPPValue;
            }
            else if (currentConfiguration == DebugUiConfiguration.LaserStuff)
            {
                value1.Text = "PlayerHealth = " + VitaruPlayfield.VitaruPlayer.Health.ToString();
            }
            else if (currentConfiguration == DebugUiConfiguration.Network)
            {
                value1.Text = "PacketsSent: " + NetworkingClient.SENTPACKETCOUNT;
            }

            //Value4.Text = "MouseGridPos = (" + EditorGrid.mousePlace.X.ToString() + ", " + EditorGrid.mousePlace.Y.ToString() + ")";

            //if you need these for other things, just comment them out, don't delete
            //Value5.Text = "RealMousePos = (" + EdenGameScreen.EdenCursor.X.ToString() + ", " + EdenGameScreen.EdenCursor.Y.ToString() + ")";

            float frameTime = 1000 / (float)Clock.FramesPerSecond;
            value6.Text = "Frametime = " + frameTime.ToString();
        }
    }

    public enum DebugUiConfiguration
    {
        LaserStuff,
        Network,
        PerformanceMetrics,
        PP
    }
}
