// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Framework.MathUtils;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Mods.Evast.Snake
{
    public class SnakePlayfield : PixelField
    {
        protected override Pixel CreateNewPixel(int size) => new SnakePixel(size);

        public SnakePlayfield(int xCount, int yCount, int pixelSize = 15)
            : base(xCount, yCount, pixelSize)
        {
        }

        private int snakeLength;
        private int headX;
        private int headY;
        private SnakeDirection direction;

        private bool hasFailed;

        protected override void OnStop()
        {
            base.OnStop();
            hasFailed = false;

            for (int y = 0; y < YCount; y++)
                for (int x = 0; x < XCount; x++)
                    (Pixels[x, y] as SnakePixel).Steps = 0;

            for (int y = 0; y < YCount; y++)
                for (int x = 0; x < XCount; x++)
                    if ((Pixels[x, y] as SnakePixel).IsFood)
                        (Pixels[x, y] as SnakePixel).IsFood = false;
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            OnStop();

            snakeLength = 1;
            setHead(XCount / 2, YCount / 2);
            direction = SnakeDirection.Right;

            placeFood();
        }

        protected override void OnContinue()
        {
            if (hasFailed)
            {
                Restart();
                return;
            }

            base.OnContinue();
        }

        private void onFail()
        {
            Pause();
            hasFailed = true;
        }

        protected override void OnNewUpdate()
        {
            base.OnNewUpdate();

            for (int y = 0; y < YCount; y++)
                for (int x = 0; x < XCount; x++)
                    if (Pixels[x, y].IsActive)
                        (Pixels[x, y] as SnakePixel).Steps--;

            newMove();
        }

        private void newMove()
        {
            switch (direction)
            {
                case SnakeDirection.Left:
                    headX--;
                    if (headX < 0)
                        headX = XCount - 1;
                    break;
                case SnakeDirection.Right:
                    headX++;
                    if (headX > XCount - 1)
                        headX = 0;
                    break;
                case SnakeDirection.Up:
                    headY--;
                    if (headY < 0)
                        headY = YCount - 1;
                    break;
                case SnakeDirection.Down:
                    headY++;
                    if (headY > YCount - 1)
                        headY = 0;
                    break;
            }

            if ((Pixels[headX, headY] as SnakePixel).Steps > 0)
            {
                Pixels[headX, headY].FadeColour(Color4.Red);
                onFail();
                return;
            }

            if ((Pixels[headX, headY] as SnakePixel).IsFood)
            {
                placeFood();

                for (int y = 0; y < YCount; y++)
                    for (int x = 0; x < XCount; x++)
                        if (Pixels[x, y].IsActive)
                            (Pixels[x, y] as SnakePixel).Steps++;

                snakeLength++;
            }

            (Pixels[headX, headY] as SnakePixel).SetActive(snakeLength);
        }

        private void placeFood()
        {
            int x = RNG.Next(XCount);
            int y = RNG.Next(YCount);

            if (Pixels[x, y].IsActive || (Pixels[x, y] as SnakePixel).IsFood)
            {
                placeFood();
                return;
            }

            (Pixels[x, y] as SnakePixel).IsFood = true;
        }

        private void setHead(int x, int y)
        {
            headX = x;
            headY = y;
        }

        private void changeDirectionRequest(SnakeDirection newDirection)
        {
            if (hasFailed)
                return;

            Scheduler.CancelDelayedTasks();
            direction = newDirection;
            NewUpdate();
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!args.Repeat)
            {
                switch (args.Key)
                {
                    case Key.Up:
                        if (direction == SnakeDirection.Down || direction == SnakeDirection.Up)
                            return true;
                        changeDirectionRequest(SnakeDirection.Up);
                        return true;
                    case Key.Down:
                        if (direction == SnakeDirection.Down || direction == SnakeDirection.Up)
                            return true;
                        changeDirectionRequest(SnakeDirection.Down);
                        return true;
                    case Key.Left:
                        if (direction == SnakeDirection.Left || direction == SnakeDirection.Right)
                            return true;
                        changeDirectionRequest(SnakeDirection.Left);
                        return true;
                    case Key.Right:
                        if (direction == SnakeDirection.Left || direction == SnakeDirection.Right)
                            return true;
                        changeDirectionRequest(SnakeDirection.Right);
                        return true;
                }
            }

            return base.OnKeyDown(state, args);
        }

        private class SnakePixel : Pixel
        {
            public SnakePixel(int size) : base(size)
            {
            }

            private bool isFood;
            public bool IsFood
            {
                set
                {
                    isFood = value;

                    Background.Colour = isFood ? Color4.Green: Color4.Black.Opacity(170);
                }
                get { return isFood; }
            }

            private int steps;
            public int Steps
            {
                set
                {
                    steps = value;

                    IsActive &= steps != 0;
                }
                get { return steps; }
            }

            public void SetActive(int steps)
            {
                isFood = false;
                IsActive = true;
                this.steps = steps;
            }
        }

        private enum SnakeDirection
        {
            Right,
            Left,
            Up,
            Down
        }
    }
}
