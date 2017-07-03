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
using weka.classifiers.meta;
using weka.attributeSelection;
using weka.classifiers.evaluation;
using weka.clusterers;


namespace StealthAssessment
{
    class BayesNet
    {
        string input = null;    //This variable stores the user's keyboard input.
        const int percentSplit = 66;    //Splits the dataset for the Bayesian Network to 66% training and 34% testing.

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
                string[] ParsedInstance=QueryData[x].Split(':','.').Select(s => s.Trim()).ToArray();     //Stores temporarily each instance of the parsed queried data.
                ParsedQueryData[x] = new string[ParsedInstance.Length];

                for(int y=0; y<ParsedQueryData[x].Length; y++)
                {
                    ParsedQueryData[x][y] = ParsedInstance[y];
                }
            }

            return ParsedQueryData;
        }

        //Query the validation data for each facet and competency declared.
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

        //Generates .arff files containing the validation data for each facet and competency.
        public void GenerateArffFilesForClustering (Tuple<string[][], string[][][]> ValidationData, Tuple<string[], string[], string[][]> CompetencyModel)
        {
            //Declaring Variables.
            bool repeat = true;     //This variable checks if the user should re-enter input.
            var Conversion = new Exceptions();
            java.util.ArrayList atts;
            Instances data;
            double[] vals;

            System.Console.WriteLine("Generating .arff files for clustering.");

            //Generating .arff files for competencies
            for (int x=0; x<CompetencyModel.Item1.Length; x++)
            {
                // Set up attributes
                atts = new java.util.ArrayList();
                atts.Add(new weka.core.Attribute("Validation data for " + CompetencyModel.Item1[x]));

                // Create Instances object
                data = new Instances(CompetencyModel.Item1[x], atts, 0);

                // Fill with data
                vals = new double[ValidationData.Item1[x].Length];
                for(int y=0; y<ValidationData.Item1[x].Length; y++)
                {
                    while (repeat)
                    {
                        repeat = Conversion.StringToInt(ValidationData.Item1[x][y]);     //check if the input is an integer and handle exceptions        
                    }
                    repeat = true;
                    vals[y] = Convert.ToInt32(ValidationData.Item1[x][y]);
                }


                // Add data
                for(int y=0; y<vals.Length; y++)
                {
                    weka.core.Instance inst = new DenseInstance(1);
                    inst.setValue(0, vals[y]);
                    data.add(inst);
                }

                // Save data
                weka.core.converters.ArffSaver saver = new weka.core.converters.ArffSaver();
                saver.setInstances(data);
                saver.setFile(new File("./data/Clustering/"+ CompetencyModel.Item1[x] + ".arff"));
                //saver.setDestination(new File("./data/test.arff"));
                saver.writeBatch();
                System.Console.WriteLine("Validation data saved in {0}.arff file successfully.", CompetencyModel.Item1[x]);
            }

            //Generating .arff files for facets
            for (int x = 0; x < CompetencyModel.Item3.Length; x++)
            {
                for(int i = 0; i < CompetencyModel.Item3[x].Length; i++)
                {
                    // Set up attributes
                    atts = new java.util.ArrayList();
                    atts.Add(new weka.core.Attribute("Validation data for " + CompetencyModel.Item3[x][i]));

                    // Create Instances object
                    data = new Instances(CompetencyModel.Item3[x][i], atts, 0);

                    // Fill with data
                    vals = new double[ValidationData.Item2[x][i].Length];
                    for (int y = 0; y < ValidationData.Item2[x][i].Length; y++)
                    {
                        while (repeat)
                        {
                            repeat = Conversion.StringToInt(ValidationData.Item2[x][i][y]);     //check if the input is an integer and handle exceptions        
                        }
                        repeat = true;
                        vals[y] = Convert.ToInt32(ValidationData.Item2[x][i][y]);
                    }


                    // Add data
                    for (int y = 0; y < vals.Length; y++)
                    {
                        weka.core.Instance inst = new DenseInstance(1);
                        inst.setValue(0, vals[y]);
                        data.add(inst);
                    }

                    // Save data
                    weka.core.converters.ArffSaver saver = new weka.core.converters.ArffSaver();
                    saver.setInstances(data);
                    saver.setFile(new File("./data/Clustering/" + CompetencyModel.Item3[x][i] + ".arff"));
                    //saver.setDestination(new File("./data/test.arff"));
                    saver.writeBatch();
                    System.Console.WriteLine("Validation data saved in {0}.arff file successfully.", CompetencyModel.Item3[x][i]);
                }
            }

            return;
        }

        //Runs Density Based Clusterer for each facet and competency using validation data from the .arff files and stores statistics for each clusterer in .txt files.
        public Tuple<string[][], string[][][]> DensityBasedClusterer (Tuple<string[], string[], string[][]> CompetencyModel)
        {
            //Declaring Variables.
            bool repeat = true;     //This variable checks if the user should re-enter input.
            var Conversion = new Exceptions();
            string[][] ClassificationCompetencies = new string[CompetencyModel.Item1.Length][]; //Stores classification (Low, Medium, High) for the instances (validation data) of each competency.
            string[][][] ClassificationFacets = new string[CompetencyModel.Item1.Length][][]; //Stores classification (Low, Medium, High) for the instances (validation data) of each facet per competency.

            System.Console.WriteLine("Running density based clusterers.");

            try
            {
                //Run the desnity based clusterer for given competencies
                for(int x = 0; x<CompetencyModel.Item1.Length; x++)
                {
                    Instances dataComp = new Instances(new java.io.FileReader("./data/Clustering/" + CompetencyModel.Item1[x] + ".arff"));
                    ClassificationCompetencies[x] = new string[dataComp.numInstances()];
                    ClassificationFacets[x] = new string[CompetencyModel.Item3[x].Length][];

                    MakeDensityBasedClusterer clustererComp = new MakeDensityBasedClusterer();

                    // Option Setup               
                    clustererComp.setNumClusters(3);
                    clustererComp.buildClusterer(dataComp);

                    ClusterEvaluation evalComp = new ClusterEvaluation();
                    evalComp.setClusterer(clustererComp);
                    evalComp.evaluateClusterer(dataComp);

                    //Find centroids for each cluster
                    string results = clustererComp.getClusterer().ToString();
                    results = string.Join(" ", results.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
                    string[] resultsParsed = results.Split(' ').Select(s => s.Trim()).ToArray();
                    string[] centroids = new string[clustererComp.numberOfClusters()];
                    Array.Reverse(resultsParsed);
                    string[] tempReverse = new string[3];
                    for (int a = 0; a < tempReverse.Length; a++)
                    {
                        tempReverse[a] = resultsParsed[a];
                    }
                    Array.Reverse(tempReverse);

                    for (int u = 0; u < centroids.Length; u++)
                    {
                        centroids[u] = tempReverse[u];
                        centroids[u] = centroids[u].Replace('.', ',');
                    }
                    
                    double[] convertedToIntCentroids = new double[centroids.Length];

                    for (int v = 0; v < centroids.Length; v++)
                    {
                        while (repeat)
                        {
                            repeat = Conversion.StringToDouble(centroids[v]);     //check if the input is an integer and handle exceptions        
                        }
                        convertedToIntCentroids[v] = Convert.ToDouble(centroids[v]);
                        repeat = true;
                    }

                    double High = convertedToIntCentroids.Max();   //Stores maximum centroid value
                    double Low = convertedToIntCentroids.Min();    //Stores minimum centroid value

                    //Assign a name to the cluster based on its centroid value
                    string[] ClusterNamesForCentroids = new string[convertedToIntCentroids.Length];

                    for (int v = 0; v < convertedToIntCentroids.Length; v++)
                    {
                        if (convertedToIntCentroids[v] == High)
                        {
                            ClusterNamesForCentroids[v] = "High";
                        }
                        else if (convertedToIntCentroids[v] == Low)
                        {
                            ClusterNamesForCentroids[v] = "Low";
                        }
                        else
                        {
                            ClusterNamesForCentroids[v] = "Medium";
                        }
                    }

                    string[] resultsCompetenciesParsed = evalComp.clusterResultsToString().Split('\n').Select(s => s.Trim()).ToArray(); //Parse clusterer results                  

                    //Save clusterer results in .txt file.
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter("./data/Clustering/" + CompetencyModel.Item1[x] + "_Report.txt"))
                    {
                        file.WriteLine("Clusterer for {0}", CompetencyModel.Item1[x]);
                        file.WriteLine();

                        foreach (string line in resultsCompetenciesParsed)
                        {
                            file.WriteLine(line);
                        }
                        file.WriteLine();

                        for (int v = 0; v < ClusterNamesForCentroids.Length; v++)
                        {
                            file.WriteLine("Cluster [{0}] is classified as {1}", v, ClusterNamesForCentroids[v]);
                        }
                        file.WriteLine();

                        // Print Prior probabilities for each cluster.
                        for (int k = 0; k < dataComp.numInstances(); k++)
                        {
                            weka.core.Instance currentInst = dataComp.instance(k);
                            int predictedCluster = clustererComp.clusterInstance(currentInst);

                            double[] Priors = clustererComp.distributionForInstance(currentInst);
                            ClassificationCompetencies[x][k] = ClusterNamesForCentroids[predictedCluster];

                            for (int y = 0; y < Priors.Length; y++)
                            {
                                file.WriteLine("Prior probability of cluster [{0}] for instance [{1}] is: {2}", y, currentInst, Math.Round(Priors[y], 4));
                            }

                            file.WriteLine("Instance [{0}] is classified as {1}", currentInst, ClassificationCompetencies[x][k]);
                            file.WriteLine();
                        }

                        System.Console.WriteLine("Clusterer results saved in {0}_Report.txt file successfully", CompetencyModel.Item1[x]);
                    }

                    //Run the desnity based clusterer for given facets per competency.
                    for (int y=0; y<CompetencyModel.Item3[x].Length; y++)
                    {


                        Instances dataFacets = new Instances(new java.io.FileReader("./data/Clustering/" + CompetencyModel.Item3[x][y] + ".arff"));
                        ClassificationFacets[x][y] = new string[dataFacets.numInstances()];
                        MakeDensityBasedClusterer clustererFacets = new MakeDensityBasedClusterer();

                        // Option Setup.                
                        clustererFacets.setNumClusters(3);
                        clustererFacets.buildClusterer(dataFacets);

                        ClusterEvaluation evalFacets = new ClusterEvaluation();
                        evalFacets.setClusterer(clustererFacets);
                        evalFacets.evaluateClusterer(dataFacets);

                        //Find centroids for each cluster.
                        string resultsfacets = clustererFacets.getClusterer().ToString();
                        resultsfacets = string.Join(" ", resultsfacets.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
                        string[] resultsfacetsParsed = resultsfacets.Split(' ').Select(s => s.Trim()).ToArray();
                        Array.Reverse(resultsfacetsParsed);
                        string[] tempReverseFacets = new string[clustererFacets.numberOfClusters()];
                        for(int a=0; a< tempReverseFacets.Length; a++)
                        {
                            tempReverseFacets[a] = resultsfacetsParsed[a];
                        }
                        Array.Reverse(tempReverseFacets);

                        string[] centroidsFacets = new string[clustererFacets.numberOfClusters()];

                        for (int u = 0; u < centroidsFacets.Length; u++)
                        {
                            centroidsFacets[u] = tempReverseFacets[u];
                            centroidsFacets[u] = centroidsFacets[u].Replace('.', ',');
                        }

                        double[] convertedToIntCentroidsFacets = new double[centroidsFacets.Length];

                        for (int v = 0; v < centroidsFacets.Length; v++)
                        {
                            while (repeat)
                            {
                                repeat = Conversion.StringToDouble(centroidsFacets[v]);     //check if the input is an integer and handle exceptions        
                            }
                            convertedToIntCentroidsFacets[v] = Convert.ToDouble(centroidsFacets[v]);
                            repeat = true;
                        }

                        double HighFacet = convertedToIntCentroidsFacets.Max();   //Stores maximum centroid value
                        double LowFacet = convertedToIntCentroidsFacets.Min();    //Stores minimum centroid value

                        //Assign a name to the cluster based on its centroid value
                        string[] ClusterNamesForCentroidsFacets = new string[convertedToIntCentroidsFacets.Length];

                        for (int v = 0; v < convertedToIntCentroidsFacets.Length; v++)
                        {
                            if (convertedToIntCentroidsFacets[v] == HighFacet)
                            {
                                ClusterNamesForCentroidsFacets[v] = "High";
                            }
                            else if (convertedToIntCentroidsFacets[v] == LowFacet)
                            {
                                ClusterNamesForCentroidsFacets[v] = "Low";
                            }
                            else
                            {
                                ClusterNamesForCentroidsFacets[v] = "Medium";
                            }
                        }

                        string[] resultfacetsParsed = evalFacets.clusterResultsToString().Split('\n').Select(s => s.Trim()).ToArray(); //Parse clusterer results   

                        //Print all results for clusterer as in Weka
                        //Save clusterer results in .txt file
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter("./data/Clustering/" + CompetencyModel.Item3[x][y] + "_Report.txt"))
                        {

                            file.WriteLine("Clusterer for {0}", CompetencyModel.Item3[x][y]);
                            file.WriteLine();

                            foreach (string line in resultfacetsParsed)
                            {
                                file.WriteLine(line);
                            }
                            file.WriteLine();


                            for (int v = 0; v < ClusterNamesForCentroidsFacets.Length; v++)
                            {
                                file.WriteLine("Cluster [{0}] is classified as {1}", v, ClusterNamesForCentroidsFacets[v]);
                            }
                            file.WriteLine();

                            // Print Prior probabilities for each cluster
                            for (int k = 0; k < dataFacets.numInstances(); k++)
                            {
                                weka.core.Instance currentInst = dataFacets.instance(k);
                                int predictedCluster = clustererFacets.clusterInstance(currentInst);
                                double[] Priors = clustererFacets.distributionForInstance(currentInst);
                                ClassificationFacets[x][y][k] = ClusterNamesForCentroidsFacets[predictedCluster];

                                for (int l = 0; l < Priors.Length; l++)
                                {
                                    file.WriteLine("Prior probability of cluster [{0}] for instance [{1}] is: {2}", l, currentInst, Math.Round(Priors[l], 4));
                                }

                                file.WriteLine("Instance [{0}] is classified as {1}", currentInst, ClassificationFacets[x][y][k]);
                                file.WriteLine();
                            }

                            System.Console.WriteLine("Clusterer results saved in {0}_Report.txt file successfully.", CompetencyModel.Item3[x][y]);
                        }
                    }
                }
            }

            catch (java.lang.Exception ex)
            {
                ex.printStackTrace();
            }

            return new Tuple<string[][], string[][][]>(ClassificationCompetencies, ClassificationFacets);
        }

        //Recompose the game-traces from queries based on the Statistical Submodel.
        public string[][][][] RecomposeGametraces (Tuple<string[], string[], string[][]> CompetencyModel, Tuple<string[], string[], string[][], string[][], string[][][], string[][][]> EvidenceModel, string[][] ParsedQueryData)
        {
            //Declared variables.
            string[][] ExceedingData = new string[ParsedQueryData.Length][]; //Stores all the indicators from the queried data that do not match with the given Statistical Submodel.
            string[][] NonExceedingData = new string[ParsedQueryData.Length][]; //Stores all the indicators from the queried data that do not match with the given Statistical Submodel.
            string[][][][] RecomposedGametraces = new string[ParsedQueryData.Length][][][]; //Stores the recomposed game trace data per instance.
            int FoundValues = 0;   //Checks if values where found for each facet of each competency per instance.
            int MissingValues = 0;  //Stores the number of missing data for each facet per competency.
            int IncompleteDatasets = 0; //Stores the number of incomplete datasets. 
            int ExceedingValues = 0;    //Stores the number of exceeding data.
            int NonExceedingValues = 0;    //Stores the number of exceeding data.
            string[] NonUsedData = new string[] { }; //Stores data that was not declared as an indicator of the game-trace metrics but was used the user.
            int NonUsedValues = 0;  //Stores the number of non-used data. 

            System.Console.WriteLine("Recomposing the game-traces from queries based on the Statistical Submodel.");

            //Save clusterer results in .txt file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("./data/Clustering/Recompose_Report.txt"))
            {
                //Recompose game-trace data for every instance for the given Statistical Submodel.
                for (int x = 0; x < ParsedQueryData.Length; x++)
                {
                    RecomposedGametraces[x] = new string[EvidenceModel.Item6.Length][][];
                    for (int i = 0; i < EvidenceModel.Item6.Length; i++)
                    {
                        RecomposedGametraces[x][i] = new string[EvidenceModel.Item6[i].Length][];
                        for (int k = 0; k < EvidenceModel.Item6[i].Length; k++)
                        {
                            for (int y = 0; y < ParsedQueryData[x].Length; y++)
                            {
                                for (int e = 0; e < EvidenceModel.Item6[i][k].Length; e++)
                                {
                                    if (EvidenceModel.Item6[i][k][e] == ParsedQueryData[x][y])
                                    {
                                        FoundValues++;
                                    }
                                }
                            }

                            if (FoundValues > 0)
                            {
                                RecomposedGametraces[x][i][k] = new string[FoundValues];
                                FoundValues = 0;
                            }

                            else
                            {
                                RecomposedGametraces[x][i][k] = new string[1];
                                MissingValues++;
                            }
                        }
                    }

                    if (MissingValues > 0)
                    {
                        IncompleteDatasets++;
                        MissingValues = 0;
                    }
                }

                FoundValues = 0;
                MissingValues = 0;

                for (int x = 0; x < ParsedQueryData.Length; x++)
                {
                    for (int i = 0; i < EvidenceModel.Item6.Length; i++)
                    {
                        for (int k = 0; k < EvidenceModel.Item6[i].Length; k++)
                        {
                            for (int y = 0; y < ParsedQueryData[x].Length; y++)
                            {
                                for (int e = 0; e < EvidenceModel.Item6[i][k].Length; e++)
                                {
                                    if (EvidenceModel.Item6[i][k][e] == ParsedQueryData[x][y])
                                    {
                                        RecomposedGametraces[x][i][k][FoundValues] = ParsedQueryData[x][y];
                                        FoundValues++;
                                    }
                                }
                            }

                            if (FoundValues > 0)
                            {
                                FoundValues = 0;
                            }

                            else
                            {
                                RecomposedGametraces[x][i][k][FoundValues] = "?";
                            }
                        }
                    }
                }

                for (int x = 0; x < RecomposedGametraces.Length; x++)
                {
                    file.WriteLine("Instance no: {0}", x + 1);
                    for (int i = 0; i < RecomposedGametraces[x].Length; i++)
                    {
                        file.WriteLine(CompetencyModel.Item1[i] + ":");
                        for (int k = 0; k < RecomposedGametraces[x][i].Length; k++)
                        {
                            file.Write(CompetencyModel.Item3[i][k] + ":");

                            for (int e = 0; e < RecomposedGametraces[x][i][k].Length; e++)
                            {
                                file.Write(RecomposedGametraces[x][i][k][e] + " ");
                            }

                            NonExceedingValues += RecomposedGametraces[x][i][k].Length;
                            file.WriteLine();
                        }

                        file.WriteLine();
                    }
                    NonExceedingData[x] = new string[NonExceedingValues];
                    NonExceedingValues = 0;
                }

                for (int x = 0; x < RecomposedGametraces.Length; x++)
                {
                    for (int i = 0; i < RecomposedGametraces[x].Length; i++)
                    {
                        for (int k = 0; k < RecomposedGametraces[x][i].Length; k++)
                        {
                            for (int e = 0; e < RecomposedGametraces[x][i][k].Length; e++)
                            {
                                NonExceedingData[x][NonExceedingValues] = RecomposedGametraces[x][i][k][e];
                                NonExceedingValues++;
                            }
                        }
                    }

                    NonExceedingValues = 0;
                }

                //NO. OF INCOMPLETE DATASETS
                file.WriteLine("No. of incomplete datasets: {0}", IncompleteDatasets);
                file.WriteLine();

                //EXCEEDING DATA
                bool check = false;

                for (int x = 0; x < ParsedQueryData.Length; x++)
                {
                    for (int y = 0; y < ParsedQueryData[x].Length; y++)
                    {
                        for (int i = 0; i < NonExceedingData[x].Length; i++)
                        {
                            if (ParsedQueryData[x][y] == NonExceedingData[x][i])
                            {
                                check = true;
                            }
                        }

                        if (check != true)
                        {
                            ExceedingValues++;
                        }

                        check = false;
                    }

                    ExceedingData[x] = new string[ExceedingValues];
                    ExceedingValues = 0;
                }

                for (int x = 0; x < ParsedQueryData.Length; x++)
                {
                    for (int y = 0; y < ParsedQueryData[x].Length; y++)
                    {
                        for (int i = 0; i < NonExceedingData[x].Length; i++)
                        {
                            if (ParsedQueryData[x][y] == NonExceedingData[x][i])
                            {
                                check = true;
                            }
                        }

                        if (check != true)
                        {
                            ExceedingData[x][ExceedingValues] = ParsedQueryData[x][y];
                            ExceedingValues++;
                        }

                        check = false;
                    }

                    ExceedingValues = 0;
                }


                for (int x = 0; x < ExceedingData.Length; x++)
                {
                    file.Write("Exceeding data for instance {0} is: ", x + 1);
                    for (int y = 0; y < ExceedingData[x].Length; y++)
                    {
                        file.Write(ExceedingData[x][y] + " ");
                    }
                    file.WriteLine();
                }

                //NON-USED DATA
                for (int x = 0; x < ExceedingData.Length; x++)
                {
                    for (int y = 0; y < ExceedingData[x].Length; y++)
                    {
                        check = false;

                        for (int i = 0; i < EvidenceModel.Item4.Length; i++)
                        {
                            for (int k = 0; k < EvidenceModel.Item4[i].Length; k++)
                            {
                                if (ExceedingData[x][y] == EvidenceModel.Item4[i][k])
                                {
                                    check = true;
                                }
                            }
                        }

                        if (check != true)
                        {
                            NonUsedValues++;
                        }
                    }
                }

                NonUsedData = new string[NonUsedValues];
                NonUsedValues = 0;

                for (int x = 0; x < ExceedingData.Length; x++)
                {
                    for (int y = 0; y < ExceedingData[x].Length; y++)
                    {
                        check = false;

                        for (int i = 0; i < EvidenceModel.Item4.Length; i++)
                        {
                            for (int k = 0; k < EvidenceModel.Item4[i].Length; k++)
                            {
                                if (ExceedingData[x][y] == EvidenceModel.Item4[i][k])
                                {
                                    check = true;
                                }
                            }
                        }

                        if (check != true)
                        {
                            NonUsedData[NonUsedValues] = ExceedingData[x][y];
                            NonUsedValues++;
                        }
                    }
                }


                var uniqueValues = new HashSet<string>();
                foreach (string d in NonUsedData)
                {
                    if (!uniqueValues.Contains(d))
                    {
                        uniqueValues.Add(d);
                    }
                }


                file.WriteLine();
                file.Write("Non-used indicators: ");
                NonUsedValues = 0;
                foreach (string d in uniqueValues)
                {
                    NonUsedValues++;
                    file.Write("[{0}]{1} ", NonUsedValues, d);
                }
                file.WriteLine();

                System.Console.WriteLine("Recomposing results saved in Recompose_Report.txt file successfully.");
            }

            return RecomposedGametraces;
        }

        //Generates .arff files containing the classification data for each facet of the declared competencies.
        public void GenerateArffFilesForClassificationOfFacets(Tuple<string[][], string[][][]> ClassificationData, string[][][][] RecomposedGametraces, 
            Tuple<string[], string[], string[][]> CompetencyModel, Tuple<string[], string[], string[][], string[][], string[][][], string[][][]> EvidenceModel)
        {
            //Declaring Variables.
            java.util.ArrayList atts;
            java.util.ArrayList attVals;
            java.util.ArrayList[] facetAtts;
            Instances data;
            string[][] vals;
            string[] Class = new string[3] { "Low", "Medium", "High" };

            System.Console.WriteLine("Generating .arff files for classification.");

            //Join re-composed game traces for each facet for every instance using a semicolon as seperator.
            string[][][] JoinedRecomposedGametraces = new string[RecomposedGametraces.Length][][];

            for (int x=0; x<RecomposedGametraces.Length; x++)
            {
                JoinedRecomposedGametraces[x] = new string[RecomposedGametraces[x].Length][];
                for(int y=0; y<RecomposedGametraces[x].Length; y++)
                {
                    JoinedRecomposedGametraces[x][y] = new string[RecomposedGametraces[x][y].Length];
                    for(int i=0; i<RecomposedGametraces[x][y].Length; i++)
                    {
                        string[] temp;
                        int count = 0;

                        for(int k=0; k<RecomposedGametraces[x][y][i].Length; k++)
                        {
                            count++;
                        }

                        temp = new string[count];
                        count = 0;

                        for (int k = 0; k < RecomposedGametraces[x][y][i].Length; k++)
                        {
                            temp[count] = RecomposedGametraces[x][y][i][k];
                            count++;
                        }

                        JoinedRecomposedGametraces[x][y][i] = string.Join(":", temp);
                    }
                }
            }

            //Generating .arff files for facets
            for (int x = 0; x < CompetencyModel.Item3.Length; x++)
            {
                facetAtts = new java.util.ArrayList[CompetencyModel.Item3[x].Length];

                for (int z=0; z<CompetencyModel.Item3[x].Length; z++)
                {
                    //Remove duplicates from joined recomposed game traces
                    string[] QueryDataRecomposed = new string[JoinedRecomposedGametraces.Length];
                    for (int f=0; f< JoinedRecomposedGametraces.Length; f++)
                    {
                        QueryDataRecomposed[f] = JoinedRecomposedGametraces[f][x][z];
                    }


                    var uniqueValuesRecomposed = new HashSet<string>();
                    foreach (string s in QueryDataRecomposed)
                    {
                        if (!uniqueValuesRecomposed.Contains(s))
                        {
                            uniqueValuesRecomposed.Add(s);
                        }
                    }

                    string[] QueryDataUniqueRecomposed = new string[uniqueValuesRecomposed.Count()];
                    int noOfUniqueDataRecomposed = 0;
                    foreach (string s in uniqueValuesRecomposed)
                    {
                        QueryDataUniqueRecomposed[noOfUniqueDataRecomposed] = s;
                        noOfUniqueDataRecomposed++;
                    }


                    // Set up attributes
                    atts = new java.util.ArrayList();

                    facetAtts[z] = new java.util.ArrayList();

                    for (int i = 0; i < QueryDataUniqueRecomposed.Length; i++)
                    {
                        facetAtts[z].add(QueryDataUniqueRecomposed[i]);
                    }

                    atts.add(new weka.core.Attribute("Queried game traces", facetAtts[z]));

                    attVals = new java.util.ArrayList();
                    for (int i = 0; i < 3; i++)
                        attVals.add(Class[i]);

                    weka.core.Attribute clusters = new weka.core.Attribute("Clustering for " + CompetencyModel.Item3[x][z], attVals);
                    atts.add(clusters);

                    // Create Instances object
                    data = new Instances(CompetencyModel.Item3[x][z], atts, 0);

                    // Fill with data
                    vals = new string[ClassificationData.Item2[x][z].Length][];
                    for (int y = 0; y < ClassificationData.Item2[x][z].Length; y++)
                    {
                        vals[y] = new string[data.numAttributes()];
                        for (int i = 0; i < data.numAttributes(); i++)
                        {
                            if (i == 0)
                            {
                                vals[y][i] = JoinedRecomposedGametraces[y][x][z];
                            }
                            else if (i == 1)
                            {
                                vals[y][i] = ClassificationData.Item2[x][z][y];
                            }
                        }

                    }

                    // Add data
                    for (int y = 0; y < vals.Length; y++)
                    {
                        weka.core.Instance inst = new DenseInstance(2);
                        inst.setDataset(data);

                        for (int i = 0; i < vals[y].Length; i++)
                        {
                            inst.setValue(i, vals[y][i]);

                        }

                        data.add(inst);
                    }

                    // Save data
                    weka.core.converters.ArffSaver saver = new weka.core.converters.ArffSaver();
                    saver.setInstances(data);
                    saver.setFile(new File("./data/Classification/" + CompetencyModel.Item3[x][z] + ".arff"));
                    //saver.setDestination(new File("./data/test.arff"));
                    saver.writeBatch();
                    System.Console.WriteLine("Classification data saved in {0}.arff file successfully.", CompetencyModel.Item3[x][z]);
                }
            }
        }

        //Runs a Bayesian Network for each declared facet. 
        public static void BayesFacets(Tuple<string[], string[], string[][]> CompetencyModel)
        {
            try
            {
                for(int x=0; x<CompetencyModel.Item3.Length; x++)
                {
                    for(int y=0; y<CompetencyModel.Item3[x].Length; y++)
                    {
                        weka.core.Instances insts = new weka.core.Instances(new java.io.FileReader("./data/Classification/" + CompetencyModel.Item3[x][y] + ".arff"));
                        insts.setClassIndex(insts.numAttributes() - 1);

                        weka.classifiers.Classifier cl = new weka.classifiers.bayes.BayesNet();

                        //randomize the order of the instances in the dataset.
                        /*
                        weka.filters.Filter myRandom = new weka.filters.unsupervised.instance.Randomize();
                        myRandom.setInputFormat(insts);
                        insts = weka.filters.Filter.useFilter(insts, myRandom);*/

                        int trainSize = (int)Math.Round((double)insts.numInstances() * percentSplit / 100);
                        int testSize = insts.numInstances() - trainSize;
                        weka.core.Instances train = new weka.core.Instances(insts, 0, trainSize);
                        weka.core.Instances test = new weka.core.Instances(insts, 0, 0);


                        cl.buildClassifier(train);

                        //Save BayesNet results in .txt file
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter("./data/Classification/" + CompetencyModel.Item3[x][y] + "_Report.txt"))
                        {
                            file.WriteLine("Performing " + percentSplit + "% split evaluation.");
                            file.WriteLine();

                            //print model
                            file.WriteLine(cl);
                            file.WriteLine();

                            // Train the model
                            weka.classifiers.Evaluation eTrain = new weka.classifiers.Evaluation(train);
                            eTrain.evaluateModel(cl, train);

                            // Print the results as in Weka explorer:
                            //Print statistics
                            String strSummaryTrain = eTrain.toSummaryString();
                            file.WriteLine("EVALUATION OF TRAINING DATASET.");
                            file.WriteLine(strSummaryTrain);
                            file.WriteLine();

                            //Print detailed class statistics
                            file.WriteLine(eTrain.toClassDetailsString());
                            file.WriteLine();

                            //Print confusion matrix
                            file.WriteLine(eTrain.toMatrixString());
                            file.WriteLine();

                            // Get the confusion matrix
                            double[][] cmMatrixTrain = eTrain.confusionMatrix();

                            //Print prior probabilities for each instance.                            
                            file.WriteLine("EVALUATION OF TEST DATASET.");
                            int numCorrect = 0;
                            for (int i = trainSize; i < insts.numInstances(); i++)
                            {
                                weka.core.Instance currentInst = insts.instance(i);
                                double predictedClass = cl.classifyInstance(currentInst);
                                test.add(currentInst);

                                double[] prediction = cl.distributionForInstance(currentInst);

                                for (int p = 0; p < prediction.Length; p++)
                                {
                                    file.WriteLine("Probability of class [{0}] for [{1}] is: {2}", currentInst.classAttribute().value(p), currentInst, Math.Round(prediction[p], 4));
                                }
                                file.WriteLine();

                                file.WriteLine();
                                if (predictedClass == insts.instance(i).classValue())
                                    numCorrect++;
                            }

                            file.WriteLine(numCorrect + " out of " + testSize + " correct (" +
                                        (double)((double)numCorrect / (double)testSize * 100.0) + "%)");

                            // Test the model
                            weka.classifiers.Evaluation eTest = new weka.classifiers.Evaluation(test);
                            eTest.evaluateModel(cl, test);

                            // Print the results as in Weka explorer:
                            //Print statistics
                            String strSummaryTest = eTest.toSummaryString();

                            file.WriteLine(strSummaryTest);
                            file.WriteLine();

                            //Print detailed class statistics
                            file.WriteLine(eTest.toClassDetailsString());
                            file.WriteLine();

                            //Print confusion matrix
                            file.WriteLine(eTest.toMatrixString());
                            file.WriteLine();

                            // Get the confusion matrix
                            double[][] cmMatrixTest = eTest.confusionMatrix();

                            System.Console.WriteLine("Bayesian Network results saved in {0}_Report.txt file successfully.", CompetencyModel.Item3[x][y]);
                        }
                    }
                }
            }

            catch (java.lang.Exception ex)
            {
                ex.printStackTrace();
            }

        }

        //Generates .arff files containing the classification data for each declared competency.
        public void GenerateArffFilesForClassificationOfCompetencies(Tuple<string[][], string[][][]> ClassificationData, string[][] ParsedQueryData,
            Tuple<string[], string[], string[][]> CompetencyModel, Tuple<string[], string[], string[][], string[][], string[][][], string[][][]> EvidenceModel)
        {
            //Declaring Variables.
            java.util.ArrayList atts;
            java.util.ArrayList attVals;
            java.util.ArrayList queryVals;
            java.util.ArrayList[] facetVals;
            Instances data;
            string[][] vals;
            string[] Class = new string[3] { "Low", "Medium", "High" };

            System.Console.WriteLine("Generating .arff files for classification.");

            //Remove data from the parsed query data that was not declared in the Statistical Submodel.
            string[] QueryData = new string[ParsedQueryData.Length];

            for (int i = 0; i < ParsedQueryData.Length; i++)
            {
                string[] temp;
                int count = 0;
                for (int k = 0; k < ParsedQueryData[i].Length; k++)
                {
                    for (int x = 0; x < EvidenceModel.Item4.Length; x++)
                    {
                        for (int y = 0; y < EvidenceModel.Item4[x].Length; y++)
                        {
                            if (ParsedQueryData[i][k] == EvidenceModel.Item4[x][y])
                            {
                                //Count no. of parsed query data that was declared in the Statistical Submodel for each instance.
                                count++;
                            }
                        }
                    }
                }

                temp = new string[count];
                count = 0;

                for (int k = 0; k < ParsedQueryData[i].Length; k++)
                {
                    for (int x = 0; x < EvidenceModel.Item4.Length; x++)
                    {
                        for (int y = 0; y < EvidenceModel.Item4[x].Length; y++)
                        {
                            if (ParsedQueryData[i][k] == EvidenceModel.Item4[x][y])
                            {
                                //Temporarily store the parsed query data that was declared in the Statistical Submodel for each instance.
                                temp[count] = ParsedQueryData[i][k];
                                count++;
                            }
                        }
                    }
                }

                //Re-compose the parsed query data that was declared in the Statistical Submodel using a semicolon as seperator for each instance.
                QueryData[i] = string.Join(":", temp);
            }


            //Remove duplicates from Query data
            var uniqueValues = new HashSet<string>();
            foreach (string s in QueryData)
            {
                if (!uniqueValues.Contains(s))
                {
                    uniqueValues.Add(s);
                }
            }

            string[] QueryDataUnique = new string[uniqueValues.Count()];
            int noOfUniqueData = 0;
            foreach (string s in uniqueValues)
            {
                QueryDataUnique[noOfUniqueData] = s;
                noOfUniqueData++;
            }


            //Generating .arff files for competencies
            for (int x = 0; x < CompetencyModel.Item1.Length; x++)
            {
                // Set up attributes
                atts = new java.util.ArrayList();

                facetVals = new java.util.ArrayList[CompetencyModel.Item3[x].Length];
                for(int q=0; q<facetVals.Length; q++)
                {
                    facetVals[q] = new java.util.ArrayList();

                    for (int i = 0; i < 3; i++)
                        facetVals[q].add(Class[i]);

                weka.core.Attribute clustersFacets = new weka.core.Attribute("Clustering for " + CompetencyModel.Item3[x][q], facetVals[q]);
                atts.add(clustersFacets);
                }

                queryVals = new java.util.ArrayList();
                for (int i = 0; i < QueryDataUnique.Length; i++)
                    queryVals.add(QueryDataUnique[i]);

                atts.add(new weka.core.Attribute("Queried game traces", queryVals));

                attVals = new java.util.ArrayList();
                for (int i = 0; i < 3; i++)
                    attVals.add(Class[i]);

                weka.core.Attribute clusters = new weka.core.Attribute("Clustering for " + CompetencyModel.Item1[x], attVals);
                atts.add(clusters);

                // Create Instances object
                data = new Instances(CompetencyModel.Item1[x], atts, 0);

                // Fill with data
                vals = new string[ClassificationData.Item1[x].Length][];
                for (int y = 0; y < ClassificationData.Item1[x].Length; y++)
                {
                    vals[y] = new string[data.numAttributes()];

                    for (int i=0; i<facetVals.Length; i++)
                    {
                        vals[y][i] = ClassificationData.Item2[x][i][y];
                    }

                    for (int i = facetVals.Length; i < data.numAttributes(); i++)
                    {
                        if (i == data.numAttributes()-2)
                        {
                            vals[y][i] = QueryData[y];
                        }
                        else if (i==data.numAttributes()-1)
                        {
                            vals[y][i] = ClassificationData.Item1[x][y];
                        }
                    }
                }

                // Add data
                for (int y = 0; y < vals.Length; y++)
                {
                    weka.core.Instance inst = new DenseInstance(data.numAttributes());
                    inst.setDataset(data);

                    for (int i = 0; i < vals[y].Length; i++)
                    {
                        inst.setValue(i, vals[y][i]);

                    }

                    data.add(inst);
                }

                // Save data
                weka.core.converters.ArffSaver saver = new weka.core.converters.ArffSaver();
                saver.setInstances(data);
                saver.setFile(new File("./data/Classification/" + CompetencyModel.Item1[x] + ".arff"));
                //saver.setDestination(new File("./data/test.arff"));
                saver.writeBatch();
                System.Console.WriteLine("Classification data saved in {0}.arff file successfully.", CompetencyModel.Item1[x]);
            }
        }

        //Runs a Bayesian Network for each declared competency. 
        public static void BayesCompetencies(Tuple<string[], string[], string[][]> CompetencyModel)
        {
            try
            {
                for (int x = 0; x < CompetencyModel.Item3.Length; x++)
                {

                    weka.core.Instances insts = new weka.core.Instances(new java.io.FileReader("./data/Classification/" + CompetencyModel.Item1[x] + ".arff"));
                    insts.setClassIndex(insts.numAttributes() - 1);

                    weka.classifiers.Classifier cl = new weka.classifiers.bayes.BayesNet();

                    // randomize data
                    int seed = 1;
                    java.util.Random rand = new java.util.Random(seed);
                    weka.core.Instances randData = new weka.core.Instances(insts);
                    randData.randomize(rand);

                    //set the sizes of the training and testing datasets according to the 66% split rule.
                    int trainSize = (int)Math.Round((double)insts.numInstances() * percentSplit / 100);
                    int testSize = insts.numInstances() - trainSize;

                    //Initialize the training and testing datasets.
                    weka.core.Instances train = new weka.core.Instances(insts, 0, 0);
                    weka.core.Instances test = new weka.core.Instances(insts, 0, 0);

                    for (int j = 0; j < insts.numInstances(); j++)
                    {

                        weka.core.Instance currentInst = randData.instance(j);

                        if (j < trainSize)
                        {
                            train.add(currentInst);
                        }

                        else
                        {
                            test.add(currentInst);
                        }

                    }

                    cl.buildClassifier(train);

                    //Save BayesNet results in .txt file
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter("./data/Classification/" + CompetencyModel.Item1[x] + "_Report.txt"))
                    {
                        //Print classifier analytics for all the dataset
                        file.WriteLine("EVALUATION OF ALL DATASET.");

                        // Train the model
                        weka.classifiers.Evaluation eval = new weka.classifiers.Evaluation(randData);
                        eval.evaluateModel(cl, test);

                        // Print the results as in Weka explorer:
                        //Print statistics
                        String strSummaryAlldata = eval.toSummaryString();
                        file.WriteLine(strSummaryAlldata);
                        file.WriteLine();

                        //Print detailed class statistics
                        file.WriteLine(eval.toClassDetailsString());
                        file.WriteLine();

                        //Print confusion matrix
                        file.WriteLine(eval.toMatrixString());

                        int numCorrect = 0;
                        for (int i = 0; i < test.numInstances(); i++)
                        {
                            weka.core.Instance currentInst = test.instance(i);
                            double predictedClass = cl.classifyInstance(currentInst);

                            double[] prediction = cl.distributionForInstance(currentInst);

                            for (int p = 0; p < prediction.Length; p++)
                            {
                                file.WriteLine("Probability of class [{0}] for [{1}] is: {2}", currentInst.classAttribute().value(p), currentInst, Math.Round(prediction[p], 4));
                            }
                            file.WriteLine();

                            file.WriteLine();
                            if (predictedClass == test.instance(i).classValue())
                                numCorrect++;
                        }

                        file.WriteLine(numCorrect + " out of " + testSize + " correct (" +
                                    (double)((double)numCorrect / (double)testSize * 100.0) + "%)");


                        System.Console.WriteLine("Bayesian Network results saved in {0}_Report.txt file successfully.", CompetencyModel.Item1[x]);
                    }
                }
            }

            catch (java.lang.Exception ex)
            {
                ex.printStackTrace();
            }
        }

    }
}