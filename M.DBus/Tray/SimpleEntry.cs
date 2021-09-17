using System;
using System.Collections.Generic;
using M.DBus.Utils.Canonical.DBusMenuFlags;

namespace M.DBus.Tray
{
    /// <summary>
    /// 翻译自
    /// https://github.com/gnustep/libs-dbuskit/blob/master/Bundles/DBusMenu/com.canonical.dbusmenu.xml#L39
    /// </summary>
    public class SimpleEntry
    {
        public int ChildId { get; internal set; } = -2;

        public Action OnPropertyChanged;

        public void TriggerPropertyChangedEvent() => OnPropertyChanged?.Invoke();

        /// <summary>
        /// 该项目的类型<br/>
        /// <![CDATA[供应商特定类型可以通过添加前缀“x-<vendor>-”来添加。]]>
        /// </summary>
        public string Type
        {
            get => type;
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private string type = EntryType.SStandard;

        /// <summary>
        /// 该项目的文本，但是：<br/>
        /// -# 两个连续的下划线字符“__”显示为单下划线，<br/>
        /// -# 任何剩余的下划线字符不会显示<br/>
        /// -# 剩下的第一个下划线字符（除非它是字符串中的最后一个字符）表示以下字符是访问键。
        /// </summary>
        public string Label
        {
            get => label;
            set
            {
                if (label != value)
                {
                    label = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private string label = string.Empty;

        /// <summary>
        /// 该项目是否可以被激活。
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private bool enabled = true;

        /// <summary>
        /// 如果该项目在菜单中可见，则为True。
        /// </summary>
        public bool Visible
        {
            get => visible;
            set
            {
                if (visible != value)
                {
                    visible = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private bool visible = true;

        /// <summary>
        /// 项目的图标名称，遵循 freedesktop.org 图标规范。
        /// </summary>
        public string IconName
        {
            get => iconName;
            set
            {
                if (iconName != value)
                {
                    iconName = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private string iconName = string.Empty;

        /// <summary>
        /// 图标的 PNG 数据。
        /// </summary>
        public byte[] IconData
        {
            get => iconData;
            set
            {
                if (iconData != value)
                {
                    iconData = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private byte[] iconData = EmptyPngBytes;

        public static byte[] EmptyPngBytes =
        {
            //空的1x1 PNG格式图片
            //使用GIMP创建，导出选项全关
            137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0,
            13, 73, 72, 68, 82,
            0, 0, 0, 1,
            0, 0, 0, 1,
            8, 6, 0, 0, 0,
            31, 21, 196, 137,
            0, 0, 0, 11, 73, 68, 65, 84, 8, 215, 99, 96, 0,
            2, 0, 0, 5, 0, 1, 226, 38, 5, 155,
            0, 0, 0, 0, 73, 69, 78, 68, 174, 66, 96, 130
        };

        /// <summary>
        /// 项目的快捷方式。<br/>
        /// 每个数组代表按键列表中的按键。<br/>
        /// 每个字符串列表都包含一个修饰符列表，然后是使用的键。<br/>
        /// 允许的修饰符字符串是：“Control”、“Alt”、“Shift”和“Super”。<br/>
        ///<br/>
        /// - 像 Ctrl+S 这样的简单快捷方式表示为：<br/>
        /// [[“Ctrl”，“S”]]<br/>
        /// - 像 Ctrl+Q、Alt+X 这样的复杂快捷键表示为：<br/>
        /// [["Control", "Q"], ["Alt", "X"]]<br/><br/>
        /// 你可能需要手动执行TriggerPropertyChangedEvent来通知此属性的变更
        /// <br/>
        /// </summary>
        public IList<string[]> Shortcuts
        {
            get => shortcuts;
            set
            {
                if (!ReferenceEquals(shortcuts, value))
                {
                    shortcuts = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private IList<string[]> shortcuts = new List<string[]>();

        /// <summary>
        /// 如果该项目可以切换，则此属性应设置为：<br/>
        /// - ToggleType.IndependentToggleable("checkmark")：项目是一个独立的可切换项目(一个选项?)<br/>
        /// - ToggleType.Radio：单选框<br/>
        /// - "": 项目不能切换
        /// </summary>
        public string ToggleType
        {
            get => toggleType;
            set
            {
                if (toggleType != value)
                {
                    toggleType = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private string toggleType = Utils.Canonical.DBusMenuFlags.ToggleType.SIndependentToggleable;

        /// <summary>
        /// 描述“可切换”项目的当前状态。 可以是以下之一：<br/>
        /// - 0 = 关闭<br/>
        /// - 1 = 开<br/>
        /// - 其他任意值 = 不确定
        /// </summary>
        /// <remarks>
        /// 实现本身并不确保无线电组中只有一个项目设置为“on”，或者一个组没有同时具有“on”和“indeterminate”项目；<br/>
        /// 维护此策略取决于工具包包装器。
        /// </remarks>
        public int ToggleState
        {
            get => toggleState;
            set
            {
                if (toggleState != value)
                {
                    toggleState = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private int toggleState;

        /// <summary>
        /// 如果菜单项有子项，则此属性应设置为 ChildrenDisplayType.Submenu ("submenu")
        /// </summary>
        public string ChildrenDisplay
        {
            get => childrenDisplay;
            set
            {
                if (childrenDisplay != value)
                {
                    childrenDisplay = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private string childrenDisplay = ChildrenDisplayType.SNone;

        /// <summary>
        /// 该项目被激活时要执行的动作
        /// </summary>
        public Action OnActive { get; set; }

        /// <summary>
        /// 该菜单的子项列表<br/>
        /// 你可能需要手动执行TriggerPropertyChangedEvent来通知此属性的变更
        /// </summary>
        public IList<SimpleEntry> Children
        {
            get => children;
            set
            {
                if (!ReferenceEquals(children, value))
                {
                    children = value;
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private IList<SimpleEntry> children = new List<SimpleEntry>();

        public override string ToString() => $"({ChildId}) DBusMenuEntry '{Label.Replace("\n", "\\n")}'";
    }
}
