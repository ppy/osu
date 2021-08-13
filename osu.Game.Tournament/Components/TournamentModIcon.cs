// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets;
using osu.Game.Rulesets.UI;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Components
{
    /// <summary>
    /// Mod icon displayed in tournament usages, allowing user overridden graphics.
    /// </summary>
    public class TournamentModIcon : CompositeDrawable
    {
        private readonly string modAcronym;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        public TournamentModIcon(string modAcronym)
        {
            this.modAcronym = modAcronym;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, LadderInfo ladderInfo)
        {
            var customTexture = textures.Get($"mods/{modAcronym}");

            if (customTexture != null)
            {
                AddInternal(new Sprite
                {
                    FillMode = FillMode.Fit,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Texture = customTexture
                });

                return;
            }

            var ruleset = rulesets.GetRuleset(ladderInfo.Ruleset.Value?.ID ?? 0);
            var modIcon = ruleset?.CreateInstance().GetAllMods().FirstOrDefault(mod => mod.Acronym == modAcronym);

            if (modIcon == null)
                return;

            AddInternal(new ModIcon(modIcon, false)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(0.5f)
            });
        }
    }
}
