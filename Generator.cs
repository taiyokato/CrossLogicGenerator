#undef DEBUG

using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossLogicGenerator
{
    public class Solver
    {
        #region Def
        public List<int[]> Vertical_Input;
        public List<int[]> Horizontal_Input;

        /// <summary>
        /// Main Grid
        /// </summary>
        public static int[][] Grid;
        public static int[][] BackupGrid;
        public static int FullGridWidth = 10;
        /// <summary>
        /// Total number of blocks in the grid
        /// </summary>
        public int FullGridCount
        {
            get { return FullGridWidth * FullGridWidth; }
        }
        #endregion
        public Solver(bool fileread = false)
        {
            if (fileread)
            {
                Reader.ProcessRaw();
                FullGridWidth = Reader.GridSize;
                Grid = Reader.Grid; //copy value
                Solve();
                return;
            }
#if (false)
            

        //Get the size first, then initialize Grid
            Console.WriteLine("Size is limited to 10x10");


            Console.WriteLine("Input Each line\nEnter empty values as x\nSeparate values with a space: ");
            Console.WriteLine("Input \"resize\" to re-set grid size");
            //Console.WriteLine("Input \"redo\" to re-enter line");
            if (ReadLines()) { Console.Clear(); goto SETSIZE; } //if ReadLines return true, reset grid size
            Console.WriteLine();
            Console.WriteLine("Input values are: ");
            PrintAll();
            Console.Read();

            Solve(skipprint);
            
#endif
            Solve();


        }
        public void Solve()
        {
            bool hidecross = true;
            bool usecolor = true;

            Horizontal_Input = new List<int[]>();
            Vertical_Input = new List<int[]>();

            Console.WriteLine("Input:");
            PrintAll(false);

            CreateHeaders();
            HorizontalLabelSize = GetMaxCharCount(Axis.HORIZONTAL);
            VerticalLabelSize = GetMaxCharCount(Axis.VERTICAL);
            PrintAll();
            ClearGrid();
            PrintAll();

            Console.Read();
        }

        #region Grid
        public static bool GridSame(ref int[][] main, ref int[][] backup)
        {
            for (int i = 0; i < FullGridWidth; i++)
            {
                for (int ii = 0; ii < FullGridWidth; ii++)
                {
                    if (main[i][ii] != backup[i][ii])
                        return false;
                }
            }
            return true;
        }
        public static void CopyGrid(int[][] main, ref int[][] backup)
        {
            for (int i = 0; i < FullGridWidth; i++)
            {
                for (int ii = 0; ii < FullGridWidth; ii++)
                {
                    backup[i][ii] = main[i][ii];
                }
            }
        }
        #endregion
        
        /// <summary>
        /// Create values
        /// </summary>
        public void CreateHeaders()
        {
            //vertical
            //horizontal
            for (int i = 0; i < FullGridWidth; i++)
            {
                Vertical_Input.Add(AdjacentFilledBlockVertical(i));
                Horizontal_Input.Add(AdjacentFilledBlockHorizontal(i));
            }
            
        }

        public void ClearGrid()
        {
            for (int i = 0; i < FullGridWidth; i++)
            {
                for (int ii = 0; ii < FullGridWidth; ii++)
                {
                    Grid[i][ii] = 0;
                }
            }
        }



        public void Advanced()
        {
            int[] tmp;

            //Horizontal
            for (int y = 0; y < FullGridWidth; y++)
            {
                tmp = AdvGetOverLappingArea3(Grid[y], Horizontal_Input[y]);
                if (tmp == null) continue;
                for (int i = 0; i < FullGridWidth; i++)
                {
                    //if (tmp[i] != 0) System.Diagnostics.Debugger.Break();
                    Grid[y][i] = tmp[i];
                }
#if (DEBUG)
                    PrintAll();
#endif
            }
            

            //Vertical
            for (int x = 0; x < FullGridWidth; x++)
            {
                tmp = AdvGetOverLappingArea3(GetVerticalGrid(x), Vertical_Input[x]);
                if (tmp == null) continue;
                for (int i = 0; i < FullGridWidth; i++)
                {
                    Grid[i][x] = tmp[i];
                }
#if (DEBUG)
                    PrintAll();
#endif
            }
        }
        public void Advanced(int hori, int verti)
        {
            int[] tmp;
            if (hori == 1)
            {
                //Horizontal
                for (int y = 0; y < FullGridWidth; y++)
                {
                    tmp = AdvGetOverLappingArea3(Grid[y], Horizontal_Input[y]);
                    if (tmp == null) continue;
                    for (int i = 0; i < FullGridWidth; i++)
                    {
                        //if (tmp[i] != 0) System.Diagnostics.Debugger.Break();
                        Grid[y][i] = tmp[i];
                    }
#if (DEBUG)
                    PrintAll();
#endif
                }
            }

            if (verti == 1)
            {
                //Vertical
                for (int x = 0; x < FullGridWidth; x++)
                {
                    tmp = AdvGetOverLappingArea3(GetVerticalGrid(x), Vertical_Input[x]);
                    if (tmp == null) continue;
                    for (int i = 0; i < FullGridWidth; i++)
                    {
                        Grid[i][x] = tmp[i];
                    }
#if (DEBUG)
                    PrintAll();
#endif
                }
            }
        }


        private int[] GetVerticalGrid(int col)
        {
            int[] ret = new int[FullGridWidth];
            for (int i = 0; i < FullGridWidth; i++)
            {
                ret[i] = Grid[i][col];
            }
            return ret;
        }
        
        #region Overlap 2
        public static int[] AdvGetOverLappingArea3(int[] baserow, int[] input)
        {
            List<int[]> trials = new List<int[]>();

            int[] ret = new int[FullGridWidth];
            int[] padspaces = new int[input.Length];
            padspaces.Populate(1, input.Length, 1); // {0,1,1}

            int index = 0; //main ptr
            int return_index = 0; //return point index
            int padindex = padspaces.Length - 1; //start from the last item
            int padsum, inpsum = SumInputs(input);

            #region iteration
            while (true)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    for (int ii = 0; ii < padspaces[i]; ii++)
                    {
                        ret[(index + ii)] = 2; //add padding
                    }
                    index += padspaces[i];
                    for (int ii = 0; ii < input[i]; ii++)
                    {
                        ret[(index + ii)] = 1; //add filling
                    }
                    index += input[i];
                }
                return_index = index;

                for (int i = index; i < FullGridWidth; i++)
                {
                    ret[index++] = 2; //fill in with x
                }

                trials.Add(ret.ToArray());                
                //Debug.WriteLine(string.Join(",",ret.Select(x=>x.ToString())));
                
                if (return_index == FullGridWidth) 
                {
                    //continuous shift down
                    do
                    {
                        RETRY:
                        if (padspaces[padindex] == 1)
                        {
                            padindex--;
                            if (padindex < 0) break; //break doesn't get out fully
                            padspaces[padindex] = 1;
                        }
                        else padspaces[padindex] = 1; //reset value
                        padindex--;
                        if (padindex < 0) goto JUMP; //break doesn't get out fully
                        padspaces[padindex]++;
                        padsum = SumInputs(padspaces);
                        if (padsum + inpsum > FullGridWidth) goto RETRY;
                    } while (padspaces[padindex] == 1);

                    padsum = SumInputs(padspaces);
                    //if (padsum + inpsum > FullGridWidth) break;

                    if (padsum + inpsum > FullGridWidth) goto JUMP;

                    padindex = padspaces.Length - 1; //reset value

                }
                else padspaces[padindex]++;
                return_index = index = 0;

            }
            #endregion
            JUMP:
            //debug check
#if (DEBUG)
            System.Diagnostics.Debug.WriteLine(string.Join(",",input.Select(x => x.ToString()).ToArray()));
            System.Diagnostics.Debug.WriteLine("base:");
            System.Diagnostics.Debug.WriteLine(string.Join(",", baserow.Select(x => (x == 0) ? "_" : (x == 1) ? "o" : "x").ToArray()));
            VisualCheck(trials.ToArray());
#endif
#region filter
            int t = 0;
            while (t < trials.Count)
            {
                for (int i = 0; i < FullGridWidth; i++)
                {
                    if (baserow[i] != trials[t][i])
                    {
                        if (baserow[i] != 0)
                        {
                            trials.RemoveAt(t);
                            t--;
                            break;
                        }
                    }
                }
                t++;
            }
            #endregion
#if (DEBUG)
            System.Diagnostics.Debug.WriteLine(string.Join(",", input.Select(x => x.ToString()).ToArray()));
            System.Diagnostics.Debug.WriteLine("base:");
            System.Diagnostics.Debug.WriteLine(string.Join(",", baserow.Select(x => (x == 0) ? "_" : (x == 1) ? "o" : "x").ToArray()));
            VisualCheck(finalist.ToArray());
#endif
            if (trials.Count == 1) return trials[0];
            if (trials.Count > 0) return MergeSame(trials.ToArray());
            //if (finalist.Count == 1) return finalist[0];
            //if (finalist.Count > 0) return MergeSame(finalist.ToArray());
            return null;

        }

        private static void VisualCheck(int[][] trials)
        {
            foreach (int[] item in trials)
            {
                string concatenated = string.Join(",", item.Select(x => (x == 0) ? "_" : (x == 1) ? "o" : "x" ).ToArray());
                System.Diagnostics.Debug.WriteLine(concatenated);
            }
        }

        public static int[] MergeSame(params int[][] arr)
        {

            int[] final = arr[0]; //copy
            foreach (int[] item in arr)
            {
                for (int i = 0; i < final.Length; i++)
                {
                    if (final[i] != item[i]) final[i] = 0; //reset only if not same
                }
            }
#if (DEBUG)
            string concatenated = string.Join(",", final.Select(x => (x == 0) ? "_" : (x == 1) ? "o" : "x").ToArray());
            System.Diagnostics.Debug.WriteLine(concatenated);
#endif
            return final;  
        }
        public static int SumInputs(int[] inp)
        {
            int sum = 0;
            for (int i = 0; i < inp.Length; i++)
            {
                sum += inp[i];
            }
            return sum;
        }
#endregion

#region Count Blocks
        public void CheckHVInputCount()
        {


        }
        public int FilledBlockVertical(int x)
        {
            int i = 0;
            for (int k = 0; k < FullGridWidth; k++)
            {
                if (Grid[k][x] == 1) i++;
            }
            return i;
        }
        public int FilledBlockHorizontal(int y)
        {
            int i = 0;
            for (int k = 0; k < FullGridWidth; k++)
            {
                if (Grid[y][k] == 1) i++;
            }
            return i;
        }

        public int[] AdjacentFilledBlockVertical(int x)
        {
            Queue<int> track = new Queue<int>();
            int count = 0;
            for (int k = 0; k < FullGridWidth; k++)
            {
                if (Grid[k][x] == 1) count++;
                if (count != 0 && Grid[k][x] != 1)
                {
                    track.Enqueue(count);
                    count = 0;
                }
            }
            if (count != 0)
            {
                track.Enqueue(count);
            }

            if (track.Count == 0)
                track.Enqueue(0);

            return track.ToArray();
        }
        public int[] AdjacentFilledBlockHorizontal(int y)
        {
            Queue<int> track = new Queue<int>();
            int count = 0;
            for (int k = 0; k < FullGridWidth; k++)
            {
                if (Grid[y][k] == 1) count++;
                if (count != 0 && Grid[y][k] != 1)
                {
                    track.Enqueue(count);
                    count = 0;
                }
            }
            if (count != 0)
            {
                track.Enqueue(count);
            }

            if (track.Count == 0)
                track.Enqueue(0);

            return track.ToArray();
        }
        
        #endregion
        public void CrossOutCheck()
        {
            //REDO THIS
        }

        #region Adjacent
        public bool IsInBound(Point p)
        {
            return IsInBound(p.x, p.y);
        }
        public bool IsInBound(int x, int y)
        {
            return IsInBound(x) && (IsInBound(y));
        }
        public bool IsInBound(int x)
        {
            return (x >= 0) && (x < FullGridWidth);
        }
        
#endregion

#region Printout
        private static readonly int SingleBlockWidth = 1; //width of each cell
        private void PrintHorizontalBorder(bool withnewline = false, bool headerfooter = false)
        {
            string outstr = (headerfooter) ? "+" : "|";

            int itemwidth =  2; //2 char + 1 space, 1 char + 1 space

            for (int a = 0; a < FullGridWidth; a++) //all segments
            {
                for (int b = 0; b < itemwidth; b++) //each segment
                {
                    outstr += '-';
                    
                    //outstr += (a == SingleBlockWidth - 1 && b == itemwidth - 1) ? "" : (headerfooter) ? "-" : (b == itemwidth - 1) ? "+" : "-"; //if last segment, add +
                }
                if (a != FullGridWidth - 1) outstr += '+';
            }
            outstr += (headerfooter) ? '+' : '|';
            //Console.WriteLine(outstr);
            Console.WriteLine("{0}{1}", ((withnewline) ? "\n" : string.Empty), outstr);
        }
        private void PrintAll(bool printlabel = true, bool usecolor = true, bool hidecross = false)
        {
            if (printlabel)
            {
                PrintLabel(0, Axis.VERTICAL);
                Console.Write("".PadRight(HorizontalLabelSize));
            }
            PrintHorizontalBorder(false, true);
            for (int x = 0; x < FullGridWidth; x++)
            {
                if (printlabel)
                {
                    PrintLabel(x, Axis.HORIZONTAL);
                }
                for (int y = 0; y < FullGridWidth; y++)
                {
                    Console.Write((y % SingleBlockWidth == 0) ? "|" : " ");
                    int block = Grid[x][y];

                    if (usecolor)
                    {
                        if (block == 1)
                            Console.BackgroundColor = ConsoleColor.White;
                        Console.Write("  ");
                        Console.BackgroundColor = ConsoleColor.Black;
                    } //0: empty 1: fill 2: cross
                    else Console.Write("{0}", block==1 ? "##" : block==2 ? (hidecross) ? "  " : "><" : "  ");
                }
                Console.Write("|");
                if ((x + 1) % SingleBlockWidth == 0)
                {                    
                    if (x != FullGridWidth - 1)
                    {
                        if (printlabel) Console.Write("\n".PadRight(HorizontalLabelSize + 1));
                        else Console.WriteLine();
                        PrintHorizontalBorder();
                    }
                    else
                    {
                        if (printlabel) Console.Write("\n".PadRight(HorizontalLabelSize + 1));
                        else Console.WriteLine();
                        PrintHorizontalBorder(false, true);
                        
                    }                    
                }
                else
                {
                    Console.WriteLine();
                }
            }
        }
       
        private void PrintLabel(int num, Axis ax)
        {
            string label = "";
            switch (ax)
            {
                case Axis.VERTICAL:
                    PrintVerticalLabel();
                    break;
                case Axis.HORIZONTAL:
                    label = CreateLabel(Horizontal_Input[num]);
                    if (label.Length < HorizontalLabelSize)
                    {
                        label = label.PadLeft(HorizontalLabelSize);
                    }
                    Console.Write(label);

                    break;
            }
        }
        private string CreateLabel(int[] arr)
        {
            string ret = "";
            foreach (int item in arr)
            {
                ret += item + " ";
            }
            return ret.Substring(0, ret.Length - 1);
        }

        private int Digits(int val)
        {
            int i = 0;
            while (val != 0)
            {
                val /= 10;
                i++;
            }
            return i;
        }
        private static int HorizontalLabelSize = 0;
        private static int VerticalLabelSize = 0;

        private int GetMaxCharCount(Axis ax)
        {
            int maxcount = 0;
            int curcount = 0;
            switch (ax)
            {
                case Axis.VERTICAL:
                    foreach (int[] item in Vertical_Input)
                    {
                        maxcount = (item.Length > maxcount) ? item.Length : maxcount;
                    }
                    break;
                case Axis.HORIZONTAL:
                    foreach (int[] item in Horizontal_Input)
                    {
                        foreach (int val in item)
                        {
                            curcount += Digits(val) + 1;
                        }
                        curcount--;
                        maxcount = (curcount > maxcount) ? curcount : maxcount;
                        curcount = 0;
                    }
                    break;
            }
            return maxcount;
        }

#region Vertical Print Out
        private int[][] verticallabel;
        


        /// <summary>
        /// TO FIX:: SPACES EITHER BECOMES "" OR NULL
        /// </summary>
        private void VerticalLabelPrepare()
        {
            verticallabel = new int[FullGridWidth][];
            for (int i = 0; i < FullGridWidth; i++)
            {
                verticallabel[i] = new int[VerticalLabelSize];
            }

            int counter = 0, tracker = 0;
            for (int i = 0; i < FullGridWidth; i++)
            {
                while (tracker < (VerticalLabelSize - Vertical_Input[i].Length))
                {
                    verticallabel[i][tracker++] = -1;                    
                }

                for (int ii = 0; ii < Vertical_Input[i].Length; ii++)
                {
                    verticallabel[i][tracker++] = Vertical_Input[i][counter++];
                }
                tracker = counter = 0;
            }
            

            
        }
        private void PrintVerticalLabel()
        {
            if (verticallabel == null)
                VerticalLabelPrepare();

            for (int ii = 0; ii < VerticalLabelSize; ii++)
            {
                Console.Write("".PadRight(HorizontalLabelSize + 1));
                for (int i = 0; i < FullGridWidth; i++)
                {
                    if (verticallabel[i][ii] == -1)
                        Console.Write(("").PadLeft(2) + " ");
                    else
                        Console.Write((verticallabel[i][ii] + "").PadLeft(2) + " ");
                }
                Console.WriteLine();
            }

        }
#endregion

#endregion

    }

    public static class SolverHelper
    {
        public static void Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }
        public static void Populate<T>(this T[] arr, int start, int end, T value)
        {
            for (int i = Math.Max(0, start); i < Math.Min(end, arr.Length); i++)
            {
                arr[i] = value;
            }
        }
    }
    public enum Axis
    {
        VERTICAL,
        HORIZONTAL
    }
    public static class Reader
    {
        public static int[][] Grid;
        public static string RawInput;
        public static int GridSize;
        public static bool ReadFromFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(path);
                RawInput = sr.ReadToEnd();
                sr.Close();

                return true;
            }
            return false;
        }

        public static bool ProcessRaw()
        {
            if (string.IsNullOrWhiteSpace(RawInput)) return false;

            string[] alllines = RawInput.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            GridSize = alllines.Length;

            Grid = new int[GridSize][];
            
            for (int i = 0; i < GridSize; i++)
            {
                Grid[i] = new int[GridSize];
                for (int ii = 0; ii < GridSize; ii++)
                {
                    Grid[i][ii] = (char.ToLower(alllines[i][ii]) == 'o') ? 1 : 0;
                }
            }

            return true;

        }
    }
}
