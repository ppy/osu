// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Audio;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using Humanizer;
using osu.Game.Rulesets.Edit;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class HitSoundTrackTable : CompositeDrawable
    {

        private const int bank_column_width = 155;
        private const int header_height = 25;
        private const int row_height = 30;

        [Resolved]
        private OverlayColourProvider colours { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            RelativeSizeAxes = Axes.Both;

            if (composer == null)
                return;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Background4,
                    RelativeSizeAxes = Axes.Both,
                },
                new Box
                {
                    Colour = colours.Background3,
                    RelativeSizeAxes = Axes.Y,
                    Width = bank_column_width,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = header_height,
                    Padding = new MarginPadding { Horizontal = 10 },
                    Children = new[]
                    {
                        new TableHeaderText("Sound bank")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new TableHeaderText("Attributes")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = bank_column_width }
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = header_height },
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Spacing = new Vector2(1f),
                        Children = new[]
                        {
                            createBankHeader("Samples"),
                            createHitSoundTrackDisplay(HitSampleInfo.AllAdditions.ToList(), HitSoundTrackMode.Sample),
                            createBankHeader("Normal bank"),
                            createHitSoundTrackDisplay(HitSampleInfo.AllBanks.ToList(), HitSoundTrackMode.NormalBank),
                            createBankHeader("Addition bank"),
                            createHitSoundTrackDisplay(HitSampleInfo.AllBanks.ToList(), HitSoundTrackMode.AdditionBank),
                        }
                    }
                },
            };
        }

        private Drawable createHitSoundTrackDisplay(List<string> samples, HitSoundTrackMode mode)
        {
            return new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new[] {
                    new HitSoundTrackTimeline(
                        new EditorSkinProvidingContainer(editorBeatmap).WithChild(new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = new SoundTrackObjectsDisplay(mode)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                        })
                    ),
                    new Box
                    {
                        Width = bank_column_width,
                        RelativeSizeAxes = Axes.Y,
                        Colour = colours.Background4
                    },
                    createHitSoundTracksLabel(samples),
                },
            };
        }

        private Drawable createBankHeader(string title)
        {
            return new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes  = Axes.X,
                        Margin = new MarginPadding {Left = bank_column_width},
                        Anchor = Anchor.CentreLeft,
                        Height = 2,
                        Colour = colours.Background2,
                    },
                    new OsuSpriteText
                    {
                        Margin = new MarginPadding {Left = 10, Vertical = header_height / 2},
                        Text = title,
                    }
                }
            };
        }

        private Drawable createHitSoundTracksLabel(List<string> samples)
        {
            return new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                Width = bank_column_width,
                Spacing = new Vector2(2f),
                Children = samples.ConvertAll<Drawable>(sample =>
                {
                    return new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = row_height,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Y,
                                Colour = colours.Background2,
                                Width = bank_column_width,
                            },
                            new OsuSpriteText
                            {
                                Margin = new MarginPadding { Left = 10 },
                                Origin = Anchor.CentreLeft,
                                Anchor =  Anchor.CentreLeft,
                                Text = sample.Replace("hit", string.Empty).Titleize(),
                            }
                        }
                    };
                }),
            };
        }
    }
}
