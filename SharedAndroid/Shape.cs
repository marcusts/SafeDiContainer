#region License

// MIT License
// 
// Copyright (c) 2018 
// Marcus Technical Services, Inc.
// http://www.marcusts.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

// #define USE_SHADOW

namespace SharedAndroid
{
   #region Imports

   using System;
   using System.Collections.ObjectModel;
   using System.Linq;
   using Android.Content;
   using Android.Graphics;
   using Android.Views;
   using SharedForms.Views.Controls;
   using Xamarin.Forms.Platform.Android;

   #endregion

   public class Shape : View
   {
      private readonly float _density;
      private readonly ShapeView _shapeView;

      public Shape(float density, Context context, ShapeView shapeView) : base(context)
      {
         if (shapeView == null)
         {
            throw new ArgumentNullException(nameof(shapeView));
         }

         _density = density;
         _shapeView = shapeView;
      }

#if USE_SHADOW
      private static int convertTo255ScaleColor(double color)
      {
         return (int)Math.Ceiling(color * 255);
      }
#endif

      protected override void OnDraw(Canvas canvas)
      {
         base.OnDraw(canvas);

         var x = GetX() + Resize(_shapeView.Padding.Left);
         var y = GetY() + Resize(_shapeView.Padding.Top);
         var width = Width - Resize(_shapeView.Padding.HorizontalThickness);
         var height = Height - Resize(_shapeView.Padding.VerticalThickness);
         var cx = width / 2f + Resize(_shapeView.Padding.Left);
         var cy = height / 2f + Resize(_shapeView.Padding.Top);
         var strokeWidth = 0f;

         Paint strokePaint = null;

         if (_shapeView.BorderWidth > 0 && _shapeView.BorderColor.A > 0)
         {
            strokeWidth = Resize(_shapeView.BorderWidth);

            strokePaint = new Paint(PaintFlags.AntiAlias);
            strokePaint.SetStyle(Paint.Style.Stroke);
            strokePaint.StrokeWidth = strokeWidth;
            strokePaint.StrokeCap = Paint.Cap.Round;
            strokePaint.Color = _shapeView.BorderColor.ToAndroid();

#if USE_SHADOW
            // NEEDS TO BE A BOTTOM RIGHT SHADOW -- THE REST SHOULD NOT DRAW
            strokePaint.SetARGB(convertTo255ScaleColor(_shapeView.BorderColor.A), convertTo255ScaleColor(_shapeView.BorderColor.R), convertTo255ScaleColor(_shapeView.BorderColor.G), convertTo255ScaleColor(_shapeView.BorderColor.B));
            var shadowOffset = (float)(Math.Min(_shapeView.WidthRequest, _shapeView.HeightRequest) * 0.05);
            strokePaint.SetShadowLayer(_shapeView.CornerRadius, shadowOffset, shadowOffset, Android.Graphics.Color.Argb(150, 0, 0, 0));
            SetLayerType(Android.Views.LayerType.Software, strokePaint);
#endif

            x += strokeWidth / 2f;
            y += strokeWidth / 2f;
            width -= strokeWidth;
            height -= strokeWidth;
         }

         Paint fillPaint = null;

         if (_shapeView.Color.A > 0)
         {
            fillPaint = new Paint(PaintFlags.AntiAlias);
            fillPaint.SetStyle(Paint.Style.Fill);
            fillPaint.Color = _shapeView.Color.ToAndroid();
         }

         if (_shapeView.CornerRadius > 0)
         {
            switch (_shapeView.ShapeType)
            {
               case ShapeType.Star:
               case ShapeType.Triangle:
               case ShapeType.Diamond:
               case ShapeType.Path:
                  var cr = Resize(_shapeView.CornerRadius);

                  var cornerPathEffect = new CornerPathEffect(cr);
                  fillPaint?.SetPathEffect(cornerPathEffect);
                  strokePaint?.SetPathEffect(cornerPathEffect);
                  break;
            }
         }

         switch (_shapeView.ShapeType)
         {
            case ShapeType.Box:
               DrawBox(canvas, x, y, width, height, _shapeView.CornerRadius, fillPaint, strokePaint);
               break;
            case ShapeType.Circle:
               DrawCircle(canvas, cx, cy, Math.Min(height, width) / 2f, fillPaint, strokePaint);
               break;
            case ShapeType.Oval:
               DrawOval(canvas, x, y, width, height, fillPaint, strokePaint);
               break;
            case ShapeType.Star:
               var outerRadius = (Math.Min(height, width) - strokeWidth) / 2f;
               var innerRadius = outerRadius * _shapeView.RadiusRatio;

               DrawStar(canvas, cx, cy, outerRadius, innerRadius, _shapeView.CornerRadius, _shapeView.NumberOfPoints,
                  fillPaint, strokePaint);
               break;
            case ShapeType.Triangle:
               DrawTriangle(canvas, x + strokeWidth / 2, y + strokeWidth / 2, width - strokeWidth, height - strokeWidth,
                  fillPaint, strokePaint);
               break;
            case ShapeType.Diamond:
               DrawDiamond(canvas, x + strokeWidth / 2, y + strokeWidth / 2, width - strokeWidth, height - strokeWidth,
                  fillPaint, strokePaint);
               break;
            case ShapeType.Heart:
               DrawHeart(canvas, x, y, width, height, Resize(_shapeView.CornerRadius), fillPaint, strokePaint);
               break;
            case ShapeType.ProgressCircle:
               DrawCircle(canvas, cx, cy, Math.Min(height, width) / 2f, fillPaint, strokePaint);

               if (_shapeView.ProgressBorderWidth > 0 && _shapeView.ProgressBorderColor.A > 0)
               {
                  var progressStrokeWidth = Resize(_shapeView.ProgressBorderWidth);

                  var progressPaint = new Paint(PaintFlags.AntiAlias);
                  progressPaint.SetStyle(Paint.Style.Stroke);
                  progressPaint.StrokeWidth = progressStrokeWidth;
                  progressPaint.Color = _shapeView.ProgressBorderColor.ToAndroid();

                  var deltaWidth = progressStrokeWidth - strokeWidth;

                  if (deltaWidth > 0)
                  {
                     width -= deltaWidth;
                     height -= deltaWidth;
                  }

                  DrawProgressCircle(canvas, cx, cy, Math.Min(height, width) / 2f, _shapeView.Progress, progressPaint);
               }

               break;
            case ShapeType.Path:
               DrawPath(canvas, _shapeView.Points, x, y, fillPaint, strokePaint);
               break;
         }
      }

#region Drawing Methods

#region Basic shapes

      protected virtual void DrawBox(Canvas canvas, float left, float top, float width, float height,
         float cornerRadius, Paint fillPaint, Paint strokePaint)
      {
         var rect = new RectF(left, top, left + width, top + height);
         if (cornerRadius > 0)
         {
            var cr = Resize(cornerRadius);
            if (fillPaint != null)
            {
               canvas.DrawRoundRect(rect, cr, cr, fillPaint);
            }

            if (strokePaint != null)
            {
               canvas.DrawRoundRect(rect, cr, cr, strokePaint);
            }
         }
         else
         {
            if (fillPaint != null)
            {
               canvas.DrawRect(rect, fillPaint);
            }

            if (strokePaint != null)
            {
               canvas.DrawRect(rect, strokePaint);
            }
         }
      }

      protected virtual void DrawCircle(Canvas canvas, float cx, float cy, float radius, Paint fillPaint,
         Paint strokePaint)
      {
         if (fillPaint != null)
         {
            canvas.DrawCircle(cx, cy, radius, fillPaint);
         }

         if (strokePaint != null)
         {
            canvas.DrawCircle(cx, cy, radius, strokePaint);
         }
      }

      protected virtual void DrawOval(Canvas canvas, float left, float top, float width, float height, Paint fillPaint,
         Paint strokePaint)
      {
         var rect = new RectF(left, top, left + width, top + height);

         if (fillPaint != null)
         {
            canvas.DrawOval(rect, fillPaint);
         }

         if (strokePaint != null)
         {
            canvas.DrawOval(rect, strokePaint);
         }
      }

      protected virtual void DrawProgressCircle(Canvas canvas, float cx, float cy, float radius, float progress,
         Paint progressPaint)
      {
         if (progressPaint != null)
         {
            using (var rectF = new RectF(cx - radius, cy - radius, cx + radius, cy + radius))
            {
               canvas.DrawArc(rectF, -90, 360f * (progress / 100f), false, progressPaint);
            }
         }
      }

#endregion

#region Path Methods

      protected virtual void DrawStar(Canvas canvas, float cx, float cy, float outerRadius, float innerRadius,
         float cornerRadius, int numberOfPoints, Paint fillPaint, Paint strokePaint)
      {
         if (numberOfPoints <= 0)
         {
            return;
         }

         var baseAngle = Math.PI / numberOfPoints;
         var isOuter = false;
         var xPath = cx;
         var yPath = innerRadius + cy;

         var path = new Path();
         path.MoveTo(xPath, yPath);

         var i = baseAngle;
         while (i <= Math.PI * 2)
         {
            var currentRadius = isOuter ? innerRadius : outerRadius;
            isOuter = !isOuter;

            xPath = (float) (currentRadius * Math.Sin(i)) + cx;
            yPath = (float) (currentRadius * Math.Cos(i)) + cy;

            path.LineTo(xPath, yPath);

            i += baseAngle;
         }

         path.Close();

         if (fillPaint != null)
         {
            canvas.DrawPath(path, fillPaint);
         }

         if (strokePaint != null)
         {
            canvas.DrawPath(path, strokePaint);
         }
      }

      protected virtual void DrawDiamond(Canvas canvas, float x, float y, float width, float height, Paint fillPaint,
         Paint strokePaint)
      {
         var path = new Path();

         var centerX = width / 2f + x;
         var centerY = height / 2f + y;

         path.MoveTo(centerX, y);
         path.LineTo(x + width, centerY);
         path.LineTo(centerX, height + y);
         path.LineTo(x, centerY);
         path.Close();

         DrawPath(canvas, path, fillPaint, strokePaint);
      }

      protected virtual void DrawTriangle(Canvas canvas, float x, float y, float width, float height, Paint fillPaint,
         Paint strokePaint)
      {
         var path = new Path();

         path.MoveTo(x, height + y);
         path.LineTo(x + width, height + y);
         path.LineTo(width / 2f + x, y);
         path.Close();

         DrawPath(canvas, path, fillPaint, strokePaint);
      }

      protected virtual void DrawHeart(Canvas canvas, float x, float y, float width, float height, float cornerRadius,
         Paint fillPaint, Paint strokePaint)
      {
         var length = Math.Min(height, width);

         var p1 = new PointF(x, y + length);
         var p2 = new PointF(x + 2f * length / 3f, y + length);
         var p3 = new PointF(x + 2f * length / 3f, y + length / 3f);
         var p4 = new PointF(x, y + length / 3f);
         var radius = length / 3f;

         var path = new Path();
         path.MoveTo(p4.X, p4.Y);
         path.LineTo(p1.X, p1.Y - cornerRadius);
         path.LineTo(p1.X + cornerRadius, p1.Y);

         path.LineTo(p2.X, p2.Y);
         path.LineTo(p3.X, p3.Y);
         path.Close();

         if (cornerRadius > 0)
         {
            path.AddArc(new RectF(p1.X, (p1.Y + p4.Y) / 2f, (p2.X + p1.X) / 2f, p2.Y), 90, 90);
         }

         path.AddArc(new RectF(p3.X - radius, p3.Y, p3.X + radius, p2.Y), -90f, 180f);
         path.AddArc(new RectF(p4.X, p4.Y - radius, p3.X, p4.Y + radius), 180f, 180f);

         var matrix = new Matrix();
         matrix.SetTranslate(-length / 3f, -length * 2f / 3f);
         path.Transform(matrix);

         matrix.Reset();

         matrix.SetRotate(-45f);
         path.Transform(matrix);

         matrix.Reset();
         matrix.SetScale(0.85f, 0.85f);
         path.Transform(matrix);

         matrix.Reset();
         matrix.SetTranslate(width / 2f, 1.1f * height / 2f);
         path.Transform(matrix);

         DrawPath(canvas, path, fillPaint, strokePaint);
      }

      protected virtual void DrawPath(Canvas canvas, ObservableCollection<Xamarin.Forms.Point> points, float x, float y,
         Paint fillPaint, Paint strokePaint)
      {
         if (points == null || points.Count == 0)
         {
            return;
         }

         var path = new Path();

         var resizedPoints = points.Select(p => new PointF(Resize(p.X), Resize(p.Y))).ToList();

         path.MoveTo(resizedPoints[0].X, resizedPoints[0].Y);

         for (var i = 1; i < resizedPoints.Count; ++i)
         {
            path.LineTo(resizedPoints[i].X, resizedPoints[i].Y);
         }

         path.Close();

         var matrix = new Matrix();
         matrix.SetTranslate(x, y);

         path.Transform(matrix);

         DrawPath(canvas, path, fillPaint, strokePaint);
      }

      protected virtual void DrawPath(Canvas canvas, Path path, Paint fillPaint, Paint strokePaint)
      {
         if (fillPaint != null)
         {
            canvas.DrawPath(path, fillPaint);
         }

         if (strokePaint != null)
         {
            canvas.DrawPath(path, strokePaint);
         }
      }

#endregion

#endregion

#region Density Helpers

      protected float Resize(float input)
      {
         return input * _density;
      }

      protected float Resize(double input)
      {
         return Resize((float) input);
      }

#endregion
   }
}