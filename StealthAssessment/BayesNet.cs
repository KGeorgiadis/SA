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

namespace StealthAssessment
{
    class BayesNet
    {
        //Constructor
        public BayesNet()
        {
            return;
        }

        //Destructor
        ~BayesNet()
        {
            return;
        }

        //Loads all the data from the game log file in a jagged array
        public string[][] LoadAllData ( string filename )
        {
            string[][] AllData = new string[][] { };
            var Excel = new Excel();

            Excel.ExcelOpen(filename);
            AllData = Excel.LoadData();
            Console.WriteLine("Data retrieved from game log file.");
            Excel.ExcelSave(filename);
            Excel.ExcelQuit();

            return AllData;
        }


        /**  
         * FUTURE ADDITIONS FOR BAYESIAN NETWORK
         *  
         *  1) Classification.
         *  2) Query data (including its parsing).
         *  3) Training (select sample size and calculate the prior probabilities tables from the queried data).
         *  4) Testing (select testing sample and run Bayes Net for results).
         *  5) Save and export results.
        */

    }
}
