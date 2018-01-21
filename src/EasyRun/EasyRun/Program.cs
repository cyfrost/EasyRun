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
using System.Windows.Forms;
using CommandLine;

namespace EasyRun
{
    class Options
    {
        [Option('a', "add-file", HelpText = "Add EasyRun shortcut targetting a specified file", Required = false)]
        public string filePath { get; set; }

        [Option('d', "add-dir", HelpText = "Add EasyRun shortcut targetting a specified directory", Required = false)]
        public string dirPath { get; set; }

        [Option('e', "enable-integration", HelpText = "Enable Windows Explorer integration module for EasyRun", Required = false, DefaultValue = false)]
        public bool enable { get; set; }

        [Option('r', "remove-integration", HelpText = "Enable Windows Explorer integration module for EasyRun", Required = false, DefaultValue = false)]
        public bool removeIntegration { get; set; }

    }





    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main(String []args)
        {
            //checking for 4libraries in the executing assembly location
            //when adding new libraries in Form1, rem to perform checks for fail-safe run in the below snippet

            new FailSafeInit().CheckFailSafe();


            UDialogs msg = new UDialogs() ;

            try
            {
                var options = new Options();
                UDialogs x = new UDialogs();

                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                  

                    if (options.filePath != null)
                    {
                        CommandLineHandler cmdHndl = new CommandLineHandler();
                        cmdHndl.askUser("Enter alias: (example: apple)", false, "Create file shortcut - EasyRun");
                        if (!String.IsNullOrEmpty(cmdHndl.userInput) && !cmdHndl.userInput.Contains(" "))
                        {
                            cmdHndl.CreateFileShortcut(cmdHndl.userInput, options.filePath);
                        }


                    }
                    else if (options.dirPath != null)
                    {

                        CommandLineHandler cmdHndl = new CommandLineHandler();
                        cmdHndl.askUser("Enter alias: (example: cats)", true, "Create directory shortcut - EasyRun");
                        if (!String.IsNullOrEmpty(cmdHndl.userInput) && !cmdHndl.userInput.Contains(" "))
                        {


                            cmdHndl.CreateDirectoryShortcut(cmdHndl.userInput, options.dirPath);
                        }


                    }
                    else if (options.enable == true)
                    {
                        new ExplorerIntegration().CreateIntegration(false);
                        Application.Exit();
                    }
                    else if (options.removeIntegration == true) {
                        new ExplorerIntegration().DeleteIntegration();
                      //  msg.ShowMessage("Removed Integration");
                        Application.Exit();
                    }

                    else
                    {
                        
                        try
                        {
                            Application.EnableVisualStyles();
                            Application.SetCompatibleTextRenderingDefault(false);
                            Application.Run(new Form1());
                        }
                        catch (Exception ex)
                        {
                            msg.ShowError(ex.Message);
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }

          


        }
    }
}
