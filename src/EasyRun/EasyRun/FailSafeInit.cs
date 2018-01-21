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
using System.IO;
using System.Windows.Forms;

namespace EasyRun
{
    class FailSafeInit
    {

        //checking for 4libraries in the executing assembly location
        //when adding new libraries in Form1, rem to perform checks for fail-safe run in the below snippet

        private static string curr = new res().getLocation(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static string lib1 = curr + @"\Interop.IWshRuntimeLibrary.dll";
        private static string lib2 = curr + @"\Microsoft.Win32.TaskScheduler.dll";
        private static string lib3 = curr + @"\Explintegrate.dll";
        private static string lib4 = curr + @"\CommandLine.dll";

        public void CheckFailSafe() {
            if (File.Exists(lib1) && File.Exists(lib2) && File.Exists(lib3) && File.Exists(lib4)) {
               
                return;
            }
              
            else
            {
                
                System.Windows.Forms.MessageBox.Show("Fatal Error: One or more dependencies are missing, please re-install EasyRun to fix this problem.\n\nThe application will exit now.", "EasyRun", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
                
        }
        
       

    }
}
