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
namespace EasyRun
{
    class res
    {
        UDialogs msg = new UDialogs();

        public string getExtension(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    return path.Substring(path.LastIndexOf(".") + 1);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
                return null;

            }

         
        }
        public string getLocation(string pt) {
            try
            {
                if (File.Exists(pt))
                {
                    return pt.Substring(0, pt.LastIndexOf(@"\"));
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
                return null;
            }
         
        }
        public string getFileName(string pt) {
            try
            {
                if (File.Exists(pt))
                {
                    return pt.Substring(pt.LastIndexOf(@"\") + 1);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
                return null;
            }

         
        }
        public string removeExtension(string pt) {
            try
            {
                return pt.Substring(0, pt.IndexOf("."));
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
                return null;
            }

           
        }
        public bool isPathVerified(string ptr) {
            try
            {
                return File.Exists(ptr);

            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);

                return false;
            }

           
        }
        public string getRoot(string pt) {
            try
            {
                return pt.Substring(0, 2);
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
                return null;
            }

           
        }
    }
}
