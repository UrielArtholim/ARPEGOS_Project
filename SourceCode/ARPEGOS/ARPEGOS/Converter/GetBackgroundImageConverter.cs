using ARPEGOS.Helpers;
using ARPEGOS.Themes;
using Autofac;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace ARPEGOS.Converter
{
    class GetBackgroundImageConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ResourceDictionary currentDictionary;
            ImageSource themeImage = null;
            if (value is string theme)
            {
                currentDictionary = theme switch
                {
                    "Noche" => DependencyHelper.Container.Resolve<DarkTheme>(),
                    "Bosque" => DependencyHelper.Container.Resolve<ForestTheme>(),
                    "Desierto" => DependencyHelper.Container.Resolve<DesertTheme>(),
                    "Tundra" => DependencyHelper.Container.Resolve<TundraTheme>(),
                    "Valle" => DependencyHelper.Container.Resolve<ValleyTheme>(),
                    "Oceano" => DependencyHelper.Container.Resolve<OceanTheme>(),
                    _ => DependencyHelper.Container.Resolve<LightTheme>(),
                };
                themeImage = currentDictionary["BackgroundImageSource"] as ImageSource;                
            }
            return themeImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
