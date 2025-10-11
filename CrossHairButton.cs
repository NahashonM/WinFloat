
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinRT;

namespace WinFloat
{
    public class CrossHairButton : Button
    {
        public InputCursor InputCursor
        {
            get => ProtectedCursor;
            set => ProtectedCursor = value;
        }
    }
}
