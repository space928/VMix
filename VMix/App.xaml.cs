using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VMix
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void SetTheme(string appTheme)
        {
            this.Resources.MergedDictionaries[0].Source = new Uri($"/Themes/{appTheme}.xaml", UriKind.Relative);
        }
    }
}
