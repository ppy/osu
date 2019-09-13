using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;
using System.Linq;

namespace osu.Game.Screens.Play.HUD.HitErrorMeters.Bar
{
    public abstract class BarHitErrorMeter : HitErrorMeter
    {
        protected Container JudgmentsContainer;
        protected Container ColourBars;
        protected Container ColourBarsEarly;
        protected Container ColourBarsLate;

        protected Drawable PerfectHit;

        protected SpriteIcon Arrow;

        protected double MaxHitWindow;

        [Resolved]
        private OsuColour colours { get; set; }

        public BarHitErrorMeter(HitWindows hitWindows)
            : base(hitWindows)
        {
        }

        public override void OnNewJudgement(JudgementResult judgement)
        {
            if (!judgement.IsHit)
                return;

            JudgmentsContainer.Add(CreateJudgement(judgement));
            MoveArrow(judgement);
        }

        protected override void LoadComplete()
        {
            Arrow.Alpha = 0;
            Arrow.Delay(200).FadeInFromZero(600);

            base.LoadComplete();
        }

        protected abstract Drawable CreateColourBar(Color4 color, (HitResult, double) window, bool isFirst);

        protected void CreateColourBars()
        {
            var windows = HitWindows.GetAllAvailableWindows().ToArray();

            MaxHitWindow = windows.First().length;

            for (var i = 0; i < windows.Length; i++)
            {
                ColourBarsEarly.Add(CreateColourBar(getColour(windows[i].result), windows[i], i == 0));
                ColourBarsLate.Add(CreateColourBar(getColour(windows[i].result), windows[i], i == 0));
            }

            PerfectHit = CreateColourBar(getColour(windows.Last().result), windows.Last(), false);
        }

        protected abstract JudgementLine CreateJudgement(JudgementResult result);

        protected abstract void MoveArrow(JudgementResult res);

        private Color4 getColour(HitResult result)
        {
            switch (result)
            {
                case HitResult.Meh:
                    return colours.Yellow;

                case HitResult.Ok:
                    return colours.Green;

                case HitResult.Good:
                    return colours.GreenLight;

                case HitResult.Great:
                    return colours.Blue;

                default:
                    return colours.BlueLight;
            }
        }

        public class JudgementLine : CompositeDrawable
        {
            private const int judgement_fade_duration = 10000;

            private bool is_vertical;

            public JudgementLine(bool isVertical)
            {
                is_vertical = isVertical;

                InternalChild = new CircularContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (is_vertical)
                {
                    Height = 0;
                    this.ResizeHeightTo(1, 200, Easing.OutElasticHalf);
                }
                else
                {
                    Width = 0;
                    this.ResizeWidthTo(1, 200, Easing.OutElasticHalf);
                }
                
                this.FadeTo(0.8f, 150).Then().FadeOut(judgement_fade_duration, Easing.OutQuint).Expire();
            }
        }
    }
}
