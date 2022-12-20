using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Numbers.Components
{
    public partial class NumbersPlayfield : CompositeDrawable
    {
        private const int spacing = 10;
        private const int move_duration = 175;

        public BindableInt Score = new BindableInt();

        private readonly int rowCount;
        private readonly int columnCount;

        private readonly BindableBool hasFailed = new BindableBool();

        private readonly Container<DrawableNumber> numbersLayer;
        private readonly Container failOverlay;
        private readonly OsuSpriteText scoreText;

        public NumbersPlayfield(int rowCount = 4, int columnCount = 4)
        {
            if (rowCount < 2 || columnCount < 2)
                throw new ArgumentException("Rows and columns count can't be less that 2");

            this.rowCount = rowCount;
            this.columnCount = columnCount;

            Size = new Vector2(columnCount * DrawableNumber.SIZE + spacing * (columnCount + 1), rowCount * DrawableNumber.SIZE + spacing * (rowCount + 1));
            Masking = true;
            CornerRadius = 4;
            InternalChildren = new Drawable[]
            {
                new PlayfieldBackground(rowCount, columnCount),
                numbersLayer = new Container<DrawableNumber>
                {
                    RelativeSizeAxes = Axes.Both
                },
                failOverlay = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White.Opacity(0.5f),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                            Text = "Game Over",
                            Font = OsuFont.GetFont(size: 50, weight: FontWeight.Bold),
                            Colour = new Color4(119, 110, 101, 255),
                            Shadow = false,
                        },
                        scoreText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                            Colour = new Color4(119, 110, 101, 255),
                            Shadow = false,
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            hasFailed.BindValueChanged(onFailChanged);
            Score.BindValueChanged(score => scoreText.Text = $@"score: {score.NewValue}", true);
            Restart();
        }

        private void onFailChanged(ValueChangedEvent<bool> failed)
        {
            failOverlay.FadeTo(failed.NewValue ? 1 : 0, 500, Easing.OutQuint);
            inputIsBlocked = failed.NewValue;
        }

        public void Restart()
        {
            Score.Value = 0;
            hasFailed.Value = false;
            numbersLayer.Clear(true);
            tryAddNumber();
            tryAddNumber();
        }

        private void tryAddNumber()
        {
            // Do we have empty space?
            if (numbersLayer.Count == rowCount * columnCount)
                return;

            int x = RNG.Next(columnCount);
            int y = RNG.Next(rowCount);

            if (getNumberAt(x, y) != null)
            {
                tryAddNumber();
                return;
            }

            setNumberAt(x, y);
        }

        private DrawableNumber getNumberAt(int x, int y)
        {
            if (!numbersLayer.Any())
                return null;

            if (x < 0 || y < 0)
                return null;

            DrawableNumber? number = null;

            numbersLayer.Children.ForEach(c =>
            {
                if (c.XIndex == x && c.YIndex == y)
                {
                    number = c;
                    return;
                }
            });

            return number;
        }

        private void setNumberAt(int x, int y)
        {
            numbersLayer.Add(new DrawableNumber(x, y, RNG.NextBool(0.9) ? 1 : 2)
            {
                Origin = Anchor.Centre,
                Position = getPositionForAxes(x, y)
            });
        }

        private Vector2 getPositionForAxes(int x, int y) => new Vector2(getPosition(x), getPosition(y));

        private int getPosition(int axisValue) => axisValue * DrawableNumber.SIZE + spacing * (axisValue + 1) + DrawableNumber.SIZE / 2;

        private void checkFailCondition()
        {
            if (numbersLayer.Count < rowCount * columnCount)
                return;

            // Field is full, checking for possible moves

            foreach (var n in numbersLayer)
            {
                var topNumber = getNumberAt(n.XIndex, n.YIndex - 1);

                if (topNumber != null)
                {
                    if (topNumber.Power == n.Power)
                        return;
                }

                var bottomNumber = getNumberAt(n.XIndex, n.YIndex + 1);

                if (bottomNumber != null)
                {
                    if (bottomNumber.Power == n.Power)
                        return;
                }

                var leftNumber = getNumberAt(n.XIndex - 1, n.YIndex);

                if (leftNumber != null)
                {
                    if (leftNumber.Power == n.Power)
                        return;
                }

                var rightNumber = getNumberAt(n.XIndex + 1, n.YIndex);

                if (rightNumber != null)
                {
                    if (rightNumber.Power == n.Power)
                        return;
                }
            }

            hasFailed.Value = true;
        }

        private void onInvalidMove()
        {
            this.FadeColour(new Color4(255, 150, 150, 255), 20, Easing.OutQuint).Then().FadeColour(Color4.White, 500, Easing.Out);
        }

        #region Move logic

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat)
            {
                switch (e.Key)
                {
                    case Key.Right:
                        tryMove(MoveDirection.Right);
                        return true;
                    case Key.Left:
                        tryMove(MoveDirection.Left);
                        return true;
                    case Key.Up:
                        tryMove(MoveDirection.Up);
                        return true;
                    case Key.Down:
                        tryMove(MoveDirection.Down);
                        return true;
                }
            }

            return base.OnKeyDown(e);
        }

        private bool inputIsBlocked;

        private void tryMove(MoveDirection direction)
        {
            if (inputIsBlocked || hasFailed.Value)
                return;

            inputIsBlocked = true;

            bool moveIsValid = false;

            switch (direction)
            {
                case MoveDirection.Up:
                    moveIsValid = moveUp();
                    break;

                case MoveDirection.Down:
                    moveIsValid = moveDown();
                    break;

                case MoveDirection.Left:
                    moveIsValid = moveLeft();
                    break;

                case MoveDirection.Right:
                    moveIsValid = moveRight();
                    break;
            }

            if (!moveIsValid)
                onInvalidMove();

            finishMove(moveIsValid);
        }

        private void finishMove(bool moveIsValid)
        {
            Scheduler.AddDelayed(() =>
            {
                numbersLayer.ForEach(n => n.IsBlocked = false);
                inputIsBlocked = false;

                if (moveIsValid)
                    tryAddNumber();

                checkFailCondition();
            }, move_duration + 10);
        }

        private bool moveUp()
        {
            bool moveHasBeenMade = false;

            for (int x = 0; x < columnCount; x++)
            {
                for (int y = 1; y < rowCount; y++)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber == null)
                        continue;

                    DrawableNumber? closest = null;

                    for (int k = y - 1; k >= 0; k--)
                    {
                        var possibleClosest = getNumberAt(x, k);

                        if (possibleClosest != null)
                        {
                            closest = possibleClosest;
                            break;
                        }
                    }

                    if (!tryTargetInteraction(MoveDirection.Up, currentNumber, closest))
                        continue;

                    moveHasBeenMade = true;
                }
            }

            return moveHasBeenMade;
        }

        private bool moveDown()
        {
            bool moveHasBeenMade = false;

            for (int x = 0; x < columnCount; x++)
            {
                for (int y = rowCount - 1; y >= 0; y--)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber == null)
                        continue;

                    DrawableNumber? closest = null;

                    for (int k = y + 1; k < rowCount; k++)
                    {
                        var possibleClosest = getNumberAt(x, k);

                        if (possibleClosest != null)
                        {
                            closest = possibleClosest;
                            break;
                        }
                    }

                    if (!tryTargetInteraction(MoveDirection.Down, currentNumber, closest))
                        continue;

                    moveHasBeenMade = true;
                }
            }

            return moveHasBeenMade;
        }

        private bool moveLeft()
        {
            bool moveHasBeenMade = false;

            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 1; x < columnCount; x++)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber == null)
                        continue;

                    DrawableNumber? closest = null;

                    for (int k = x - 1; k >= 0; k--)
                    {
                        var possibleClosest = getNumberAt(k, y);

                        if (possibleClosest != null)
                        {
                            closest = possibleClosest;
                            break;
                        }
                    }

                    if (!tryTargetInteraction(MoveDirection.Left, currentNumber, closest))
                        continue;

                    moveHasBeenMade = true;
                }
            }

            return moveHasBeenMade;
        }

        private bool moveRight()
        {
            bool moveHasBeenMade = false;

            for (int y = 0; y < rowCount; y++)
            {
                for (int x = columnCount - 1; x >= 0; x--)
                {
                    var currentNumber = getNumberAt(x, y);
                    if (currentNumber == null)
                        continue;

                    DrawableNumber? closest = null;

                    for (int k = x + 1; k < columnCount; k++)
                    {
                        var possibleClosest = getNumberAt(k, y);

                        if (possibleClosest != null)
                        {
                            closest = possibleClosest;
                            break;
                        }
                    }

                    if (!tryTargetInteraction(MoveDirection.Right, currentNumber, closest))
                        continue;

                    moveHasBeenMade = true;
                }
            }

            return moveHasBeenMade;
        }

        /// <summary>
        /// Returns true if move has been made.
        /// </summary>
        private bool tryTargetInteraction(MoveDirection direction, DrawableNumber current, DrawableNumber? target)
        {
            bool horizontal = direction == MoveDirection.Left || direction == MoveDirection.Right;

            if (target == null)
            {
                var newIndex = direction switch
                {
                    MoveDirection.Right => columnCount - 1,
                    MoveDirection.Down => rowCount - 1,
                    _ => 0
                };

                if (newIndex == getNumberIndex(horizontal, current))
                    return false;

                setNewIndex(horizontal, newIndex, current, false);

                return true;
            }

            if (target.IsBlocked || target.Power != current.Power)
            {
                var newIndex = direction switch
                {
                    MoveDirection.Left => target.XIndex + 1,
                    MoveDirection.Right => target.XIndex - 1,
                    MoveDirection.Down => target.YIndex - 1,
                    MoveDirection.Up => target.YIndex + 1,
                    _ => 0
                };

                if (newIndex == getNumberIndex(horizontal, current))
                    return false;

                setNewIndex(horizontal, newIndex, current, false);

                return true;
            }

            current.IsBlocked = true;

            setNewIndex(horizontal, getNumberIndex(horizontal, target), current, true);

            target.IsBlocked = true;
            target.Increment(move_duration);

            Score.Value += target.NumericValue;

            return true;
        }

        private void setNewIndex(bool horizontal, int newIndex, DrawableNumber number, bool expire)
        {
            if (horizontal)
            {
                number.XIndex = newIndex;
                number.MoveToX(getPosition(newIndex), move_duration, Easing.OutQuint);
            }
            else
            {
                number.YIndex = newIndex;
                number.MoveToY(getPosition(newIndex), move_duration, Easing.OutQuint);
            }

            if (expire)
                number.Expire();
        }

        private int getNumberIndex(bool horizontal, DrawableNumber number) => horizontal ? number.XIndex : number.YIndex;

        #endregion

        private enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        private partial class PlayfieldBackground : CompositeDrawable
        {
            public PlayfieldBackground(int rowCount, int columnCount)
            {
                Container mainLayout;

                RelativeSizeAxes = Axes.Both;
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(187, 173, 160, 255)
                    },
                    mainLayout = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(spacing),
                    }
                };

                var rows = new List<FillFlowContainer>();

                for (int i = 0; i < rowCount; i++)
                {
                    var row = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(spacing, 0)
                    };

                    for (int y = 0; y < columnCount; y++)
                    {
                        row.Add(new Container
                        {
                            Size = new Vector2(DrawableNumber.SIZE),
                            Masking = true,
                            CornerRadius = 4,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = new Color4(205, 192, 179, 255)
                            }
                        });
                    }

                    rows.Add(row);
                }

                mainLayout.Add(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, spacing),
                    Children = rows
                });
            }
        }
    }
}
