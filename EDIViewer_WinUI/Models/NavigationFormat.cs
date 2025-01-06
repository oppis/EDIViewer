using System;
using System.Collections.ObjectModel;

using Microsoft.UI.Xaml.Controls;

namespace EDIViewer_WinUI.Models
{
    public class NavigationFormat
    {
        public String Name { get; set; }
        public IconElement Icon { get; set; }
        public String ToolTip { get; set; }
        public String Tag { get; set; }
        public ObservableCollection<NavigationFormat> Children { get; set; }
    }
}