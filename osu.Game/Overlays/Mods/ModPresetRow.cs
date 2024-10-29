// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public partial class ModPresetRow : FillFlowContainer
    {
        public ModPresetRow(Mod mod, HashSet<Mod>? rootSet = null, ModRowMode mode = ModRowMode.None)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(4);
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(7),
                    Children = new Drawable[]
                    {
                        rootSet != null && mode != ModRowMode.None
                            ? new IconButton
                            {
                                Icon = mode == ModRowMode.Remove ? FontAwesome.Solid.TimesCircle : FontAwesome.Solid.PlusCircle,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                TooltipText = mode == ModRowMode.Remove ? ModSelectOverlayStrings.RemoveThisMod : ModSelectOverlayStrings.AddThisMod,
                                Action = mode == ModRowMode.Remove
                                    ? () => Scheduler.AddOnce(() =>
                                    {
                                        if (rootSet.Count <= 1)
                                            this.FlashColour(Color4.Red, 300);
                                        else
                                        {
                                            this.FadeOut(200, Easing.OutQuint).Then().Expire();
                                            rootSet.Remove(mod);
                                        }
                                    })
                                    : () => Scheduler.AddOnce(() => rootSet.Add(mod))
                            }
                            : new Container(),
                        new ModSwitchTiny(mod)
                        {
                            Active = { Value = true },
                            Scale = new Vector2(0.6f),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft
                        },
                        new OsuSpriteText
                        {
                            Text = mod.Name,
                            Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Bottom = 2 }
                        }
                    }
                }
            };

            if (mode == ModRowMode.None)
            {
                if (!string.IsNullOrEmpty(mod.SettingDescription))
                {
                    AddInternal(new OsuTextFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Left = 14 },
                        Text = mod.SettingDescription
                    });
                }
            }
            else
            {
                var settings = mod.CreateSettingsControls().ToList();

                if (settings.Count > 0)
                    AddRange(settings);
            }
        }
    }

    public enum ModRowMode
    {
        None,
        Remove,
        Add
    }
}
