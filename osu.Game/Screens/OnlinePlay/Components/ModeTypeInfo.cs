// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public class ModeTypeInfo : OnlinePlayComposite
    {
        private const float height = 28;
        private const float transition_duration = 100;

        [Resolved]
        private RulesetStore rulesets { get; set; }

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
            var ruleset = item == null ? null : rulesets.GetRuleset(item.RulesetID)?.CreateInstance();

            if (item?.Beatmap != null && ruleset != null)
            {
                var mods = item.RequiredMods.Select(m => m.ToMod(ruleset)).ToArray();

                drawableRuleset.FadeIn(transition_duration);
                drawableRuleset.Child = new DifficultyIcon(item.Beatmap, ruleset.RulesetInfo, mods) { Size = new Vector2(height) };
            }
            else
                drawableRuleset.FadeOut(transition_duration);
        }
    }
}
