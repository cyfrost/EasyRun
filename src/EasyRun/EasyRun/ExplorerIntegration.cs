/*

The MIT License (MIT)
---------------------

EasyRun - EasyRun has supercow powers!

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
using Microsoft.Win32;

namespace EasyRun
{
    class ExplorerIntegration
    {
        UDialogs msg = new UDialogs();
        string KeyName = "EasyRun";
        Explintegrate.Main expl = new Explintegrate.Main();


     


        public void CreateIntegration(bool silent) {

            try
            {
                string command_str_file_assoc = AppDomain.CurrentDomain.BaseDirectory + "EasyRun.exe --add-file \"%1\"";
                string command_str_dir_assoc = AppDomain.CurrentDomain.BaseDirectory + "EasyRun.exe --add-dir \"%1\"";


                if (DeleteIntegration() == 0)
                {

                    //creating file assoc
                    expl.CreateAssociation("*", KeyName, command_str_file_assoc, "Add as EasyRun shortcut", AppDomain.CurrentDomain.BaseDirectory + "EasyRun.exe");

                    //creating directory assoc
                    expl.CreateAssociation("Directory", KeyName, command_str_dir_assoc, "Add as EasyRun shortcut", AppDomain.CurrentDomain.BaseDirectory + "EasyRun.exe");

                    if (!silent) {
                        msg.ShowMessage("Explorer Integration has been enabled successfully");

                    }


                }
                else
                {
                    msg.ShowWarning("The requested operation failed. an error occured while trying to unregistering handlers.");
                }


            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }





        }

        public int DeleteIntegration() {

            try {
                
                if (expl.SubKeyExists("HKCR", @"*\shell\" + KeyName))
                    expl.DeleteAssoc("*", KeyName);
                if (expl.SubKeyExists("HKCR", @"Directory\shell\" + KeyName))
                    expl.DeleteAssoc("Directory", KeyName);
                return 0;

            }
            catch (Exception e)
            {

                msg.ShowError(e.Message);
                return -1;

            }
        }

        public bool IntegrationEnabled() {

            try
            {


                if (expl.SubKeyExists("HKCR", @"*\shell\" + KeyName) && expl.SubKeyExists("HKCR", @"Directory\shell\" + KeyName))
                    return true;
                else
                    return false;

            }
            catch (Exception e)
            {

                msg.ShowError(e.Message);
                return false;


            }
        }

        public bool IntegrationCorrupt() {
            try
            {
                string currExecFile = AppDomain.CurrentDomain.BaseDirectory + "EasyRun.exe --add-file \"%1\"";
                string currExecDir = AppDomain.CurrentDomain.BaseDirectory + "EasyRun.exe --add-dir \"%1\"";

                if (IntegrationEnabled()) {
                    using (RegistryKey foo = Registry.ClassesRoot.OpenSubKey(@"*\shell\EasyRun\command", true))
                    {
                        using (RegistryKey foo1 = Registry.ClassesRoot.OpenSubKey(@"Directory\shell\EasyRun\command", true))
                        {
                        

                            if ((string)foo.GetValue("") == currExecFile && (string)foo1.GetValue("") == currExecDir)
                                return false; //integration is current
                            else
                                return true; //integration is corrupt

                        }
                    }

                 

                }
                return false;



            }
            catch (Exception e)
            {

                msg.ShowError(e.Message);
                return false;


            }
        }
    }
}
