
namespace ARPEGOS.Converter
{
    using ARPEGOS.Helpers;
    using ARPEGOS.Services;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
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
                var imagePath = FindImagePathByName(name, imageSize, length, size);
                return imagePath;
            }

            return null;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        public string FindImagePathByName(string name, int imageSize, int length, double[] size)
        {
            var imagePath = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                var folder = FileService.GetGameBasePath(name);
                if (Directory.Exists(folder))
                {
                    var files = Directory.GetFiles(folder).ToList();
                    if (files.Count == 0)
                    {
                        var ontologyFolder = Directory.GetDirectories(folder).Where(d => d.ToLowerInvariant().Contains("gamefiles")).Single();
                        folder = Path.Combine(folder, ontologyFolder);
                        files = Directory.GetFiles(folder).ToList();
                        if (files.Count > 0)
                        {
                            var coincidentFiles = files.Where(f => f.EndsWith(".jpg") && f.Contains(name));
                            if (coincidentFiles.Count() > 0)
                                imagePath = coincidentFiles.Single();
                        }
                    }
                    else
                        imagePath = files.Where(f => f.EndsWith(".jpg") && f.Contains(name)).Single();
                }
                else
                {
                    var searchPattern = $"{name}.jpg";
                    var di = new DirectoryInfo(FileService.GetBaseFolder());
                    var directories = di.GetDirectories();
                    foreach (var dir in directories)
                    {
                        var gameDir = dir.GetDirectories("gamefiles").Single();
                        var gameDirFiles = gameDir.GetFiles();
                        if (gameDirFiles.Count() > 0)
                        {
                            var coincidences = gameDirFiles.Where(f => string.Equals(searchPattern, f.Name));
                            if (coincidences.Count() == 1)
                                imagePath = coincidences.Single().FullName;
                        }
                    }
                }
            }
            else
                imagePath = $"https://ui-avatars.com/api/?background=4899de&color=fff&name=crear&size={imageSize}&rounded=true&length=5&uppercase=false&font-size={size[5 - 2]}";

            if(string.IsNullOrEmpty(imagePath))
                imagePath = $"https://ui-avatars.com/api/?background=424953&color=eee&name={name}&size={imageSize}&rounded=true&length={length}&uppercase=false&font-size={size[length - 2]}";

            return imagePath;
        }
    }
}
