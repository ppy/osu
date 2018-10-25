// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.States;
using osu.Framework.MathUtils;

namespace osu.Mods.Evast.LifeGame
{
    public class LifeGamePlayfield : PixelField
    {
        public override bool HandleNonPositionalInput => false;
        public override bool HandlePositionalInput => false;

        private readonly bool[,] previousIterationMap;

        protected override Pixel CreateNewPixel(int size) => new Cell(size);

        public LifeGamePlayfield(int xCount, int yCount, int pixelSize = 15)
            : base(xCount, yCount, pixelSize)
        {
            previousIterationMap = new bool[xCount, yCount];
        }

        protected override void OnStop()
        {
            base.OnStop();

            resetField();
            resetMap();
        }

        protected override void OnNewUpdate()
        {
            base.OnNewUpdate();

            mapCurrentIteration();

            if (fieldIsEmpty())
            {
                Stop();
                return;
            }

            for (int y = 0; y < YCount; y++)
            {
                for (int x = 0; x < XCount; x++)
                {
                    int nearbyCellsAmount = countNearbyCells(x, y);

                    if (!previousIterationMap[x, y] && nearbyCellsAmount == 3)
                    {
                        Pixels[x, y].IsActive = true;
                        continue;
                    }

                    if (previousIterationMap[x, y] && !(nearbyCellsAmount == 2 || nearbyCellsAmount == 3))
                        Pixels[x, y].IsActive = false;
                }
            }
        }

        public void GenerateRandom()
        {
            for (int y = 0; y < YCount; y++)
                for (int x = 0; x < XCount; x++)
                    Pixels[x, y].IsActive = RNG.NextBool();
        }

        private void resetMap()
        {
            for (int y = 0; y < YCount; y++)
                for (int x = 0; x < XCount; x++)
                    previousIterationMap[x, y] = false;
        }

        private void resetField()
        {
            for (int y = 0; y < YCount; y++)
                for (int x = 0; x < XCount; x++)
                    Pixels[x, y].IsActive = false;
        }

        private bool fieldIsEmpty()
        {
            for (int y = 0; y < YCount; y++)
                for (int x = 0; x < XCount; x++)
                    if (Pixels[x, y].IsActive)
                        return false;

            return true;
        }

        private int countNearbyCells(int x, int y)
        {
            int amount = 0;

            ////
            int checkableX = x - 1;
            int checkableY = y - 1;

            if (checkableX < 0) checkableX = XCount - 1;
            if (checkableY < 0) checkableY = YCount - 1;

            if (previousIterationMap[checkableX, checkableY]) amount++;

            ////

            if (previousIterationMap[x, checkableY]) amount++;

            ////

            checkableX = x + 1;

            if (checkableX > XCount - 1) checkableX = 0;

            if (previousIterationMap[checkableX, checkableY]) amount++;

            ////

            checkableX = x - 1;

            if (checkableX < 0) checkableX = XCount - 1;

            if (previousIterationMap[checkableX, y]) amount++;

            ////

            checkableX = x + 1;

            if (checkableX > XCount - 1) checkableX = 0;

            if (previousIterationMap[checkableX, y]) amount++;

            ////

            checkableX = x - 1;
            checkableY = y + 1;

            if (checkableX < 0) checkableX = XCount - 1;
            if (checkableY > YCount - 1) checkableY = 0;

            if (previousIterationMap[checkableX, checkableY]) amount++;

            ////

            if (previousIterationMap[x, checkableY]) amount++;

            ////

            checkableX = x + 1;

            if (checkableX > XCount - 1) checkableX = 0;

            if (previousIterationMap[checkableX, checkableY]) amount++;
            ////

            return amount;
        }

        private void mapCurrentIteration()
        {
            for (int y = 0; y < YCount; y++)
                for (int x = 0; x < XCount; x++)
                    previousIterationMap[x, y] = Pixels[x, y].IsActive;
        }

        private class Cell : Pixel
        {
            public Cell(int size) : base(size)
            {
            }

            protected override bool OnClick(InputState state)
            {
                IsActive = !IsActive;
                return base.OnClick(state);
            }
        }
    }
}
