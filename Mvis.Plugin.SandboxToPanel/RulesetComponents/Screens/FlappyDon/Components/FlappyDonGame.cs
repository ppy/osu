using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.FlappyDon.Components
{
    public partial class FlappyDonGame : CompositeDrawable
    {
        public static readonly Vector2 SIZE = new Vector2(1920, 1080);
        public static readonly int GROUND_HEIGHT = 200;

        private readonly Bindable<GameState> gameState = new Bindable<GameState>();
        private readonly BindableInt score = new BindableInt();
        private readonly BindableInt highScore = new BindableInt();

        [Resolved(canBeNull: true)]
        private SandboxRulesetConfigManager config { get; set; }

        private readonly Backdrop background;
        private readonly Backdrop ground;
        private readonly Bird bird;
        private readonly Obstacles obstacles;
        private readonly OsuSpriteText drawableScore;
        private readonly OsuSpriteText drawableHighScore;
        private readonly Box flash;
        private readonly Sprite readySprite;
        private readonly Sprite gameOverSprite;

        private Sample pointSample;
        private Sample punchSample;
        private Sample deathSample;

        private bool clickIsBlocked;

        public FlappyDonGame()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChild = new FlappyDonScalingContainer(SIZE)
            {
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        background = new Backdrop(() => new BackgroundSprite(), 20000),
                        obstacles = new Obstacles(),
                        bird = new Bird(),
                        ground = new Backdrop(() => new GroundSprite(), 2250),
                        new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Y = 150,
                            AutoSizeAxes = Axes.Y,
                            Width = 300,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.5f,
                                    Child = drawableScore = new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Font = OsuFont.GetFont(size: 80, weight: FontWeight.SemiBold)
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.5f,
                                    Child = drawableHighScore = new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Font = OsuFont.GetFont(size: 80, weight: FontWeight.SemiBold)
                                    }
                                },
                            }
                        },
                        flash = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Alpha = 0
                        },
                        readySprite = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0
                        },
                        gameOverSprite = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0
                        }
                    }
                }
            };

            bird.GroundY = SIZE.Y - GROUND_HEIGHT;
            obstacles.BirdThreshold = bird.X;

            obstacles.ThresholdCrossed += _ =>
            {
                score.Value++;
                pointSample?.Play();
            };
        }

        [BackgroundDependencyLoader]
        private void load(ISampleStore samples, TextureStore textures)
        {
            pointSample = samples.Get("point");
            punchSample = samples.Get("hit");
            deathSample = samples.Get("die");

            var readyTexture = textures.Get("FlappyDon/message");
            var gameOverTexture = textures.Get("FlappyDon/gameover");

            readySprite.Texture = readyTexture;
            readySprite.Size = readyTexture.Size;
            gameOverSprite.Texture = gameOverTexture;
            gameOverSprite.Size = gameOverTexture.Size;

            config?.BindWith(SandboxRulesetSetting.FlappyDonGameBestScore, highScore);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            highScore.BindValueChanged(h => drawableHighScore.Text = h.NewValue.ToString(), true);

            score.BindValueChanged(score =>
            {
                drawableScore.Text = score.NewValue.ToString();

                if (score.NewValue > highScore.Value)
                    highScore.Value = score.NewValue;
            }, true);

            gameState.BindValueChanged(state =>
            {
                switch (state.NewValue)
                {
                    case GameState.Ready:
                        ready();
                        return;

                    case GameState.Playing:
                        play();
                        return;

                    case GameState.GameOver:
                        fail();
                        return;
                }
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            if (gameState.Value != GameState.Playing)
                return;

            if (!bird.IsTouchingGround && !obstacles.CheckForCollision(bird.CollisionQuad))
                return;

            gameState.Value = GameState.GameOver;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (clickIsBlocked)
                return true;

            switch(gameState.Value)
            {
                case GameState.GameOver:
                    gameState.Value = GameState.Ready;
                    return true;

                case GameState.Playing:
                    bird.FlyUp();
                    return true;

                case GameState.Ready:
                    gameState.Value = GameState.Playing;
                    return true;
            };

            return true;
        }

        private void ready()
        {
            gameOverSprite.ClearTransforms();
            gameOverSprite.Hide();

            readySprite.Show();
            Scheduler.CancelDelayedTasks();

            flash.FinishTransforms();
            background.Start();
            ground.Start();

            score.Value = 0;
            bird.Reset();
            obstacles.Reset();
        }

        private void play()
        {
            readySprite.Hide();
            obstacles.Start();
            bird.FlyUp();
        }

        private void fail()
        {
            clickIsBlocked = true;
            Scheduler.AddDelayed(() => clickIsBlocked = false, 300);

            bird.FallDown();
            gameOverSprite.FadeIn(250, Easing.OutQuint);

            // Play the punch sound, and then the 'fall' sound slightly after
            punchSample.Play();
            Scheduler.AddDelayed(() => deathSample.Play(), 100);

            obstacles.Freeze();
            background.Freeze();
            ground.Freeze();

            flash.FadeIn(20, Easing.OutQuint).Then().FadeOut(750, Easing.Out);
        }

        private enum GameState
        {
            Ready,
            Playing,
            GameOver
        }
    }
}
