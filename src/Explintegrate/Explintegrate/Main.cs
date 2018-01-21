/*

The MIT License (MIT)
---------------------

Explintegrate

Copyright © 2018 cyfrost <cyrus.frost@hotmail.com>

Permission is hereby granted, free of charge, to any person obtaining 
a copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies
or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
FOR AND CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
DEALINGS IN THE SOFTWARE.

*/


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace Explintegrate
{
    public class Main
    {

     


        //uses *, .jpeg etc, usually inflates the string "HKEY_CLASSES_ROOT\" + <str_association> + "\shell"


        //icon target can be exe

        //dev manually constructs command string


        //objects (target, parameters and options) are bound within double quotes and delimited with a white space

        //bail immediately if any required parameter equals null


        public int CreateAssociation(string Association, string KeyName, string Command_Str) {
            try
            {
                Registry.ClassesRoot.CreateSubKey(Association + @"\shell\" + KeyName).Close();
                Registry.ClassesRoot.CreateSubKey(Association + @"\shell\" + KeyName + @"\command").Close();
                Registry.ClassesRoot.OpenSubKey(Association + @"\shell\" + KeyName + @"\command", true).SetValue("", Command_Str);

                return 0;

            }
            catch (Exception e) {
                throw new Exception(e.Message);


            }

        }

        public int CreateAssociation(string Association, string KeyName, string Command_Str, string DisplayText) {
            try
            {
                Registry.ClassesRoot.CreateSubKey(Association + @"\shell\" + KeyName).Close();
                Registry.ClassesRoot.OpenSubKey(Association + @"\shell\" + KeyName, true).SetValue("", DisplayText);

                Registry.ClassesRoot.CreateSubKey(Association + @"\shell\" + KeyName + @"\command").Close();
                Registry.ClassesRoot.OpenSubKey(Association + @"\shell\" + KeyName + @"\command", true).SetValue("", Command_Str);

                return 0;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);


            }

        }
        public int CreateAssociation(string Association, string KeyName, string Command_Str, string DisplayText, string Icon_Target)
        {
            try
            {
                Registry.ClassesRoot.CreateSubKey(Association + @"\shell\" + KeyName).Close();
                Registry.ClassesRoot.OpenSubKey(Association + @"\shell\" + KeyName, true).SetValue("", DisplayText);
                Registry.ClassesRoot.OpenSubKey(Association + @"\shell\" + KeyName, true).SetValue("Icon", Icon_Target);

                Registry.ClassesRoot.CreateSubKey(Association + @"\shell\" + KeyName + @"\command").Close();
                Registry.ClassesRoot.OpenSubKey(Association + @"\shell\" + KeyName + @"\command", true).SetValue("", Command_Str);

                return 0;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);


            }

        }

        public int DeleteAssoc(string Association, string KeyName) {
            try
            {

                Registry.ClassesRoot.DeleteSubKeyTree(Association + @"\shell\" + KeyName);

                return 0;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);


            }

        }

        public bool SubKeyExists(string hive, string SubKey) {
            try
            {
                /*
               HKEY_CLASSES_ROOT (HKCR)
               HKEY_CURRENT_USER (HKCU)
               HKEY_LOCAL_MACHINE (HKLM)
               HKEY_USERS (HKU)
               HKEY_CURRENT_CONFIG (HKCC)
               */
               

                    switch (hive)
                    {

                        case "HKCU":
                            string rand = (string)Registry.LocalMachine.OpenSubKey(SubKey).GetValue("");
                            break;
                        case "HKLM":
                            string rand1 = (string)Registry.CurrentUser.OpenSubKey(SubKey).GetValue("");

                            break;
                        case "HKCR":
                            string rand2 = (string)Registry.ClassesRoot.OpenSubKey(SubKey).GetValue("");

                            break;
                        case "HKU":
                            string rand3 = (string)Registry.Users.OpenSubKey(SubKey).GetValue("");

                            break;
                        case "HKCC":
                            string rand4 = (string)Registry.CurrentConfig.OpenSubKey(SubKey).GetValue("");

                            break;

                    }

                return true;
              
               
            }
            catch (NullReferenceException e) {
                return false;
            }
            return false;

        }
       
    }
}
