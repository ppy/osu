// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinEditorComponentsList : SkinnableTestScene
    {
        [Test]
        public void TestToggleEditor()
        {
            AddStep("show available components", () =>
            {
                SetContents(() =>
                {
                    FillFlowContainer fill;

                    var scroll = new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = fill = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Width = 0.5f,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(20)
                        }
                    };

                    var skinnableTypes = typeof(OsuGame).Assembly.GetTypes().Where(t => typeof(ISkinnableComponent).IsAssignableFrom(t)).ToArray();

                    foreach (var type in skinnableTypes)
                    {
                        try
                        {
                            fill.Add(new OsuSpriteText { Text = type.Name });

                            var instance = (Drawable)Activator.CreateInstance(type);

                            Debug.Assert(instance != null);

                            instance.Anchor = Anchor.TopCentre;
                            instance.Origin = Anchor.TopCentre;

                            var container = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 100,
                                Children = new[]
                                {
                                    instance
                                }
                            };

                            switch (instance)
                            {
                                case IScoreCounter score:
                                    score.Current.Value = 133773;
                                    break;

                                case IComboCounter combo:
                                    combo.Current.Value = 727;
                                    break;
                            }

                            fill.Add(container);
                        }
                        catch { }
                    }

                    return scroll;
                });
            });
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();
    }
}
