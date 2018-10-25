using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Settings;
using OpenTK;
using OpenTK.Graphics;
using Symcol.Core.Graphics.Containers;
using Symcol.osu.Core.Containers;

namespace Symcol.osu.Mods.Caster.Pieces
{
    /// <summary>
    /// Handles selection and creation of tournement files / folders and reading from said files
    /// </summary>
    public class CasterControlPanel : SymcolContainer
    {
        public readonly Bindable<string> Cup = new Bindable<string>();
        public readonly Bindable<string> Year = new Bindable<string>();
        public readonly Bindable<string> Stage = new Bindable<string>();

        public readonly Bindable<bool> Editable = new Bindable<bool>();

        private readonly SettingsDropdown<string> cup;
        private readonly SettingsDropdown<string> year;
        private readonly SettingsDropdown<string> stage;


        private Storage storage;

        public CasterControlPanel()
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.18f, 1);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,

                    Child = new FillFlowContainer
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,

                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,

                        Children = new Drawable[]
                        {
                            new SettingsCheckbox
                            {
                                LabelText = "Enable Edit Mode",
                                Bindable = Editable
                            },
                            cup = new SettingsDropdown<string>
                            {
                                LabelText = "Selected Cup",
                                Bindable = Cup
                            },
                            year = new SettingsDropdown<string>
                            {
                                LabelText = "Selected Year",
                                Bindable = Year
                            },
                            stage = new SettingsDropdown<string>
                            {
                                LabelText = "Selected Stage",
                                Bindable = Stage
                            },
                            new SymcolSettingsTextBox((text, n) => { createCup(text.Current); })
                            {
                                LabelText = "Add New Cup"
                            },
                            new SymcolSettingsTextBox((text, n) => { createYear(text.Current, Cup.Value); })
                            {
                                LabelText = "Add New Year"
                            },
                            new SymcolSettingsTextBox((text, n) => { createStage(text.Current, Cup.Value, Year.Value); })
                            {
                                LabelText = "Add New Stage"
                            },
                            new SettingsButton
                            {
                                Text = "Manually Refresh Cups",
                                Action = refresh
                            },
                        }
                    }
                }
            };

            Cup.ValueChanged += value => 
            {
                year.Items = years(value);
                stage.Items = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("None", "None")
                };
                Year.Value = "None";
                Stage.Value = "None";
            };
            Year.ValueChanged += value =>
            {
                stage.Items = stages(Cup.Value, value);
                Stage.Value = "None";
            };
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            this.storage = storage;
            refresh();
        }

        private void refresh()
        {
            cup.Items = cups();
            year.Items = years("None");
            stage.Items = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("None", "None")
            };
            Cup.Value = "None";
        }

        #region IO

        /// <summary>
        /// Returns a Stream path from the selected Cup + Year + Stage for the filename.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string GetStreamPath(string filename)
        {
            if (Stage.Value == "None") return null;
            return $"caster\\Cups\\{Cup.Value}\\{Year.Value}\\{Stage.Value}\\{filename}";
        }

        /// <summary>
        /// Returns a Stream from the specified path
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Stream GetStream(string path, FileAccess access, FileMode mode)
        {
            return storage.GetStream(path, access, mode);
        }

        /// <summary>
        /// Returns a StreamReader from the selected Cup + Year + Stage for the filename. If none is selected return null
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public StreamReader GetStreamReader(Stream stream)
        {
            if (Stage.Value == "None") return null;
            return new StreamReader(stream);
        }

        /// <summary>
        /// Returns a StreamWriter from the selected Cup + Year + Stage for the filename. If none is selected return null
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public StreamWriter GetStreamWriter(Stream stream)
        {
            if (Stage.Value == "None") return null;
            return new StreamWriter(stream);
        }

        private void createCasterStorage()
        {
            if (!storage.ExistsDirectory("caster\\Cups"))
            {
                StreamWriter writer = new StreamWriter(storage.GetStream("caster\\Cups\\wanks.wang", FileAccess.Write, FileMode.Create));
                writer.Write("wanks");
            }

            if (storage.Exists("caster\\Cups\\wanks.wang"))
                storage.Delete("caster\\Cups\\wanks.wang");
        }

        private List<KeyValuePair<string, string>> cups()
        {
            List<KeyValuePair<string, string>> cups = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("None", "None") };

            createCasterStorage();

            foreach (string cup in storage.GetDirectories("caster\\Cups"))
            {
                string[] args = cup.Split('\\');
                cups.Add(new KeyValuePair<string, string>(args.Last(), args.Last()));
            }

            return cups;
        }

        private List<KeyValuePair<string, string>> years(string cup)
        {
            List<KeyValuePair<string, string>> years = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("None", "None") };

            if (cup == "None")
                return years;

            if (storage.Exists($"caster\\Cups\\{cup}\\wanks.wang"))
                storage.Delete($"caster\\Cups\\{cup}\\wanks.wang");

            foreach (string year in storage.GetDirectories($"caster\\Cups\\{cup}"))
            {
                string[] args = year.Split('\\');
                years.Add(new KeyValuePair<string, string>(args.Last(), args.Last()));
            }

            return years;
        }

        private List<KeyValuePair<string, string>> stages(string cup, string year)
        {
            List<KeyValuePair<string, string>> stages = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("None", "None") };

            if (year == "None")
                return stages;

            if (storage.Exists($"caster\\Cups\\{cup}\\{year}\\wanks.wang"))
                storage.Delete($"caster\\Cups\\{cup}\\{year}\\wanks.wang");

            foreach (string stage in storage.GetDirectories($"caster\\Cups\\{cup}\\{year}"))
            {
                string[] args = stage.Split('\\');
                stages.Add(new KeyValuePair<string, string>(args.Last(), args.Last()));
            }

            return stages;
        }

        private void createCup(string name)
        {
            createCasterStorage();

            if (!storage.ExistsDirectory($"caster\\Cups\\{name}"))
            {
                try
                {
                    StreamWriter writer = new StreamWriter(storage.GetStream($"caster\\Cups\\{name}\\wanks.wang", FileAccess.Write, FileMode.Create));
                    writer.Write("wanks");
                    cup.Items = cups();
                    Cup.Value = name;
                }
                catch
                {
                    Logger.Log($"Failed to create a cup with name {name}", LoggingTarget.Database, LogLevel.Error);
                }
            }
            else
                Logger.Log($"{name} already exists!", LoggingTarget.Database, LogLevel.Error);
        }

        private void createYear(string name, string cup)
        {
            if (cup == "None")
            {
                Logger.Log("No cup selected, cannot add year!", LoggingTarget.Database, LogLevel.Error);
                return;
            }

            if (!storage.ExistsDirectory($"caster\\Cups\\{cup}\\{name}"))
            {
                try
                {
                    StreamWriter writer = new StreamWriter(storage.GetStream($"caster\\Cups\\{cup}\\{name}\\wanks.wang", FileAccess.Write, FileMode.Create));
                    writer.Write("wanks");
                    year.Items = years(cup);
                    Year.Value = name;
                }
                catch
                {
                    Logger.Log($"Failed to create a year with name {name}", LoggingTarget.Database, LogLevel.Error);
                }
            }
            else
                Logger.Log(name + " already exists!", LoggingTarget.Database, LogLevel.Error);
        }

        private void createStage(string name, string cup, string year)
        {
            if (cup == "Year")
            {
                Logger.Log("No year selected, cannot add stage!", LoggingTarget.Database, LogLevel.Error);
                return;
            }

            if (!storage.ExistsDirectory($"caster\\Cups\\{cup}\\{year}\\{name}"))
            {
                try
                {
                    StreamWriter writer = new StreamWriter(storage.GetStream($"caster\\Cups\\{cup}\\{year}\\{name}\\wanks.wang", FileAccess.Write, FileMode.Create));
                    writer.Write("wanks");
                    stage.Items = stages(cup, year);
                    Stage.Value = name;
                }
                catch
                {
                    Logger.Log($"Failed to create a stage with name {name}", LoggingTarget.Database, LogLevel.Error);
                }
            }
            else
                Logger.Log(name + " already exists!", LoggingTarget.Database, LogLevel.Error);
        }

        #endregion
    }
}
