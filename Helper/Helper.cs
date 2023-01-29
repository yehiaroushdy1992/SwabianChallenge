using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SwabianChallenge.Helper
{
    public static class Helper
    {

        //Builds a string representation of the data points in the format [x,y]
        public static StringBuilder BuildDataString(List<double> x, List<double> y)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (int num in x)
            {
                text.Append("[" + num + "," + y[i] + "]");
                text.Append(" ");
                i++;
            }
            return text;
        }


        //Builds an array of doubles from a list of doubles
        public static double[] BuildArray(List<double> points)
        {
            double[] array = new double[points.Count];
            int i = 0;
            foreach (var num in points)
            {
                array[i] = num;
                i++;
            }
            return array;
        }

        //Builds a LineSeries of fitted data points
        public static LineSeries BuildFittingPoints(List<double> x, double a, double b, string Mode)
        {
            var LineSeries = new LineSeries();
            int i = 0;
            bool Linear = false;
            bool PowerFunction = false;
            bool Exponential = false;
            switch (Mode)
            {
                case "Linear":
                    Linear = true;
                    break;
                case "PowerFunction":
                    PowerFunction = true;
                    break;
                case "Exponential":
                    Exponential = true;
                    break;
                default:
                    break;
            }

            //calculates y values based on the fitting equation , create Data Point and then add it to Line Series
            foreach (var num in x)
            {
                double y = 0;
                if (Linear)
                    y = (a * x[i]) + b;
                else if (Exponential)
                    y = a * Math.Exp(b * num);
                else if (PowerFunction)
                    y = a * Math.Pow(num, b);
                LineSeries.Points.Add(new DataPoint(x[i], y));
                i++;
            }
            return LineSeries;
        }


        //Builds a list of PointAnnotations for the data points
        public static List<PointAnnotation> BuildAnnotations(List<double> x, List<double> y)
        {
            int i = 0;
            List<PointAnnotation> list = new List<PointAnnotation>();
            foreach (var point in x)
            {
                var pointAnnotation = new PointAnnotation
                {
                    X = point,
                    Y = y[i],
                    Text = point + "," + y[i],
                    FontSize = 8,
                    FontWeight = OxyPlot.FontWeights.Bold,
                    Shape = MarkerType.Diamond
                };
                i++;
                list.Add(pointAnnotation);
            }
            return list;
        }

        //Builds a string representation of fitted data points 
        public static StringBuilder BuildFittedDataPointsLabel(LineSeries LineSeries)
        {
            int i = 0;
            StringBuilder text = new StringBuilder();
            text.Append("Fitted Data Points : ");
            text.AppendLine();
            foreach (var LineSerie in LineSeries.Points)
            {
                text.Append("[" + LineSerie.X + "," + LineSerie.Y + "]");
                text.Append(" ");
                i++;
                if (i % 3 == 0)
                    text.AppendLine();
            }
            return text;
        }

        //Update PlotView properties for rendering
        public static PlotView UpdatePlotViewProperties(PlotView PlotView)
        {
            PlotView.ActualController.UnbindMouseWheel();
            PlotView.ResetAllAxes();
            PlotView.Model.ResetAllAxes();
            PlotView.UpdateLayout();
            return PlotView;
        }
    }
}
