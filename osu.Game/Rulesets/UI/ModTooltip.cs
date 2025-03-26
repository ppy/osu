// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.UI
{
    public partial class ModTooltip : VisibilityContainer, ITooltip<Mod>
    {
        private readonly OverlayColourProvider colourProvider;

        private OsuSpriteText nameText = null!;
        private TextFlowContainer settingsLabelsFlow = null!;
        private TextFlowContainer settingsValuesFlow = null!;

        public ModTooltip(OverlayColourProvider? colourProvider = null)
        {
            this.colourProvider = colourProvider ?? new OverlayColourProvider(OverlayColourScheme.Aquamarine);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            CornerRadius = 7;
            Masking = true;

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.2f),
                Radius = 10f,
            };

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background6,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding(10f),
                    Spacing = new Vector2(20f, 0f),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, 5f),
                            Children = new Drawable[]
                            {
                                nameText = new OsuSpriteText
                                {
                                    Font = OsuFont.Torus.With(size: 16f, weight: FontWeight.SemiBold),
                                    Colour = colourProvider.Content1,
                                    UseFullGlyphHeight = false,
                                },
                                settingsLabelsFlow = new TextFlowContainer(t =>
                                {
                                    t.Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.SemiBold);
                                })
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Colour = colourProvider.Content2,
                                },
                            },
                        },
                        settingsValuesFlow = new TextFlowContainer(t =>
                        {
                            t.Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.SemiBold);
                        })
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            AutoSizeAxes = Axes.Both,
                            Colour = colourProvider.Content1,
                            TextAnchor = Anchor.TopRight,
                        },
                    },
                }
            };
        }

        private Mod? displayedContent;

        public void SetContent(Mod content)
        {
            if (content == displayedContent)
                return;

            displayedContent = content;
            nameText.Text = content.Name;
            settingsLabelsFlow.Clear();
            settingsValuesFlow.Clear();

            if (content.SettingDescription.Any())
            {
                settingsLabelsFlow.Show();
                settingsValuesFlow.Show();

                foreach (var part in content.SettingDescription)
                {
                    settingsLabelsFlow.AddText(part.setting);
                    settingsLabelsFlow.NewLine();

                    settingsValuesFlow.AddText(part.value);
                    settingsValuesFlow.NewLine();
                }
            }
            else
            {
                settingsLabelsFlow.Hide();
                settingsValuesFlow.Hide();
            }
        }

        protected override void PopIn() => this.FadeIn(300, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(300, Easing.OutQuint);
        public void Move(Vector2 pos) => Position = pos;
    }
}
