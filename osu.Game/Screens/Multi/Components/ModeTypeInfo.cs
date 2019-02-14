// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.Multi.Components
{
    public class ModeTypeInfo : MultiplayerComposite
    {
        private const float height = 30;
        private const float transition_duration = 100;

        private Container rulesetContainer;

        public ModeTypeInfo()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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

            CurrentItem.BindValueChanged(updateBeatmap, true);

            Type.BindValueChanged(v => gameTypeContainer.Child = new DrawableGameType(v) { Size = new Vector2(height) }, true);
        }

        private void updateBeatmap(PlaylistItem item)
        {
            if (item?.Beatmap != null)
            {
                rulesetContainer.FadeIn(transition_duration);
                rulesetContainer.Child = new DifficultyIcon(item.Beatmap, item.Ruleset) { Size = new Vector2(height) };
            }
            else
                rulesetContainer.FadeOut(transition_duration);
        }
    }
}
