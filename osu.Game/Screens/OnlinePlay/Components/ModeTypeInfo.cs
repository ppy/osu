﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Drawables;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class ModeTypeInfo : OnlinePlayComposite
    {
        private const float height = 28;
        private const float transition_duration = 100;

        private Container drawableRuleset;

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
                    drawableRuleset = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                    },
                    gameTypeContainer = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                    },
                },
            };

            Type.BindValueChanged(type => gameTypeContainer.Child = new DrawableGameType(type.NewValue) { Size = new Vector2(height) }, true);

            Playlist.CollectionChanged += (_, __) => updateBeatmap();

            updateBeatmap();
        }

        private void updateBeatmap()
        {
            var item = Playlist.FirstOrDefault();

            if (item?.Beatmap != null)
            {
                drawableRuleset.FadeIn(transition_duration);
                drawableRuleset.Child = new DifficultyIcon(item.Beatmap.Value, item.Ruleset.Value, item.RequiredMods) { Size = new Vector2(height) };
            }
            else
                drawableRuleset.FadeOut(transition_duration);
        }
    }
}
