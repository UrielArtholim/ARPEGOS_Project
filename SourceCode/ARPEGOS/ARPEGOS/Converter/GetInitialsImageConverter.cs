
namespace ARPEGOS.Converter
{
    using System;
    using System.Globalization;

    using Xamarin.Forms;

    /// <inheritdoc />
    public class GetInitialsImageConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string name)
            {
                var length = Math.Max(Math.Min(name.Split(' ').Length, 5), 2);
                var size = new[] { 0.5, 0.4, 0.35, 0.3 };
                return name != string.Empty
                           ? $"https://ui-avatars.com/api/?background=424953&color=eee&name={name}&size=128&rounded=true&length={length}&uppercase=false&font-size={size[length - 2]}"
                           : "https://ui-avatars.com/api/?background=4899de&color=fff&name=crear&size=128&rounded=true&length=5&uppercase=false&font-size=0.30";
            }

            return null;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
