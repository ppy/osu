using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.Mania.UI
{
    /// <summary>
    /// controls that from up to down
    /// </summary>
    internal class ManiaColumnGroup : ScrollingPlayfield
    {
        public const float HIT_TARGET_POSITION = 50;

        private SpecialColumnPosition specialColumnPosition;

        /// <summary>
        /// The style to use for the special column.
        /// </summary>
        public SpecialColumnPosition SpecialColumnPosition
        {
            get { return specialColumnPosition; }
            set
            {
                if (IsLoaded)
                    throw new InvalidOperationException($"Setting {nameof(SpecialColumnPosition)} after the playfield is loaded requires re-creating the playfield.");
                specialColumnPosition = value;
            }
        }

        private readonly FillFlowContainer<Column> columns;
        public IEnumerable<Column> Columns => columns.Children;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        private readonly Container<DrawableManiaJudgement> judgements;  
        public Container<DrawableManiaJudgement> Judgements => judgements;

        private readonly Container topLevelContainer;
        public Container TopLevelContainer => topLevelContainer;

        private List<Color4> normalColumnColours = new List<Color4>();
        private Color4 specialColumnColour;

        public int ColumnCount { get; protected set; }

        public ManiaColumnGroup(int columnCount) : base(Axes.Y)
        {
            ColumnCount = columnCount;
            Name = "Playfield elements";
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            //RelativeSizeAxes = Axes.Y;
            //AutoSizeAxes = Axes.X;
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "Columns mask",
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Name = "Background",
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(0,0,0,0.8f)
                                },
                                columns = new FillFlowContainer<Column>
                                {
                                    Name = "Columns",
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Direction = FillDirection.Horizontal,
                                    Padding = new MarginPadding { Left = 1, Right = 1 },
                                    Spacing = new Vector2(1, 0)
                                },
                            }
                        },
                        new Container
                        {
                            Name = "Barlines mask",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Y,
                            Width = 1366, // Bar lines should only be masked on the vertical axis
                            BypassAutoSizeAxes = Axes.Both,
                            Masking = true,
                            Child = content = new Container
                            {
                                Name = "Bar lines",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Y,
                                Padding = new MarginPadding { Top = HIT_TARGET_POSITION }
                            }
                        },
                        judgements = new Container<DrawableManiaJudgement>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Y = HIT_TARGET_POSITION + 150,
                            BypassAutoSizeAxes = Axes.Both
                        },
                        topLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
                    }
                }

            };
        }

        /// <summary>
        /// Whether the column index is a special column for this playfield.
        /// </summary>
        /// <param name="column">The 0-based column index.</param>
        /// <returns>Whether the column is a special column.</returns>
        private bool isSpecialColumn(int column)
        {
            switch (SpecialColumnPosition)
            {
                default:
                case SpecialColumnPosition.Normal:
                    return ColumnCount % 2 == 1 && column == ColumnCount / 2;
                case SpecialColumnPosition.Left:
                    return column == 0;
                case SpecialColumnPosition.Right:
                    return column == ColumnCount - 1;
            }
        }

        public void AddColumn(Column c)
        {
            c.VisibleTimeRange.BindTo(VisibleTimeRange);
            topLevelContainer.Add(c.TopLevelContainer.CreateProxy());
            columns.Add(c);
        }

        public void AddJudgement(Judgement judgement)
        {
            judgements.Clear();
            judgements.Add(new DrawableManiaJudgement(judgement)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            normalColumnColours = new List<Color4>
            {
                colours.RedDark,
                colours.GreenDark
            };

            specialColumnColour = colours.BlueDark;

            // Set the special column + colour + key
            foreach (var column in Columns)
            {
                if (!column.IsSpecial)
                    continue;

                column.AccentColour = specialColumnColour;
            }

            var nonSpecialColumns = Columns.Where(c => !c.IsSpecial).ToList();

            // We'll set the colours of the non-special columns in a separate loop, because the non-special
            // column colours are mirrored across their centre and special styles mess with this
            for (int i = 0; i < Math.Ceiling(nonSpecialColumns.Count / 2f); i++)
            {
                Color4 colour = normalColumnColours[i % normalColumnColours.Count];
                nonSpecialColumns[i].AccentColour = colour;
                nonSpecialColumns[nonSpecialColumns.Count - 1 - i].AccentColour = colour;
            }
        }


        protected override void Update()
        {
            // Due to masking differences, it is not possible to get the width of the columns container automatically
            // While masking on effectively only the Y-axis, so we need to set the width of the bar line container manually
            content.Width = columns.Width;
        }
    }
}
