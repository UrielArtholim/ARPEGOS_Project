
namespace ARPEGOS.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using SkiaSharp;

    /// <summary>
    /// Common Extension Methods for the App
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// [Extension Method] Appends the <see cref="IEnumerable{T}"/> collection to the source collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            items.ToList().ForEach(collection.Add);
        }

        /// <summary>
        /// Gets the Hex value string of a color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GetHexString(this Xamarin.Forms.Color color)
        {
            var red = (int)(color.R * 255);
            var green = (int)(color.G * 255);
            var blue = (int)(color.B * 255);
            var alpha = (int)(color.A * 255);
            var hex = $"#{alpha:X2}{red:X2}{green:X2}{blue:X2}";

            return hex;
        }

        public static T Convert<T, X>(this X result)
        {
            var derivedClassInstance = Activator.CreateInstance<T>();
            var derivedType = derivedClassInstance.GetType();

            var properties = result.GetType().GetProperties();
            foreach (var property in properties)
            {
                var propToSet = derivedType.GetProperty(property.Name);
                if (propToSet?.SetMethod != null)
                {
                    propToSet.SetValue(derivedClassInstance, property.GetValue(result));
                }
            }
            return derivedClassInstance;
        }

        public static IEnumerable<T> Convert<T, X>(this IEnumerable<X> listResult)
        {
            var derivedList = new List<T>();
            foreach (var r in listResult)
            {
                derivedList.Add(Convert<T, X>(r));
            }
            return derivedList;
        }

        public static List<T> Convert<T, X>(this List<X> listResult)
        {
            var derivedList = new List<T>();
            foreach (var r in listResult)
            {
                derivedList.Add(Convert<T, X>(r));
            }
            return derivedList;
        }

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var retVal = 0;
            foreach (var item in items) {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }

        public static object GetItem(this IEnumerable e, int index)
        {
            IEnumerator enumerator = e.GetEnumerator();
            int num = 0;
            while (enumerator.MoveNext())
            {
                if (num == index)
                    return enumerator.Current;
                ++num;
            }
            return (object) null;
        }

        public static int GetCount(this IEnumerable e)
        {
            IEnumerator enumerator = e.GetEnumerator();
            int num = 0;
            while (enumerator.MoveNext())
                ++num;
            return num;
        }

        public static List<object> GetList(this IEnumerable e)
        {
            IEnumerator enumerator = e.GetEnumerator();
            List<object> objectList = new List<object>();
            while (enumerator.MoveNext())
                objectList.Add(enumerator.Current);
            return objectList;
        }

        public static void DrawCaptionLabels(this SKCanvas canvas, string label, SKColor labelColor, string value, SKColor valueColor, float textSize, SKPoint point, SKTextAlign horizontalAlignment)
        {
            var hasLabel = !string.IsNullOrEmpty(label);
            var hasValueLabel = !string.IsNullOrEmpty(value);

            if (hasLabel || hasValueLabel)
            {
                var hasOffset = hasLabel && hasValueLabel;
                var captionMargin = textSize * 0.60f;
                var space = hasOffset ? captionMargin : 0;

                if (hasLabel)
                {
                    using (var paint = new SKPaint
                    {
                        TextSize = textSize,
                        IsAntialias = true,
                        Color = labelColor,
                        IsStroke = false,
                        TextAlign = horizontalAlignment,
                    })
                    {
                        var bounds = new SKRect();
                        var text = label;
                        paint.MeasureText(text, ref bounds);

                        var y = point.Y - ((bounds.Top + bounds.Bottom) / 2) - space;

                        canvas.DrawText(text, point.X, y, paint);
                    }
                }

                if (hasValueLabel)
                {
                    using (var paint = new SKPaint
                    {
                        TextSize = textSize,
                        IsAntialias = true,
                        FakeBoldText = true,
                        Color = valueColor,
                        IsStroke = false,
                        TextAlign = horizontalAlignment,
                    })
                    {
                        var bounds = new SKRect();
                        var text = value;
                        paint.MeasureText(text, ref bounds);

                        var y = point.Y - ((bounds.Top + bounds.Bottom) / 2) + space;

                        canvas.DrawText(text, point.X, y, paint);
                    }
                }
            }
        }

        public enum PointMode
        {
            None = 0,
            Circle = 1,
            Square = 2
        }

        /// <summary>
        /// Draws the given point.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="point">The point.</param>
        /// <param name="color">The fill color.</param>
        /// <param name="size">The point size.</param>
        /// <param name="mode">The point mode.</param>
        public static void DrawPoint(this SKCanvas canvas, SKPoint point, SKColor color, float size, PointMode mode)
        {
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = color,
            })
            {
                switch (mode)
                {
                    case PointMode.Square:
                        canvas.DrawRect(SKRect.Create(point.X - (size / 2), point.Y - (size / 2), size, size), paint);
                        break;

                    case PointMode.Circle:
                        paint.IsAntialias = true;
                        canvas.DrawCircle(point.X, point.Y, size / 2, paint);
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Draws a line with a gradient stroke.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="startPoint">The starting point.</param>
        /// <param name="startColor">The starting color.</param>
        /// <param name="endPoint">The end point.</param>
        /// <param name="endColor">The end color.</param>
        /// <param name="size">The stroke size.</param>
        public static void DrawGradientLine(this SKCanvas canvas, SKPoint startPoint, SKColor startColor, SKPoint endPoint, SKColor endColor, float size)
        {
            using (var shader = SKShader.CreateLinearGradient(startPoint, endPoint, new[] { startColor, endColor }, null, SKShaderTileMode.Clamp))
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = size,
                    Shader = shader,
                    IsAntialias = true,
                })
                {
                    canvas.DrawLine(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, paint);
                }
            }
        }
    }
}
