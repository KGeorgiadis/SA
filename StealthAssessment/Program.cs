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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StealthAssessment
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //Declaring Variables.
            bool session = true;    //This variable allows the user to terminate the program or initiate a new stealth assessment.
            bool repeat = true;     //This variable checks if the user should re-enter input.
            string input = null;    //This variable stores the user's keyboard input.
            var Exceptions = new Exceptions();  //This variable allows access to functions of the Exceptions class.
            var ECD = new ECD();  //This variable allows access to functions of the ECD class.
            var Excel = new Excel();    //This variable allows access to functions of the Excel class.
            var BayesNet = new BayesNet();  //This variable allows access to functions of the BayesNet class.

            string[] Competencies = new string[] { };   //Stores competencies.
            string[] Facets = new string[] { };         //Stores facets unparsed per competency.
            string[][] InfluenceDiagram = new string[][] { };   //Stores the structure of the influence diagram.
            Tuple<string[], string[], string[][]> CompetencyModel = new Tuple<string[], string[], string[][]>(Competencies, Facets, InfluenceDiagram);  //Stores all the elements declared at the Competency Model.
            string[] TasksPerCompetency = new string[] { }; //Stores tasks unparsed per competency.
            string[][] TaskModels = new string[][] { };     //Stores the tasks structure(s) per competency.
            Tuple<string[], string[][]> TaskModel = new Tuple<string[], string[][]>(TasksPerCompetency, TaskModels);    //Stores all the elements declared at the Task Model.
            string[] SelectedMetadataMetrics = new string[] { };    //Stores selected meta-data metrics.
            string[] SelectedGametraceMetrics = new string[] { };   //Stores selected game-trace metrics.
            string[][] NoDubsParsedIndicatorsOfSelectedMetadataMetrics = new string[][] { };    //Stores indicators of selected meta-data metrics.
            string[][] NoDubsParsedIndicatorsOfSelectedGametraceMetrics = new string[][] { };    //Stores indicators of selected game-trace metrics.
            string[][][] EvidenceRules = new string[][][] { };          //Stores the Evidence Rules (mapping of indicators from selected game-trace metrics to tasks).
            string[][][] StatisticalSubmodel = new string[][][] { };    //Stores the Statistical Submodel (mapping of indicators from selected game-trace metrics to facets).
            Tuple<string[], string[], string[][], string[][], string[][][], string[][][]> EvidenceModel = 
                new Tuple<string[], string[], string[][], string[][], string[][][], string[][][]>
                (SelectedMetadataMetrics, SelectedGametraceMetrics, NoDubsParsedIndicatorsOfSelectedMetadataMetrics,
                NoDubsParsedIndicatorsOfSelectedGametraceMetrics, EvidenceRules, StatisticalSubmodel); //Stores all the elements declared at the Evidence Model.
            string[][] AllData = new string[][] { };    //Stores all the data from the game log file.    
            string[] QueryData = new string[] { }; //Stores the queried data.
            string[][] SelectedMetadataIndicators = new string[][] { }; //Contains the selected meta-data.
            Tuple<string[], string[][]> Query = new Tuple<string[], string[][]>(QueryData, SelectedMetadataIndicators); //Contains the queried data and the selected meta-data.
            string[][] ParsedQueryData = new string[][] { }; //Stores the parsed queried data.
            string[][][] ValidationMetricsFacets = new string[][][] { }; //Stores the validation data for every facet of each competency.
            string[][] ValidationMetricsCompetencies = new string[][] { }; //Stores the validation data for each competency.
            Tuple<string[][], string[][][]> ValidationData = new Tuple<string[][], string[][][]>(ValidationMetricsCompetencies, ValidationMetricsFacets);   //Contains the Validation data for each facet and competency.
            string[][][][] RecomposedGametraces = new string[][][][] { }; //Stores the recomposed game trace data per instance.
            string[][] ClassificationCompetencies = new string[][] { }; //Stores classification (Low, Medium, High) for the instances (validation data) of each competency.
            string[][][] ClassificationFacets = new string[][][] { }; //Stores classification (Low, Medium, High) for the instances (validation data) of each facet per competency.
            Tuple<string[][], string[][][]> ClassificationData = new Tuple<string[][], string[][][]> (ClassificationCompetencies, ClassificationFacets);


            //Core execution of the Stealth Assessment prototype.
            while (session)
            {
                Console.Write("Start a new session? (y/n):");
                input = Console.ReadLine();
                session = Exceptions.StringToChar(input);   //Check if the input is a char and handle exceptions.

                if (Convert.ToChar(input) == 'y' || Convert.ToChar(input) == 'Y')   //Check if the input is a 'y' or 'Y' character.
                {
                    repeat = true;

                    //Automatically set the ECD Model (Hardcoded version for Communication).
                    while (repeat)
                    {
                        Console.Write("Load an existing ECD Model? (y/n):");
                        input = Console.ReadLine();
                        repeat = Exceptions.StringToChar(input); //Check if the input is a char and handle exceptions.

                        if (Convert.ToChar(input) == 'y' || Convert.ToChar(input) == 'Y')
                        {
                            //AUTO-GENERETING THE COMPETENCY MODEL.
                            CompetencyModel = ECD.CompetencyModelAuto();

                                //Browsing Competency Model.
                                Console.Clear(); //Clear console.
                                Console.WriteLine("Competency Model");

                                for (int x = 0; x < CompetencyModel.Item3.Length; x++)
                                {                                
                                    Console.Write("[{0}]{1}: ", x+1, CompetencyModel.Item1[x]);
                                    Console.WriteLine();
                                    int count = 0;

                                    for (int y = 0; y < CompetencyModel.Item3[x].Length; y++)
                                    {
                                        Console.Write("[{0}]{1} ", count + 1, CompetencyModel.Item3[x][y]);
                                        count++;
                                    }
                                    Console.WriteLine();
                                }

                            //AUTO-GENERETING THE TASK MODEL.
                            TaskModel = ECD.TaskModelAuto(CompetencyModel);

                                //Browsing Task Model.
                                Console.WriteLine();
                                Console.WriteLine("Task Model");

                                for (int x = 0; x < CompetencyModel.Item3.Length; x++)
                                {
                                    Console.Write("[{0}]{1}: ", x+1, CompetencyModel.Item1[x]);
                                    Console.WriteLine();
                                    int count = 0;

                                    for (int y = 0; y < TaskModel.Item2[x].Length; y++)
                                    {
                                        Console.Write("[{0}]{1} ", count + 1, TaskModel.Item2[x][y]);
                                        count++;
                                    }
                                    Console.WriteLine();
                                }

                            //AUTO-GENERETING THE EVIDENCE MODEL.
                            EvidenceModel = ECD.EvidenceModelAuto(TaskModel, CompetencyModel);

                                //Browsing Evidence Model.
                                Console.WriteLine();
                                Console.WriteLine("Evidence Model");

                                //Browsing meta-data metrics.
                                int countMetadataMetrics = 0;
                                Console.WriteLine("Meta-data metrics");

                                for (int x = 0; x < EvidenceModel.Item1.Length; x++)
                                {
                                    Console.Write("[{0}]{1} ", countMetadataMetrics+1, EvidenceModel.Item1[x]);
                                    countMetadataMetrics++;
                                }

                                //Browsing game-trace metrics.
                                int countGametraceMetrics = 0;
                                Console.WriteLine();
                                Console.WriteLine();
                                Console.WriteLine("Game-trace metrics");

                                for (int x = 0; x < EvidenceModel.Item2.Length; x++)
                                {
                                    Console.Write("[{0}]{1} ", countGametraceMetrics+1, EvidenceModel.Item2[x]);
                                    countGametraceMetrics++;
                                }

                                //Browsing indicators of meta-data metrics.
                                Console.WriteLine();
                                Console.WriteLine();
                                Console.WriteLine("Indicators of meta-data metrics");

                                for(int x = 0; x < EvidenceModel.Item3.Length; x++)
                                {
                                    Console.Write("[{0}]{1}: ", x + 1, EvidenceModel.Item1[x]);
                                    Console.WriteLine();
                                    int count = 0;

                                    for (int y = 0; y < EvidenceModel.Item3[x].Length; y++)
                                    {
                                        Console.Write("[{0}]{1} ", count + 1, EvidenceModel.Item3[x][y]);
                                        count++;
                                    }

                                    Console.WriteLine();
                                }

                                //Browsing indicators of meta-data metrics.
                                Console.WriteLine();
                                Console.WriteLine("Indicators of game-trace metrics");

                                for (int x = 0; x < EvidenceModel.Item4.Length; x++)
                                {
                                    Console.Write("[{0}]{1}: ", x + 1, EvidenceModel.Item2[x]);
                                    Console.WriteLine();
                                    int count = 0;

                                    for (int y = 0; y < EvidenceModel.Item4[x].Length; y++)
                                    {
                                        Console.Write("[{0}]{1} ", count + 1, EvidenceModel.Item4[x][y]);
                                        count++;
                                    }

                                    Console.WriteLine();
                                }

                                //Browsing Evidence rules.
                                Console.WriteLine();
                                Console.WriteLine("Evidence Rules");

                                for(int x = 0; x < EvidenceModel.Item5.Length; x++)
                                {
                                    Console.Write("[{0}]{1}: ", x + 1, CompetencyModel.Item1[x]);
                                    Console.WriteLine();
                                    int countTasks = 0;

                                    for (int y = 0; y < EvidenceModel.Item5[x].Length; y++)
                                    {
                                        Console.Write("[{0}]{1}: ", countTasks + 1, TaskModel.Item2[x][y]);
                                        countTasks++;
                                        int countIndicators = 0;

                                        for (int i = 0; i < EvidenceModel.Item5[x][y].Length; i++)
                                        {
                                            Console.Write("[{0}]{1} ", countIndicators + 1, EvidenceModel.Item5[x][y][i]);
                                            countIndicators++;
                                        }

                                        Console.WriteLine();
                                    }

                                    Console.WriteLine();
                                }

                                //Browsing Statistical Sub-model.
                                Console.WriteLine();
                                Console.WriteLine("Statistical Sub-model");

                                for (int x = 0; x < EvidenceModel.Item6.Length; x++)
                                {
                                    Console.Write("[{0}]{1}: ", x + 1, CompetencyModel.Item1[x]);
                                    Console.WriteLine();
                                    int countFacets = 0;

                                    for (int y = 0; y < EvidenceModel.Item6[x].Length; y++)
                                    {
                                        Console.Write("[{0}]{1}: ", countFacets + 1, CompetencyModel.Item3[x][y]);
                                        countFacets++;
                                        int countIndicators = 0;

                                        for (int i = 0; i < EvidenceModel.Item6[x][y].Length; i++)
                                        {
                                            Console.Write("[{0}]{1} ", countIndicators + 1, EvidenceModel.Item6[x][y][i]);
                                            countIndicators++;
                                        }

                                        Console.WriteLine();

                                    }

                                    Console.WriteLine();
                                }

                            //Query data for Bayes Network.
                            Query = BayesNet.QueryAuto(EvidenceModel.Item1, EvidenceModel.Item2, EvidenceModel.Item3);
                            Console.WriteLine();

                            //Parse queried data.
                            ParsedQueryData = BayesNet.ParseQueryData(Query.Item1);
                            Console.WriteLine();

                            //Query validation data.
                            ValidationData = BayesNet.ValidationMetrics(EvidenceModel.Item1, Query.Item2, CompetencyModel);
                            Console.WriteLine();

                            //Generate .arff files.
                            BayesNet.GenerateArffFilesForClustering(ValidationData, CompetencyModel);
                            Console.WriteLine();

                            //Run density based clusterers on validation data (.arff) files.
                            ClassificationData = BayesNet.DensityBasedClusterer(CompetencyModel);
                            Console.WriteLine();

                            //Recompose the game-traces from queries based on the Statistical Submodel.
                            RecomposedGametraces = BayesNet.RecomposeGametraces(CompetencyModel, EvidenceModel, ParsedQueryData);
                            Console.WriteLine();

                            //Generates .arff files containing the classification data for each facet of the declared competencies.
                            BayesNet.GenerateArffFilesForClassificationOfFacets(ClassificationData, RecomposedGametraces, CompetencyModel, EvidenceModel);
                            Console.WriteLine();

                            //Runs a Bayesian Network for each declared facet.
                            BayesNet.BayesFacets(CompetencyModel);
                            Console.WriteLine();

                            //Generates .arff files containing the classification data for each declared competency.
                            BayesNet.GenerateArffFilesForClassificationOfCompetencies(ClassificationData, ParsedQueryData, CompetencyModel, EvidenceModel);
                            Console.WriteLine();

                            //Runs a Bayesian Network for each declared competency.
                            BayesNet.BayesCompetencies(CompetencyModel);
                            Console.WriteLine();

                            /**  
                             * FUTURE ADDITIONS
                             *  1) Factor analysis with Accord.NET
                            */

                            repeat = false;
                        }

                        else if (Convert.ToChar(input) == 'n' || Convert.ToChar(input) == 'N')
                        {
                            Console.Clear(); //Clear console.

                            //Manually set Competency Model.
                            CompetencyModel = ECD.CompetencyModelCustom();

                            //Manually set Task Model.
                            TaskModel = ECD.TaskModelCustom(CompetencyModel);

                            //Manually set Evidence Model.
                            EvidenceModel = ECD.EvidenceModelCustom(TaskModel, CompetencyModel);

                            //Query data for Bayes Network.
                            Query = BayesNet.QueryCustom(EvidenceModel.Item1, EvidenceModel.Item2, EvidenceModel.Item3);
                            Console.WriteLine();

                            //Parse queried data.
                            ParsedQueryData = BayesNet.ParseQueryData(Query.Item1);
                            Console.WriteLine();

                            //Query validation data.
                            ValidationData = BayesNet.ValidationMetrics(EvidenceModel.Item1, Query.Item2, CompetencyModel);
                            Console.WriteLine();

                            //Generate .arff files.
                            BayesNet.GenerateArffFilesForClustering(ValidationData, CompetencyModel);
                            Console.WriteLine();

                            //Run density based clusterers on validation data (.arff) files.
                            ClassificationData = BayesNet.DensityBasedClusterer(CompetencyModel);
                            Console.WriteLine();

                            //Recompose the game-traces from queries based on the Statistical Submodel.
                            RecomposedGametraces = BayesNet.RecomposeGametraces(CompetencyModel, EvidenceModel, ParsedQueryData);
                            Console.WriteLine();

                            //Generates .arff files containing the classification data for each facet of the declared competencies.
                            BayesNet.GenerateArffFilesForClassificationOfFacets(ClassificationData, RecomposedGametraces, CompetencyModel, EvidenceModel);
                            Console.WriteLine();

                            //Runs a Bayesian Network for each declared facet.
                            BayesNet.BayesFacets(CompetencyModel);
                            Console.WriteLine();

                            //Generates .arff files containing the classification data for each declared competency.
                            BayesNet.GenerateArffFilesForClassificationOfCompetencies(ClassificationData, ParsedQueryData, CompetencyModel, EvidenceModel);
                            Console.WriteLine();

                            //Runs a Bayesian Network for each declared competency.
                            BayesNet.BayesCompetencies(CompetencyModel);
                            Console.WriteLine();

                            /**  
                             * FUTURE ADDITIONS
                             *  1) Factor analysis with Accord.NET
                            */

                            repeat = false;
                        }

                        else
                        {
                            Console.WriteLine("Incorrect input. Allowed characters: 'y','Y','n','N'.");
                            repeat = true;
                        }
                    }

                    session = true;
                }

                else if (Convert.ToChar(input) == 'n' || Convert.ToChar(input) == 'N')  //Check if the input is a 'n' or 'N' character.
                {
                    session = false;
                }

                else
                {
                    Console.WriteLine("Incorrect input. Allowed characters: 'y','Y','n','N'.");
                    session = true;
                }
            }


            //Termination
            sw.Stop();
            TimeSpan elapsedTime = sw.Elapsed;
            string output = "Time: " + elapsedTime.ToString(@"dd\.hh\:mm\:ss\:ms");
            Console.WriteLine(output); //Execution time print

            Console.Write("Enter a key to terminate the program");
            Console.ReadKey();
        }
    }
}