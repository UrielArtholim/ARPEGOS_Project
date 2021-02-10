
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
                var imageSize = parameter != null && int.TryParse(parameter.ToString(), out var b) ? b : 128;
                var length = Math.Max(Math.Min(name.Split(' ').Length, 5), 2);
                var size = new[] { 0.5, 0.4, 0.35, 0.3 };
                return name != string.Empty
                           ? $"https://ui-avatars.com/api/?background=424953&color=eee&name={name}&size={imageSize}&rounded=true&length={length}&uppercase=false&font-size={size[length - 2]}" : string.Empty;
                           //: $"https://ui-avatars.com/api/?background=4899de&color=fff&name=crear&size={imageSize}&rounded=true&length=5&uppercase=false&font-size={size[5 - 2]}";
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
