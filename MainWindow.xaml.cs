using OxyPlot.Series;
using OxyPlot;
using System.Windows;
using OxyPlot.Axes;
using OxyPlot.Legends;
using System;
using System.Text;
using System.Data;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;

namespace AlgorithmGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string dataString;
        //9 questions, 8 iterations, 3 values (run times, averages, standard deviation)
        public MainWindow(double[,,] array)
        {
            InitializeComponent();
            RenderOptions.ProcessRenderMode = RenderMode.Default;
            populateRichTextBox(this);
            //Plots the graphs
            PlotGraphs(array, this);
            //Loads the data to a csv format
            dataString = dataToCSV(array);
            //Polulates the datagrid with the csv format data
            makeDataTable(this);
        }

        private static void populateRichTextBox(MainWindow mainWindow)
        {
            mainWindow.TextBoxInfo.Text = "Graph Controls!\nMouse Wheel: Zoom\nRight Click: Move\nHide Elements: Click the Question Number at the bottom!\n\nQ: Graph showing an error?\nA: Just rerun D)!\n\nQ: Why is Algorithm H not running completely?\nA: Because it would take way too long otherwise, trust me.\n\nQ: Why are there 2 entries for 1 algorithm in the legend?\nA: So you can choose to hide the points or the line. This was the only way I was able to get a line with error bars.\n\nMade by Nove for Algorithm Analysis and Data Structures, 2023";
        }

        private static void PlotGraphs(double[,,] array, MainWindow mainWindow)
        {
            //Plot the graphs
            var devGraph = new PlotModel();
            var timeGraph = new PlotModel();

            int[] sizes = new int[] { 8, 16, 32, 64, 128, 256, 512, 1024 };

            //add the axis labels
            devGraph.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Cardinality of S" });
            devGraph.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Average Deviation from ideal" });
            timeGraph.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Cardinality of S" });
            timeGraph.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Average Execution time (ms)" });

            //add a legend
            var devLegend = new Legend { LegendTitle = "Question Number", LegendPosition = LegendPosition.BottomCenter, LegendPlacement = LegendPlacement.Outside, LegendOrientation = LegendOrientation.Horizontal };
            var timeLegend = new Legend { LegendPosition = LegendPosition.BottomCenter, LegendPlacement = LegendPlacement.Outside, LegendOrientation = LegendOrientation.Horizontal };

            devGraph.Legends.Add(devLegend);
            timeGraph.Legends.Add(timeLegend);

            //Synchronises the colours
            var customColors = new[]
            {
                OxyColors.Red,
                OxyColors.Orange,
                OxyColors.Yellow,
                OxyColors.LightGreen,
                OxyColors.Green,
                OxyColors.LightBlue,
                OxyColors.Blue,
                OxyColors.DarkBlue,
                OxyColors.Purple
            };

            //populate the data into the graph

            for (int question = 1; question < 10; question++)
            {
                var devScatterPoints = new ScatterErrorSeries { Title = $"Question {(char)(question + 64)}", DataFieldX = "Array Length", DataFieldY = "Deviation from ideal", DataFieldErrorY = "Standard Deviation" };
                var devLine = new LineSeries { Title = $"Question {(char)(question + 64)}", DataFieldX = "Array Length", DataFieldY = "Deviation from ideal" };
                var timeSeries = new LineSeries { Title = $"Question {(char)(question + 64)}", DataFieldX = "Array Length", DataFieldY = "Runtime (ms)" };

                //Sets the colours
                devLine.Color = customColors[question - 1];
                devScatterPoints.MarkerFill = customColors[question - 1];
                timeSeries.Color = customColors[question - 1];

                for (int arrayLength = 0; arrayLength < 8; arrayLength++)
                {
                    double devAverageValue = array[question - 1, arrayLength, 1];
                    double timeAverageValue = array[question - 1, arrayLength, 0];

                    devLine.Points.Add(new DataPoint(sizes[arrayLength], devAverageValue));
                    devScatterPoints.Points.Add(new ScatterErrorPoint(sizes[arrayLength], devAverageValue, 0, array[question - 1, arrayLength, 2]));
                    timeSeries.Points.Add(new DataPoint(sizes[arrayLength], timeAverageValue));
                }

                //Add the lines to the graph models
                devGraph.Series.Add(devLine);
                devGraph.Series.Add(devScatterPoints);
                timeGraph.Series.Add(timeSeries);
            }

            //assign the data to the graph views
            mainWindow.deviationGraph.Model = devGraph;
            mainWindow.runtimeGraph.Model = timeGraph;
        }

        private static void makeDataTable(MainWindow mainWindow)
        {
            string[] rows = dataString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] columns = rows[0].Split(new char[] { ',' });

            DataTable rawInfo = new();

            // Add the columns to the DataTable
            foreach (string column in columns)
            {
                rawInfo.Columns.Add(column);
            }

            // Add the rows to the DataTable
            for (int i = 1; i < rows.Length; i++)
            {
                string[] rowValues = rows[i].Split(new char[] { ',' });
                DataRow dataRow = rawInfo.NewRow();
                for (int j = 0; j < rowValues.Length; j++)
                {
                    dataRow[j] = rowValues[j];
                }
                rawInfo.Rows.Add(dataRow);
            }

            // Bind the DataTable to the DataGrid
            rawInfo.DefaultView.Sort = "Question ASC";
            mainWindow.Data.ItemsSource = rawInfo.DefaultView;
            
        }

        public static string dataToCSV(double[,,] array)
        {
            StringBuilder csvBuilder = new();
            string[] types = { "Runtime (ms)", "Average", "S.D" };

            csvBuilder.Append("Question,Type,8,16,32,64,128,256,512,1024\n");

            Parallel.For(1, 10, question =>
            {
                for (int rowNum = 0; rowNum < 3; rowNum++)
                {
                    var row = new StringBuilder();
                    row.Append((char)(question + 64) + "," + types[rowNum]);

                    for (int arraySize = 0; arraySize < 8; arraySize++)
                    {
                        row.Append("," + array[question - 1, arraySize, rowNum]);
                    }

                    row.Append('\n');
                    lock (csvBuilder) // prevent multiple threads from writing to the CSV string at the same time
                    {
                        csvBuilder.Append(row);
                    }
                }
            });

            return csvBuilder.ToString();


        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            // Create a SaveFileDialog object
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "Comma Separated File (*.csv)|*.csv|Text File (*.txt)|*.txt|All files|*.*";
            saveFileDialog.Title = "Save as a CSV file!";

            // Show the dialog and get the result
            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                // Get the selected file name and path
                string filePath = saveFileDialog.FileName;

                // Writes the data to the file, meaning it only needs to be processed once.
                File.WriteAllText(filePath, dataString);
            }

        }

        private void Data_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {

        }
    }
}
