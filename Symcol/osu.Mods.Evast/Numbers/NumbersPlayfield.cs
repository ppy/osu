// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Mods.Evast.Numbers
{
    public class NumbersPlayfield : Container
    {
        private const int move_duration = 120;

        public BindableInt Score = new BindableInt();

        private readonly Container<Number> numbersLayer;
        private readonly Container failedOverlay;

        private bool failed;
        private bool hasFailed
        {
            set
            {
                failed = value;

                failedOverlay.FadeTo(value ? 1 : 0, 200);
            }
            get
            {
                return failed;
            }
        }

        public NumbersPlayfield()
        {
            Size = new Vector2(500);
            CornerRadius = 6;
            Masking = true;
            Children = new Drawable[]
            {
                new Container //Background layer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"BBADA0"),
                        },
                        new BackgroundPanel(20,20),
                        new BackgroundPanel(140,20),
                        new BackgroundPanel(260,20),
                        new BackgroundPanel(380,20),
                        new BackgroundPanel(20,140),
                        new BackgroundPanel(140,140),
                        new BackgroundPanel(260,140),
                        new BackgroundPanel(380,140),
                        new BackgroundPanel(20,260),
                        new BackgroundPanel(140,260),
                        new BackgroundPanel(260,260),
                        new BackgroundPanel(380,260),
                        new BackgroundPanel(20,380),
                        new BackgroundPanel(140,380),
                        new BackgroundPanel(260,380),
                        new BackgroundPanel(380,380),
                    }
                },
                numbersLayer = new Container<Number>
                {
                    RelativeSizeAxes = Axes.Both,
                },
                failedOverlay = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White.Opacity(220),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                            Text = @"Game Over",
                            Font = @"Exo2.0-Bold",
                            TextSize = 70,
                            Colour = OsuColour.FromHex(@"776E65"),
                            Shadow = false,
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            setNumber();
            setNumber();
        }

        public void Reset()
        {
            numbersLayer.Clear(true);

            if (hasFailed)
                hasFailed = false;

            Score.Value = 0;

            setNumber();
            setNumber();
        }

        private void setNumber()
        {
            int x = RNG.Next(4);
            int y = RNG.Next(4);

            if (getNumberAt(x, y) != null)
            {
                setNumber();
                return;
            }

            numbersLayer.Add(new Number
            {
                Position = new Vector2(70 + (x * 120), 70 + (y * 120)),
                Coordinates = new Vector2(70 + (x * 120), 70 + (y * 120))
            });
        }

        private void unlockAll()
        {
            foreach (var n in numbersLayer)
                n.IsLocked = false;
        }

        private void finishAllTransforms()
        {
            FinishTransforms(true);
            Scheduler.Update();
        }

        private void checkMoveAbility()
        {
            hasFailed |= !canMove();
        }

        private bool canMove()
        {
            for (int y = 0; y < 4; y++)
            {
                for (int x = 1; x <= 3; x++)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber != null)
                    {
                        var possibleSwap = getNumberAt(x - 1, y);
                        if (possibleSwap == null)
                            return true;

                        if (possibleSwap.Value == currentNumber.Value)
                            return true;
                    }
                }
            }

            for (int y = 0; y < 4; y++)
            {
                for (int x = 2; x >= 0; x--)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber != null)
                    {
                        var possibleSwap = getNumberAt(x + 1, y);
                        if (possibleSwap == null)
                            return true;

                        if (possibleSwap.Value == currentNumber.Value)
                            return true;
                    }
                }
            }

            for (int x = 0; x < 4; x++)
            {
                for (int y = 2; y >= 0; y--)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber != null)
                    {
                        var possibleSwap = getNumberAt(x, y + 1);
                        if (possibleSwap == null)
                            return true;

                        if (possibleSwap.Value == currentNumber.Value)
                            return true;
                    }
                }
            }

            for (int x = 0; x < 4; x++)
            {
                for (int y = 1; y <= 3; y++)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber != null)
                    {
                        var possibleSwap = getNumberAt(x, y - 1);
                        if (possibleSwap == null)
                            return true;

                        if (possibleSwap.Value == currentNumber.Value)
                            return true;
                    }
                }
            }

            return false;
        }

        #region Move logic

        private void moveRight()
        {
            finishAllTransforms();

            int moveCount = 0;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 2; x >= 0; x--)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber != null)
                    {
                        for (int k = x + 1; k <= 3; k++)
                        {
                            var possibleSwap = getNumberAt(k, y);
                            if (possibleSwap == null)
                            {
                                if (k == 3)
                                {
                                    currentNumber.Coordinates = new Vector2(430, currentNumber.Coordinates.Y);
                                    currentNumber.MoveToX(430, move_duration);
                                    moveCount++;
                                    break;
                                }

                                continue; // go check the next field
                            }

                            if (!possibleSwap.IsLocked && possibleSwap.Value == currentNumber.Value)
                            {
                                currentNumber.Coordinates = new Vector2(70 + (k * 120), currentNumber.Coordinates.Y);
                                currentNumber.MoveToX(70 + (k * 120), move_duration).Finally((d) => d.Expire());
                                possibleSwap.Lock();
                                possibleSwap.IncreaseValue();
                                Score.Value += possibleSwap.Value;

                                Scheduler.AddDelayed(possibleSwap.IncreaseValueAnimation, move_duration);

                                moveCount++;
                                break;
                            }

                            var possibleNewX = 70 + ((k - 1) * 120);
                            if (currentNumber.X != possibleNewX)
                            {
                                currentNumber.Coordinates = new Vector2(possibleNewX, currentNumber.Coordinates.Y);
                                currentNumber.MoveToX(possibleNewX, move_duration);
                                moveCount++;
                                break;
                            }

                            break;
                        }
                    }
                }
            }

            unlockAll();

            if (moveCount > 0)
                setNumber();

            checkMoveAbility();
        }

        private void moveDown()
        {
            finishAllTransforms();

            int moveCount = 0;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 2; y >= 0; y--)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber != null)
                    {
                        for (int k = y + 1; k <= 3; k++)
                        {
                            var possibleSwap = getNumberAt(x, k);
                            if (possibleSwap == null)
                            {
                                if (k == 3)
                                {
                                    currentNumber.Coordinates = new Vector2(currentNumber.Coordinates.X, 430);
                                    currentNumber.MoveToY(430, move_duration);
                                    moveCount++;
                                    break;
                                }

                                continue; // go check the next field
                            }

                            if (!possibleSwap.IsLocked && possibleSwap.Value == currentNumber.Value)
                            {
                                currentNumber.Coordinates = new Vector2(currentNumber.Coordinates.X, 70 + (k * 120));
                                currentNumber.MoveToY(70 + (k * 120), move_duration).Finally((d) => d.Expire());
                                possibleSwap.Lock();
                                possibleSwap.IncreaseValue();
                                Score.Value += possibleSwap.Value;

                                Scheduler.AddDelayed(possibleSwap.IncreaseValueAnimation, move_duration);

                                moveCount++;
                                break;
                            }

                            var possibleNewY = 70 + ((k - 1) * 120);
                            if (currentNumber.Y != possibleNewY)
                            {
                                currentNumber.Coordinates = new Vector2(currentNumber.Coordinates.X, possibleNewY);
                                currentNumber.MoveToY(possibleNewY, move_duration);
                                moveCount++;
                                break;
                            }

                            break;
                        }
                    }
                }
            }

            unlockAll();

            if (moveCount > 0)
                setNumber();

            checkMoveAbility();
        }

        private void moveLeft()
        {
            finishAllTransforms();

            int moveCount = 0;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 1; x <= 3; x++)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber != null)
                    {
                        for (int k = x - 1; k >= 0; k--)
                        {
                            var possibleSwap = getNumberAt(k, y);
                            if (possibleSwap == null)
                            {
                                if (k == 0)
                                {
                                    currentNumber.Coordinates = new Vector2(70, currentNumber.Coordinates.Y);
                                    currentNumber.MoveToX(70, move_duration);
                                    moveCount++;
                                    break;
                                }

                                continue; // go check the next field
                            }

                            if (!possibleSwap.IsLocked && possibleSwap.Value == currentNumber.Value)
                            {
                                currentNumber.Coordinates = new Vector2(70 + (k * 120), currentNumber.Coordinates.Y);
                                currentNumber.MoveToX(70 + (k * 120), move_duration).Finally((d) => d.Expire());
                                possibleSwap.Lock();
                                possibleSwap.IncreaseValue();
                                Score.Value += possibleSwap.Value;

                                Scheduler.AddDelayed(possibleSwap.IncreaseValueAnimation, move_duration);

                                moveCount++;
                                break;
                            }

                            var possibleNewX = 70 + ((k + 1) * 120);
                            if (currentNumber.X != possibleNewX)
                            {
                                currentNumber.Coordinates = new Vector2(possibleNewX, currentNumber.Coordinates.Y);
                                currentNumber.MoveToX(possibleNewX, move_duration);
                                moveCount++;
                                break;
                            }

                            break;
                        }
                    }
                }
            }

            unlockAll();

            if (moveCount > 0)
                setNumber();

            checkMoveAbility();
        }

        private void moveUp()
        {
            finishAllTransforms();

            int moveCount = 0;

            for (int x = 0; x < 4; x++)
            {
                for (int y = 1; y <= 3; y++)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber != null)
                    {
                        for (int k = y - 1; k >= 0; k--)
                        {
                            var possibleSwap = getNumberAt(x, k);
                            if (possibleSwap == null)
                            {
                                if (k == 0)
                                {
                                    currentNumber.Coordinates = new Vector2(currentNumber.Coordinates.X, 70);
                                    currentNumber.MoveToY(70, move_duration);
                                    moveCount++;
                                    break;
                                }

                                continue; // go check the next field
                            }

                            if (!possibleSwap.IsLocked && possibleSwap.Value == currentNumber.Value)
                            {
                                currentNumber.Coordinates = new Vector2(currentNumber.Coordinates.X, 70 + (k * 120));
                                currentNumber.MoveToY(70 + (k * 120), move_duration).Finally((d) => d.Expire());
                                possibleSwap.Lock();
                                possibleSwap.IncreaseValue();
                                Score.Value += possibleSwap.Value;

                                Scheduler.AddDelayed(possibleSwap.IncreaseValueAnimation, move_duration);

                                moveCount++;
                                break;
                            }

                            var possibleNewY = 70 + ((k + 1) * 120);
                            if (currentNumber.Y != possibleNewY)
                            {
                                currentNumber.Coordinates = new Vector2(currentNumber.Coordinates.X, possibleNewY);
                                currentNumber.MoveToY(possibleNewY, move_duration);
                                moveCount++;
                                break;
                            }

                            break;
                        }
                    }
                }
            }

            unlockAll();

            if (moveCount > 0)
                setNumber();

            checkMoveAbility();
        }

        #endregion

        private Number getNumberAt(int x, int y)
        {
            if (numbersLayer.Children.Count > 0)
            {
                foreach (var n in numbersLayer.Children)
                {
                    if (n.Coordinates == new Vector2(70 + (x * 120), 70 + (y * 120)))
                        return n;
                }
            }

            return null;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!hasFailed)
            {
                if (!e.Repeat)
                {
                    switch (e.Key)
                    {
                        case Key.Right:
                            moveRight();
                            return true;
                        case Key.Left:
                            moveLeft();
                            return true;
                        case Key.Up:
                            moveUp();
                            return true;
                        case Key.Down:
                            moveDown();
                            return true;
                    }
                }
            }

            return base.OnKeyDown(e);
        }

        private class BackgroundPanel : Container
        {
            public BackgroundPanel(int x, int y)
            {
                Position = new Vector2(x, y);
                Size = new Vector2(100);
                CornerRadius = 6;
                Masking = true;
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"CDC0B3"),
                };
            }
        }
    }
}
