using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Audio.Track;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Rulesets.Shape.Objects.Drawables.Pieces
{
    public class BaseDial : BeatSyncedContainer
    {
        private BaseShape shape;
        private ShapeCircle baseCircle;
        private ShapeSquare baseSquare;
        private ShapeTriangle baseTriangle;
        private ShapeX baseX;
        private Container arrow;
        private Container box;
        public int ShapeID { get; set; }

        public BaseDial(BaseShape Shape)
        {
            shape = Shape;
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Children = new Drawable[]
            {
                arrow = new Container
                {
                    Depth = 0,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(shape.ShapeSize / 6 , shape.ShapeSize * 0.6f),
                    Colour = Color4.DarkGray,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Alpha = 1,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                        },
                        new Triangle
                        {
                            Colour = Color4.White,
                            Size = new Vector2(shape.ShapeSize / 4),
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.TopCentre,
                        },
                    }
                },
                box = new Container
                {
                    Depth = -1,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(shape.ShapeSize / 4),
                    Colour = Color4.DarkGray,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Alpha = 1,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                        },
                    }
                },
            };
            switch (ShapeID)
            {
                  case 1:
                    AddRange(new Drawable[]
                    {
                        baseCircle = new ShapeCircle(shape) { Colour = Color4.Gray, Depth = 1 },
                    });
                    break;
                case 2:
                    AddRange(new Drawable[]
                    {
                        baseSquare = new ShapeSquare(shape) { Colour = Color4.Gray, Depth = 1 },
                    });
                    break;
                case 3:
                    AddRange(new Drawable[]
                    {
                        baseTriangle = new ShapeTriangle(shape) { Colour = Color4.Gray, Depth = 1 },
                    });
                    break;
                case 4:
                    AddRange(new Drawable[]
                    {
                        baseX = new ShapeX(shape) { Colour = Color4.Gray, Depth = 1 },
                    });
                    break;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {

        }

        public void StartSpinning(float time)
        {
            box.RotateTo(360, time);
            arrow.RotateTo(360, time);
        }
    }
}
