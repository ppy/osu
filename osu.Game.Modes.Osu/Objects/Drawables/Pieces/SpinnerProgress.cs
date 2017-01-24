using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SpinnerProgress : CircularContainer
    {
        private const int segmentCount = 100;
        private CircularContainer gray = new CircularContainer
        {
            Size = new Vector2(207),
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            BorderThickness = 1,
            BorderColour = Color4.Gray,
            Alpha = 0.64f,
            Depth = segmentCount + 1,
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.Gray,
                    Alpha = 0.01f,
                    FillMode = FillMode.Fill
                }
            }
        };

        public SpinnerProgress(Spinner s)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(215);
            Alpha = 1;
        }

        public float Progress = 0;
        public bool IsSpinningLeft;

        protected override void Update()
        {
            base.Update();
            int count = -1;
            foreach (Container seg in Children)
            {
                if (Progress > (float)count / segmentCount)
                    seg.RotateTo(360*((float)count / segmentCount), 100);
                else
                    seg.RotateTo(Progress * 360, 100);
                //seg.Alpha = Convert.ToInt32((360/segmentCount*count)-(360/segmentCount/2) <= Progress * 360);
                count++;
            }
            if (IsSpinningLeft)
                RotateTo(Progress * -360, 100);
            else
                RotateTo(0, 100);
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            for (int i = 0; i < segmentCount; i++)
            {
                Add(new SpinnerProgressSegment() as Container);
            }
            Add(gray);
        }
    }
}
