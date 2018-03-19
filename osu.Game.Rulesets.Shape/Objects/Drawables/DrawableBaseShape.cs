using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Shape.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Shape.Judgements;
using System.Linq;
using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Shape.Objects.Drawables
{
    public class DrawableBaseShape : DrawableShapeHitObject<ShapeHitObject>
    {
        private BaseDial baseDial;
        private ShapeCircle circle;
        private ShapeSquare square;
        private ShapeTriangle triangle;
        private ShapeX x;

        private bool validKeyPressed;

        private readonly BaseShape shape;

        public DrawableBaseShape(BaseShape Shape) : base(Shape)
        {
            shape = Shape;
            Position = shape.StartPosition;
            Alpha = 0;
            AlwaysPresent = true;
            Anchor = Anchor.TopLeft;
            Origin = Anchor.Centre;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LifetimeStart = shape.StartTime - (TIME_PREEMPT + 1000f);
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            base.CheckForJudgements(userTriggered, timeOffset);

            if (LifetimeStart <= Time.Current)
            {
                if (!userTriggered)
                {
                    if (timeOffset > shape.HitWindowGood)
                    {
                        AddJudgement(new ShapeJudgement { Result = HitResult.Miss });
                        Delete();
                    }
                    return;
                }

                double hitOffset = Math.Abs(timeOffset);

                if (hitOffset > shape.HitWindowMiss)
                    return;

                if (!validKeyPressed)
                {
                    AddJudgement(new ShapeJudgement { Result = HitResult.Miss });
                    Delete();
                }
                else if (hitOffset < shape.HitWindowGood)
                {
                    AddJudgement(new ShapeJudgement { Result = hitOffset < shape.HitWindowGreat ? HitResult.Great : HitResult.Good });
                    Delete();
                }
                else
                {
                    AddJudgement(new ShapeJudgement { Result = HitResult.Miss });
                    Delete();
                }
            }   
        }

        public override bool OnPressed(ShapeAction action)
        {
            if (LifetimeStart <= Time.Current)
            {
                switch (shape.ShapeID)
                {
                    case 1:
                        ShapeAction[] hitActionsNorth = { ShapeAction.SouthLeftButton, ShapeAction.SouthRightButton };
                        validKeyPressed = hitActionsNorth.Contains(action);
                        return UpdateJudgement(true);
                    case 2:
                        ShapeAction[] hitActionsSouth = { ShapeAction.WestLeftButton, ShapeAction.WestRightButton };
                        validKeyPressed = hitActionsSouth.Contains(action);
                        return UpdateJudgement(true);
                    case 3:
                        ShapeAction[] hitActionsWest = { ShapeAction.EastLeftButton, ShapeAction.EastRightButton };
                        validKeyPressed = hitActionsWest.Contains(action);
                        return UpdateJudgement(true);
                    case 4:
                        ShapeAction[] hitActionsEast = { ShapeAction.NorthLeftButton, ShapeAction.NorthRightButton };
                        validKeyPressed = hitActionsEast.Contains(action);
                        return UpdateJudgement(true);
                }
            }
            return UpdateJudgement(false);
        }

        protected override void Update()
        {
            base.Update();

            if (LifetimeStart <= Time.Current)
            {
                if (Time.Current >= (shape.StartTime - TIME_PREEMPT) - 500 && !loaded)
                {
                    preLoad();
                }

                if (Time.Current >= shape.StartTime - TIME_PREEMPT && !started)
                {
                    start();
                }
            }
        }

        private bool loaded = false;
        private bool started = false;

        private void preLoad()
        {
            loaded = true;
            switch (shape.ShapeID)
            {
                case 1:
                    Children = new Drawable[]
                    {
                        baseDial = new BaseDial(shape)
                        {
                            Depth = -1,
                            ShapeID = shape.ShapeID,
                        },
                        circle = new ShapeCircle(shape) { Depth = -2, Colour = Color4.Red, },
                    };
                    break;
                case 2:
                    Children = new Drawable[]
                    {
                        baseDial = new BaseDial(shape)
                        {
                            Depth = -1,
                            ShapeID = shape.ShapeID,
                        },
                        square = new ShapeSquare(shape) { Depth = -2, Colour = Color4.Violet, },
                    };
                    break;
                case 3:
                    Children = new Drawable[]
                    {
                        baseDial = new BaseDial(shape)
                        {
                            Depth = -1,
                            ShapeID = shape.ShapeID,
                        },
                        triangle = new ShapeTriangle(shape) { Depth = -2, Colour = Color4.Green, },
                    };
                    break;
                case 4:
                    Children = new Drawable[]
                    {
                        baseDial = new BaseDial(shape)
                        {
                            Depth = -1,
                            ShapeID = shape.ShapeID,
                        },
                        x = new ShapeX(shape) { Depth = -2, Colour = Color4.Blue, },
                    };
                    break;
            }
        }

        private void start()
        {
            started = true;
            this.FadeIn(TIME_FADEIN);
            baseDial.StartSpinning(TIME_PREEMPT);
            switch (shape.ShapeID)
            {
                case 1:
                    circle.Position = new Vector2(RNG.Next(-200, 200), -400);
                    circle.MoveTo(baseDial.Position, TIME_PREEMPT);
                    break;
                case 2:
                    square.Position = new Vector2(RNG.Next(-200, 200), -400);
                    square.MoveTo(baseDial.Position, TIME_PREEMPT);
                    break;
                case 3:
                    triangle.Position = new Vector2(RNG.Next(-200, 200), -400);
                    triangle.MoveTo(baseDial.Position, TIME_PREEMPT);
                    break;
                case 4:
                    x.Position = new Vector2(RNG.Next(-200, 200), -400);
                    x.MoveTo(baseDial.Position, TIME_PREEMPT);
                    break;
            }
        }

        //Marks this for death
        internal void Delete()
        {
            AlwaysPresent = true;
            Alpha = 0;
            LifetimeEnd = Time.Current;
            Expire();
        }
    }
}
