using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;
using OxyPlot.Series;
using OxyPlot.Annotations;
using OxyPlot.Legends;
using OxyPlot.Wpf;

namespace SwabianChallenge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        List<double> x;
        List<double> y;

        public MainWindow()
        {
            InitializeComponent();

            //Disabling checkboxes , Clear Button , Hiding Labels , Making Window Maximized on Loading the Window
            this.WindowState = WindowState.Maximized;
            this.Label.Visibility = Visibility.Hidden;
            this.Linear.IsEnabled = false;
            this.Exponential.IsEnabled = false;
            this.PowerFunction.IsEnabled = false;
            this.CoefficientsLabel.Visibility = Visibility.Hidden;
            this.FittedDataPoints.Visibility = Visibility.Hidden;
        }

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            x = new List<double>();
            y = new List<double>();


            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                //Reading input file and adding input to X List and Y List
                using (StreamReader file = new StreamReader(openFileDialog.FileName))
                {
                    string ln;
                    while ((ln = file.ReadLine()) != null)
                    {
                        string[] temp = ln.Split(' ');
                        /* 
                         * Fetching the input and trying to convert it to double to make sure 
                         * its in the correct format
                        */
                        try
                        {
                            x.Add(Convert.ToDouble(temp[0]));
                            y.Add(Convert.ToDouble(temp[1]));
                        }
                        catch
                        {
                            MessageBox.Show("Invalid input , Please review your input file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    file.Close();
                }

                //Checking Input Count if any equal to zero , or  X and Y count isnt the same 
                if (x.Count != y.Count || x.Count == 0 || y.Count == 0)
                {
                    MessageBox.Show("Incosistent input , Please review your input file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                x = x.OrderBy(x => x).ToList();
                y = y.OrderBy(y => y).ToList();

                //Formatting the input so it shows on the textbox in a proper way before proceeding
                if (x.Count > 0 && y.Count > 0)
                {
                    Label.Visibility = Visibility.Visible;
                    Label.Content = "Loaded Data Points : " + Helper.Helper.BuildDataString(x, y).ToString();

                    /* 
                     * Enabling checkboxes to enable user interaction.
                     * Also , showing the graph in case thats another
                     * load after clearing previous data 
                     */
                    this.Linear.IsEnabled = true;
                    this.PowerFunction.IsEnabled = true;
                    this.Exponential.IsEnabled = true;
                    this.PlotView.Visibility = Visibility.Visible;
                }
            }
        }

        private void Linear_Checked(object sender, RoutedEventArgs e)
        {
            //Disabling other checkboxes if any is checked
            if (Linear.IsChecked == true && (Exponential.IsChecked == true || PowerFunction.IsChecked == true))
            {
                PowerFunction.IsChecked = false;
                Exponential.IsChecked = false;
            }
            CurveFittingImplementation("Linear");
        }

        private void Exponential_Checked(object sender, RoutedEventArgs e)
        {
            //Disabling other checkboxes if any is checked
            if (Exponential.IsChecked == true && (Linear.IsChecked == true || PowerFunction.IsChecked == true))
            {
                PowerFunction.IsChecked = false;
                Linear.IsChecked = false;
            }
            CurveFittingImplementation("Exponential");
        }

        private void PowerFunction_Checked(object sender, RoutedEventArgs e)
        {
            //Disabling other checkboxes if any is checked
            if (PowerFunction.IsChecked == true && (Linear.IsChecked == true || Exponential.IsChecked == true))
            {
                Linear.IsChecked = false;
                Exponential.IsChecked = false;
            }
            CurveFittingImplementation("PowerFunction");
        }

        public void CurveFittingImplementation(string Mode)
        {
            //Determining the current mode selected
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

            /*Preparing Plot Model title with different colors based on mode selected
             * Linear : Color is Red
             * Exponential : Color is Blue
             * Power Function : Color is Green
             */

            PlotModel pm = new PlotModel();
            if (Linear)
            {
                pm.Title = "Linear Curve Fitting";
                pm.TitleColor = OxyColor.FromRgb(255, 0, 0);
            }
            else if (PowerFunction)
            {
                pm.Title = "Power Function Curve Fitting";
                pm.TitleColor = OxyColor.FromRgb(0, 255, 0);
            }
            else if (Exponential)
            {
                pm.Title = "Exponential Curve Fitting";
                pm.TitleColor = OxyColor.FromRgb(0, 0, 255);
            }

            //Building x and y arrays
            double[] xdata = Helper.Helper.BuildArray(x);
            double[] ydata = Helper.Helper.BuildArray(y);

            double a = 0;
            double b = 0;

            //Start data fiting based on mode selected to fetch fitted coefficients
            if (Linear)
            {
                var p = Fit.Line(xdata, ydata);
                a = p.Item1;
                b = p.Item2;
            }
            else if (Exponential)
            {
                var p = Fit.Exponential(xdata, ydata);
                a = p.Item1;
                b = p.Item2;
            }
            else if (PowerFunction)
            {
                var p = Fit.Power(xdata, ydata);
                a = p.Item1;
                b = p.Item2;
            }

            /* Build fitting points with the loaded data and calculated fitted coefficients 
             * Also , the created line graph will have different colors based on mode selected 
             * same as previously stated
             */
            var lineSeries = Helper.Helper.BuildFittingPoints(x, a, b, Mode);
            if (Linear)
                lineSeries.Color = OxyColor.FromRgb(255, 0, 0);
            else if (Exponential)
                lineSeries.Color = OxyColor.FromRgb(0, 0, 255);
            else if (PowerFunction)
                lineSeries.Color = OxyColor.FromRgb(0, 255, 0);

            // Build annotations for loaded data points , to add them to the graph
            var Annotations = Helper.Helper.BuildAnnotations(x, y);
            foreach (var annotation in Annotations)
            {
                pm.Annotations.Add(annotation);
            }

            pm.Series.Add(lineSeries);

            /* Building string representation for Coefficient label 
             * and fitted data points , also show labels 
             */
            this.CoefficientsLabel.Content = "Coefficients : " + Environment.NewLine + " Intercept : " + a + " [a] " + Environment.NewLine + " Slope : " + b + " [b] ";
            this.FittedDataPoints.Content = Helper.Helper.BuildFittedDataPointsLabel(lineSeries);
            this.FittedDataPoints.Visibility = Visibility.Visible;
            this.CoefficientsLabel.Visibility = Visibility.Visible;

            //Modify Graph Height so it shows data in a proper way
            PlotView.Height = this.Window.ActualHeight;
            PlotView.Model = pm;
            PlotView = Helper.Helper.UpdatePlotViewProperties(PlotView);
        }


        //Clearing loaded data , hide graph , un-checking fitting modes (if selected) and also disabling them
        private void ClearData_Click(object sender, RoutedEventArgs e)
        {
            if (x.Count > 0 && y.Count > 0 && x.Count == y.Count)
            {
                this.Label.Content = "Loaded data cleared";
                this.Linear.IsChecked = false;
                this.Exponential.IsChecked = false;
                this.PowerFunction.IsChecked = false;
                this.Linear.IsEnabled = false;
                this.Exponential.IsEnabled = false;
                this.PowerFunction.IsEnabled = false;
                this.CoefficientsLabel.Content = "";
                this.CoefficientsLabel.Visibility = Visibility.Hidden;
                this.FittedDataPoints.Content = "";
                this.FittedDataPoints.Visibility = Visibility.Hidden;
                this.PlotView.Visibility = Visibility.Hidden;
            }
        }
    }
}
