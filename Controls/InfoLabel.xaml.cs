using System.Windows;
using System.Windows.Controls;

namespace DnDToolkit.Controls
{
    public partial class InfoLabel : UserControl
    {
        public InfoLabel()
        {
            InitializeComponent();
        }

        // 1. Label 属性：显示的短标题 (例如 "法术攻击加值")
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(InfoLabel), new PropertyMetadata(""));

        // 2. Title 属性：提示框里的加粗标题 (例如 "Spell Attack Bonus")
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(InfoLabel), new PropertyMetadata(""));

        // 3. Description 属性：提示框里的详细说明
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(InfoLabel), new PropertyMetadata(""));
    }
}