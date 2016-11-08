//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Game.GameModes.Play;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Caching;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;

namespace osu.Game.Overlays
{
    class ToolbarModeSelector : Container
    {
        const float padding = 10;

        private FlowContainer modeButtons;
        private Box modeButtonLine;
        private ToolbarModeButton activeButton;

        public Action<PlayMode> OnPlayModeChange;
        
        public ToolbarModeSelector()
        {
            RelativeSizeAxes = Axes.Y;
        }

        [Initializer]
        private void Load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(20, 20, 20, 255)
                },
                modeButtons = new FlowContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FlowDirection.HorizontalOnly,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
                modeButtonLine = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0.3f, 3),
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopCentre,
                    Colour = Color4.White
                }
            };

            foreach (PlayMode m in Enum.GetValues(typeof(PlayMode)))
            {
                var localMode = m;
                modeButtons.Add(new ToolbarModeButton
                {
                    Mode = m,
                    Action = delegate
                    {
                        SetGameMode(localMode);
                        OnPlayModeChange?.Invoke(localMode);
                    }
                });
            }

            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(modeButtons.Children.Count() * ToolbarButton.WIDTH + padding * 2, 1);
        }

        public void SetGameMode(PlayMode mode)
        {
            foreach (ToolbarModeButton m in modeButtons.Children.Cast<ToolbarModeButton>())
            {
                bool isActive = m.Mode == mode;
                m.Active = isActive;
                if (isActive)
                    activeButton = m;
            }

            activeMode.Invalidate();
        }

        private Cached activeMode = new Cached();

        protected override void UpdateLayout()
        {
            base.UpdateLayout();

            if (!activeMode.EnsureValid())
                activeMode.Refresh(() => modeButtonLine.MoveToX(activeButton.DrawPosition.X + activeButton.DrawSize.X / 2 + padding, 200, EasingTypes.OutQuint));
        }
    }
}
