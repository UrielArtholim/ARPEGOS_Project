
namespace ARPEGOS.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ARPEGOS.Helpers;

    using SkiaSharp;
    using SkiaSharp.Views.Forms;

    using Xamarin.Forms;

    class SlidingProgressStepBar  : SKCanvasView
    {
        public static readonly BindableProperty ActiveColorProperty = 
            BindableProperty.Create(nameof(ActiveColor), typeof(Color), typeof(SlidingProgressStepBar));

        public static readonly BindableProperty CurrentIndexProperty =
            BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(SlidingProgressStepBar), 0, BindingMode.TwoWay, propertyChanged: CurrentIndexPropertyChanged);

        public static readonly BindableProperty NumberOfStepsProperty =
            BindableProperty.Create(nameof(NumberOfSteps), typeof(int), typeof(SlidingProgressStepBar), 0, BindingMode.OneTime);

        public static readonly BindableProperty SecondaryColorProperty =
            BindableProperty.Create(nameof(SecondaryColor), typeof(Color), typeof(SlidingProgressStepBar));

        public static readonly BindableProperty NotificationColorProperty =
            BindableProperty.Create(nameof(NotificationColor), typeof(Color), typeof(SlidingProgressStepBar));

        public static readonly BindableProperty MinimumSeparationProperty =
            BindableProperty.Create(nameof(MinimumSeparation), typeof(float), typeof(SlidingProgressStepBar), 0f, BindingMode.OneTime);

        public static readonly BindableProperty BooleanSourceProperty =
            BindableProperty.Create(nameof(BooleanSource), typeof(IEnumerable), typeof(SlidingProgressStepBar), defaultBindingMode: BindingMode.TwoWay, propertyChanged: BooleanSourcePropertyChanged);

        private double totaldx = 0;

        protected float PointSize;

        protected float Separation;

        protected float CalculatedLineWidth;

        /// <summary>
        /// Full length calculated line
        /// </summary>
        protected SKPoint[] CalculatedPoints;

        protected SKSize CurrentSize = SKSize.Empty;

        private bool panEnabled;

        /// <summary>
        /// Current displacement in X axis 
        /// </summary>
        private double currentPandx = 0;

        private SKPoint clickPoint = SKPoint.Empty;

        public SlidingProgressStepBar()
        {
            this.Margin = new Thickness(2);
            this.EnableTouchEvents = true;
            var rotationRecognizer = new PanGestureRecognizer();
            rotationRecognizer.PanUpdated += this.PanUpdated;
            this.GestureRecognizers.Add(rotationRecognizer);
            var touch = new TapGestureRecognizer();
            touch.Tapped += this.Touch_Tapped;
            this.GestureRecognizers.Add(touch);
        }

        public int CurrentIndex
        {
            get => (int)this.GetValue(CurrentIndexProperty);
            set => this.SetValue(CurrentIndexProperty, value);
        }

        public int NumberOfSteps
        {
            get => (int)this.GetValue(NumberOfStepsProperty);
            set => this.SetValue(NumberOfStepsProperty, value);
        }

        public Color ActiveColor
        {
            get => (Color)this.GetValue(ActiveColorProperty);
            set => this.SetValue(ActiveColorProperty, value);
        }

        public Color SecondaryColor
        {
            get => (Color)this.GetValue(SecondaryColorProperty);
            set => this.SetValue(SecondaryColorProperty, value);
        }

        public Color NotificationColor
        {
            get => (Color)this.GetValue(NotificationColorProperty);
            set => this.SetValue(NotificationColorProperty, value);
        }

        public float MinimumSeparation
        {
            get => (float)this.GetValue(MinimumSeparationProperty);
            set => this.SetValue(MinimumSeparationProperty, value);
        }

        public IEnumerable BooleanSource
        {
            get => (IEnumerable)this.GetValue(BooleanSourceProperty);
            set => this.SetValue(BooleanSourceProperty, value);
        }

        public async void ScrollTo(int index)
        {
            if (this.CurrentSize == SKSize.Empty)
                return;

            if (0 > index || index >= this.NumberOfSteps)
                throw new ArgumentOutOfRangeException($"index: {index}");
            var tdx = this.CurrentSize.Width / 2 + Margin.Left - this.CalculatedPoints[index].X;
            tdx = Math.Min(Math.Max(tdx, -this.CalculatedLineWidth + this.CurrentSize.Width), 0);
            var step = Math.Abs(tdx - this.totaldx) / 10;
            step = Math.Max(step, 1);
            if (tdx > this.totaldx)
            {
                for (var i = this.totaldx; i <= tdx; i+=step)
                {
                    this.totaldx = i;
                    this.InvalidateSurface();
                    await Task.Delay(2);
                }
            }
            else
            {
                for (var i = this.totaldx; i >= tdx; i-=step)
                {
                    this.totaldx = i;
                    this.InvalidateSurface();
                    await Task.Delay(2);
                }
            }
            this.totaldx = tdx;
            this.clickPoint = SKPoint.Empty;
            this.InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            if (this.NumberOfSteps < 2)
                return;

            var size = new SKSize((float)(e.Info.Width - this.Margin.HorizontalThickness), (float)(e.Info.Height - this.Margin.VerticalThickness));
            if (this.CurrentSize == SKSize.Empty || this.CurrentSize != size)
            {
                this.CurrentSize = size;
                this.PointSize = (float)(size.Height * 0.75) + 2;
                if (this.MinimumSeparation <= 1)
                {
                    this.Separation = this.PointSize;
                }

                this.CalculatedLineWidth = (this.NumberOfSteps * this.PointSize) + (this.NumberOfSteps - 1) * this.Separation;
                this.panEnabled = size.Width < this.CalculatedLineWidth;

                if (!this.panEnabled)
                {
                    this.Separation = (size.Width - this.NumberOfSteps * this.PointSize) / (this.NumberOfSteps - 1);
                }
                this.CalculatedPoints = this.CalculatePoints(size);
            }
            
            var canvas = e.Surface.Canvas;
            canvas.Clear(this.BackgroundColor.ToSKColor());
            this.DrawOutline(canvas, size);
            this.DrawActiveLine(canvas, size, this.CorrectPoint(this.CalculatedPoints[this.CurrentIndex], out var f));
            var drawingPoints = this.GetDrawingPoints();

            // We split the drawing so the points in the far right are overlapping the right way
            var index = drawingPoints.FindIndex(x => x.Item1.X >= size.Width / 2);
            for (var i = 0; i < index; i++)
            {
                this.DrawScaledPoint(canvas, drawingPoints[i], i <= this.CurrentIndex, this.BooleanSource?.GetCount() == this.NumberOfSteps && (bool)this.BooleanSource.GetItem(i));
            }

            for (var i = drawingPoints.Length - 1; i >= index; i--)
            {
                this.DrawScaledPoint(canvas, drawingPoints[i], i <= this.CurrentIndex, this.BooleanSource?.GetCount() == this.NumberOfSteps && (bool)this.BooleanSource.GetItem(i));
            }
        }

        protected override void OnTouch(SKTouchEventArgs e)
        {
            if (e.ActionType == SKTouchAction.Pressed)
            {
                this.clickPoint = e.Location;
            }
            
            base.OnTouch(e);
        }

        private void Touch_Tapped(object sender, EventArgs e)
        {
            // if clicks on the edge when its contracted it should be ignored
            if (this.clickPoint == SKPoint.Empty //or a point wasn't registered
                || (this.clickPoint.X < this.Margin.Left + this.PointSize / 2 + this.PointSize + this.Separation && this.totaldx < 0) 
                || (this.clickPoint.X > this.CurrentSize.Width - (this.PointSize / 2) + Margin.Left - (this.PointSize + this.Separation) && this.totaldx > -this.CalculatedLineWidth + this.CurrentSize.Width))
            {
                this.clickPoint = SKPoint.Empty;
                return;
            }

            var x = this.clickPoint.X + Math.Abs(this.totaldx);
            var tolerance = this.PointSize / 2 + this.Separation / 4;
            var index = this.CalculatedPoints.FindIndex(p => x - tolerance <= p.X && p.X <= x + tolerance);

            if (index == -1)
            {
                this.clickPoint = SKPoint.Empty;
                return;
            }

            this.CurrentIndex = index;
        }

        protected SKPoint[] CalculatePoints(SKSize itemSize)
        {
            var result = new List<SKPoint>();
            var y = (itemSize.Height / 2) + (float)this.Margin.Top;
            for (var i = 0; i < this.NumberOfSteps; i++)
            {
                var x = (float)(i*(this.PointSize + this.Separation) + (this.Margin.Left + this.PointSize/2));
                var point = new SKPoint(x, y);
                result.Add(point);
            }

            return result.ToArray();
        }

        protected Tuple<SKPoint, float>[] GetDrawingPoints()
        {
            if (!this.panEnabled)
            {
                return this.CalculatedPoints.Select(x => new Tuple<SKPoint, float>(x, 1)).ToArray();
            }

            var result = new List<Tuple<SKPoint, float>>(this.CalculatedPoints.Length);
            for (var i = 0; i < this.CalculatedPoints.Length; i++)
            {
                var p = this.CorrectPoint(this.CalculatedPoints[i], out var f);

                // first and last point will trick the scaling getting the value from the adjacent point
                if (i == 0)
                {
                    _ = this.CorrectPoint(this.CalculatedPoints[i + 1], out f);
                }

                if (i == this.CalculatedPoints.Length - 1)
                {
                    _ = this.CorrectPoint(this.CalculatedPoints[i - 1], out f);
                }

                // As the point scales down we have to scale the limit point to the borders
                if (p.X < this.Margin.Left + this.PointSize / 2 + this.PointSize + this.Separation)
                {
                    p.X -= (1 - f) * 0.4f * (this.PointSize / 2);
                }

                if (p.X > this.CurrentSize.Width - (this.PointSize / 2) + Margin.Left - (this.PointSize + this.Separation))
                {
                    p.X += (1 - f) * 0.4f * (this.PointSize / 2);
                }

                result.Add(new Tuple<SKPoint, float>(p, f));
            }

            return result.ToArray();
        }

        protected void DrawOutline(SKCanvas canvas, SKSize itemSize)
        {
            using (var paint = new SKPaint { Style = SKPaintStyle.Stroke, Color = this.ActiveColor.ToSKColor(), IsAntialias = true, StrokeWidth = 2 })
            {
                var x = (float)this.Margin.Left;
                var height = (float)(itemSize.Height * 0.2);
                var y = (float)(this.Margin.Top + (itemSize.Height - height) / 2);
                var r = height / 2;
                paint.Style = SKPaintStyle.Fill;
                paint.Color = this.SecondaryColor.ToSKColor();
                canvas.DrawRoundRect(SKRect.Create(x, y, itemSize.Width, height), r, r, paint);
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = this.ActiveColor.ToSKColor();
                canvas.DrawRoundRect(SKRect.Create(x, y, itemSize.Width, height - 4), r, r, paint);
            }
        }

        protected void DrawActiveLine(SKCanvas canvas, SKSize itemSize, SKPoint point)
        {
            using (var paint = new SKPaint { Style = SKPaintStyle.Fill, Color = this.ActiveColor.ToSKColor(), IsAntialias = true, StrokeWidth = 2 })
            {
                var x = (float)this.Margin.Left;
                var height = (float)(itemSize.Height * 0.2);
                var y = (float)(this.Margin.Top + (itemSize.Height - height) / 2);
                var r = height / 2;
                canvas.DrawRoundRect(SKRect.Create(x, y, point.X - x, height), r, r, paint);
            }
        }

        private void DrawDebugLines(SKCanvas canvas, SKSize itemSize)
        {
            var min = (this.Margin.Left + this.PointSize / 2);
            var max = this.CurrentSize.Width - (this.PointSize / 2) + Margin.Left;
            var mid = this.CurrentSize.Width / 2 + Margin.Left;
            var curveSegment = this.PointSize + this.Separation;
            using (var paint = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.Red, IsAntialias = true, StrokeWidth = 2 })
            {
                var x = (float)min;
                canvas.DrawRect(x - 2, 0, 5, itemSize.Height, paint);
                x = (float)max;
                canvas.DrawRect(x - 2, 0, 5, itemSize.Height, paint);
                x = (float)min + curveSegment;
                canvas.DrawRect(x - 2, 0, 5, itemSize.Height, paint);
                x = (float)max - curveSegment;
                canvas.DrawRect(x - 2, 0, 5, itemSize.Height, paint);
                x = (float)mid;
                canvas.DrawRect(x - 2, 0, 5, itemSize.Height, paint);
            }
        }

        protected void DrawScaledPoint(SKCanvas canvas, Tuple<SKPoint, float> point, bool active = false, bool notification = false)
        {
            var d = (float) (this.PointSize * (0.6 + 0.4 * point.Item2));
            using (var paint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true, StrokeCap = SKStrokeCap.Round})
            {
                paint.Color = this.ActiveColor.ToSKColor();
                paint.StrokeWidth = d + 4;
                canvas.DrawPoint(point.Item1, paint);
                paint.Color = active ? this.ActiveColor.ToSKColor() : this.SecondaryColor.ToSKColor();
                paint.StrokeWidth = d - 4;
                canvas.DrawPoint(point.Item1, paint);
                if (notification)
                {
                    paint.Color = this.NotificationColor.ToSKColor();
                    paint.StrokeWidth = d / 2;
                    var c = (float) (d / (2 * Math.Sqrt(2)));
                    canvas.DrawPoint(new SKPoint(point.Item1.X + c, point.Item1.Y - c), paint);
                }
            }
        }

        private SKPoint CorrectPoint(SKPoint point, out float factor)
        {
            float x = 0;
            var min = (this.Margin.Left + this.PointSize / 2);
            var max = this.CurrentSize.Width - (this.PointSize / 2) + Margin.Left;
            var curveSegment = this.PointSize + this.Separation;
            var realSegment = this.CurrentSize.Width - 2 * curveSegment - this.PointSize - Margin.HorizontalThickness;
            if (!this.panEnabled || point.X <= min || point.X >= this.CalculatedLineWidth - (this.PointSize / 2) + Margin.Left ||
                    (point.X >= Math.Abs(this.totaldx) + curveSegment + min && 
                     point.X <= Math.Abs(this.totaldx) + max - curveSegment))
            {
                factor = 1;
                x = Math.Min(Math.Max(point.X + (float)this.totaldx, (float)min), (float)max);
            }
            else
            {
                if (point.X <= Math.Abs(this.totaldx) + curveSegment + min)
                {
                    factor = (float)Math.Pow(point.X / (min + curveSegment + Math.Abs(this.totaldx)), 3);
                    x = (float)(min + (this.PointSize + this.Separation) * factor);
                } 
                else
                {
                    factor = (float)Math.Pow(1- ((point.X - min - curveSegment - Math.Abs(this.totaldx) - realSegment) / (this.CalculatedLineWidth - min - curveSegment - Math.Abs(this.totaldx) - realSegment)), 3);
                    x = (float)(max - (this.PointSize + this.Separation) * factor);
                }
            }
            
            return new SKPoint(x, point.Y);
        }

        private void PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (!this.panEnabled)
            {
                return;
            }

            if (e.StatusType != GestureStatus.Running)
            {
                this.currentPandx = 0;
                return;
            }

            var d = e.TotalX - this.currentPandx;
            this.currentPandx = e.TotalX;
            this.totaldx += d;
            this.totaldx = Math.Min(Math.Max(this.totaldx, -this.CalculatedLineWidth + this.CurrentSize.Width), 0);
            this.InvalidateSurface();
        }

        private static void CurrentIndexPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SlidingProgressStepBar control)
            {
                control.ScrollTo((int)newValue);
            }
        }

        private static void BooleanSourcePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is SlidingProgressStepBar control)
            {
                if(control.NumberOfSteps == 0 && control.BooleanSource?.GetCount() > 0)
                {
                    control.NumberOfSteps = control.BooleanSource.GetCount();
                    control.InvalidateSurface();
                }
            }
        }
    }
}
