//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.GameModes.Backgrounds;
using osu.Framework;
using osu.Game.Database;
using osu.Framework.Graphics.Primitives;
using OpenTK;
using System.Linq;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using OpenTK.Graphics;

namespace osu.Game.GameModes.Play
{
    class BeatmapGroup : AutoSizeContainer
    {
        private const float collapsedAlpha = 0.75f;

        public event Action<BeatmapSet> SetSelected;
        public event Action<BeatmapSet, Beatmap> BeatmapSelected;
        public BeatmapSet BeatmapSet;
        private FlowContainer topContainer;
        private FlowContainer difficulties;
        private bool collapsed;
        public bool Collapsed
        {
            get { return collapsed; }
            set
            {
                if (collapsed == value)
                    return;
                collapsed = value;
                this.ClearTransformations();
                const float uncollapsedAlpha = 1;
                Transforms.Add(new TransformAlpha(Clock)
                {
                    StartValue = collapsed ? uncollapsedAlpha : collapsedAlpha,
                    EndValue = collapsed ? collapsedAlpha : uncollapsedAlpha,
                    StartTime = Time,
                    EndTime = Time + 250,
                });
                if (collapsed)
                    topContainer.Remove(difficulties);
                else
                    topContainer.Add(difficulties);
            }
        }

        public BeatmapGroup(BeatmapSet beatmapSet)
        {
            BeatmapSet = beatmapSet;
            Alpha = collapsedAlpha;
            Children = new[]
            {
                topContainer = new FlowContainer
                {
                    Direction = FlowDirection.VerticalOnly,
                    Children = new[]
                    {
                        new AutoSizeContainer
                        {
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = new Color4(0, 0, 0, 0.75f),
                                    RelativeSizeAxes = Axes.Both,
                                    Size = new Vector2(1),
                                },
                                new SpriteText { Text = this.BeatmapSet.Metadata.Title, TextSize = 25 } 
                            }
                        }
                    }
                }
            };
            difficulties = new FlowContainer // Deliberately not added to children
            {
                Margin = new MarginPadding { Top = 10 },
                Padding = new MarginPadding { Left = 50 },
                Spacing = new Vector2(0, 10),
                Direction = FlowDirection.VerticalOnly,
                Children = this.BeatmapSet.Beatmaps.Select(b => new BeatmapButton(this.BeatmapSet, b))
            };
            collapsed = true;
        }
        
        protected override bool OnClick(InputState state)
        {
            SetSelected?.Invoke(BeatmapSet);
            return true;
        }
    }
}
