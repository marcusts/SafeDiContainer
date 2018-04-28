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

#region Imports

using SharedForms.Views.Controls;
using SharedIOS;
using Xamarin.Forms;

#endregion

[assembly: ExportRenderer(typeof(ShapeView), typeof(ShapeRenderer))]

namespace SharedIOS
{
   #region Imports

   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Drawing;
   using System.Linq;
   using CoreGraphics;
   using SharedForms.Views.Controls;
   using UIKit;
   using Xamarin.Forms.Platform.iOS;

   #endregion

   public class ShapeRenderer : VisualElementRenderer<ShapeView>
   {
      #region Public Constructors

      public ShapeRenderer()
      {
         ContentMode = UIViewContentMode.Redraw;
      }

      #endregion Public Constructors

      #region Private Methods

      private static void DrawPath(CGContext context, bool fill, bool stroke)
      {
         if (fill && stroke)
         {
            context.DrawPath(CGPathDrawingMode.FillStroke);
         }
         else if (fill)
         {
            context.DrawPath(CGPathDrawingMode.Fill);
         }
         else if (stroke)
         {
            context.DrawPath(CGPathDrawingMode.Stroke);
         }
      }

      #endregion Private Methods

      #region Public Methods

      public new static void Init()
      {
         using (var r = new ShapeRenderer())
         {
         }
      }

      public override void Draw(CGRect rect)
      {
         base.Draw(rect);

         var x = (float) (rect.X + Element.Padding.Left);
         var y = (float) (rect.Y + Element.Padding.Top);
         var width = (float) (rect.Width - Element.Padding.HorizontalThickness);
         var height = (float) (rect.Height - Element.Padding.VerticalThickness);
         var cx = (float) (width / 2f + Element.Padding.Left);
         var cy = (float) (height / 2f + Element.Padding.Top);

         var context = UIGraphics.GetCurrentContext();

         var fillColor = Element.Color.ToUIColor();
         var strokeColor = Element.BorderColor.ToUIColor();
         var fill = false;
         var stroke = false;

         var strokeWidth = 0f;

         if (Element.BorderWidth > 0 && Element.BorderColor.A > 0)
         {
            context.SetLineWidth(Element.BorderWidth);
            strokeColor.SetStroke();

            stroke = true;
            strokeWidth = Element.BorderWidth;

            x += strokeWidth / 2f;
            y += strokeWidth / 2f;
            width -= strokeWidth;
            height -= strokeWidth;
         }

         if (Element.Color.A > 0)
         {
            fillColor.SetFill();
            fill = true;
         }

         switch (Element.ShapeType)
         {
            case ShapeType.Box:
               DrawBox(context, x, y, width, height, Element.CornerRadius, fill, stroke);
               break;

            case ShapeType.Circle:
               DrawCircle(context, cx, cy, Math.Min(height, width) / 2f, fill, stroke);
               break;

            case ShapeType.Oval:
               DrawOval(context, x, y, width, height, fill, stroke);
               break;

            case ShapeType.Star:
               var outerRadius = (Math.Min(height, width) - strokeWidth) / 2f;
               var innerRadius = outerRadius * Element.RadiusRatio;

               DrawStar(context, cx, cy, outerRadius, innerRadius, Element.NumberOfPoints, Element.CornerRadius, fill,
                  stroke);
               break;

            case ShapeType.Triangle:
               DrawTriangle(context, x + strokeWidth / 2, y + strokeWidth / 2, width - strokeWidth,
                  height - strokeWidth, Element.CornerRadius, fill, stroke);
               break;

            case ShapeType.Diamond:
               DrawDiamond(context, x + strokeWidth / 2, y + strokeWidth / 2, width - strokeWidth, height - strokeWidth,
                  Element.CornerRadius, fill, stroke);
               break;

            case ShapeType.Heart:
               DrawHeart(context, x, y, width, height, Element.CornerRadius, fill, stroke);
               break;

            case ShapeType.ProgressCircle:
               var drawProgress = false;
               if (Element.ProgressBorderWidth > 0 && Element.ProgressBorderColor.A > 0)
               {
                  var deltaWidth = Element.ProgressBorderWidth - strokeWidth;

                  if (deltaWidth > 0)
                  {
                     width -= 2f * deltaWidth;
                     height -= 2f * deltaWidth;
                  }

                  drawProgress = true;
               }

               var radius = Math.Min(height, width) / 2f;

               DrawCircle(context, cx, cy, radius, fill, stroke);

               if (drawProgress)
               {
                  context.SetLineWidth(Element.ProgressBorderWidth);

                  var progressStrokeColor = Element.ProgressBorderColor.ToUIColor();
                  progressStrokeColor.SetStroke();

                  DrawProgressCircle(context, cx, cy, radius, Element.Progress, false, true);
               }

               break;

            case ShapeType.Path:
               DrawPoints(context, Element.Points.Select(p => p.ToCGPoint()).ToList(),
                  Element.CornerRadius, fill, stroke, x, y);
               break;
         }
      }

      #endregion Public Methods

      #region Protected Methods

      protected virtual void DrawBox(CGContext context, float x, float y, float width, float height, float cornerRadius,
         bool fill, bool stroke)
      {
         var rect = new RectangleF(x, y, width, height);
         if (cornerRadius > 0)
         {
            context.AddPath(UIBezierPath.FromRoundedRect(rect, cornerRadius).CGPath);
         }
         else
         {
            context.AddRect(rect);
         }

         DrawPath(context, fill, stroke);
      }

      protected virtual void DrawCircle(CGContext context, float cx, float cy, float radius, bool fill, bool stroke)
      {
         context.AddArc(cx, cy, radius, 0, (float) Math.PI * 2, true);
         DrawPath(context, fill, stroke);
      }

      protected virtual void DrawDiamond(CGContext context, float x, float y, float width, float height,
         float cornerRadius, bool fill, bool stroke)
      {
         var centerX = width / 2f + x;
         var centerY = height / 2f + y;

         var points = new List<CGPoint>
         {
            new CGPoint(x, centerY),
            new CGPoint(centerX, y),
            new CGPoint(x + width, centerY),
            new CGPoint(centerX, height + y)
         };

         DrawPoints(context, points, cornerRadius, fill, stroke);
      }

      protected virtual void DrawHeart(CGContext context, float x, float y, float width, float height,
         float cornerRadius, bool fill, bool stroke)
      {
         var length = Math.Min(height, width);

         var startPoint = new CGPoint(x, y + 2f * length / 3f);
         var p1 = new CGPoint(x, y + length);
         var p2 = new CGPoint(x + 2f * length / 3f, y + length);
         var c1 = new CGPoint(x + 2f * length / 3f, y + 2f * length / 3f);
         var c2 = new CGPoint(x + length / 3f, y + length / 3f);
         var radius = length / 3f;

         var path = new CGPath();

         path.MoveToPoint(startPoint.X, startPoint.Y);

         path.AddArcToPoint(p1.X, p1.Y, p2.X, p2.Y, cornerRadius);
         path.AddLineToPoint(p2.X, p2.Y);
         path.AddArc(c1.X, c1.Y, radius, (float) -Math.PI / 2f, (float) Math.PI / 2f, false);
         path.AddArc(c2.X, c2.Y, radius, 0f, (float) Math.PI, true);
         path.CloseSubpath();

         var transform = CGAffineTransform.MakeTranslation(-length / 3f, -length * 2f / 3f);
         transform.Rotate((float) -Math.PI / 4f);
         transform.Scale(0.85f, 0.85f);
         transform.Translate(width / 2f, 1.1f * height / 2f);
         path = path.CopyByTransformingPath(transform);
         context.AddPath(path);

         DrawPath(context, fill, stroke);
      }

      protected virtual void DrawOval(CGContext context, float x, float y, float width, float height, bool fill,
         bool stroke)
      {
         context.AddEllipseInRect(new RectangleF(x, y, width, height));
         DrawPath(context, fill, stroke);
      }

      protected virtual void DrawPoints(CGContext context, List<CGPoint> points, float cornerRadius, bool fill,
         bool stroke, float x = 0f, float y = 0f)
      {
         if (points == null || points.Count == 0)
         {
            return;
         }

         var midPoint = new CGPoint(0.5 * (points[0].X + points[1].X), 0.5 * (points[0].Y + points[1].Y));
         var path = new CGPath();

         path.MoveToPoint(midPoint);

         for (var i = 0; i < points.Count; ++i)
         {
            path.AddArcToPoint(points[(i + 1) % points.Count].X, points[(i + 1) % points.Count].Y,
               points[(i + 2) % points.Count].X, points[(i + 2) % points.Count].Y, cornerRadius);
         }

         path.CloseSubpath();

         var transform = CGAffineTransform.MakeTranslation(x, y);
         //path = path.CopyByTransformingPath(transform);

         context.AddPath(path);

         DrawPath(context, fill, stroke);
      }

      protected virtual void DrawProgressCircle(CGContext context, float cx, float cy, float radius, float progress,
         bool fill, bool stroke)
      {
         context.AddArc(cx, cy, radius, (float) -Math.PI / 2f, (float) (2f * Math.PI * progress / 100f - Math.PI / 2f),
            false);
         DrawPath(context, fill, stroke);
      }

      protected virtual void DrawStar(CGContext context, float x, float y, float outerRadius, float innerRadius,
         int numberOfPoints, float cornerRadius, bool fill, bool stroke)
      {
         if (numberOfPoints <= 0)
         {
            return;
         }

         var baseAngle = Math.PI / numberOfPoints;
         var isOuter = false;

         var points = new List<CGPoint>();

         var ba = baseAngle;
         while (ba <= Math.PI * 2)
         {
            var currentRadius = isOuter ? innerRadius : outerRadius;
            isOuter = !isOuter;

            var xPath = (float) (currentRadius * Math.Sin(ba)) + x;
            var yPath = (float) (currentRadius * Math.Cos(ba)) + y;

            points.Add(new CGPoint(xPath, yPath));

            ba += baseAngle;
         }

         DrawPoints(context, points, cornerRadius, fill, stroke);
      }

      protected virtual void DrawTriangle(CGContext context, float x, float y, float width, float height,
         float cornerRadius, bool fill, bool stroke)
      {
         var points = new List<CGPoint>
         {
            new CGPoint(x, y + height),
            new CGPoint(x + width / 2, y),
            new CGPoint(x + width, y + height)
         };

         DrawPoints(context, points, cornerRadius, fill, stroke);
      }

      protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         base.OnElementPropertyChanged(sender, e);

         if (Element == null)
         {
            return;
         }

         switch (e.PropertyName)
         {
            case nameof(Element.ShapeType):
            case nameof(Element.Color):
            case nameof(Element.BorderColor):
            case nameof(Element.BorderWidth):
            case nameof(Element.RadiusRatio):
            case nameof(Element.NumberOfPoints):
            case nameof(Element.CornerRadius):
            case nameof(Element.Progress):
            case nameof(Element.Points):
               SetNeedsDisplay();
               break;
         }
      }

      #endregion Protected Methods
   }
}