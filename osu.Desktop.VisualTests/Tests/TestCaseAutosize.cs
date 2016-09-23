// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.GameModes.Testing;

namespace osu.Framework.VisualTests.Tests
{
    class TestCaseAutosize : TestCase
    {
        public override string Name => @"Autosize";
        public override string Description => @"Various scenarios which potentially challenge autosize calculations.";

        private ToggleButton toggleDebugAutosize;

        private Container testContainer;

        public override void Reset()
        {
            base.Reset();

            toggleDebugAutosize = AddToggle(@"debug autosize", reloadCallback);

            Add(testContainer = new LargeContainer());

            for (int i = 1; i <= 6; i++)
            {
                int test = i;
                AddButton($@"Test {i}", delegate { loadTest(test); });
            }

            loadTest(1);

            Add(new Box
            {
                Colour = Color4.Black,
                Size = new Vector2(22, 4),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            Add(new Box
            {
                Colour = Color4.Black,
                Size = new Vector2(4, 22),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            Add(new Box
            {
                Colour = Color4.WhiteSmoke,
                Size = new Vector2(20, 2),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            Add(new Box
            {
                Colour = Color4.WhiteSmoke,
                Size = new Vector2(2, 20),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        private void reloadCallback()
        {
            loadTest(currentTest);
        }

        private int currentTest;

        private void loadTest(int testType)
        {
            currentTest = testType;

            testContainer.Clear();

            Container box;

            switch (currentTest)
            {
                case 1:
                    testContainer.Add(box = new InfofulBoxAutoSize
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });

                    addCornerMarkers(box);

                    box.Add(new InfofulBox(RectangleF.Empty, 0, Color4.Blue)
                    {
                        //chameleon = true,
                        Position = new Vector2(0, 0),
                        Size = new Vector2(25, 25),
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre
                    });

                    box.Add(box = new InfofulBox(RectangleF.Empty, 0, Color4.DarkSeaGreen)
                    {
                        Size = new Vector2(250, 250),
                        Alpha = 0.5f,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre
                    });

                    box.OnUpdate += delegate { box.Rotation += 0.05f; };
                    break;
                case 2:
                    testContainer.Add(box = new InfofulBoxAutoSize
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });

                    addCornerMarkers(box, 5);


                    box.Add(box = new InfofulBoxAutoSize
                    {
                        Colour = Color4.DarkSeaGreen,
                        Alpha = 0.5f,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre
                    });

                    Drawable localBox = box;
                    box.OnUpdate += delegate { localBox.Rotation += 0.05f; };

                    box.Add(new InfofulBox(RectangleF.Empty, 0, Color4.Blue)
                    {
                        //chameleon = true,
                        Size = new Vector2(100, 100),
                        Position = new Vector2(50, 50),
                        Alpha = 0.5f,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre
                    });
                    break;
                case 3:
                    testContainer.Add(box = new InfofulBoxAutoSize
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });

                    addCornerMarkers(box, 10, Color4.YellowGreen);

                    for (int i = 0; i < 100; i++)
                    {
                        box.Add(box = new InfofulBoxAutoSize
                        {
                            Colour = new Color4(253, 253, 253, 255),
                            Position = new Vector2(3, 3),
                            Origin = Anchor.BottomRight,
                            Anchor = Anchor.BottomRight
                        });
                    }

                    addCornerMarkers(box, 2);

                    box.Add(new InfofulBox(RectangleF.Empty, 1, Color4.SeaGreen)
                    {
                        //chameleon = true,
                        Size = new Vector2(50, 50),
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft
                    });
                    break;
                case 4:
                    testContainer.Add(box = new InfofulBoxAutoSize
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreLeft
                    });

                    box.Add(new InfofulBox(RectangleF.Empty, 0, Color4.OrangeRed)
                    {
                        Position = new Vector2(5, 0),
                        Size = new Vector2(300, 80),
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft
                    });

                    box.Add(new SpriteText
                    {
                        Position = new Vector2(5, -20),
                        Text = "Test CentreLeft line 1",
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft
                    });

                    box.Add(new SpriteText
                    {
                        Position = new Vector2(5, 20),
                        Text = "Test CentreLeft line 2",
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft
                    });
                    break;
                case 5:
                    testContainer.Add(box = new InfofulBoxAutoSize
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreLeft
                    });

                    box.Add(new InfofulBox(RectangleF.Empty, 0, Color4.OrangeRed)
                    {
                        Position = new Vector2(5, 0),
                        Size = new Vector2(300, 80),
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft
                    });

                    box.Add(new SpriteText
                    {
                        Position = new Vector2(5, -20),
                        Text = "123,456,789=",
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        Scale = new Vector2(2f)
                    });

                    box.Add(new SpriteText
                    {
                        Position = new Vector2(5, 20),
                        Text = "123,456,789ms",
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft
                    });
                    break;
                case 6:
                    testContainer.Add(box = new InfofulBoxAutoSize
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    });

                    box.Add(box = new InfofulBoxAutoSize
                    {
                        Colour = Color4.OrangeRed,
                        Position = new Vector2(100, 100),
                        Origin = Anchor.Centre,
                        Anchor = Anchor.TopLeft
                    });

                    box.Add(new InfofulBox(RectangleF.Empty, 0, Color4.OrangeRed)
                    {
                        Position = new Vector2(100, 100),
                        Size = new Vector2(100, 100),
                        Origin = Anchor.Centre,
                        Anchor = Anchor.TopLeft
                    });
                    break;
            }

#if DEBUG
            //if (toggleDebugAutosize.State)
            //    testContainer.Children.FindAll(c => c.HasAutosizeChildren).ForEach(c => c.AutoSizeDebug = true);
#endif
        }

        private void addCornerMarkers(Container box, int size = 50, Color4? colour = null)
        {
            box.Add(new InfofulBox(RectangleF.Empty, 2, colour ?? Color4.Red)
            {
                //chameleon = true,
                Size = new Vector2(size, size),
                Origin = Anchor.TopLeft,
                Anchor = Anchor.TopLeft,
                AllowDrag = false
            });

            box.Add(new InfofulBox(RectangleF.Empty, 2, colour ?? Color4.Red)
            {
                //chameleon = true,
                Size = new Vector2(size, size),
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                AllowDrag = false
            });

            box.Add(new InfofulBox(RectangleF.Empty, 2, colour ?? Color4.Red)
            {
                //chameleon = true,
                Size = new Vector2(size, size),
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                AllowDrag = false
            });

            box.Add(new InfofulBox(RectangleF.Empty, 2, colour ?? Color4.Red)
            {
                //chameleon = true,
                Size = new Vector2(size, size),
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
                AllowDrag = false
            });
        }
    }

    class InfofulBoxAutoSize : AutoSizeContainer
    {
        public override void Load()
        {
            base.Load();

            Masking = true;

            Add(new Box
            {
                SizeMode = InheritMode.XY
            });
        }

        public bool AllowDrag = true;

        protected override bool OnDrag(InputState state)
        {
            if (!AllowDrag) return false;

            Position += state.Mouse.Delta;
            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            return true;
        }

        protected override bool OnDragStart(InputState state) => AllowDrag;
    }

    class InfofulBox : Container
    {
        private SpriteText debugInfo;

        public bool chameleon = false;

        public InfofulBox(RectangleF rectangle, float depth, Color4 color)
        {
            Position = new Vector2(rectangle.X, rectangle.Y);
            Size = new Vector2(rectangle.Width, rectangle.Height);
            Depth = depth;
            Colour = color;
        }

        public bool AllowDrag = true;

        protected override bool OnDrag(InputState state)
        {
            if (!AllowDrag) return false;

            Position += state.Mouse.Delta;
            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            return true;
        }

        protected override bool OnDragStart(InputState state) => AllowDrag;

        public override void Load()
        {
            base.Load();

            Add(new Box
            {
                SizeMode = InheritMode.XY
            });

            debugInfo = new SpriteText
            {
                Colour = Color4.Black
            };
            Add(debugInfo);
        }

        int lastSwitch;

        protected override void Update()
        {
            if (chameleon && (int)Time / 1000 != lastSwitch)
            {
                lastSwitch = (int)Time / 1000;
                switch (lastSwitch % 6)
                {
                    case 0:
                        Anchor = (Anchor)((int)Anchor + 1);
                        Origin = (Anchor)((int)Origin + 1);
                        break;
                    case 1:
                        MoveTo(new Vector2(0, 0), 800, EasingTypes.Out);
                        break;
                    case 2:
                        MoveTo(new Vector2(200, 0), 800, EasingTypes.Out);
                        break;
                    case 3:
                        MoveTo(new Vector2(200, 200), 800, EasingTypes.Out);
                        break;
                    case 4:
                        MoveTo(new Vector2(0, 200), 800, EasingTypes.Out);
                        break;
                    case 5:
                        MoveTo(new Vector2(0, 0), 800, EasingTypes.Out);
                        break;
                }
            }

            base.Update();

            //debugInfo.Text = ToString();
        }
    }
}
