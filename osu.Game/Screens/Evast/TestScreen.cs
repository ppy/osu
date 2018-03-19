// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Evast
{
    public abstract class TestScreen : BeatmapScreen
    {
        private readonly Container objectParent;
        private readonly FillFlowContainer settingParent;

        public TestScreen()
        {
            Children = new Drawable[]
            {
                objectParent = new Container
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.7f
                },
                settingParent = new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 20),
                    Margin = new MarginPadding(20)
                },
            };

            AddTestObject(objectParent);
            AddSettings(settingParent);
            Connect();
        }

        protected virtual void AddTestObject(Container parent)
        {
        }
        protected virtual void AddSettings(FillFlowContainer parent)
        {
        }
        protected virtual void Connect()
        {
        }
    }
}
