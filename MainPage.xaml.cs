using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Windows.UI.Core;
using Microsoft.Graphics.Canvas.Geometry;
using System.Numerics;
using Windows.System;
using Windows.Graphics.Display;
using Microsoft.Graphics.Canvas.UI;
using win2d_sandbox.Controls;

namespace win2d_sandbox
{
    public sealed partial class MainPage : Page
    {
        public static MainPage _MainPage;

        public MainPage()
        {
            this.InitializeComponent();
            _MainPage = this;
        }

        private void btnLine_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.cancelAll();
            mainCanvas.isDrawing = true;
            mainCanvas.drawLine = true;

            mainCanvas._mainCanvas.canvas.Focus(FocusState.Programmatic);
        }

        private void btnRect_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.cancelAll();
            mainCanvas.isDrawing = true;
            mainCanvas.drawRect = true;

            mainCanvas._mainCanvas.canvas.Focus(FocusState.Programmatic);
        }

        private void btnCircle_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.cancelAll();
            mainCanvas.isDrawing = true;
            mainCanvas.drawCircle = true;

            mainCanvas._mainCanvas.canvas.Focus(FocusState.Programmatic);
        }

        private void btnArcPtP_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.cancelAll();
            mainCanvas.isDrawing = true;
            mainCanvas.drawArcPtP = true;

            mainCanvas._mainCanvas.canvas.Focus(FocusState.Programmatic);
        }

        private void btnColuna_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.cancelAll();
            mainCanvas.isDrawing = true;
            mainCanvas.drawColuna = true;

            mainCanvas._mainCanvas.canvas.Focus(FocusState.Programmatic);
        }

        private void btnRuler_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.cancelAll();
            mainCanvas.isDrawing = true;
            mainCanvas.measure = true;

            mainCanvas._mainCanvas.canvas.Focus(FocusState.Programmatic);
        }

        private void btnPOSType_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.isCartesian = mainCanvas.isCartesian ? false : true;
            btnPOSType.Content = mainCanvas.isCartesian ? "CARTESIANO" : "ABSOLUTO";

            mainCanvas._mainCanvas.canvas.Focus(FocusState.Programmatic);
        }

        private void btnCursor_Click(object sender, RoutedEventArgs e)
        {
            if (mainCanvas.windowsCursor == false)
            {
                mainCanvas.windowsCursor = true;

                mainCanvas._mainCanvas.canvas.Invalidate();
            }
            else
            {
                mainCanvas.windowsCursor = false;
            }

            mainCanvas._mainCanvas.canvas.Focus(FocusState.Programmatic);
        }

        private void btnOrtho_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.isOrthoActivated = mainCanvas.isOrthoActivated ? false : true;

            mainCanvas._mainCanvas.canvas.Focus(FocusState.Programmatic);
        }

        private void btnSnap_Click(object sender, RoutedEventArgs e)
        {
            mainCanvas.isSnapActivated = mainCanvas.isSnapActivated ? false : true;

            mainCanvas._mainCanvas.canvas.Focus(FocusState.Programmatic);
        }
    }
}
