using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlagiChecker
{
    class ScanOperations //main Scanner Class
    {
        int visualized = 0; //for loading-line visualization
        bool text_flag = false; //is text (for code decomposing to ignore text)
        bool comment_line_flag = false; //is linear comment (for code decomposing to ignore linear comments)
        bool comment_flag = false; //is big comment (similar)
        string f1, f2; //paths to files
        List<List<List<string>>> code1 = new List<List<List<string>>>();
        List<List<List<string>>> code2 = new List<List<List<string>>>(); //Lists for decomposed codes
        int pointRange = -1;  //max number of points
        int totalResult = -1; //points recieved

        public ScanOperations(string f1, string f2) //create class obj, decompose codes
        {
            this.f1 = f1;
            this.f2 = f2;
        }

        public void DecomposeCodes()
        {
            Console.WriteLine(); //loading progress line
            Console.WriteLine($"Decomposing progress:");
            Console.WriteLine($"\t0%                     25%                      50%                      75%                      100%");
            Console.WriteLine($"\t|                       |                        |                        |                        |");
            Console.Write("\t");
            DecomposeCode(File.ReadAllText(f1, Encoding.ASCII), ref code1); //decompose first code
            visualized = 0;
            DecomposeCode(File.ReadAllText(f2, Encoding.ASCII), ref code2); //decompose second code
        }

        public void CompareCodes()
        {
            Console.WriteLine(); //loading progress line
            Console.WriteLine($"Comparing progress:");
            Console.WriteLine($"\t0%                     25%                      50%                      75%                      100%");
            Console.WriteLine($"\t|                       |                        |                        |                        |");
            Console.Write("\t");
            CompareCodes(ref code1, ref code2);
            visualized = 0;
            CompareCodes(ref code2, ref code1);
            Console.WriteLine();
        }

        void DecomposeCode(string code, ref List<List<List<string>>> end)
        {
            int pos = 0;
            bool include_flag = true; //to skip preprocesses (#include, #define, using, import)
            for (; pos < code.Length && include_flag;) //skip preprocesses
            {
                if (pos + 8 < code.Length && code.Substring(pos, 8) == "#include"
                    || code.Substring(pos, 7) == "#define" || code.Substring(pos, 5) == "using"
                    || code.Substring(pos, 6) == "import" || code.Substring(pos, 7) == "package")
                {
                    pos = (code.IndexOf('\n', pos) + 1);
                }
                else
                {
                    include_flag = false;
                }
                pos = SkipSpace(code, pos);
            }
            DecomposeCodeBlock(code, pos, ref end);
        }

        int DecomposeCodeBlock(string code, int pos, ref List<List<List<string>>> end)
        { //decompose code block (text block -> hierarchylist)
            text_flag = false;
            pos = SkipSpace(code, pos);
            if (pos == code.Length) //if end of code
            {
                return pos;
            }

            List<List<string>> block = new List<List<string>>(); //save temporary block
            List<string> command = new List<string>(); //save temporary line == command
            StringBuilder word = new StringBuilder(); //save temporary word

            for (; pos < code.Length; pos++) //while not end of code
            {
                for (int k = visualized; k < (50 * pos / code.Length) + 1; k++) //loading progress
                {
                    Console.Write("|");
                }
                visualized = (50 * pos / code.Length) + 1;

                if (code.ElementAt(pos) == '"') //text appearance
                {
                    if (!text_flag) //text starts
                    {
                        text_flag = true;
                    }
                    else //text ends
                    {
                        text_flag = false;
                    }
                    continue;
                }
                if (text_flag) continue; //if text
                if (comment_line_flag) //if linear comment
                {
                    if (code.ElementAt(pos) == '\n') //linear comment ends
                    {
                        comment_line_flag = false; //end line commentary
                        if (pos + 1 < code.Length)
                        {
                            pos = SkipSpace(code, pos + 1) - 1;
                        }
                    }
                    continue;
                }
                if (comment_flag) //if big commentary
                {
                    if (code.ElementAt(pos) == '*' &&
                        pos + 1 < code.Length && code.ElementAt(pos + 1) == '/') //is end of big commentary
                    {
                        comment_flag = false; //end commentary
                        pos++;
                        if (pos + 1 < code.Length)
                        {
                            pos = SkipSpace(code, pos + 1) - 1;
                        }
                    }
                    continue;
                }
                if (!text_flag) //if not text but code
                {
                    if (code.ElementAt(pos) == '/' && pos + 1 < code.Length) //commentary appearance
                    {
                        if (code.ElementAt(pos + 1) == '/') //start of linear commentary
                        {
                            comment_line_flag = true;
                            continue;
                        }
                        else if (code.ElementAt(pos + 1) == '*') //start of big commentary
                        {
                            comment_flag = true;
                            continue;
                        }
                    }
                    switch (code.ElementAt(pos)) //inner block start
                    {
                    case '{':
                        if (word.Length > 0) //save last word
                        {
                            command.Add(word.ToString());
                            word.Clear();
                        }
                        if (command.Count > 0) //save last command
                        {
                            block.Add(command);
                            command = new List<string>();
                        }
                        if (pos + 1 < code.Length)
                        {
                            pos = SkipSpace(code, pos + 1) - 1;
                        }
                        pos = DecomposeCodeBlock(code, pos + 1, ref end); //decompose inner code block
                        if (pos + 1 < code.Length)
                        {
                            pos = SkipSpace(code, pos + 1) - 1;
                        }
                        break;
                    case '}':
                        if (block.Count > 0) //save last block
                        {
                            end.Add(block);
                        }
                        return pos;
                    case ';':
                        if (word.Length > 0) //save last word
                        {
                            command.Add(word.ToString());
                            word.Clear();
                        }
                        if (command.Count > 0) //save last command
                        {
                            block.Add(command);
                            command = new List<string>();
                        }
                        if (pos + 1 < code.Length)
                        {
                            pos = SkipSpace(code, pos + 1) - 1;
                        }
                        break;
                    default:
                        if ((code.ElementAt(pos) == ' ' || code.ElementAt(pos) == '\t' ||
                            code.ElementAt(pos) == '\r' || code.ElementAt(pos) == '\n') && word.Length > 0) //end of word
                        {
                            if (word.Length > 0) //save last word
                            {
                                command.Add(word.ToString());
                                word.Clear();
                            }
                            pos = SkipSpace(code, pos) - 1;
                        }
                        else //word filling continue
                        {
                            word.Append(code.ElementAt(pos));
                        }
                        break;
                    }
                }
            }
            if (word.Length > 0) //save last word in block
            {
                command.Add(word.ToString());
                word.Clear();
            }
            if (command.Count > 0) //save last command in block
            {
                block.Add(command);
            }
            if (block.Count > 0) //save this block in hierarchy
            {
                end.Add(block);
            }
            return pos;
        }

        int SkipSpace(string str, int pos) /*func that skips all spaces and another syntactic unit separators
            and returns position after them*/
        {
            while (pos < str.Length && (str.ElementAt(pos) == ' ' || str.ElementAt(pos) == '\r' ||
                str.ElementAt(pos) == '\t' || str.ElementAt(pos) == '\n'))
            {
                pos++;
            }
            return pos;
        }

        void CompareCodes(ref List<List<List<string>>> code1, ref List<List<List<string>>> code2)
        { //compares code1 and code2
            //vizualization line variables
            int all = code1.Count; //number of blocks in first code, for visualization
            int current = 0; //number of blocks scanned, for visualization
            int visualized = 0; //for visualization

            int curPointRange = CalculatePointRange(ref code1); //max number of points

            //scan
            int curTotalResult = 0; //points recieved
            foreach(List<List<string>> fisrtBlock in code1) //for every block in first code
            {
                int bestResult = -1;
                foreach(List<List<string>> secondBlock in code2) //for every block in second block
                {
                    bestResult = Math.Max(CompareBlocks(fisrtBlock, secondBlock), bestResult);
                    //to find most similar block to current block1
                }
                curTotalResult += bestResult; current++; //total number of points
                for (int k = visualized; k < 50 * current / all; k += 1) //visualization loading progress
                {
                    Console.Write("|");
                }
                visualized = 50 * current / all;
            }

            if (totalResult == -1 && pointRange == -1 ||
                (double)curTotalResult / curPointRange > (double)totalResult / pointRange)
            {
                totalResult = curTotalResult;
                pointRange = curPointRange;
            }//save higher result
        }

        int CalculatePointRange(ref List<List<List<string>>> code) //counts max number of points can be recieved
        {
            int range = 0;
            foreach(List<List<string>> block in code)
            {
                foreach(List<string> com in block)
                {
                    foreach(string word in com)
                    {
                        range += word.Length + 5;
                    }
                }
            }
            return range;
        }

        int CompareBlocks(List<List<string>> block1, List<List<string>> block2) //compare blocks
        {
            int blockTotalResult = 0; //total points for current block
            for (int i = 0; i < block1.Count; i++) //for every line
            {
                int bestResult = 0; //to find best command match
                int tempResult; //temporary result
                for (int j = 0; j < block2.Count; j++) //for every command in second block
                {
                    tempResult = CompareCommand(block1.ElementAt(i), block2.ElementAt(j)); //compare commands
                    bestResult = Math.Max(tempResult, bestResult);//match result of comparing
                }
                blockTotalResult += bestResult;
            }
            return blockTotalResult;
        }

        int CompareCommand(List<string> com1, List<string> com2) //compares commands and returns number of points
        {
            int points = 0;
            //phase 1 - word by word (5 points per each similar word)
            for (int i = 0; i < com1.Count && i < com2.Count; i++)
            {
                if (com1.ElementAt(i).Equals(com2.ElementAt(i)))
                {
                    points += 5;
                }
            }
            //phase 1.5 prepare for rough scan
            StringBuilder line1 = new StringBuilder(""), line2 = new StringBuilder(""); //rough lines (no separators between words)
            foreach(string word in com1) //create first rough line
            {
                line1.Append(word);
            }
            foreach(string word in com2) //create second rough line
            {
                line2.Append(word);
            }
            List<bool> symbolFinded1 = new List<bool>();
            List<bool> symbolFinded2 = new List<bool>(); //bool list with symbol matches
            //phase 2 - front scan
            for (int i = 0; i < line1.Length && i < line2.Length; i++)
            {
                if (line1[i] == line2[i]) // +point
                {
                    symbolFinded1.Add(true);
                }
                else // don`t match
                {
                    symbolFinded1.Add(false);
                }
            }
            //phase 3 - back scan
            for (int i = line1.Length - 1, j = line2.Length - 1; i >= 0 && j >= 0; i--, j--)
            {
                if (line1[i] == line2[j]) // +point
                {
                    symbolFinded2.Add(true);
                }
                else // don`t match
                {
                    symbolFinded2.Add(false);
                }
            }
            int roughScanResult = 0;
            for (int i = 0; i < Math.Min(symbolFinded1.Count, symbolFinded2.Count); i++) //count rough scan points
            {
                if (symbolFinded1.ElementAt(i) || symbolFinded2.ElementAt(i)) roughScanResult++;
            }
            return points + roughScanResult;
        }

        public string GetComparisonResults()
        {
            double final = 100 * (double)totalResult / pointRange;
            StringBuilder result = new StringBuilder();
            result.Append("------------------- RESULTS -------------------\n");
            result.Append($"------------------>  {Math.Round(final, 1)}%  <------------------\n\n");
            result.Append($"Information:\n");
            result.Append($"Max result possible: {pointRange}\n");
            result.Append($"Points of plagiat confirmed: {totalResult}\n");
            result.Append($"Conclusion: ");
            if (final > 95) result.Append("Exactly similar codes!");
            else if (final > 75) result.Append("Codes are almost similar!");
            else if (final > 50) result.Append("Codes have some differences and similarities");
            else if (final > 25) result.Append("Codes are a bit similar");
            else result.Append("Codes are exactly different");
            return result.ToString();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"|--------------|-------------- Welcome to Progiator! --------------|--------------|{Environment.NewLine}");
            string[] files = new string[2]; //our files
            for (int i = 0; i < files.Length; i++)
            {
                while (true) //repeat until files are correct
                {
                    Console.Write(@"Enter path to first file (example: 'C:\Users\User\Desktop\code.cpp'): ");
                    files[i] = Console.ReadLine();
                    if (!File.Exists(files[i]))
                        Console.WriteLine("This path isn`t correct! Please, try again.");
                    else if (new FileInfo(files[i]).Length > 500000)
                    {
                        Console.WriteLine("This file is too big!");
                        Console.WriteLine($"Size of this file = {new FileInfo(files[i]).Length} bytes");
                    }
                    else if (new FileInfo(files[i]).Length < 100)
                    {
                        Console.WriteLine("This file is too small!");
                        Console.WriteLine($"Size of this file = {new FileInfo(files[i]).Length} bytes");
                    }
                    else if (!(files[i].EndsWith(".cpp") || files[i].EndsWith(".cs") || files[i].EndsWith(".java")
                        || files[i].EndsWith(".c")))
                        Console.WriteLine("Wrong format file! This programm works only with .cpp, .c, .cs and .java files!");
                    else break;
                }
            }
            Console.WriteLine($"First file: {files[0]}; Size = {new FileInfo(files[0]).Length} bytes");
            Console.WriteLine($"Second file: {files[1]}; Size = {new FileInfo(files[1]).Length} bytes"); //sizes output
            Console.WriteLine("Press any key to start scanning or ESC to exit...");
            char ready = Console.ReadKey().KeyChar; //prompt agreement
            if (ready == 27) Environment.Exit(0);
            ScanOperations scanner = new ScanOperations(files[0], files[1]); //DECOMPOSITION, COMPARING AND RESULTS OUTPUT
            scanner.DecomposeCodes();
            scanner.CompareCodes();
            Console.WriteLine(scanner.GetComparisonResults());

            Console.Read();
        }
    }
}