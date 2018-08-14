// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Components
{
    public class GameTypeTabControl : OsuTabControl<GameType>
    {
        protected override TabItem<GameType> CreateTabItem(GameType value) => new GameTypeTabItem(value);

        protected override Dropdown<GameType> CreateDropdown() => null;

        private readonly OsuSpriteText activeTabText;

        public GameTypeTabControl()
        {
            AddInternal(activeTabText = new OsuSpriteText
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            activeTabText.Colour = colours.Yellow;
        }

        protected override void SelectTab(TabItem<GameType> tab)
        {
            base.SelectTab(tab);

            activeTabText.Text = tab.Value.GetDescription();
        }

        private class GameTypeTabItem : OsuTabItem
        {
            private readonly DrawableGameType drawableType;

            public GameTypeTabItem(GameType value)
                : base(value)
            {
                AutoSizeAxes = Axes.None;
                RelativeSizeAxes = Axes.None;

                Child = drawableType = new DrawableGameType(value, 24)
                {
                    RelativeSizeAxes = Axes.Both,
                };

                Height = 60;
                Width = 60;

                Colour = Color4.White;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                drawableType.BorderColour = colours.Yellow;
            }

            private void fadeActive()
            {
                drawableType.BorderThickness = 5;
            }

            private void fadeInactive()
            {
                drawableType.BorderThickness = 0;
            }

            protected override void OnActivated()
            {
                base.OnActivated();

                fadeActive();
            }

            protected override void OnDeactivated()
            {
                base.OnDeactivated();

                fadeInactive();
            }
        }
    }
}
