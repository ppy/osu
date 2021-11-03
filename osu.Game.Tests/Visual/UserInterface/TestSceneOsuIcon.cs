// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneOsuIcon : OsuTestScene
    {
        public TestSceneOsuIcon()
        {
            FillFlowContainer<Icon> flow;

            AddRange(new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Teal,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = flow = new FillFlowContainer<Icon>
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                    },
                }
            });

            foreach (var p in typeof(OsuIcon).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                object propValue = p.GetValue(null);
                Debug.Assert(propValue != null);

                flow.Add(new Icon($"{nameof(OsuIcon)}.{p.Name}", (IconUsage)propValue));
            }

            AddStep("toggle shadows", () => flow.Children.ForEach(i => i.SpriteIcon.Shadow = !i.SpriteIcon.Shadow));
            AddStep("change icons", () => flow.Children.ForEach(i => i.SpriteIcon.Icon = new IconUsage((char)(i.SpriteIcon.Icon.Icon + 1))));
        }

        private class Icon : Container, IHasTooltip
        {
            public LocalisableString TooltipText { get; }

            public SpriteIcon SpriteIcon { get; }

            public Icon(string name, IconUsage icon)
            {
                TooltipText = name;

                AutoSizeAxes = Axes.Both;
                Child = SpriteIcon = new SpriteIcon
                {
                    Icon = icon,
                    Size = new Vector2(60),
                };
            }
        }
    }
}
