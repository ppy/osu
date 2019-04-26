// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class BeatmapPlaylist : RearrangeableListContainer<BeatmapPlaylistItem>
    {
        protected override DrawableRearrangeableListItem CreateDrawable(BeatmapPlaylistItem item) => new DrawableBeatmapPlaylistItem(item);

        protected class DrawableBeatmapPlaylistItem : DrawableRearrangeableListItem
        {
            protected override bool IsDraggableAt(Vector2 screenSpacePos) => dragHandle.ReceivePositionalInputAt(screenSpacePos);

            private const int fade_duration = 60;
            private readonly DragHandle dragHandle;
            private readonly RemoveButton removeButton;
            private readonly Box background;
            private readonly Box gradient;
            private readonly Color4 backgroundColour = Color4.Black;
            private readonly Color4 selectedColour = new Color4(0.1f, 0.1f, 0.1f, 1f);
            private bool suppressHover;

            public DrawableBeatmapPlaylistItem(BeatmapPlaylistItem item)
                : base(item)
            {
                RelativeSizeAxes = Axes.X;
                Height = 50;

                InternalChildren = new Drawable[]
                {
                    dragHandle = new DragHandle(),
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Left = 25,
                        },
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 5,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = backgroundColour.Opacity(40),
                                Radius = 5,
                            },
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        background = new Box
                                        {
                                            Colour = backgroundColour,
                                            RelativeSizeAxes = Axes.Both,
                                            Width = 0.5f,
                                        },
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Width = 0.5f,
                                            Children = new Drawable[]
                                            {
                                                new UpdateableBeatmapBackgroundSprite
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Beatmap = { Value = Model.PlaylistItem.Beatmap }
                                                },
                                                gradient = new Box
                                                {
                                                    Colour = ColourInfo.GradientHorizontal(backgroundColour, backgroundColour.Opacity(0.5f)),
                                                    RelativeSizeAxes = Axes.Both,
                                                },
                                            },
                                        },
                                    }
                                },
                                new BeatmapMetadataDisplay(Model.PlaylistItem)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Padding = new MarginPadding(7),
                                },
                                new ModDisplay
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Position = new Vector2(-30, 0),
                                    DisplayUnrankedText = false,
                                    Scale = new Vector2(0.6f),
                                    HoverEffect = false,
                                    Current = { Value = Model.PlaylistItem.RequiredMods }
                                }
                            },
                        }
                    },
                    removeButton = new RemoveButton
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.Centre,
                        Position = new Vector2(-20, 0),
                        Action = OnRequestRemoval
                    },
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                // If OnHover is triggered during a drag (that isn't of the current item), suppress showing the hover elements
                if (e.IsPressed(MouseButton.Left))
                {
                    if (IsBeingDragged)
                        suppressHover = true;

                    return true;
                }

                suppressHover = true;

                showHoverElements(true);
                setHighlighted(true);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                suppressHover = false;

                // If OnHoverLost is triggered while the current item is being dragged or while the remove button is being depressed, suppress hiding the hover elements
                if (IsBeingDragged || removeButton.Depressed)
                    return;

                showHoverElements(false);
                setHighlighted(false);
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                // Reshow the hover elements if their display was previously being suppressed due to (i.e. a drag ended on the current item)
                if (!suppressHover && !e.IsPressed(MouseButton.Left))
                    showHoverElements(true);

                return base.OnMouseMove(e);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (!e.IsPressed(MouseButton.Left))
                    return false;

                if (dragHandle.IsHovered)
                    removeButton.Hide();

                return base.OnMouseDown(e);
            }

            protected override bool OnMouseUp(MouseUpEvent e)
            {
                if (!suppressHover)
                {
                    showHoverElements(false);
                    setHighlighted(false);
                }
                else
                {
                    removeButton.Show();
                }

                return base.OnMouseUp(e);
            }

            private void showHoverElements(bool show)
            {
                if (show)
                {
                    removeButton.Show();
                    dragHandle.Show();
                }
                else
                {
                    removeButton.Hide();
                    dragHandle.Hide();
                }
            }

            private void setHighlighted(bool selected)
            {
                var colour = selected ? selectedColour : backgroundColour;
                background.FadeColour(colour);
                gradient.FadeColour(ColourInfo.GradientHorizontal(colour, colour.Opacity(0.5f)));
            }

            private class DragHandle : OsuClickableContainer
            {
                public DragHandle()
                {
                    RelativeSizeAxes = Axes.Y;
                    Width = 25;
                    Alpha = 0;
                    Child = new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(12),
                        Icon = FontAwesome.Solid.Bars,
                        Margin = new MarginPadding { Left = 5, Top = 2 }
                    };
                }

                public override bool HandlePositionalInput => IsPresent;

                public override void Show() => this.FadeIn(fade_duration);

                public override void Hide() => this.FadeOut(fade_duration);
            }

            private class RemoveButton : OsuClickableContainer
            {
                private readonly SpriteIcon icon;
                public bool Depressed;

                public RemoveButton()
                {
                    Alpha = 0;
                    Child = icon = new SpriteIcon
                    {
                        Colour = Color4.White,
                        Icon = FontAwesome.Solid.MinusSquare,
                        Size = new Vector2(14),
                    };
                }

                public override void Show() => this.FadeIn(fade_duration);

                public override void Hide() => this.FadeOut(fade_duration);

                protected override bool OnMouseDown(MouseDownEvent e)
                {
                    icon.ScaleTo(0.75f, 2000, Easing.OutQuint);
                    Depressed = true;
                    return base.OnMouseDown(e);
                }

                protected override bool OnMouseUp(MouseUpEvent e)
                {
                    icon.ScaleTo(1, 1000, Easing.OutElastic);
                    Depressed = false;
                    return base.OnMouseUp(e);
                }
            }

            private class BeatmapMetadataDisplay : FillFlowContainer
            {
                public BeatmapMetadataDisplay(PlaylistItem model)
                {
                    AutoSizeAxes = Axes.Both;
                    Direction = FillDirection.Horizontal;
                    Children = new Drawable[]
                    {
                        new DifficultyIcon(model.Beatmap)
                        {
                            Scale = new Vector2(1.5f)
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding
                            {
                                Left = 10,
                            },
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Spacing = new Vector2(5),
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = new LocalisedString((model.Beatmap?.BeatmapSet?.Metadata.ArtistUnicode, model.Beatmap?.BeatmapSet?.Metadata.Artist)),
                                            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold)
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "-",
                                            Font = OsuFont.GetFont()
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = new LocalisedString((model.Beatmap?.Metadata.TitleUnicode, model.Beatmap?.Metadata.Title)),
                                            Font = OsuFont.GetFont()
                                        },
                                    }
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Spacing = new Vector2(10),
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = model.Beatmap?.Version,
                                            Font = OsuFont.GetFont(size: 12)
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = $"mapped by {model.Beatmap?.BeatmapSet?.Metadata.Author.Username}",
                                            Font = OsuFont.GetFont(size: 12, italics: true),
                                            Colour = Color4.Violet,
                                        }
                                    }
                                },
                            }
                        },
                    };
                }
            }
        }
    }
}
