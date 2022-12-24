// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyStageBackground : CompositeDrawable
    {
        private Drawable leftSprite;
        private Drawable rightSprite;
        private ColumnFlow<Drawable> columnBackgrounds;

        public LegacyStageBackground()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, StageDefinition stageDefinition)
        {
            string leftImage = skin.GetManiaSkinConfig<string>(LegacyManiaSkinConfigurationLookups.LeftStageImage)?.Value
                               ?? "mania-stage-left";

            string rightImage = skin.GetManiaSkinConfig<string>(LegacyManiaSkinConfigurationLookups.RightStageImage)?.Value
                                ?? "mania-stage-right";

            InternalChildren = new[]
            {
                leftSprite = new Sprite
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopRight,
                    X = 0.05f,
                    Texture = skin.GetTexture(leftImage),
                },
                rightSprite = new Sprite
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopLeft,
                    X = -0.05f,
                    Texture = skin.GetTexture(rightImage)
                },
                columnBackgrounds = new ColumnFlow<Drawable>(stageDefinition)
                {
                    RelativeSizeAxes = Axes.Y
                },
                new HitTargetInsetContainer
                {
                    Child = new LegacyHitTarget { RelativeSizeAxes = Axes.Both }
                }
            };

            for (int i = 0; i < stageDefinition.Columns; i++)
                columnBackgrounds.SetContentForColumn(i, new ColumnBackground(i, i == stageDefinition.Columns - 1));
        }

        protected override void Update()
        {
            base.Update();

            if (leftSprite?.Height > 0)
                leftSprite.Scale = new Vector2(1, DrawHeight / leftSprite.Height);

            if (rightSprite?.Height > 0)
                rightSprite.Scale = new Vector2(1, DrawHeight / rightSprite.Height);
        }

        private partial class ColumnBackground : CompositeDrawable
        {
            private readonly int columnIndex;
            private readonly bool isLastColumn;

            public ColumnBackground(int columnIndex, bool isLastColumn)
            {
                this.columnIndex = columnIndex;
                this.isLastColumn = isLastColumn;

                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                float leftLineWidth = skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.LeftLineWidth, columnIndex)?.Value ?? 1;
                float rightLineWidth = skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.RightLineWidth, columnIndex)?.Value ?? 1;

                bool hasLeftLine = leftLineWidth > 0;
                bool hasRightLine = (rightLineWidth > 0 && skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value >= 2.4m) || isLastColumn;

                Color4 lineColour = skin.GetManiaSkinConfig<Color4>(LegacyManiaSkinConfigurationLookups.ColumnLineColour, columnIndex)?.Value ?? Color4.White;
                Color4 backgroundColour = skin.GetManiaSkinConfig<Color4>(LegacyManiaSkinConfigurationLookups.ColumnBackgroundColour, columnIndex)?.Value ?? Color4.Black;

                InternalChildren = new Drawable[]
                {
                    LegacyColourCompatibility.ApplyWithDoubledAlpha(new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    }, backgroundColour),
                    new HitTargetInsetContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                Width = leftLineWidth,
                                Scale = new Vector2(0.740f, 1),
                                Alpha = hasLeftLine ? 1 : 0,
                                Child = LegacyColourCompatibility.ApplyWithDoubledAlpha(new Box
                                {
                                    RelativeSizeAxes = Axes.Both
                                }, lineColour)
                            },
                            new Container
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                RelativeSizeAxes = Axes.Y,
                                Width = rightLineWidth,
                                Scale = new Vector2(0.740f, 1),
                                Alpha = hasRightLine ? 1 : 0,
                                Child = LegacyColourCompatibility.ApplyWithDoubledAlpha(new Box
                                {
                                    RelativeSizeAxes = Axes.Both
                                }, lineColour)
                            },
                        }
                    }
                };
            }
        }
    }
}
