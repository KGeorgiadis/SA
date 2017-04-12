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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using weka;
using weka.core;
using java.util;
using java;
using ikvm.runtime;
using java.io;
using weka.core.converters;


namespace StealthAssessment
{
    class BayesNet
    {
        string input = null;    //This variable stores the user's keyboard input.

        //Constructor.
        public BayesNet()
        {
            return;
        }

        //Destructor.
        ~BayesNet()
        {
            return;
        }

        //Loads all the data from the game log file in a jagged array.
        public string[][] LoadAllData ( string filename )
        {
            string[][] AllData = new string[][] { };
            var Excel = new Excel();

            Excel.ExcelOpen(filename);
            AllData = Excel.LoadData();
            System.Console.WriteLine("Data retrieved from game log file.");
            Excel.ExcelSave(filename);
            Excel.ExcelQuit();

            return AllData;
        }

        //Query the data to be used by the Bayes Network based on pre-settings.
        public Tuple<string[], string[][]> QueryAuto (string[] SelectedMetadataMetrics, string[] SelectedGametraceMetrics, string[][] NoDubsParsedIndicatorsOfSelectedMetadataMetrics)
        {
            string[][] SelectedMetadataIndicators = new string[SelectedMetadataMetrics.Length][]; //Stores selected meta-data indicators.
            string[] LoadMetadataIndicators= ConfigurationManager.AppSettings["Selected indicators of Meta-data Metrics"].Split(':').Select(s => s.Trim()).ToArray(); //Stores pre-selected meta-data metrics from the configuration file.

            System.Console.WriteLine("Query data for Bayes Network");

            //Load all data.
            string[][] AllData = new string[][] { };    //Stores all data from the game log file.
            AllData = LoadAllData("GameTraces");    //Load all the data from the game log file.

            for (int x=0; x<LoadMetadataIndicators.Length; x++)
            {
                string[] ParsedLoadMetadataIndicators=LoadMetadataIndicators[x].Split(',').Select(s => s.Trim()).ToArray();
                SelectedMetadataIndicators[x] = new string[ParsedLoadMetadataIndicators.Length];
                for (int y = 0; y < SelectedMetadataIndicators[x].Length; y++)
                {
                    SelectedMetadataIndicators[x][y] = ParsedLoadMetadataIndicators[y];
                }
            }

            //Query for the selected meta-data indicators to find number of matching queries.
            int noQueries = 0;


            for (int x = 0; x < SelectedMetadataMetrics.Length; x++)
            {
                for (int y = 0; y < AllData.Length; y++)
                {
                    if (SelectedMetadataMetrics[x] == AllData[y][0])
                    {
                        for (int i = 0; i < SelectedMetadataIndicators[x].Length; i++)
                        {
                            for (int k = 0; k < AllData[y].Length; k++)
                            {
                                if (SelectedMetadataIndicators[x][i] == AllData[y][k])
                                {
                                    for (int e = 0; e < SelectedGametraceMetrics.Length; e++)
                                    {
                                        for (int u = 0; u < AllData.Length; u++)
                                        {
                                            if (SelectedGametraceMetrics[e] == AllData[u][0])
                                            {
                                                noQueries++;    //Count number of queries found.
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            string[] QueryData = new string[noQueries]; //Stores the queried data.
            noQueries = 0;

            //Query for the selected meta-data indicators to store the matching queries.
            for (int x = 0; x < SelectedMetadataMetrics.Length; x++)
            {
                for (int y = 0; y < AllData.Length; y++)
                {
                    if (SelectedMetadataMetrics[x] == AllData[y][0])
                    {
                        for (int i = 0; i < SelectedMetadataIndicators[x].Length; i++)
                        {
                            for (int k = 0; k < AllData[y].Length; k++)
                            {
                                if (SelectedMetadataIndicators[x][i] == AllData[y][k])
                                {
                                    for (int e = 0; e < SelectedGametraceMetrics.Length; e++)
                                    {
                                        for (int u = 0; u < AllData.Length; u++)
                                        {
                                            if (SelectedGametraceMetrics[e] == AllData[u][0])
                                            {
                                                QueryData[noQueries] = AllData[u][k];
                                                noQueries++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return new Tuple<string[], string[][]> (QueryData, SelectedMetadataIndicators);
        }

        //Query the data to be used by the Bayes Network based on user selections.
        public Tuple<string[], string[][]> QueryCustom (string[] SelectedMetadataMetrics, string[] SelectedGametraceMetrics, string[][] NoDubsParsedIndicatorsOfSelectedMetadataMetrics)
        {
            bool repeat = true;     //This variable checks if the user should re-enter input.
            var Conversion = new Exceptions();  //This variable allows access to functions of the Exceptions class.
            string[][] SelectedMetadataIndicators = new string[SelectedMetadataMetrics.Length][]; //Stores selected meta-data indicators

            System.Console.WriteLine("Query data for Bayes Network");

            //Load all data.
            string[][] AllData = new string[][] { };    //Stores all data from the game log file.
            AllData = LoadAllData("GameTraces");               //Load all the data from the game log file.

            //Query for the selected meta-data metrics.
            for(int x=0; x< SelectedMetadataMetrics.Length; x++)
            {
                repeat = true;

                //Browse available meta-data indicators based on prior metric selection.
                System.Console.Write("Available meta-data indicators for {0}: ", SelectedMetadataMetrics[x]);
                for(int y=0; y<NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x].Length; y++)
                {
                    System.Console.Write("[{0}] {1} ", y+1, NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x][y]);
                }
                System.Console.WriteLine();

                //User defines no. of meta-data indicators to be used for quering the logged game data.
                while (repeat)
                {
                    System.Console.WriteLine("Select the no. of meta-data indicators to be used (<{0}): ", NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x].Length);
                    input = System.Console.ReadLine();
                    repeat = Conversion.StringToInt(input);     //check if the input is an integer and handle exceptions.     

                    //Check if input exceeds the no. of available indicators.
                    if (repeat == false)
                    {
                        if (Convert.ToInt32(input) > NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x].Length)
                        {
                            System.Console.WriteLine("Input exceeds the no. of available indicators.");
                            repeat = true;
                        }
                    }
                }

                SelectedMetadataIndicators[x] = new string[Convert.ToInt32(input)];

                //User selects meta-data indicators based on prior metric selection.
                for (int i = 0; i < SelectedMetadataIndicators[x].Length; i++)
                {
                    //Browse indicators
                    int current_indicator = 0;
                    System.Console.WriteLine("Select indicator (Use arrows (up/down) to scroll and 'Enter' to select): ");
                    System.Console.Write("{0}", NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x][current_indicator]);

                    ConsoleKeyInfo keyInfo = System.Console.ReadKey();
                    while (keyInfo.Key != ConsoleKey.Enter)
                    {
                        keyInfo = System.Console.ReadKey();
                        if (keyInfo.Key == ConsoleKey.UpArrow)
                        {
                            current_indicator++;
                            if (current_indicator >= NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x].Length)
                            {
                                current_indicator = 0;
                            }
                            else if (current_indicator < 0)
                            {
                                current_indicator = NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x].Length - 1;
                            }
                            System.Console.Write("\r                ");
                            System.Console.Write("\r{0}", NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x][current_indicator]);
                        }
                        else if (keyInfo.Key == ConsoleKey.DownArrow)
                        {
                            current_indicator--;
                            if (current_indicator >= NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x].Length)
                            {
                                current_indicator = 0;
                            }
                            else if (current_indicator < 0)
                            {
                                current_indicator = NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x].Length - 1;
                            }
                            System.Console.Write("\r                ");
                            System.Console.Write("\r{0}", NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x][current_indicator]);
                        }
                    }

                    SelectedMetadataIndicators[x][i] = NoDubsParsedIndicatorsOfSelectedMetadataMetrics[x][current_indicator];
                    System.Console.WriteLine();
                }
            }

            //Query for the selected meta-data indicators to find number of matching queries.
            int noQueries = 0;


            for(int x=0; x<SelectedMetadataMetrics.Length; x++)
            {
                for(int y=0; y<AllData.Length; y++)
                {
                    if (SelectedMetadataMetrics[x] == AllData[y][0])
                    {
                        for(int i=0; i<SelectedMetadataIndicators[x].Length; i++)
                        {
                            for(int k=0; k<AllData[y].Length; k++)
                            {
                                if(SelectedMetadataIndicators[x][i]==AllData[y][k])
                                {
                                    for(int e=0; e<SelectedGametraceMetrics.Length; e++)
                                    {
                                        for(int u=0; u<AllData.Length; u++)
                                        {
                                            if(SelectedGametraceMetrics[e]==AllData[u][0])
                                            {
                                                noQueries++;    //Count number of queries found.
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            string[] QueryData = new string[noQueries]; //Stores the queried data.
            noQueries = 0;

            //Query for the selected meta-data indicators to store the matching queries.
            for (int x = 0; x < SelectedMetadataMetrics.Length; x++)
            {
                for (int y = 0; y < AllData.Length; y++)
                {
                    if (SelectedMetadataMetrics[x] == AllData[y][0])
                    {
                        for (int i = 0; i < SelectedMetadataIndicators[x].Length; i++)
                        {
                            for (int k = 0; k < AllData[y].Length; k++)
                            {
                                if (SelectedMetadataIndicators[x][i] == AllData[y][k])
                                {
                                    for (int e = 0; e < SelectedGametraceMetrics.Length; e++)
                                    {
                                        for (int u = 0; u < AllData.Length; u++)
                                        {
                                            if (SelectedGametraceMetrics[e] == AllData[u][0])
                                            {
                                                QueryData[noQueries] = AllData[u][k];
                                                noQueries++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return new Tuple<string[], string[][]> (QueryData, SelectedMetadataIndicators);
        }

        //Parses the queried data for any semicolon found.
        public string[][] ParseQueryData (string[] QueryData)
        {
            string[][] ParsedQueryData = new string[QueryData.Length][];     //Stores the parsed queried data.

            System.Console.WriteLine("Parse queried data.");

            for (int x=0; x<ParsedQueryData.Length; x++)
            {
                string[] ParsedInstance=QueryData[x].Split(':').Select(s => s.Trim()).ToArray();     //Stores temporarily each instance of the parsed queried data.
                ParsedQueryData[x] = new string[ParsedInstance.Length];

                for(int y=0; y<ParsedQueryData[x].Length; y++)
                {
                    ParsedQueryData[x][y] = ParsedInstance[y];
                }
            }

            return ParsedQueryData;
        }

        public Tuple<string[][], string[][][]> ValidationMetrics (string[] SelectedMetadataMetrics, string[][] SelectedMetadataIndicators, Tuple<string[], string[], string[][]> CompetencyModel)
        {
            string[][][] ValidationMetricsFacets = new string[CompetencyModel.Item1.Length][][]; //Stores the validation data for every facet of each competency.
            string[][] ValidationMetricsCompetencies = new string[CompetencyModel.Item1.Length][]; //Stores the validation data for each competency.

            System.Console.WriteLine("Extract validation data.");

            //Load all data.
            string[][] AllData = new string[][] { };    //Stores all data from the game log file.
            AllData = LoadAllData("GameTraces");    //Load all the data from the game log file.

            //Query for the selected meta-data indicators to find number of matching queries.
            int[] noQueriesForCompetencies = new int[CompetencyModel.Item1.Length];
            int[][] noQueriesForFacets = new int[CompetencyModel.Item1.Length][];

            //Initialize (zero value) the arrays that store the no of Queries per competency and per facet. 
            for(int x=0; x<noQueriesForCompetencies.Length; x++)
            {
                noQueriesForCompetencies[x] = 0;
                noQueriesForFacets[x] = new int [CompetencyModel.Item3[x].Length];

                for(int y=0; y<noQueriesForFacets[x].Length; y++)
                {
                    noQueriesForFacets[x][y] = 0;
                }
            }

            //Query for the selected meta-data indicators to find the number of matching queries for validating data of competencies.
            for (int e = 0; e < CompetencyModel.Item3.Length; e++)
            {
                    for (int x = 0; x < SelectedMetadataMetrics.Length; x++)
                    {
                        for (int y = 0; y < AllData.Length; y++)
                        {
                            if (SelectedMetadataMetrics[x] == AllData[y][0])
                            {
                                for (int i = 0; i < SelectedMetadataIndicators[x].Length; i++)
                                {
                                    for (int k = 0; k < AllData[y].Length; k++)
                                    {
                                        if (SelectedMetadataIndicators[x][i] == AllData[y][k])
                                        {
                                            for (int u = 0; u < AllData.Length; u++)
                                            {
                                                if (CompetencyModel.Item1[e] == AllData[u][0])
                                                {
                                                    noQueriesForCompetencies[e]++;    //Count number of queries found.
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                
                ValidationMetricsCompetencies[e] = new string [noQueriesForCompetencies[e]]; //Stores the queried data.
                noQueriesForCompetencies[e] = 0;
            }

            //Query for the selected meta-data indicators to find the matching queries for validating data of competencies.
            for (int e = 0; e < ValidationMetricsCompetencies.Length; e++)
            {
                for (int x = 0; x < SelectedMetadataMetrics.Length; x++)
                {
                    for (int y = 0; y < AllData.Length; y++)
                    {
                        if (SelectedMetadataMetrics[x] == AllData[y][0])
                        {
                            for (int i = 0; i < SelectedMetadataIndicators[x].Length; i++)
                            {
                                for (int k = 0; k < AllData[y].Length; k++)
                                {
                                    if (SelectedMetadataIndicators[x][i] == AllData[y][k])
                                    {
                                        for (int u = 0; u < AllData.Length; u++)
                                        {
                                            if (CompetencyModel.Item1[e] == AllData[u][0])
                                            {
                                                ValidationMetricsCompetencies[e][noQueriesForCompetencies[e]] = AllData[u][k];
                                                noQueriesForCompetencies[e]++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                noQueriesForCompetencies[e] = 0; ;
            }


            //Query for the selected meta-data indicators to find the number of matching queries for validating data of facets.
            for (int e = 0; e < CompetencyModel.Item3.Length; e++)
            {
                ValidationMetricsFacets[e] = new string[CompetencyModel.Item3[e].Length][];

                for (int f = 0; f < CompetencyModel.Item3[e].Length; f++)
                {
                    for (int x = 0; x < SelectedMetadataMetrics.Length; x++)
                    {
                        for (int y = 0; y < AllData.Length; y++)
                        {
                            if (SelectedMetadataMetrics[x] == AllData[y][0])
                            {
                                for (int i = 0; i < SelectedMetadataIndicators[x].Length; i++)
                                {
                                    for (int k = 0; k < AllData[y].Length; k++)
                                    {
                                        if (SelectedMetadataIndicators[x][i] == AllData[y][k])
                                        {
                                            for (int u = 0; u < AllData.Length; u++)
                                            {
                                                if (CompetencyModel.Item3[e][f] == AllData[u][0])
                                                {
                                                    noQueriesForFacets[e][f]++;    //Count number of queries found.
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    ValidationMetricsFacets[e][f] = new string[noQueriesForFacets[e][f]];
                    noQueriesForFacets[e][f] = 0;
                }
            }

            //Query for the selected meta-data indicators to find the matching queries for validating data of facets.
            for (int e = 0; e < ValidationMetricsFacets.Length; e++)
            {
                for (int f = 0; f < ValidationMetricsFacets[e].Length; f++)
                {
                    for (int x = 0; x < SelectedMetadataMetrics.Length; x++)
                    {
                        for (int y = 0; y < AllData.Length; y++)
                        {
                            if (SelectedMetadataMetrics[x] == AllData[y][0])
                            {
                                for (int i = 0; i < SelectedMetadataIndicators[x].Length; i++)
                                {
                                    for (int k = 0; k < AllData[y].Length; k++)
                                    {
                                        if (SelectedMetadataIndicators[x][i] == AllData[y][k])
                                        {
                                            for (int u = 0; u < AllData.Length; u++)
                                            {
                                                if (CompetencyModel.Item3[e][f] == AllData[u][0])
                                                {
                                                    ValidationMetricsFacets[e][f][noQueriesForFacets[e][f]] = AllData[u][k];
                                                    noQueriesForFacets[e][f]++;    //Count number of queries found.
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    noQueriesForFacets[e][f] = 0;
                }
            }

            return new Tuple<string[][], string[][][]> (ValidationMetricsCompetencies, ValidationMetricsFacets);
        }

        public void GenerateArffFiles(Tuple<string[][], string[][][]> ValidationData)
        {
            java.util.ArrayList atts;
            Instances data;
            double[] vals;

            System.Console.WriteLine("Generating .arff files");

            // 1. set up attributes
            atts = new java.util.ArrayList();
            atts.Add(new weka.core.Attribute("Validation data"));

            // 2. create Instances object
            data = new Instances("MyRelation", atts, 0);

            // 3. fill with data
            vals = new double[data.numAttributes()];
            vals[0] = Math.E;

            // add
            weka.core.Instance inst = new DenseInstance(1);
            inst.setValue(0,vals[0]);
            data.add(inst);


            // 4. output data
            for (int x = 0; x < data.numInstances(); x++)
            {
                weka.core.Instance ins = data.instance(x);
                System.Console.WriteLine(ins.value(x).ToString());
            }

            // 5. save data
            weka.core.converters.ArffSaver saver = new weka.core.converters.ArffSaver();
            saver.setInstances(data);
            saver.setFile(new File("./data/test.arff"));
            //saver.setDestination(new File("./data/test.arff"));
            saver.writeBatch();

            return;
        }

        /**  
         * FUTURE ADDITIONS FOR BAYESIAN NETWORK
         *  //Create .arff files for queried data
         *  //Recompose indicators for each facet
         *  //Find incomplete datasets and exceeding indicators
         *  1) Cluster
         *  2) Training (select sample size and calculate the prior probabilities tables from the queried data).
         *  3) Testing (select testing sample and run Bayes Net for results).
         *  4) Save and export results.
        */

    }
}
