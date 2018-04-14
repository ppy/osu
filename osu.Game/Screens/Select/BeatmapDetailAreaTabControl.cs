// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailAreaTabControl : Container
    {
        public static readonly float HEIGHT = 24;
        private readonly OsuTabControlCheckbox modsCheckbox;
        private readonly OsuTabControl<BeatmapDetailTab> tabs;

        public Action<BeatmapDetailTab, bool> OnFilter; //passed the selected tab and if mods is checked

        private Bindable<BeatmapDetailTab> selectedTab;

        private void invokeOnFilter()
        {
            OnFilter?.Invoke(tabs.Current, modsCheckbox.Current);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, OsuConfigManager config)
        {
            modsCheckbox.AccentColour = tabs.AccentColour = colour.YellowLight;

            selectedTab = config.GetBindable<BeatmapDetailTab>(OsuSetting.BeatmapDetailTab);

            tabs.Current.BindTo(selectedTab);
            tabs.Current.TriggerChange();
        }

        public BeatmapDetailAreaTabControl()
        {
            Height = HEIGHT;

            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Colour = Color4.White.Opacity(0.2f),
                },
                tabs = new OsuTabControl<BeatmapDetailTab>
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                },
                modsCheckbox = new OsuTabControlCheckbox
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Text = @"Mods",
                },
            };

            tabs.Current.ValueChanged += item => invokeOnFilter();
            modsCheckbox.Current.ValueChanged += item => invokeOnFilter();
        }
    }

    public enum BeatmapDetailTab
    {
        Details,
        Local,
        Country,
        Global,
        Friends
    }
}
