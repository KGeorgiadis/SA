/**
* Copyright (C) 2016 Open University (http://www.ou.nl/)
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*         http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


namespace StealthAssessment
{
    class ECD
    {
        string input = null;    //This variable stores the user's keyboard input.

        //Constructor.
        public ECD()
        {
            return;
        }

        //Destructor.
        ~ECD()
        {
            return;
        }

        //This function generates a competency model based on pre-made settings declared in the App.config file.
        public Tuple<string[], string[], string[][]> CompetencyModelAuto()
        {
            string[] Competencies = ConfigurationManager.AppSettings["Competencies"].Split(':').Select(s => s.Trim()).ToArray();   //Stores competencies.
            string[] FacetsPerCompetency = ConfigurationManager.AppSettings["Facets"].Split(':').Select(s => s.Trim()).ToArray();  //Stores facets unparsed per competency.
            string[][] InfluenceDiagram = new string[Competencies.Length][];   //Stores the structure of the influence diagram(s).

            for(int x=0; x<Competencies.Length; x++)
            {
                try
                {
                    InfluenceDiagram[x] = new string[FacetsPerCompetency[x].Split(',').Select(s => s.Trim()).ToArray().Length];

                    for (int y=0; y< FacetsPerCompetency[x].Split(',').Select(s => s.Trim()).ToArray().Length; y++)
                    {
                        string[] facets = FacetsPerCompetency[x].Split(',').Select(s => s.Trim()).ToArray();     //Stores facets parsed for a competency.
                        InfluenceDiagram[x][y] = facets[y];
                    }
                }

                catch (IndexOutOfRangeException)
                {
                    InfluenceDiagram[x] = new string[0];
                }
   
            }

            return new Tuple<string[], string[], string[][]>(Competencies, FacetsPerCompetency, InfluenceDiagram);
        }

        //This function generates a competency model based on user input.
        public Tuple<string[], string[], string[][]> CompetencyModelCustom()
        {
            //Declaring Variables.
            bool repeat = true;     //This variable checks if the user should re-enter input.
            var Conversion = new Exceptions();

            Console.WriteLine("Competency Model");

            //Set no. of competencies to be assessed
            while (repeat)
            {
                Console.Write("No. of competencies:");
                input = Console.ReadLine();
                repeat = Conversion.StringToInt(input);     //check if the input is an integer and handle exceptions        
            }

            string[] Competencies = new string[Convert.ToInt32(input)];   //Stores competencies.
            string[] FacetsPerCompetency = new string[Competencies.Length];    //Stores facets unparsed per competency.
            string[][] InfluenceDiagram = new string[Competencies.Length][];   //Stores the structure of the influence diagram(s).

            //Set the influence diagram for each competency
            for (int x = 0; x < Competencies.Length; x++)
            {
                //Set competencies' names
                Console.Write("Name competency no. {0}:", x + 1);
                Competencies[x] = Console.ReadLine();

                //Set no. of facets to be assessed
                repeat = true;

                while (repeat)
                {
                    Console.Write("No. of facets for {0}:", Competencies[x]);
                    input = Console.ReadLine();
                    repeat = Conversion.StringToInt(input); //Check if the input is an integer and handle exceptions         
                }

                InfluenceDiagram[x] = new string[Convert.ToInt32(input)];

                //Set facets' names
                for (int y = 0; y < InfluenceDiagram[x].Length; y++)
                {
                    Console.Write("Name of facet no. {0}:", y + 1);
                    InfluenceDiagram[x][y] = Console.ReadLine();
                    FacetsPerCompetency[x] += InfluenceDiagram[x][y] + ",";
                }
            }

            Console.WriteLine();

            return new Tuple<string[], string[], string[][]>(Competencies, FacetsPerCompetency, InfluenceDiagram);
        }

        //This function generates a task model based on pre-made settings declared in the App.config file, given a competency model.
        public Tuple<string[], string[][]> TaskModelAuto(Tuple<string[], string[], string[][]> CompetencyModel)
        {
            string[] TasksPerCompetency = ConfigurationManager.AppSettings["Tasks"].Split(':').Select(s => s.Trim()).ToArray();   //Stores tasks unparsed per competency.
            string[][] TaskModel = new string[TasksPerCompetency.Length][];    //Stores the tasks structure(s) per competency. 

            for(int x=0; x<TasksPerCompetency.Length; x++)
            {

                TaskModel[x] = new string[TasksPerCompetency[x].Split(',').Select(s => s.Trim()).ToArray().Length];

                for (int y = 0; y < TasksPerCompetency[x].Split(',').Select(s => s.Trim()).ToArray().Length; y++)
                {
                    string[] tasks = TasksPerCompetency[x].Split(',').Select(s => s.Trim()).ToArray();     //Stores tasks parsed for a competency.
                    TaskModel[x][y] = tasks[y];
                }

            }
            
            return new Tuple<string[], string[][]>(TasksPerCompetency, TaskModel);
        }

        //This function generates a competency model based on user input, given a competency model.
        public Tuple<string[], string[][]> TaskModelCustom(Tuple<string[], string[], string[][]> CompetencyModel)
        {
            //Declaring Variables.
            bool repeat = true;     //This variable checks if the user should re-enter input.
            var Conversion = new Exceptions();
            string[] TasksPerCompetency = new string[CompetencyModel.Item1.Length];   //Stores tasks unparsed per competency.
            string[][] TaskModel = new string[CompetencyModel.Item1.Length][];    //Stores the tasks structure(s) per competency.

            Console.WriteLine("Task Model");

            //Set tasks per competency
            for (int x = 0; x < CompetencyModel.Item1.Length; x++)
            {
                repeat = true;

                //Set no. of tasks per competency
                while (repeat)
                {
                    Console.Write("No. of tasks for {0}:", CompetencyModel.Item1[x]);
                    input = Console.ReadLine();
                    repeat = Conversion.StringToInt(input);     //check if the input is an integer and handle exceptions        
                }

                TaskModel[x] = new string[Convert.ToInt32(input)];

                //Set task names per competency
                for (int y = 0; y < TaskModel[x].Length; y++)
                {
                    Console.Write("Name task no. {0} for {1}:", y + 1, CompetencyModel.Item1[x]);
                    input = Console.ReadLine();

                    TaskModel[x][y] = input;
                    TasksPerCompetency[x] += TaskModel[x][y] + ",";
                }
            }

            Console.WriteLine();

            return new Tuple<string[], string[][]>(TasksPerCompetency, TaskModel);
        }

        //This function generates a evidence model based on pre-made settings declared in the App.config file, given a competency and task model.
        public Tuple<string[], string[], string[][], string[][], string[][][], string[][][]> EvidenceModelAuto(Tuple<string[], string[][]> TaskModel, Tuple<string[], string[], string[][]> CompetencyModel)
        {
            //Load selected metrics
            string[] SelectedMetadataMetrics = ConfigurationManager.AppSettings["Meta-data Metrics"].Split(':').Select(s => s.Trim()).ToArray();    //Stores selected meta-data metrics
            string[] SelectedGametraceMetrics = ConfigurationManager.AppSettings["Game-traces Metrics"].Split(':').Select(s => s.Trim()).ToArray();   //Stores selected game-trace metrics

            //Load indicators for selected metrics
            string[] SelectedMetadataIndicators = ConfigurationManager.AppSettings["Indicators of Meta-data Metrics"].Split(':').Select(s => s.Trim()).ToArray();
            string[] SelectedGametraceIndicators = ConfigurationManager.AppSettings["Indicators of Game-traces Metrics"].Split(':').Select(s => s.Trim()).ToArray();
            string[][] IndicatorsOfSelectedMetadataMetrics = new string[SelectedMetadataMetrics.Length][];    //Stores indicators of selected meta-data metrics
            string[][] IndicatorsOfSelectedGametraceMetrics = new string[SelectedGametraceMetrics.Length][];    //Stores indicators of selected game-trace metrics

            for (int x = 0; x < SelectedMetadataMetrics.Length; x++)
            {

                IndicatorsOfSelectedMetadataMetrics[x] = new string[SelectedMetadataIndicators[x].Split(',').Select(s => s.Trim()).ToArray().Length];

                for (int y = 0; y < SelectedMetadataIndicators[x].Split(',').Select(s => s.Trim()).ToArray().Length; y++)
                {
                    string[] indicators = SelectedMetadataIndicators[x].Split(',').Select(s => s.Trim()).ToArray();     //Stores indicators parsed for a meta-data metric.
                    IndicatorsOfSelectedMetadataMetrics[x][y] = indicators[y];
                }

            }

            for (int x = 0; x < SelectedGametraceMetrics.Length; x++)
            {

                IndicatorsOfSelectedGametraceMetrics[x] = new string[SelectedGametraceIndicators[x].Split(',').Select(s => s.Trim()).ToArray().Length];

                for (int y = 0; y < SelectedGametraceIndicators[x].Split(',').Select(s => s.Trim()).ToArray().Length; y++)
                {
                    string[] indicators = SelectedGametraceIndicators[x].Split(',').Select(s => s.Trim()).ToArray();     //Stores indicators parsed for a game-trace metric.
                    IndicatorsOfSelectedGametraceMetrics[x][y] = indicators[y];
                }

            }

            //Evidence Rules 
            string[] EvidenceRulesIndicators = ConfigurationManager.AppSettings["Evidence Rules"].Split(':').Select(s => s.Trim()).ToArray();   //Stores indicators for evidence rules unparsed for a competency.
            string[][][] EvidenceRules = new string[TaskModel.Item2.Length][][];          //Stores the Evidence Rules (mapping of indicators from selected game-trace metrics to tasks).

            for (int x = 0; x < TaskModel.Item2.Length; x++)
            {
                EvidenceRules[x] = new string[TaskModel.Item2[x].Length][];

                for (int y=0; y< TaskModel.Item2[x].Length; y++)
                {
                    EvidenceRules[x][y] = new string[EvidenceRulesIndicators[y].Split(',').Select(s => s.Trim()).ToArray().Length];

                    for(int i=0; i<EvidenceRulesIndicators[y].Split(',').Select(s => s.Trim()).ToArray().Length; i++)
                    {
                        string[] indicators = EvidenceRulesIndicators[y].Split(',').Select(s => s.Trim()).ToArray();     //Stores indicators for evidence rules parsed for a competency.
                        EvidenceRules[x][y][i] = indicators[i];
                    }
                }
            }

            //Statistical Submodel 
            string[] StatisticalSubmodelIndicators = ConfigurationManager.AppSettings["Statistical Submodel"].Split(':').Select(s => s.Trim()).ToArray();
            string[][][] StatisticalSubmodel = new string[CompetencyModel.Item3.Length][][];    //Stores the Statistical Submodel (mapping of indicators from selected game-trace metrics to facets)

            for (int x = 0; x < CompetencyModel.Item3.Length; x++)
            {
                StatisticalSubmodel[x] = new string[CompetencyModel.Item3[x].Length][];

                for (int y = 0; y < CompetencyModel.Item3[x].Length; y++)
                {
                    StatisticalSubmodel[x][y] = new string[StatisticalSubmodelIndicators[y].Split(',').Select(s => s.Trim()).ToArray().Length];

                    for (int i = 0; i < StatisticalSubmodelIndicators[y].Split(',').Select(s => s.Trim()).ToArray().Length; i++)
                    {
                        string[] indicators = StatisticalSubmodelIndicators[y].Split(',').Select(s => s.Trim()).ToArray();     //Stores indicators for statistical submodel parsed for a competency.
                        StatisticalSubmodel[x][y][i] = indicators[i];
                    }
                }
            }


            return new Tuple<string[], string[], string[][], string[][], string[][][], string[][][]> (SelectedMetadataMetrics, SelectedGametraceMetrics, IndicatorsOfSelectedMetadataMetrics, IndicatorsOfSelectedGametraceMetrics, EvidenceRules, StatisticalSubmodel);
        }

        //This function generates a competency model based on user input, given a competency and a task model.
        public Tuple<string[], string[], string[][], string[][], string[][][], string[][][]> EvidenceModelCustom(Tuple<string[], string[][]> TaskModel, Tuple<string[], string[], string[][]> CompetencyModel)
        {
            //Declaring Variables.
            bool repeat = true;     //This variable checks if the user should re-enter input.

            Console.WriteLine("Evidence Model");

            //Open game log file and store all available metrics.
            string[] AllMetrics = new string[] { };
            var Excel = new Excel();
            AllMetrics = Excel.ExtractMetrics("GameTraces"); //(Optional to allow user select Excel file in the future)

            //Browse available metrics
            Console.WriteLine("Available metrics: ");
            for (int i = 0; i < AllMetrics.Length; i++)
            {
                Console.Write("[{0}]{1} ", i + 1, AllMetrics[i]);
            }
            Console.WriteLine();

            //Set no. of meta-data metrics. These metrics later on determine how the data is queried to update the Bayes Network.
            string[] SelectedMetadataMetrics = new string[SetNoOfMetrics(AllMetrics, "meta-data")];    //Stores selected meta-data metrics.

            //Select meta-data metrics from the list of available metrics
            SelectedMetadataMetrics = SelectMetrics(SelectedMetadataMetrics, AllMetrics);

            //Set no. of game-trace metrics. These metrics later on determine the data that updates the Bayes Network.
            string[] SelectedGametraceMetrics = new string[SetNoOfMetrics(AllMetrics,"game-trace")];   //Stores selected game-trace metrics.

            //Select game-trace metrics from the list of available metrics and check for overlaps with selected meta-data metrics
            while(repeat)
            {
                bool overlap = false;
                SelectedGametraceMetrics = SelectMetrics(SelectedGametraceMetrics, AllMetrics);
    
                for (int x = 0; x < SelectedGametraceMetrics.Length; x++)
                {
                    for (int y = 0; y < SelectedMetadataMetrics.Length; y++)
                    {
                        if (SelectedGametraceMetrics[x]==SelectedMetadataMetrics[y])
                        {
                            overlap = true;
                        }
                    }
                }

                if (overlap)
                {
                    Console.WriteLine("The selected game-trace metrics overlap with the selected meta-data metrics.");
                    repeat = true;
                }

                else
                {
                    repeat = false;
                }
            }

            Console.WriteLine();

            //Extract indicators for selected metrics
            var BayesNet = new BayesNet();
            string[][] AllData = new string[][] { };
            
            AllData = BayesNet.LoadAllData("GameTraces");               //Load all the data from the game log file.
            string[][] UnparsedIndicatorsOfSelectedMetadataMetrics = new string[SelectedMetadataMetrics.Length][];    //Stores unparsed indicators of selected meta-data metrics.
            string[][] UnparsedIndicatorsOfSelectedGametraceMetrics = new string[SelectedGametraceMetrics.Length][];    //Stores unparsed indicators of selected game-trace metrics.
            UnparsedIndicatorsOfSelectedMetadataMetrics = ExtractIndicators(AllData, SelectedMetadataMetrics);
            UnparsedIndicatorsOfSelectedGametraceMetrics = ExtractIndicators(AllData, SelectedGametraceMetrics);

            //Auto-Parsing indicators for selected metrics.
            string[][] ParsedIndicatorsOfSelectedMetadataMetrics = new string[UnparsedIndicatorsOfSelectedMetadataMetrics.Length][];    //Stores parsed indicators of selected meta-data metrics.
            string[][] ParsedIndicatorsOfSelectedGametraceMetrics = new string[UnparsedIndicatorsOfSelectedGametraceMetrics.Length][];  //Stores parsed indicators of selected game-trace metrics.
            ParsedIndicatorsOfSelectedMetadataMetrics = ParsingIndicators(UnparsedIndicatorsOfSelectedMetadataMetrics);
            ParsedIndicatorsOfSelectedGametraceMetrics = ParsingIndicators(UnparsedIndicatorsOfSelectedGametraceMetrics);

            //Remove dublicate values from parsed indicators of selected metrics.
            string[][] NoDubsParsedIndicatorsOfSelectedMetadataMetrics = new string[ParsedIndicatorsOfSelectedMetadataMetrics.Length][];
            string[][] NoDubsParsedIndicatorsOfSelectedGametraceMetrics = new string[ParsedIndicatorsOfSelectedGametraceMetrics.Length][];
            NoDubsParsedIndicatorsOfSelectedMetadataMetrics = RemoveDups(ParsedIndicatorsOfSelectedMetadataMetrics);
            NoDubsParsedIndicatorsOfSelectedGametraceMetrics = RemoveDups(ParsedIndicatorsOfSelectedGametraceMetrics);

            Console.WriteLine();

            //Browse non-dublicate parsed indicators for the selected meta-data metrics.
            int NoOfIndicators = 0;
            Console.WriteLine("Indicators for selected meta-data metrics:");
            for(int x = 0; x < NoDubsParsedIndicatorsOfSelectedMetadataMetrics.Length; x++)
            {
                Console.Write("{0}: ", SelectedMetadataMetrics[x]);

                for(int y = 0; y < NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x].Length; y++)
                {
                    NoOfIndicators++;
                    Console.Write("[{0}]{1} ", NoOfIndicators, NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x][y]);
                }

                Console.WriteLine();
            }

            Console.WriteLine();

            //Browse non-dublicate parsed indicators for the selected game-trace metrics.
            NoOfIndicators = 0;
            Console.WriteLine("Indicators for selected game-trace metrics:");
            for (int x = 0; x < NoDubsParsedIndicatorsOfSelectedGametraceMetrics.Length; x++)
            {
                Console.Write("{0}: ", SelectedGametraceMetrics[x]);

                for (int y = 0; y < NoDubsParsedIndicatorsOfSelectedGametraceMetrics[x].Length; y++)
                {
                    NoOfIndicators++;
                    Console.Write("[{0}]{1} ", NoOfIndicators, NoDubsParsedIndicatorsOfSelectedGametraceMetrics[x][y]);
                }

                Console.WriteLine();
            }

            Console.WriteLine();

            //Evidence Rules
            Console.WriteLine("Evidence Rules");
            string[][][] EvidenceRules = new string[TaskModel.Item2.Length][][];          //Stores the Evidence Rules (mapping of indicators from selected game-trace metrics to tasks).
            var Conversion = new Exceptions();
            int NoOfSelectedIndicators = 0;

            for (int x = 0; x < TaskModel.Item2.Length; x++)
            {
                EvidenceRules[x] = new string[TaskModel.Item2[x].Length][];

                for (int y = 0; y < TaskModel.Item2[x].Length; y++)
                {
                    string input = null;
                    repeat = true;

                    while (repeat)
                    {
                        Console.Write("Set no. of game-trace indicators for task {0}: ", TaskModel.Item2[x][y]);
                        input = Console.ReadLine();
                        repeat = Conversion.StringToInt(input);     //check if the input is an integer and handle exceptions        
                    }

                    EvidenceRules[x][y] = new string[Convert.ToInt32(input)];

                    for (int i = 0; i < EvidenceRules[x][y].Length; i++)
                    {
                        //Browse available game-trace metrics
                        Console.Write("Available game-trace metrics: ");
                        for (int k = 0; k < SelectedGametraceMetrics.Length; k++)
                        {
                            Console.Write("[{0}]{1} ", k + 1, SelectedGametraceMetrics[k]);
                        }

                        Console.WriteLine();
                        repeat = true;
                        int gametrace_selection = 0;

                        while (repeat)
                        {
                            Console.Write("Select game-trace metric (set int<={0}): ", SelectedGametraceMetrics.Length);
                            input = Console.ReadLine();
                            repeat = Conversion.StringToInt(input);     //check if the input is an integer and handle exceptions

                            if (repeat == false)
                            {
                                gametrace_selection = Convert.ToInt32(input);

                                if (gametrace_selection > 0 && gametrace_selection <= SelectedGametraceMetrics.Length)
                                {
                                    //Browse indicators
                                    int current_indicator = 0;
                                    Console.WriteLine("Select indicator (Use arrows (up/down) to scroll and 'Enter' to select): ");
                                    Console.Write("{0}", NoDubsParsedIndicatorsOfSelectedGametraceMetrics[gametrace_selection-1][current_indicator]);

                                    ConsoleKeyInfo keyInfo = Console.ReadKey();
                                    while (keyInfo.Key != ConsoleKey.Enter)
                                    {
                                        keyInfo = Console.ReadKey();
                                        if (keyInfo.Key == ConsoleKey.UpArrow)
                                        {
                                            current_indicator++;
                                            if (current_indicator >= NoDubsParsedIndicatorsOfSelectedGametraceMetrics[gametrace_selection-1].Length)
                                            {
                                                current_indicator = 0;
                                            }
                                            else if (current_indicator < 0)
                                            {
                                                current_indicator = NoDubsParsedIndicatorsOfSelectedGametraceMetrics[gametrace_selection-1].Length - 1;
                                            }
                                            Console.Write("\r                ");
                                            Console.Write("\r{0}", NoDubsParsedIndicatorsOfSelectedGametraceMetrics[gametrace_selection-1][current_indicator]);
                                        }
                                        else if (keyInfo.Key == ConsoleKey.DownArrow)
                                        {
                                            current_indicator--;
                                            if (current_indicator >= NoDubsParsedIndicatorsOfSelectedGametraceMetrics[gametrace_selection-1].Length)
                                            {
                                                current_indicator = 0;
                                            }
                                            else if (current_indicator < 0)
                                            {
                                                current_indicator = NoDubsParsedIndicatorsOfSelectedGametraceMetrics[gametrace_selection-1].Length - 1;
                                            }
                                            Console.Write("\r                ");
                                            Console.Write("\r{0}", NoDubsParsedIndicatorsOfSelectedGametraceMetrics[gametrace_selection-1][current_indicator]);
                                        }
                                    }

                                    EvidenceRules[x][y][i] = NoDubsParsedIndicatorsOfSelectedGametraceMetrics[gametrace_selection-1][current_indicator];
                                    NoOfSelectedIndicators++;
                                    Console.WriteLine();
                                }

                                else
                                {
                                    Console.WriteLine("Input value out of bounds.");
                                }
                            }
                        }
                    }
                }
            }

            //Store selected indicators from Evidence model to use in Statistical Submodel
            string[] SelectedIndicators = new string[NoOfSelectedIndicators];
            int count = 0;

            for(int x = 0; x < EvidenceRules.Length; x++)
            {
                for(int y = 0; y < EvidenceRules[x].Length; y++)
                {
                    for(int i = 0; i < EvidenceRules[x][y].Length; i++)
                    {
                        SelectedIndicators[count] = EvidenceRules[x][y][i];
                        count++;
                    }
                }
            }

            //Statistical Submodel
            Console.WriteLine("Statistical Submodel");
            string[][][] StatisticalSubmodel = new string[CompetencyModel.Item1.Length][][];    //Stores the Statistical Submodel (mapping of indicators from selected game-trace metrics to facets).

            for (int x = 0; x < CompetencyModel.Item3.Length; x++)
            {
                StatisticalSubmodel[x] = new string[CompetencyModel.Item3[x].Length][];

                for (int y = 0; y < CompetencyModel.Item3[x].Length; y++)
                {
                    string input = null;
                    repeat = true;

                    while (repeat)
                    {
                        Console.Write("Set no.of game-trace indicators for facet {0} (<={1}): ", CompetencyModel.Item3[x][y], SelectedIndicators.Length); //work for future to name the classes
                        input = Console.ReadLine();
                        repeat = Conversion.StringToInt(input);     //check if the input is an integer and handle exceptions        
                    }

                    StatisticalSubmodel[x][y] = new string[Convert.ToInt32(input)];

                    for (int i = 0; i < StatisticalSubmodel[x][y].Length; i++)
                    {
                        Console.Write("Available indicators: ");
                        for (int k = 0; k < SelectedIndicators.Length; k++)
                        {
                            Console.Write("[{0}]{1} ", k + 1, SelectedIndicators[k]);
                        }

                        Console.WriteLine();

                        //Browse indicators
                        int current_indicator = 0;
                        Console.WriteLine("Select indicator (Use arrows (up/down) to scroll and 'Enter' to select)");
                        Console.Write("{0}", SelectedIndicators[current_indicator]);

                        ConsoleKeyInfo keyInfo = Console.ReadKey();
                        while (keyInfo.Key != ConsoleKey.Enter)
                        {
                            keyInfo = Console.ReadKey();
                            if (keyInfo.Key == ConsoleKey.UpArrow)
                            {
                                current_indicator++;
                                if (current_indicator >= SelectedIndicators.Length)
                                {
                                    current_indicator = 0;
                                }
                                else if (current_indicator < 0)
                                {
                                    current_indicator = SelectedIndicators.Length - 1;
                                }
                                Console.Write("\r                ");
                                Console.Write("\r{0}", SelectedIndicators[current_indicator]);
                            }
                            else if (keyInfo.Key == ConsoleKey.DownArrow)
                            {
                                current_indicator--;
                                if (current_indicator >= SelectedIndicators.Length)
                                {
                                    current_indicator = 0;
                                }
                                else if (current_indicator < 0)
                                {
                                    current_indicator = SelectedIndicators.Length - 1;
                                }
                                Console.Write("\r                ");
                                Console.Write("\r{0}", SelectedIndicators[current_indicator]);
                            }
                        }

                        StatisticalSubmodel[x][y][i] = SelectedIndicators[current_indicator];
                        Console.WriteLine();
                    }
                }
            }

            Console.WriteLine();

            return new Tuple<string[], string[], string[][], string[][], string[][][], string[][][]>(SelectedMetadataMetrics, SelectedGametraceMetrics, NoDubsParsedIndicatorsOfSelectedMetadataMetrics, NoDubsParsedIndicatorsOfSelectedGametraceMetrics, EvidenceRules, StatisticalSubmodel);
        }

        //This function allows the user to determine the metrics to be used in the stealth assessment by browse them (with arrow keys) from an array of pre-loaded metrics from the game log file. 
        public string[] SelectMetrics(string[] SelectedMetrics, string [] AllMetrics)
        {

            for (int x = 0; x < SelectedMetrics.Length; x++)
            {
                int current_metric = 0;
                Console.WriteLine("Select metric no. {0} (Use arrows up/down and enter for choice): ", x + 1);
                Console.Write("{0}", AllMetrics[current_metric]);
                ConsoleKeyInfo keyInfo = Console.ReadKey();

                while (keyInfo.Key != ConsoleKey.Enter)
                {
                    keyInfo = Console.ReadKey();

                    if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        current_metric++;

                        if (current_metric >= AllMetrics.Length)
                        {
                            current_metric = 0;
                        }

                        else if (current_metric < 0)
                        {
                            current_metric = AllMetrics.Length - 1;
                        }

                        Console.Write("\r                ");
                        Console.Write("\r{0}", AllMetrics[current_metric]);

                    }

                    else if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        current_metric--;

                        if (current_metric >= AllMetrics.Length)
                        {
                            current_metric = 0;
                        }

                        else if (current_metric < 0)
                        {
                            current_metric = AllMetrics.Length - 1;
                        }

                        Console.Write("\r                ");
                        Console.Write("\r{0}", AllMetrics[current_metric]);
                    }
                }

                SelectedMetrics[x] = AllMetrics[current_metric];
                Console.WriteLine();
            }

            return SelectedMetrics;
        }

        //Set no. of metrics based on user input.
        public int SetNoOfMetrics(string[] AllMetrics, string MetricType)
        {
            int NoOfMetrics = 0; //Store no. of metrics as given by the user input.
            var Conversion = new Exceptions();
            bool check = true; //Check if the user's input length exceeds limitations 
            bool repeat = true;     //This variable checks if the user should re-enter input.

            while (check)
            {
                //Repeat user input in case of exceptions
                while (repeat)
                {
                    Console.Write("Set no. of {0} metrics to be used (<{1}): ", MetricType, AllMetrics.Length);
                    input = Console.ReadLine();
                    repeat = Conversion.StringToInt(input);     //check if the input is an integer and handle exceptions
                }

                //Check if the user's input length exceeds limitations 
                if (Convert.ToInt32(input) >= AllMetrics.Length)
                {
                    Console.WriteLine("No. of metrics exceed the no. of available metrics");
                    repeat = true;
                }

                else
                {
                    NoOfMetrics = Convert.ToInt32(input);
                    repeat = false;
                    check = false;
                }
            }

            return NoOfMetrics;
        }

        //Extract indicators for selected metrics.
        public string[][] ExtractIndicators (string[][] AllData, string[] SelectedMetrics)
        {
            string[][] ExtractedIndicators = new string[SelectedMetrics.Length][];

            for(int x=0; x<SelectedMetrics.Length; x++)
            {
                for(int y=0; y<AllData.Length; y++)
                {
                    if(SelectedMetrics[x]==AllData[y][0])   //Set to 0 for browsing only the metrics
                    {
                        ExtractedIndicators[x] = new string[AllData[y].Length-1];

                        for(int i=1; i<AllData[y].Length; i++)
                        {
                            ExtractedIndicators[x][i-1] = AllData[y][i];
                        }
                    }
                }
            }

            return ExtractedIndicators;
        }

        //Auto-parsing selected indicators where semicolons are found. (Optional for the future to allow user define other delimeter characters for parsing)
        public string[][] ParsingIndicators(string[][] UnparsedIndicatorsOfSelectedMetrics)
        {
            //Declaring Variables.
            string[][] Parsed = new string[UnparsedIndicatorsOfSelectedMetrics.Length][];
            char[] delimiterChars = { ':', '.' };
            int NoOfParsedIndicators = 0;

            for (int x = 0; x < UnparsedIndicatorsOfSelectedMetrics.Length; x++)
            {
                for (int y = 0; y < UnparsedIndicatorsOfSelectedMetrics[x].Length; y++)
                {
                    string lines = UnparsedIndicatorsOfSelectedMetrics[x][y];
                    string[] parsed = lines.Split(delimiterChars);
                    NoOfParsedIndicators += parsed.Length;
                }

                Parsed[x] = new string[NoOfParsedIndicators];
                NoOfParsedIndicators = 0;

                for (int i = 0; i < UnparsedIndicatorsOfSelectedMetrics[x].Length; i++)
                {
                    string lines = UnparsedIndicatorsOfSelectedMetrics[x][i];
                    string[] parsed = lines.Split(delimiterChars);

                    for (int q = 0; q < parsed.Length; q++)
                    {
                        Parsed[x][NoOfParsedIndicators] = parsed[q];
                        NoOfParsedIndicators++;
                    }
                }

                NoOfParsedIndicators = 0;
            }

            return Parsed;
        }

        //Remove dublicate values from parsed indicators.
        public string[][] RemoveDups(string[][] ParsedIndicatorsOfSelectedMetrics)
        {
            string[][] RemoveDups = new string[ParsedIndicatorsOfSelectedMetrics.Length][];

            for (int x = 0; x < ParsedIndicatorsOfSelectedMetrics.Length; x++)
            {
                string[] data = new string[ParsedIndicatorsOfSelectedMetrics[x].Length];

                for (int y = 0; y < ParsedIndicatorsOfSelectedMetrics[x].Length; y++)
                {
                    data[y] = ParsedIndicatorsOfSelectedMetrics[x][y];
                }

                string[] nodups = data.Distinct().ToArray();

                RemoveDups[x] = new string[nodups.Length];

                for (int i = 0; i < nodups.Length; i++)
                {
                    RemoveDups[x][i] = nodups[i];
                }
            }

            return RemoveDups;
        }
    }
}