/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilverNote.Editor;

namespace UnitTests.Editor.Drawing
{
    [TestClass]
    public class SVGTests
    {
        string MakeSVG(string elements)
        {
            string version = System.Reflection.Assembly.GetAssembly(typeof(NCanvas)).GetName().Version.ToString();
            string result = 
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>" +
                "<!--Created with SilverNote v" + version + " (http://www.silver-note.com/)-->" +
                "<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\">" +
                    elements +
                "</svg>";
            return result;
        }

        NCanvas ParseSVG(string elements)
        {
            string svg = MakeSVG(elements);
            NCanvas result = new NCanvas();
            result.Load(svg);
            return result;
        }

        [TestMethod]
        public void TestFormatLine()
        {
            Shape drawing = new Line(-100, 0, 100, 50);

            string expected = MakeSVG("<line stroke-width=\"2\" stroke=\"black\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseLine()
        {
            NCanvas canvas = ParseSVG("<line stroke-width=\"2\" stroke=\"black\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");

            Assert.AreEqual(canvas.Drawings.Count, 1);
            var drawing = canvas.Drawings[0] as Line;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.StrokeWidth, 2);
            var strokeBrush = drawing.StrokeBrush as SolidColorBrush;
            Assert.IsNotNull(strokeBrush);
            Assert.AreEqual(strokeBrush.Color, Colors.Black);
            Assert.AreEqual(drawing.FillBrush, null);
            Assert.AreEqual(drawing.X1, -100);
            Assert.AreEqual(drawing.Y1, 0);
            Assert.AreEqual(drawing.X2, 100);
            Assert.AreEqual(drawing.Y2, 50);
            Assert.IsFalse(drawing.IsConnector);
        }

        [TestMethod]
        public void TestFormatLine_ZeroWidth()
        {
            Shape drawing = new Line(-100, 0, 100, 50)
            {
                StrokeWidth = 0
            };

            string expected = MakeSVG("<line stroke-width=\"0\" stroke=\"black\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseLine_ZeroWidth()
        {
            NCanvas canvas = ParseSVG("<line stroke-width=\"0\" stroke=\"black\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");

            var drawing = canvas.Drawings[0] as Line;
            Assert.AreEqual(drawing.StrokeWidth, 0);
        }

        [TestMethod]
        public void TestFormatLine_Wide()
        {
            Shape drawing = new Line(-100, 0, 100, 50)
            {
                StrokeWidth = 4
            };

            string expected = MakeSVG("<line stroke-width=\"4\" stroke=\"black\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseLine_Wide()
        {
            NCanvas canvas = ParseSVG("<line stroke-width=\"4\" stroke=\"black\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");

            var drawing = canvas.Drawings[0] as Line;
            Assert.AreEqual(drawing.StrokeWidth, 4);
        }

        [TestMethod]
        public void TestFormatLine_Dashed()
        {
            Shape drawing = new Line(-100, 0, 100, 50)
            {
                StrokeDashArray = DashStyles.Dash.Dashes
            };

            string expected = MakeSVG("<line stroke-width=\"2\" stroke=\"black\" stroke-dasharray=\"2 2\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseLine_Dashed()
        {
            NCanvas canvas = ParseSVG("<line stroke-width=\"2\" stroke=\"black\" stroke-dasharray=\"2 2\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");

            var drawing = canvas.Drawings[0] as Line;
            Assert.AreEqual(drawing.StrokeDashArray.Count, 2);
            Assert.AreEqual(drawing.StrokeDashArray[0], 2);
            Assert.AreEqual(drawing.StrokeDashArray[1], 2);
        }

        [TestMethod]
        public void TestFormatLine_Dotted()
        {
            Shape drawing = new Line(-100, 0, 100, 50)
            {
                StrokeDashArray = DashStyles.Dot.Dashes
            };

            string expected = MakeSVG("<line stroke-width=\"2\" stroke=\"black\" stroke-dasharray=\"0 2\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseLine_Dotted()
        {
            NCanvas canvas = ParseSVG("<line stroke-width=\"2\" stroke=\"black\" stroke-dasharray=\"0 2\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");

            var drawing = canvas.Drawings[0] as Line;
            Assert.AreEqual(drawing.StrokeDashArray.Count, 2);
            Assert.AreEqual(drawing.StrokeDashArray[0], 0);
            Assert.AreEqual(drawing.StrokeDashArray[1], 2);
        }

        [TestMethod]
        public void TestFormatLine_Blue()
        {
            Shape drawing = new Line(-100, 0, 100, 50)
            {
                StrokeBrush = Brushes.Blue
            };

            string expected = MakeSVG("<line stroke-width=\"2\" stroke=\"blue\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseLine_Blue()
        {
            NCanvas canvas = ParseSVG("<line stroke-width=\"2\" stroke=\"blue\" fill=\"none\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");

            var drawing = canvas.Drawings[0] as Line;
            var strokeBrush = drawing.StrokeBrush as SolidColorBrush;
            Assert.IsNotNull(strokeBrush);
            Assert.AreEqual(strokeBrush.Color, Colors.Blue);
        }

        [TestMethod]
        public void TestFormatLine_Shadow()
        {
            Shape drawing = new Line(-100, 0, 100, 50)
            {
                Filter = new DropShadowEffect { ShadowDepth = 3, Opacity = 0.5 }
            };

            string expected =
                "<defs>" +
                    "<filter class=\"dropShadow\" id=\"resource0\">" +
                        "<feGaussianBlur in=\"SourceAlpha\" stdDeviation=\"1\" result=\"blurResult\" />" +
                        "<feOffset in=\"blurResult\" dx=\"2\" dy=\"2\" result=\"offsetResult\" />" +
                        "<feMerge>" +
                            "<feMergeNode in=\"offsetResult\" />" +
                            "<feMergeNode in=\"SourceGraphic\" />" +
                        "</feMerge>" +
                    "</filter>" +
                "</defs>" +
                "<line stroke-width=\"2\" stroke=\"black\" fill=\"none\" filter=\"url(#resource0)\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />";
            expected = MakeSVG(expected);
            string actual = NCanvas.ToSVG(drawing);
            string id = Regex.Match(actual, "(?<=id=\")[^\"]*").Value;
            actual = actual.Replace(id, "resource0");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseLine_Shadow()
        {
            NCanvas canvas = ParseSVG(
                "<defs>" +
                    "<filter id=\"resource0\" class=\"dropShadow\">" +
                        "<feGaussianBlur in=\"SourceAlpha\" stdDeviation=\"1\" result=\"blurResult\" />" +
                        "<feOffset in=\"blurResult\" dx=\"2\" dy=\"2\" result=\"offsetResult\" />" +
                        "<feMerge>" +
                            "<feMergeNode in=\"offsetResult\" />" + 
                            "<feMergeNode in=\"SourceGraphic\" />" +
                        "</feMerge>" +
                    "</filter>" +
                "</defs>" +
                "<line stroke-width=\"2\" stroke=\"black\" fill=\"none\" filter=\"url(#resource0)\" x1=\"-100\" y1=\"0\" x2=\"100\" y2=\"50\" />");

            Assert.AreEqual(canvas.Drawings.Count, 1);
            var drawing = canvas.Drawings[0] as Line;
            Assert.IsNotNull(drawing);
            var effect = drawing.Effect as DropShadowEffect;
            Assert.IsNotNull(effect);
            Assert.AreEqual(effect.ShadowDepth, 3);
            Assert.AreEqual(effect.Opacity, 0.5);
        }

        [TestMethod]
        public void TestFormatLine_Connector()
        {
            Shape drawing = new Line(0, 8, 24, 8)
            {
                IsConnector = true
            };

            string expected = MakeSVG("<line class=\"connector\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" x1=\"0\" y1=\"8\" x2=\"24\" y2=\"8\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseLine_Connector()
        {
            NCanvas canvas = ParseSVG("<line class=\"connector\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" x1=\"0\" y1=\"8\" x2=\"24\" y2=\"8\" />");

            var drawing = canvas.Drawings[0] as Line;
            Assert.IsTrue(drawing.IsConnector);
        }

        [TestMethod]
        public void TestFormatMarker()
        {
            NPath path = new NPath
            {
                StrokeBrush = null,
                Data = "M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z"
            };

            Marker marker = new Marker(path)
            {
                RefX = 10,
                RefY = 5,
                MarkerWidth = 10,
                MarkerHeight = 10
            };

            string expected = MakeSVG(
                "<marker fill=\"none\" refX=\"10\" refY=\"5\" markerWidth=\"10\" markerHeight=\"10\" markerUnits=\"userSpaceOnUse\" orient=\"auto\">" +
                    "<path stroke-width=\"1\" stroke=\"none\" fill=\"black\" d=\"M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z\" />" +
                "</marker>");
            string actual = NCanvas.ToSVG(marker);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseMarker()
        {
            NCanvas canvas = ParseSVG(
                "<marker fill=\"none\" refX=\"10\" refY=\"5\" markerWidth=\"10\" markerHeight=\"10\" markerUnits=\"userSpaceOnUse\" orient=\"auto\">" +
                    "<path stroke-width=\"2\" stroke=\"none\" fill=\"black\" d=\"M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z\" />" +
                "</marker>");

            Assert.AreEqual(canvas.Drawings.Count, 1);
            var drawing = canvas.Drawings[0] as Marker;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.RefX, 10);
            Assert.AreEqual(drawing.RefY, 5);
            Assert.AreEqual(drawing.MarkerWidth, 10);
            Assert.AreEqual(drawing.MarkerHeight, 10);
            Assert.AreEqual(drawing.Orient, 0);
            Assert.AreEqual(drawing.Children.Count, 1);
            var path = drawing.Children[0] as NPath;
            Assert.IsNotNull(path);
            Assert.AreEqual(path.Data, "M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z");
            Assert.AreEqual(path.StrokeWidth, 2);
            Assert.AreEqual(path.StrokeBrush, null);
        }

        [TestMethod]
        public void TestFormatRectangle()
        {
            Shape drawing = new Rectangle(-1, 0, 20, 35);

            string expected = MakeSVG("<rect stroke-width=\"1\" stroke=\"none\" fill=\"black\" x=\"-1\" y=\"0\" width=\"20\" height=\"35\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseRectangle()
        {
            NCanvas canvas = ParseSVG("<rect stroke-width=\"2\" stroke=\"black\" fill=\"none\" x=\"-1\" y=\"0\" width=\"20\" height=\"35\" />");

            var drawing = canvas.Drawings[0] as Rectangle;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.X, -1);
            Assert.AreEqual(drawing.Y, 0);
            Assert.AreEqual(drawing.Width, 20);
            Assert.AreEqual(drawing.Height, 35);
        }

        [TestMethod]
        public void TestFormatRectangle_Rounded()
        {
            Shape drawing = new Rectangle(-1, 0, 20, 35, 2, 4);

            string expected = MakeSVG("<rect stroke-width=\"1\" stroke=\"none\" fill=\"black\" x=\"-1\" y=\"0\" width=\"20\" height=\"35\" rx=\"2\" ry=\"4\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseRectangle_Rounded()
        {
            NCanvas canvas = ParseSVG("<rect stroke-width=\"2\" stroke=\"black\" fill=\"none\" x=\"-1\" y=\"0\" width=\"20\" height=\"35\" rx=\"2\" ry=\"4\" />");

            var drawing = canvas.Drawings[0] as Rectangle;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.X, -1);
            Assert.AreEqual(drawing.Y, 0);
            Assert.AreEqual(drawing.Width, 20);
            Assert.AreEqual(drawing.Height, 35);
            Assert.AreEqual(drawing.RX, 2);
            Assert.AreEqual(drawing.RY, 4);
        }

        [TestMethod]
        public void TestFormatRectangle_WithLinearGradientFill()
        {
            Shape drawing = new Rectangle(-1, 0, 20, 35);
            var linearGradient = new LinearGradientBrush();
            linearGradient.StartPoint = new Point(0, 0);
            linearGradient.EndPoint = new Point(0, 1);
            linearGradient.GradientStops.Add(new GradientStop(Colors.Red, 0));
            linearGradient.GradientStops.Add(new GradientStop(Colors.Blue, 1));
            drawing.FillBrush = linearGradient;

            string expected = MakeSVG(
                "<defs>" +
                    "<linearGradient x1=\"0\" y1=\"0\" x2=\"0\" y2=\"1\" id=\"resource0\">" +
                        "<stop offset=\"0\" stop-color=\"red\" />" +
                        "<stop offset=\"1\" stop-color=\"blue\" />" +
                    "</linearGradient>" +
                "</defs>" +
                "<rect stroke-width=\"1\" stroke=\"none\" fill=\"url(#resource0)\" x=\"-1\" y=\"0\" width=\"20\" height=\"35\" />"
                );
            string actual = NCanvas.ToSVG(drawing);
            string id = Regex.Match(actual, "(?<=id=\")[^\"]*").Value;
            actual = actual.Replace(id, "resource0");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseRectangle_WithLinearGradientFill()
        {
            NCanvas canvas = ParseSVG(
                "<defs>" +
                    "<linearGradient x1=\"0\" y1=\"0\" x2=\"0\" y2=\"1\" id=\"resource0\">" +
                        "<stop offset=\"0\" stop-color=\"red\" />" +
                        "<stop offset=\"1\" stop-color=\"blue\" />" +
                    "</linearGradient>" +
                "</defs>" +
                "<rect stroke-width=\"2\" stroke=\"black\" fill=\"url(#resource0)\" x=\"-1\" y=\"0\" width=\"20\" height=\"35\" />");

            var drawing = canvas.Drawings[0] as Rectangle;
            Assert.IsNotNull(drawing);
            var brush = drawing.FillBrush as LinearGradientBrush;
            Assert.IsNotNull(brush);
            Assert.AreEqual(brush.StartPoint.X, 0);
            Assert.AreEqual(brush.StartPoint.Y, 0);
            Assert.AreEqual(brush.EndPoint.X, 0);
            Assert.AreEqual(brush.EndPoint.Y, 1);
            Assert.AreEqual(brush.GradientStops.Count, 2);
            Assert.AreEqual(brush.GradientStops[0].Offset, 0);
            Assert.AreEqual(brush.GradientStops[0].Color, Colors.Red);
            Assert.AreEqual(brush.GradientStops[1].Offset, 1);
            Assert.AreEqual(brush.GradientStops[1].Color, Colors.Blue);
        }

        [TestMethod]
        public void TestFormatEllipse()
        {
            Shape drawing = new Ellipse(-5, 5, 10, 20);

            string expected = MakeSVG("<ellipse stroke-width=\"1\" stroke=\"none\" fill=\"black\" cx=\"-5\" cy=\"5\" rx=\"10\" ry=\"20\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseEllipse()
        {
            NCanvas canvas = ParseSVG("<ellipse stroke-width=\"2\" stroke=\"black\" fill=\"none\" cx=\"-5\" cy=\"5\" rx=\"10\" ry=\"20\" />");

            var drawing = canvas.Drawings[0] as Ellipse;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.CX, -5);
            Assert.AreEqual(drawing.CY, 5);
            Assert.AreEqual(drawing.RX, 10);
            Assert.AreEqual(drawing.RY, 20);
        }

        [TestMethod]
        public void TestFormatSemiEllipse()
        {
            Shape drawing = new SemiEllipse
            {
                CX = 10,
                CY = 10,
                RX = 10,
                RY = 10,
                StartPoint = new Point(10, 0),
                EndPoint = new Point(10, 20),
                SweepDirection = SweepDirection.Clockwise
            };

            string expected = MakeSVG("<path class=\"semiEllipse\" stroke-width=\"1\" stroke=\"none\" fill=\"black\" d=\"M10,0 A10,10 0 1 1 10,20\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseSemiEllipse()
        {
            NCanvas canvas = ParseSVG("<path class=\"semiEllipse\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" d=\"M10,0 A10,10 0 1 1 10,20\" />");

            var drawing = canvas.Drawings[0] as SemiEllipse;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.CX, 10);
            Assert.AreEqual(drawing.CY, 10);
            Assert.AreEqual(drawing.RX, 10);
            Assert.AreEqual(drawing.RY, 10);
            Assert.AreEqual(drawing.StartPoint.X, 10);
            Assert.AreEqual(drawing.StartPoint.Y, 0);
            Assert.AreEqual(drawing.EndPoint.X, 10);
            Assert.AreEqual(drawing.EndPoint.Y, 20);
            Assert.AreEqual(drawing.SweepDirection, SweepDirection.Clockwise);
        }

        [TestMethod]
        public void TestFormatPolygon()
        {
            Shape drawing = new Polygon(0, 6, 6, 6, 10, 0, 14, 6, 20, 6, 14, 12, 17, 19, 10, 16, 3, 19, 6, 12);

            string expected = MakeSVG("<polygon stroke-width=\"1\" stroke=\"none\" fill=\"black\" points=\"0,6 6,6 10,0 14,6 20,6 14,12 17,19 10,16 3,19 6,12\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParsePolygon()
        {
            NCanvas canvas = ParseSVG("<polygon stroke-width=\"2\" stroke=\"black\" fill=\"none\" points=\"0,6 6,6 10,0 14,6 20,6 14,12 17,19 10,16 3,19 6,12\" />");

            var drawing = canvas.Drawings[0] as Polygon;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.Points.Count, 10);
            Assert.AreEqual(drawing.Points[0].X, 0);
            Assert.AreEqual(drawing.Points[0].Y, 6);
            Assert.AreEqual(drawing.Points[9].X, 6);
            Assert.AreEqual(drawing.Points[9].Y, 12);
        }

        [TestMethod]
        public void TestFormatPolyline()
        {
            Shape drawing = new PolyLine(0, 6, 6, 6, 10, 0, 14, 6, 20, 6, 14, 12, 17, 19, 10, 16, 3, 19);

            string expected = MakeSVG("<polyline stroke-width=\"2\" stroke=\"black\" fill=\"none\" points=\"0,6 6,6 10,0 14,6 20,6 14,12 17,19 10,16 3,19\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParsePolyline()
        {
            NCanvas canvas = ParseSVG("<polyline stroke-width=\"2\" stroke=\"black\" fill=\"none\" points=\"0,6 6,6 10,0 14,6 20,6 14,12 17,19 10,16 3,19\" />");

            var drawing = canvas.Drawings[0] as PolyLine;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.Points.Count, 9);
            Assert.AreEqual(drawing.Points[0].X, 0);
            Assert.AreEqual(drawing.Points[0].Y, 6);
            Assert.AreEqual(drawing.Points[8].X, 3);
            Assert.AreEqual(drawing.Points[8].Y, 19);
        }

        [TestMethod]
        public void TestFormatQuadraticCurve()
        {
            Shape drawing = new QuadraticCurve(0, 5, 12, 20, 24, 5);

            string expected = MakeSVG("<path class=\"quadraticCurve quadraticBezier\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" d=\"M0,5 Q12,20 24,5\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseQuadraticCurve()
        {
            NCanvas canvas = ParseSVG("<path class=\"quadraticCurve quadraticBezier\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" d=\"M0,5 Q12,20 24,5\" />");

            var drawing = canvas.Drawings[0] as QuadraticCurve;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.X1, 0);
            Assert.AreEqual(drawing.Y1, 5);
            Assert.AreEqual(drawing.X2, 12);
            Assert.AreEqual(drawing.Y2, 20);
            Assert.AreEqual(drawing.X3, 24);
            Assert.AreEqual(drawing.Y3, 5);
        }

        [TestMethod]
        public void TestFormatQuadraticCurve_Connector()
        {
            Shape drawing = new QuadraticCurve(0, 5, 12, 20, 24, 5)
            {
                IsConnector = true
            };

            string expected = MakeSVG("<path class=\"quadraticCurve quadraticBezier connector\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" d=\"M0,5 Q12,20 24,5\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseQuadraticCurve_Connector()
        {
            NCanvas canvas = ParseSVG("<path class=\"quadraticCurve quadraticBezier connector\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" d=\"M0,5 Q12,20 24,5\" />");

            var drawing = canvas.Drawings[0] as QuadraticCurve;
            Assert.IsNotNull(drawing);
            Assert.IsTrue(drawing.IsConnector);
        }

        [TestMethod]
        public void TestFormatRoutedLine()
        {
            Shape drawing = new RoutedLine(0, 4, 12, 4, 12, 12, 24, 12);

            string expected = MakeSVG("<polyline class=\"routedLine\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" points=\"0,4 12,4 12,12 24,12\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseRoutedLine()
        {
            NCanvas canvas = ParseSVG("<polyline class=\"routedLine\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" points=\"0,4 12,4 12,12 24,12\" />");

            var drawing = canvas.Drawings[0] as RoutedLine;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.Points.Count, 4);
            Assert.AreEqual(drawing.Points[0].X, 0);
            Assert.AreEqual(drawing.Points[0].Y, 4);
            Assert.AreEqual(drawing.Points[3].X, 24);
            Assert.AreEqual(drawing.Points[3].Y, 12);
        }

        [TestMethod]
        public void TestFormatRoutedLine_Connector()
        {
            Shape drawing = new RoutedLine(0, 4, 12, 4, 12, 12, 24, 12)
            {
                IsConnector = true
            };

            string expected = MakeSVG("<polyline class=\"routedLine connector\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" points=\"0,4 12,4 12,12 24,12\" />");
            string actual = NCanvas.ToSVG(drawing);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseRoutedLine_Connector()
        {
            NCanvas canvas = ParseSVG("<polyline class=\"routedLine connector\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" points=\"0,4 12,4 12,12 24,12\" />");

            var drawing = canvas.Drawings[0] as RoutedLine;
            Assert.IsNotNull(drawing);
            Assert.IsTrue(drawing.IsConnector);
        }

        [TestMethod]
        public void TestFormatRoutedLine_WithEndMarker()
        {
            var line = new RoutedLine(0, 4, 12, 4, 12, 12, 24, 12);
            var endMarker = new NPath
            {
                StrokeBrush = null,
                Data = "M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z"
            };
            line.MarkerEnd = new Marker(endMarker)
            {
                RefX = 10,
                RefY = 5,
                MarkerWidth = 10,
                MarkerHeight = 10
            };

            string expected = MakeSVG(
                "<defs>" +
                    "<marker fill=\"none\" refX=\"10\" refY=\"5\" markerWidth=\"10\" markerHeight=\"10\" markerUnits=\"userSpaceOnUse\" orient=\"auto\" id=\"resource0\">" +
                        "<path stroke-width=\"1\" stroke=\"none\" fill=\"black\" d=\"M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z\" />" +
                    "</marker>" +
                "</defs>" +
                "<polyline class=\"routedLine\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" marker-end=\"url(#resource0)\" points=\"0,4 12,4 12,12 24,12\" />");
            string actual = NCanvas.ToSVG(line);
            string id = Regex.Match(actual, "(?<=id=\")[^\"]*").Value;
            actual = actual.Replace(id, "resource0");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseRoutedLine_WithEndMarker()
        {
            NCanvas canvas = ParseSVG(
                "<defs>" +
                    "<marker id=\"resource0\" fill=\"black\" refX=\"10\" refY=\"5\" markerWidth=\"10\" markerHeight=\"10\" markerUnits=\"userSpaceOnUse\" orient=\"auto\">" +
                        "<path d=\"M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z\" stroke-width=\"2\" stroke=\"none\" fill=\"black\" />" +
                    "</marker>" +
                "</defs>" +
                "<polyline class=\"routedLine\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" points=\"0,4 12,4 12,12 24,12\" marker-end=\"url(#resource0)\" />");

            var drawing = canvas.Drawings[0] as RoutedLine;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.Points.Count, 4);
            Assert.IsNotNull(drawing.MarkerEnd);
            Assert.AreEqual(drawing.MarkerEnd.Children.Count, 1);
            var path = drawing.MarkerEnd.Children[0] as NPath;
            Assert.IsNotNull(path);
            Assert.AreEqual(path.Data, "M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z");
        }

        [TestMethod]
        public void TestFormatRoutedLine_WithStartAndEndMarkers()
        {
            var line = new RoutedLine(0, 4, 12, 4, 12, 12, 24, 12);
            var startMarker = new NPath
            {
                StrokeBrush = null,
                Data = "M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z"
            };
            var endMarker = new NPath
            {
                StrokeBrush = null,
                Data = "M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z"
            };
            line.MarkerStart = new Marker(startMarker)
            {
                RefX = 0,
                RefY = 5,
                MarkerWidth = 10,
                MarkerHeight = 10
            };
            line.MarkerEnd = new Marker(endMarker)
            {
                RefX = 10,
                RefY = 5,
                MarkerWidth = 10,
                MarkerHeight = 10
            };

            string expected = MakeSVG(
                "<defs>" +
                    "<marker fill=\"none\" refX=\"0\" refY=\"5\" markerWidth=\"10\" markerHeight=\"10\" markerUnits=\"userSpaceOnUse\" orient=\"auto\" id=\"resource0\">" +
                        "<path stroke-width=\"1\" stroke=\"none\" fill=\"black\" d=\"M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z\" />" +
                    "</marker>" +
                    "<marker fill=\"none\" refX=\"10\" refY=\"5\" markerWidth=\"10\" markerHeight=\"10\" markerUnits=\"userSpaceOnUse\" orient=\"auto\" id=\"resource1\">" +
                        "<path stroke-width=\"1\" stroke=\"none\" fill=\"black\" d=\"M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z\" />" +
                    "</marker>" +
                "</defs>" +
                "<polyline class=\"routedLine\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" marker-start=\"url(#resource0)\" marker-end=\"url(#resource1)\" points=\"0,4 12,4 12,12 24,12\" />");
            string actual = NCanvas.ToSVG(line);
            var ids = Regex.Matches(actual, "(?<=id=\")[^\"]*");
            Assert.IsTrue(ids.Count >= 2);
            actual = actual.Replace(ids[0].Value, "resource0");
            actual = actual.Replace(ids[1].Value, "resource1");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseRoutedLine_WithStartAndEndMarkers()
        {
            NCanvas canvas = ParseSVG(
                "<defs>" +
                    "<marker id=\"resource0\" fill=\"black\" refX=\"0\" refY=\"5\" markerWidth=\"10\" markerHeight=\"10\" markerUnits=\"userSpaceOnUse\" orient=\"auto\">" +
                        "<path d=\"M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z\" stroke-width=\"2\" stroke=\"none\" fill=\"black\" />" +
                    "</marker>" +
                    "<marker id=\"resource1\" fill=\"black\" refX=\"10\" refY=\"5\" markerWidth=\"10\" markerHeight=\"10\" markerUnits=\"userSpaceOnUse\" orient=\"auto\">" +
                        "<path d=\"M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z\" stroke-width=\"2\" stroke=\"none\" fill=\"black\" />" +
                    "</marker>" +
                "</defs>" +
                "<polyline class=\"routedLine\" stroke-width=\"2\" stroke=\"black\" fill=\"none\" points=\"0,4 12,4 12,12 24,12\" marker-start=\"url(#resource0)\" marker-end=\"url(#resource1)\" />");

            var drawing = canvas.Drawings[0] as RoutedLine;
            Assert.IsNotNull(drawing);
            Assert.IsNotNull(drawing.MarkerEnd);
            Assert.AreEqual(drawing.MarkerEnd.Children.Count, 1);
            var endPath = drawing.MarkerEnd.Children[0] as NPath;
            Assert.IsNotNull(endPath);
            Assert.AreEqual(endPath.Data, "M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z");
            Assert.IsNotNull(drawing.MarkerStart);
            Assert.AreEqual(drawing.MarkerStart.Children.Count, 1);
            var startPath = drawing.MarkerStart.Children[0] as NPath;
            Assert.IsNotNull(startPath);
            Assert.AreEqual(startPath.Data, "M0.01,0 C0.03,0 10,5 10,5 9.96,5.03 0,10 0,10 0,10 0.48,8.86 1.07,7.49 L2.15,5 1.07,2.51 C0.48,1.14 0,0.01 0,0.01 0,0 0,2.06 0.01,0z");
        }

        [TestMethod]
        public void TestParseTextBox()
        {
            NCanvas canvas = ParseSVG("<text style=\"text-align:center;width:140;height:60;vertical-align:middle;label:Text;\" x=\"10\" y=\"10\" font-family=\"Tahoma\" font-size=\"13.333\" font-weight=\"bold\" baseline-shift=\"top\" />");

            var drawing = canvas.Drawings[0] as NTextBox;
            Assert.IsNotNull(drawing);
            Assert.AreEqual(drawing.X, 10);
            Assert.AreEqual(drawing.Y, 10);
            Assert.AreEqual(drawing.Width, 140);
            Assert.AreEqual(drawing.Height, 60);
            Assert.AreEqual(drawing.HorizontalAlignment, TextAlignment.Center);
            Assert.AreEqual(drawing.VerticalAlignment, VerticalAlignment.Center);
            //Assert.AreEqual(drawing.Label, "Text");
            Assert.AreEqual(drawing.GetProperty(TextProperties.FontSizeProperty), 13.333);
            Assert.AreEqual(drawing.GetProperty(TextProperties.FontFamilyProperty).ToString(), "Tahoma");
            Assert.AreEqual(drawing.GetProperty(TextProperties.FontWeightProperty), FontWeights.Bold);
            Assert.AreEqual(drawing.GetProperty(TextProperties.BaselineAlignmentProperty), BaselineAlignment.Top);

        }

        [TestMethod]
        public void TestFormatMultilineTextBox()
        {
            var textBox = new NTextBox
            {
                X = 675,
                Y = 160,
                Width = 114,
                Height = 65,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = TextAlignment.Center
            };

            textBox.Paragraph.Text = "The quick brown fox jumped over the lazy dog";

            string expected = MakeSVG(
                "<text fill=\"black\" x=\"675\" y=\"160\" style=\"padding:3px;width:114px;height:65px;vertical-align:middle;display:block;text-align:center;\">" +
                    "<tspan x=\"679\" y=\"179\" font-family=\"Calibri, Arial, Tahoma, Geneva, Helvetica, Sans-serif\" font-size=\"16\" font-weight=\"normal\" font-style=\"normal\" text-decoration=\"none\" baseline-shift=\"baseline\" fill=\"black\">The quick brown </tspan>" +
                    "<tspan x=\"680\" y=\"198\" font-family=\"Calibri, Arial, Tahoma, Geneva, Helvetica, Sans-serif\" font-size=\"16\" font-weight=\"normal\" font-style=\"normal\" text-decoration=\"none\" baseline-shift=\"baseline\" fill=\"black\">fox jumped over </tspan>" +
                    "<tspan x=\"693\" y=\"217\" font-family=\"Calibri, Arial, Tahoma, Geneva, Helvetica, Sans-serif\" font-size=\"16\" font-weight=\"normal\" font-style=\"normal\" text-decoration=\"none\" baseline-shift=\"baseline\" fill=\"black\">the lazy dog</tspan>" +
                "</text>");
            string actual = NCanvas.ToSVG(textBox);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseMultilineTextBox()
        {
            NCanvas canvas = ParseSVG(
                "<text x=\"675\" y=\"160\" fill=\"black\" style=\"width:114px;height:65px;vertical-align:middle;display:block;text-align:center;\">" +
                    "<tspan x=\"679\" y=\"183\" class=\"\" font-family=\"Calibri, Arial, Tahoma, Geneva, Helvetica, Sans-serif\" font-size=\"16\" font-weight=\"normal\" font-style=\"normal\" text-decoration=\"none\" baseline-shift=\"baseline\" fill=\"rgb(0,0,0)\" href=\"\">The quick brown </tspan>" +
                    "<tspan x=\"680\" y=\"202\" class=\"\" font-family=\"Calibri, Arial, Tahoma, Geneva, Helvetica, Sans-serif\" font-size=\"16\" font-weight=\"normal\" font-style=\"normal\" text-decoration=\"none\" baseline-shift=\"baseline\" fill=\"rgb(0,0,0)\" href=\"\">fox jumped over </tspan>" +
                    "<tspan x=\"693\" y=\"221\" class=\"\" font-family=\"Calibri, Arial, Tahoma, Geneva, Helvetica, Sans-serif\" font-size=\"16\" font-weight=\"normal\" font-style=\"normal\" text-decoration=\"none\" baseline-shift=\"baseline\" fill=\"rgb(0,0,0)\" href=\"\">the lazy dog</tspan>" +
                "</text>");

            Assert.AreEqual(1, canvas.Drawings.Count);
            var textBox = canvas.Drawings[0] as NTextBox;
            Assert.IsNotNull(textBox);
            Assert.AreEqual(675, textBox.X);
            Assert.AreEqual(160, textBox.Y);
            Assert.AreEqual(114, textBox.Width);
            Assert.AreEqual(65, textBox.Height);
            Assert.AreEqual(VerticalAlignment.Center, textBox.VerticalAlignment);
            Assert.AreEqual(TextAlignment.Center, textBox.HorizontalAlignment);
            Assert.AreEqual("The quick brown fox jumped over the lazy dog", textBox.Paragraph.Text);
        }

        [TestMethod]
        public void TestParseOvoid()
        {
            NCanvas canvas = ParseSVG(
                "<g class=\"locked\" fill=\"none\">" +
                "<path d=\"M22,10L38,10C42.2871856689454,10 46.2487106323242,12.8589832782745 48.3923034667968,17.5 50.5359001159668,22.1410155296326 50.5358963012696,27.8589844703675 48.3923034667968,32.5 46.2487106323242,37.1410179138182 42.2871856689454,40 38,40L22,40C17.7128129005432,40 13.7512879371643,37.1410155296325 11.6076946258545,32.5 9.4641016125679,27.8589844703675 9.46410197019577,22.1410155296326 11.6076952219009,17.4999988079071 13.7512891292572,12.8589820861817 17.7128129005432,10 22,10z\" stroke-width=\"2\" stroke=\"rgb(0,0,0)\" fill=\"none\" />" +
                "<line stroke-width=\"2\" stroke=\"rgb(0,0,0)\" fill=\"none\" x1=\"10\" y1=\"10\" x2=\"10\" y2=\"10\" />" +
                "<line stroke-width=\"2\" stroke=\"rgb(0,0,0)\" fill=\"none\" x1=\"50\" y1=\"10\" x2=\"50\" y2=\"10\" />" +
                "<line stroke-width=\"2\" stroke=\"rgb(0,0,0)\" fill=\"none\" x1=\"10\" y1=\"40\" x2=\"10\" y2=\"40\" />" +
                "<line stroke-width=\"2\" stroke=\"rgb(0,0,0)\" fill=\"none\" x1=\"50\" y1=\"40\" x2=\"50\" y2=\"40\" />" +
                "</g>");

            var drawing = canvas.Drawings[0] as ShapeGroup;
            Assert.IsNotNull(drawing);
            Assert.IsTrue(drawing.IsLocked);
            Assert.AreEqual(5, drawing.Drawings.Count);
            var path = drawing.Drawings[0] as NPath;
            Assert.IsNotNull(path);
            var line = drawing.Drawings[1] as Line;
            Assert.IsNotNull(line);
            line = drawing.Drawings[2] as Line;
            Assert.IsNotNull(line);
            line = drawing.Drawings[3] as Line;
            Assert.IsNotNull(line);
            line = drawing.Drawings[4] as Line;
            Assert.IsNotNull(line);
        }


        /*
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], expected.Substring(i, 15) + " != " + actual.Substring(i, 15));
            }
            */
    }
}
