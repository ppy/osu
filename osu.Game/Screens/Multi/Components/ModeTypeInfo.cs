// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.Multi.Components
{
    public class ModeTypeInfo : CompositeDrawable
    {
        private const float height = 30;
        private const float transition_duration = 100;

        private readonly Container rulesetContainer;

        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();
        public readonly Bindable<GameType> Type = new Bindable<GameType>();

        public ModeTypeInfo()
        {
            AutoSizeAxes = Axes.Both;

            Container gameTypeContainer;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5f, 0f),
                LayoutDuration = 100,
                Children = new[]
                {
                    rulesetContainer = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                    },
                    gameTypeContainer = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                    },
                },
            };

            Beatmap.BindValueChanged(updateBeatmap);
            Type.BindValueChanged(v => gameTypeContainer.Child = new DrawableGameType(v) { Size = new Vector2(height) });
        }

        private void updateBeatmap(BeatmapInfo beatmap)
        {
            if (beatmap != null)
            {
                rulesetContainer.FadeIn(transition_duration);
                rulesetContainer.Child = new DifficultyIcon(beatmap) { Size = new Vector2(height) };
            }
            else
                rulesetContainer.FadeOut(transition_duration);
        }
    }
}
