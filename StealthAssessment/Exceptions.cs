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
    //This class handles any type of exceptions which generate either from user input or during runtime.
    class Exceptions
    {
        //Constructor.
        public Exceptions()
        {
            return;
        }

        //Destructor
        ~Exceptions()
        {
            return;
        }

        //Check user input for conversion exceptions from a string to a character.
        public bool StringToChar(string input)
        {
            bool check = true;  //This variable checks if any exception occurs (true: no exception, false: exception).
            bool repeat = false;    //This variable suggests if the user needs to re-enter input (true: repeat input, false: don't repeat input).

            try
            {
                Convert.ToChar(input);
            }

            catch (FormatException)
            {
                Console.WriteLine("Input string is not a character.");   //Check if input is integer.
                check = false;
            }

            catch (ArgumentNullException)
            {
                Console.WriteLine("A null string cannot be converted to a Char.");  //Check for null input.
                check = false;
            }

            finally
            {
                if (check == false)
                {
                    repeat = true;
                }
                else
                {
                    repeat = false;
                }
            }

            return repeat;
        }

        //Check user input for conversion exceptions from a string to an integer.
        public bool StringToInt(string input)
        {
            bool check = true;  //This variable checks if any exception occurs (true: no exception, false: exception).
            bool repeat = false;    //This variable suggests if the user needs to re-enter input (true: repeat input, false: don't repeat input).

            try
            {
                Convert.ToInt32(input);
            }

            catch (FormatException)
            {
                Console.WriteLine("Input string is not an integer.");   //Check if input is integer.
                check = false;
            }

            catch (OverflowException)
            {
                Console.WriteLine("The number cannot fit in an Int32.");    //Check size of integer.
                check = false;
            }

            finally
            {
                if (check == false)
                {
                    repeat = true;
                }
                else
                {
                    repeat = false;
                }
            }
            return repeat;
        }

        //Check user input for conversion exceptions from a double to a string.
        public bool DoubleToString(double input)
        {
            bool check = true;  //This variable checks if any exception occurs (true: no exception, false: exception).
            bool repeat = false;    //This variable suggests if the user needs to re-enter input (true: repeat input, false: don't repeat input).

            try
            {
                Convert.ToString(input);
            }

            catch (FormatException)
            {
                Console.WriteLine("Input string is not an double.");    //Check if input is double.
                check = false;
            }

            catch (OverflowException)
            {
                Console.WriteLine("The double cannot fit in a string.");    //Check size of double.
                check = false;
            }

            finally
            {
                if (check == false)
                {
                    repeat = true;
                }
                else
                {
                    repeat = false;
                }
            }
            return repeat;
        }

        //Check user input for conversion exceptions from a string to an double.
        public bool StringToDouble(string input)
        {
            bool check = true;  //This variable checks if any exception occurs (true: no exception, false: exception).
            bool repeat = false;    //This variable suggests if the user needs to re-enter input (true: repeat input, false: don't repeat input).

            try
            {
                Convert.ToDouble(input);
            }

            catch (FormatException)
            {
                Console.WriteLine("Input string is not a double.");   //Check if input is double.
                check = false;
            }

            catch (OverflowException)
            {
                Console.WriteLine("The number cannot fit in a double.");    //Check size of double.
                check = false;
            }

            finally
            {
                if (check == false)
                {
                    repeat = true;
                }
                else
                {
                    repeat = false;
                }
            }
            return repeat;
        }
    }
}
