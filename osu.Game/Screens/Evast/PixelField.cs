// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Evast
{
    public class PixelField : Container
    {
        protected virtual Pixel CreateNewPixel(int size) => new Pixel(size);

        protected readonly Pixel[,] Pixels;

        private double updateDelay;
        public double UpdateDelay
        {
            set { updateDelay = value; }
            get { return updateDelay; }
        }

        protected readonly int XCount;
        protected readonly int YCount;

        public PixelField(int xCount, int yCount, int pixelSize = 15)
        {
            XCount = xCount;
            YCount = yCount;
            updateDelay = 200;

            Pixels = new Pixel[xCount, yCount];

            Size = new Vector2(xCount * pixelSize, yCount * pixelSize);

            for (int y = 0; y < yCount; y++)
            {
                for (int x = 0; x < xCount; x++)
                {
                    Pixels[x, y] = CreateNewPixel(pixelSize);
                    Pixels[x, y].Position = new Vector2(x * pixelSize, y * pixelSize);

                    Add(Pixels[x, y]);
                }
            }

            for (int x = 0; x <= xCount; x++)
            {
                Add(new Container
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = 2,
                    Position = new Vector2(x * pixelSize - 1, 0),
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray,
                    }
                });
            }

            // horizontal lines
            for (int y = 0; y <= yCount; y++)
            {
                Add(new Container
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 2,
                    Position = new Vector2(0, y * pixelSize - 1),
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray,
                    }
                });
            }
        }

        protected virtual void OnRestart()
        {
        }

        protected virtual void OnPause()
        {
        }

        protected virtual void OnContinue()
        {
            if (!isGoing)
            {
                isGoing = true;
                NewUpdate();
            }
        }

        protected virtual void OnStop()
        {
        }

        protected virtual void OnNewUpdate()
        {
        }

        private bool isGoing;

        public void Restart()
        {
            Scheduler.CancelDelayedTasks();
            OnRestart();

            isGoing = true;

            NewUpdate();
        }

        public void Stop()
        {
            Scheduler.CancelDelayedTasks();
            OnStop();

            isGoing = false;
        }

        public void Pause()
        {
            Scheduler.CancelDelayedTasks();
            OnPause();

            isGoing = false;
        }

        public void Continue() => OnContinue();

        public void NewUpdate()
        {
            OnNewUpdate();

            if (isGoing)
                Scheduler.AddDelayed(NewUpdate, updateDelay);
        }

        protected class Pixel : Container
        {
            protected readonly Box Background;

            private bool isActive;
            public bool IsActive
            {
                set
                {
                    if (isActive == value)
                        return;
                    isActive = value;

                    Background.Colour = isActive ? Color4.White : Color4.Black.Opacity(170);
                }
                get { return isActive; }
            }

            public Pixel(int size)
            {
                Size = new Vector2(size);
                Child = Background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(170),
                };
            }
        }
    }
}
