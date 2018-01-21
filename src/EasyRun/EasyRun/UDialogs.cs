﻿/*

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


using System.Windows.Forms;

namespace EasyRun
{
    class UDialogs
    {
        public void ShowError(string errorText)
        {
            MessageBox.Show(errorText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void ShowWarning(string WarningText)
        {
            MessageBox.Show(WarningText, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public void ShowMessage(string messageText)
        {
            MessageBox.Show(messageText, "EasyRun", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public DialogResult ConfirmAction(string actionText)
        {
            return MessageBox.Show(actionText, "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

    }
}
