using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Symcol.Pieces
{
    public abstract class Mascot : Container
    {
        protected Sprite Idle;
        protected Container SpeechBubble;
        protected Box SpeechBubbleBackground;
        protected OsuTextFlowContainer Speech;

        public bool LeftSide = true;
        private bool speaking = false;

        private double speakingDuration = 0;

        public double ReadyToSpeakTime = double.MaxValue;
        public double CloseTime = double.MaxValue;
        public double LeaveTime = double.MaxValue;

        public Mascot()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            AlwaysPresent = true;

            Children = new Drawable[]
            {
                Idle = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AlwaysPresent = true,
                    FillMode = FillMode.Fill
                },
                SpeechBubble = new Container
                {
                    Masking = true,
                    Position = new Vector2(0),
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopLeft,
                    Size = new Vector2(400, 200),
                    Scale = new Vector2(1, 0),
                    BorderColour = Color4.White,
                    BorderThickness = 10,
                    CornerRadius = 8,

                    Children = new Drawable[]
                    {
                        SpeechBubbleBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        Speech = new OsuTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Scale = new Vector2(0.95f, 1),
                            Margin = new MarginPadding { Top = 12, Right = 12 },
                            Position = new Vector2(12, 6)
                        }
                    }
                }
            };
        }

        public void Speak(string text, double duration = -1)
        {
            if (duration == -1)
            {
                string[] args = text.Split(' ');
                foreach (string arg in args)
                    duration += 250;
            }

            Speech.Text = text;
            speakingDuration = duration;

            if (!speaking)
                enter();
            else
                CloseTime = Time.Current + speakingDuration + 200;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Position = new Vector2(-Idle.Width, 0);
        }

        private void enter(double duration = 1000)
        {
            ReadyToSpeakTime = Time.Current + duration;
            this.MoveTo(Vector2.Zero, duration)
                .FadeInFromZero(duration);
        }

        private void exit(double duration = 1000)
        {
            if (LeftSide)
                this.MoveTo(new Vector2(-Idle.Width, 0), duration)
                    .FadeOutFromOne(duration);
            else
                this.MoveTo(new Vector2(Idle.Width, 0), duration)
                    .FadeOutFromOne(duration);
        }

        protected override void Update()
        {
            base.Update();

            if (Time.Current >= ReadyToSpeakTime && !speaking)
            {
                speaking = true;
                ReadyToSpeakTime = double.MaxValue;
                SpeechBubble.ScaleTo(Vector2.One, 200)
                    .FadeInFromZero(200);
                CloseTime = Time.Current + speakingDuration + 200;
            }

            if (Time.Current >= CloseTime && speaking)
            {
                speaking = false;
                CloseTime = double.MaxValue;
                LeaveTime = Time.Current + 200;
                SpeechBubble.ScaleTo(new Vector2(1, 0), 200)
                    .FadeOutFromOne(200);
            }

            if (Time.Current >= LeaveTime)
            {
                LeaveTime = double.MaxValue;
                exit();
            }
        }

        public void ToggleDirection()
        {
            if (LeftSide)
            {
                LeftSide = false;
                SpeechBubble.Anchor = Anchor.TopLeft;
                SpeechBubble.Origin = Anchor.TopRight;
                Anchor = Anchor.BottomRight;
                Origin = Anchor.BottomRight;
                Idle.Scale = new Vector2(-1, 1);
                Position = new Vector2(Idle.Width, 0);
            }
            else
            {
                LeftSide = true;
                SpeechBubble.Anchor = Anchor.TopRight;
                SpeechBubble.Origin = Anchor.TopLeft;
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;
                Idle.Scale = Vector2.One;
                Position = new Vector2(-Idle.Width, 0);
            }
        }
    }
}
