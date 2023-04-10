using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlgorithmGUI
{
    public class Program
    {
        //Can be changed
        public static int SLength = 11, maxRange = 20, repeatForXTimes = 1000;
        //public static int SMidpoint = (int)Math.Ceiling((double)SLength / 2);

        //Program start/main menu
        [STAThread]
        public static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.Write("Choose an option!" +
                    "\nA) Run all Algorithms sequentially" +
                    "\nB) Run a single Algorithms once" +
                    $"\nC) Run a single Algorithm ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(repeatForXTimes);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" times.\n");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"D) Run all Algorithms {repeatForXTimes} times");
                Console.ForegroundColor = ConsoleColor.Gray;  
                Console.WriteLine("E) Change Parameters" +"\n>>> ");
                char input = Console.ReadKey().KeyChar;
                Console.Clear();

                int question;
                switch (input)
                {
                    case 'a':
                    case '1':
                        for (int i = 1; i < 10; i++)
                        {
                            AnswerCollection(i, false);
                        }
                        break;

                    case 'b':
                    case '2':
                        question = ChooseQuestion();
                        AnswerCollection(question, false);
                        break;

                    case 'c':
                    case '3':
                        question = ChooseQuestion();
                        AnswerCollection(question, true);

                        break;

                    case 'd':
                    case '4':
                        AnswerCollection(-1, true);
                        break;

                    case 'e':
                    case '5':
                        Console.WriteLine("Select an option to modify." +
                            $"\nA) Length of Starting Array: {SLength}" +
                            $"\nB) Highest value of starting array: {maxRange}" +
                            $"\nC) Number of times to repeat a question: {repeatForXTimes}");
                        Console.Write(">>> ");
                        char modifyChoice = Console.ReadKey().KeyChar;

                        if (modifyChoice is 'a' or '1' or 'b' or '2' or 'c' or '3')
                        {
                            Console.WriteLine("\nEnter a number.");
                            Console.Write(">>> ");
                            int modifyValue = Convert.ToInt32(Console.ReadLine());

                            switch (modifyChoice)
                            {
                                case 'a':
                                case '1':
                                    SLength = modifyValue;
                                    break;
                                case 'b':
                                case '2':
                                    maxRange = modifyValue;
                                    break;
                                case 'c':
                                case '3':
                                    repeatForXTimes = modifyValue;
                                    break;
                            }
                        }
                        Console.Clear();
                        break;
                }
            }
        }

        public static int ChooseQuestion()
        {
            while (true)
            {
                Console.WriteLine("What Question would you like to run?\nPress ESC to return to the main menu.");
                Console.Write("(A...I) >>> ");
                char question = Console.ReadKey().KeyChar;
                Console.Clear();
                if (question == (char)ConsoleKey.Escape)
                {
                    return -1;
                }
                else
                {
                    int x = char.ToUpper(question) - 64;
                    if (x >= 1 && x <= 9)
                    {
                        return x;
                    }
                }
            }
        }

        public static void AnswerCollection(int questionNum, bool isRepeated)
        {

            if (isRepeated)
            {
                //store every deviation per x loops
                double[] deviaitonsFromTarget = new double[repeatForXTimes];
                double[] runTimeLengths = new double[repeatForXTimes];

                //Detects if looping through every question or just 1
                if (questionNum == -1)
                {
                    isRepeatingALot = true;
                    //9 questions, 8 iterations, 3 values(run times, averages, standard deviation)
                    double[,,] graphResults = new double[9, 8, 3];

                    for (int question = 1; question < 10; question++)
                    {
                        //reset S size
                        increasingSSize = 4;
                        Console.WriteLine($"Starting Question {(char)(question + 64)}...");

                        for (int arraySizeIncrement = 0; arraySizeIncrement < 8; arraySizeIncrement++)
                        {
                            increaseSSize = true;

                            for (int thousandLoop = 0; thousandLoop < 1000; thousandLoop++)
                            {
                                double[] results = AnswerBench(question);
                                //Save the data to an array for this iteration
                                deviaitonsFromTarget[thousandLoop] = results[0];
                                runTimeLengths[thousandLoop] = results[1];
                            }

                            //Save the data for this array size in the deviations array
                            graphResults[question - 1, arraySizeIncrement, 0] = runTimeLengths.Average();
                            graphResults[question - 1, arraySizeIncrement, 1] = deviaitonsFromTarget.Average();
                            graphResults[question - 1, arraySizeIncrement, 2] = CalcStandardDeviation(deviaitonsFromTarget);

                            //Empty the arrays for the next cycle
                            Array.Clear(deviaitonsFromTarget);
                            Array.Clear(runTimeLengths);
                        }
                        Console.Write("Done.\n");
                    }

                    //Display the results!
                    Thread thread = new(() =>
                    {
                        MainWindow mainWindow = new(graphResults);
                        mainWindow.Show();
                        System.Windows.Threading.Dispatcher.Run();
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
                else
                {
                    for (int loopNum = 0; loopNum < repeatForXTimes; loopNum++)
                    {
                        double[] results = AnswerBench(questionNum);
                        deviaitonsFromTarget[loopNum] = results[0];
                        runTimeLengths[loopNum] = results[1];
                    }
                    RepeadedReport1Question(questionNum, deviaitonsFromTarget, runTimeLengths);
                }
            }
            else
            {
                double[] results = AnswerBench(questionNum);
                RepeadedReport1Question(questionNum, new double[] { results[0] }, new double[] { results[1] });
            }
        }

        public static double[] AnswerBench(int questionNum)
        {
            //refresh arrays and data
            //A dictionary would allow for less work operations on each list buuuut I don't know how to use them!

            int[] S = GenerateRandomS();
            double targetDeviation = S.Sum() / 2.0;
            List<int> S1 = new();
            List<int> S2 = new();

            //Run the algorithm
            Stopwatch timer = Stopwatch.StartNew();
            OpenQuestion(questionNum, S, S1, S2);
            timer.Stop();

            double[] result = new double[2];

            //Save the data, unless S1 and S2 are empty.
            if (!S1.Any() && !S2.Any())
            {
                result[0] = 0;
                result[1] = 0;
            }
            else
            {
                result[0] = CalcDeviation(targetDeviation, S1.Sum(), S2.Sum());
                result[1] = timer.Elapsed.TotalMilliseconds;
            }
            return result;
        }

        public static void OpenQuestion(int questionNum, int[] S, List<int> S1, List<int> S2)
        {
            switch (questionNum)
            {
                case 1:
                    QuestionA(S, S1, S2);
                    break;
                case 2:
                    QuestionB(S, S1, S2);
                    break;
                case 3:
                    QuestionC(S, S1, S2);
                    break;
                case 4:
                    QuestionD(S, S1, S2);
                    break;
                case 5:
                    QuestionE(S, S1, S2);
                    break;
                case 6:
                    QuestionF(S, S1, S2);
                    break;
                case 7:
                    QuestionG(S, S1, S2);
                    break;
                case 8:
                    QuestionH(S, S1, S2);
                    break;
                case 9:
                    QuestionI(S, S1, S2);
                    break;
            }
        }

        //QUESTIONS

        public static void QuestionA(int[] S, List<int> S1, List<int> S2)
        {
            int SMidpoint = S.Length / 2;
            //Adds the range of values up to and after the midpoint respectively
            S1.AddRange(S.Take(SMidpoint));
            S2.AddRange(S.Skip(SMidpoint));
        }
        public static void QuestionB(int[] S, List<int> S1, List<int> S2)
        {
            for (int i = 0; i < S.Length; i++)
            {
                //if i divided by 2 is 0, add to S1
                (i % 2 == 0 ? S1 : S2).Add(S[i]);
            }
        }
        public static void QuestionC(int[] S, List<int> S1, List<int> S2)
        {
            foreach (int element in S)
            {
                if (element % 2 == 0)
                {
                    S1.Add(element);
                }
                else
                {
                    S2.Add(element);
                }
            }
        }
        public static void QuestionD(int[] S, List<int> S1, List<int> S2)
        {
            //Adds the first and second values to S1/ 2
            S1.Add(S[0]);
            int S1Sum = S[0];
            S2.Add(S[1]);
            int S2Sum = S[1];

            for (int i = 2; i < S.Length; i++)
            {
                if (S1Sum < S2Sum)
                {
                    S1.Add(S[i]);
                    S1Sum += S[i];
                }
                else
                {
                    S2.Add(S[i]);
                    S2Sum += S[i];
                }
            }
        }
        public static void QuestionE(int[] S, List<int> S1, List<int> S2)
        {
            //Sort ascending
            Array.Sort(S);
            for (int i = 0; i < S.Length; i++)
            {
                if (i % 2 == 0)
                {
                    S1.Add(S[i]);
                }
                else
                {
                    S2.Add(S[i]);
                }
            }
        }
        public static void QuestionF(int[] S, List<int> S1, List<int> S2)
        {
            //Sort ascending
            Array.Sort(S);

            //Adds the first and second values to S1/2
            S1.Add(S[0]);
            int S1Sum = S[0];
            S2.Add(S[1]);
            int S2Sum = S[1];

            for (int i = 2; i < S.Length; i++)
            {
                if (S1Sum < S2Sum)
                {
                    S1.Add(S[i]);
                    S1Sum += S[i];
                }
                else
                {
                    S2.Add(S[i]);
                    S2Sum += S[i];
                }
            }
        }
        public static void QuestionG(int[] S, List<int> S1, List<int> S2)
        {
            //Sort S by descending
            Array.Sort(S, (x, y) => y.CompareTo(x));

            //Same as D and F
            S1.Add(S[0]);
            int S1Sum = S[0];
            S2.Add(S[1]);
            int S2Sum = S[1];

            for (int i = 2; i < S.Length; i++)
            {
                if (S1Sum < S2Sum)
                {
                    S1.Add(S[i]);
                    S1Sum += S[i];
                }
                else
                {
                    S2.Add(S[i]);
                    S2Sum += S[i];
                }
            }
        }
        public static void QuestionH(int[] S, List<int> S1, List<int> S2)
        {
            SLength = S.Length;
            double idealDeviation = (double)S.Sum() / 2;
            double smallestDeviation = double.MaxValue;

            //For the sake of not being here all day.
            //if (S.Length > 256) { Console.WriteLine($"🗿 Array size of {SLength} is too large, skipping..."); return; }

            //Runs the for loop in parallel while locking access to S1 sequentially.
            Parallel.For(0, SLength, i =>
            {
                for (int j = 0; j < SLength; j++)
                {
                    List<int> tempS1 = new();
                    List<int> tempS2 = new();

                    for (int k = 0; k < SLength; k++)
                    {
                        //if this loop is within the bounds of the 2 parent loops
                        if (k <= j && k >= i)
                        {
                            //Add to S1
                            tempS1.Add(S[k]);
                        }
                        else
                        {
                            //Add all other values to S2 if they aren't in range of the loop we're currently in.
                            tempS2.Add(S[k]);
                        }
                    }

                    //calc the deviation
                    double iterationDeviation = Math.Abs(tempS1.Sum() - idealDeviation) + Math.Abs(tempS2.Sum() - idealDeviation);

                    //Forces the threads to synchronise before checking the deviation
                    lock (S1)
                    {
                        //update the current smallest combo if best
                        if (iterationDeviation < smallestDeviation)
                        {
                            smallestDeviation = iterationDeviation;
                            S1.Clear();
                            S1.AddRange(tempS1);
                            S2.Clear();
                            S2.AddRange(tempS2);

                            //exit if the value is perfect
                            if (smallestDeviation == 0) { return; }
                        }
                    }
                }
            });

        }
        public static void QuestionI(int[] S, List<int> S1, List<int> S2)
        {
            int SSum = S.Sum();
            int S1Sum = 0, S2Sum = 0;
            double targetSDeviation = (double)S.Sum() / 2;

            //Adds items from S (ordered) to the 2 sublists
            foreach (var currentSItem in S.OrderByDescending(x => x))
            {
                if (S1Sum <= S2Sum)
                {
                    S1.Add(currentSItem);
                    S1Sum += currentSItem;
                }
                else
                {
                    S2.Add(currentSItem);
                    S2Sum += currentSItem;
                }
            }

            //If the sums of S1 and S2 already match, all is well!
            if (S1Sum != S2Sum)
            {
                //Swaps S1 and S2 so that S2 is largest
                if (S1Sum > S2Sum)
                {
                    //Switch list contents
                    (S1, S2) = (S2, S1);

                    //Switch sums (very important)
                    (S1Sum, S2Sum) = (S2Sum, S1Sum);
                }

                //Tracks the smallest possible deviation from the ideal
                double closestDeviation = Math.Abs(targetSDeviation - S1Sum);
                //if the current item in S1 can be swapped with an item in S2 to improve the sum, do it!
                for (int x = 0; x < S1.Count; x++)
                {
                    int currentS1Item = S1[x];
                    for (int y = 0; y < S2.Count; y++)
                    {
                        int currentS2Item = S2[y];
                        //To determine if we're swapping, need to first figure out if it's beneficial or not.
                        int tempS1Sum = S1Sum - currentS1Item + currentS2Item;
                        int tempS2Sum = S2Sum - currentS2Item + currentS1Item;
                        double tempDeviation = Math.Abs(targetSDeviation - tempS1Sum);

                        //If the deviation from the target is smaller with this new pairing, swap
                        if (tempDeviation < closestDeviation)
                        {
                            S1.Remove(currentS1Item);
                            S2.Remove(currentS2Item);

                            S1.Add(currentS2Item);
                            S2.Add(currentS1Item);

                            S1Sum = tempS1Sum;
                            S2Sum = tempS2Sum;

                            //Updates the closest deviaiton
                            closestDeviation = tempDeviation;
                        }
                    }
                }
            }
        }


        //REPORTS
        public static void RepeadedReport1Question(int questionNum, double[] deviaitonsFromTarget, double[] runTimeLengths)
        {
            Console.WriteLine($"Successfully ran Question {(char)(questionNum + 64)}" +
                $"\nAverage Runtime: {runTimeLengths.Average()} ms" +
                $"\nAverage Deviation: {deviaitonsFromTarget.Average()}");

            Console.Write("\nContinue?\n>>> ");
            Console.ReadLine();
            Console.Clear();
        }

        //FUNCTIONS

        //Calculate the deviation from the 2 sums
        public static double CalcDeviation(double target, int S1Sum, int S2Sum)
        {
            double s1Deviation = Math.Abs(S1Sum - target);
            double s2Deviation = Math.Abs(S2Sum - target);
            return (s1Deviation + s2Deviation) / 2;
        }

        public static double CalcStandardDeviation(double[] array)
        {
            double avg = array.Average();
            return Math.Sqrt(array.Average(v => Math.Pow(v - avg, 2)));
        }

        //Generates a random list S
        private static int increasingSSize = 4;
        private static bool isRepeatingALot = false;
        private static bool increaseSSize = false;

        public static int[] GenerateRandomS()
        {
            if (increaseSSize)
            {
                increaseSSize = false;
                increasingSSize *= 2;
                return RandomNumArray(increasingSSize, maxRange);
            }
            else if (isRepeatingALot)
            {
                return RandomNumArray(increasingSSize, maxRange);
            }
            else
            {
                return RandomNumArray(SLength, maxRange);
            }
        }

        //Returns the contents of the array
        public static string PrintArray(int[] array)
        {
            int arrayLength = array.Count();

            if (SLength.Equals(0))
            {
                return "No data";
            }

            StringBuilder sb = new();
            for (int i = 0; i < arrayLength; i++)
            {
                if (i.Equals(array.Length - 1))
                {
                    sb.Append(array[i]);
                }
                else
                {
                    sb.Append(array[i] + ",");
                }
            }

            return sb.ToString();
        }

        //Generates a semi-random array of numbers
        public static int[] RandomNumArray(int length, int maxRange)
        {
            List<int> list = new();
            Random random = new();

            for (int i = 0; i < length; i++)
            {
                list.Add(random.Next(1, maxRange));
            }

            return list.ToArray();
        }
    }
}


