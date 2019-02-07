using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace win2d_sandbox.Controls
{
    public sealed partial class mainCanvas : UserControl
    {
        public static mainCanvas _mainCanvas;

        public mainCanvas()
        {
            this.InitializeComponent();
            _mainCanvas = this;
        }

        public List<snap> Snaps = new List<snap>();
        public List<guide> Guides = new List<guide>();

        public List<line> Lines = new List<line>();
        public List<rect> Rects = new List<rect>();
        public List<circle> Circles = new List<circle>();
        public List<coluna> Colunas = new List<coluna>();

        public class snap
        {
            public int type { get; set; } //1 = quadrado extremidade, 2 = quadrado no meio das linhas, 3 = centro geométrico de objetos
            public Rect rect { get; set; }
            public bool isVisible { get; set; }
        }

        public class guide
        {
            public Point pos { get; set; }
        }

        public class line
        {
            public float x1 { get; set; }
            public float y1 { get; set; }
            public float x2 { get; set; }
            public float y2 { get; set; }
            public bool isVisible { get; set; } //Se true, exibe o retangulo azul e o texto do valor
            public Rect rec { get; set; } //Armazena o retangulo do final da linha
        }

        public class rect
        {
            public float x1 { get; set; }
            public float y1 { get; set; }
            public float x2 { get; set; }
            public float y2 { get; set; }
            public bool isVisible { get; set; } //Se true, exibe o retangulo azul e o texto do valor
            public Rect rec { get; set; } //Armazena o retangulo do final da linha

            public bool Contains(Point pos, float near)
            {
                if (pos.X >= x1 && pos.X <= x2 && pos.Y >= y1 && pos.Y <= y2)
                {
                    return true;
                }
                else if (pos.X <= x1 && pos.X >= x2 && pos.Y >= y1 && pos.Y <= y2)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        public class circle
        {
            public float cx1 { get; set; }
            public float cy1 { get; set; }
            public float rx2 { get; set; }
            public float ry2 { get; set; }
            public float radius { get; set; }

            public bool InsideCircle(Point p)
            {
                float dx = cx1 - (float)p.X;
                float dy = cy1 - (float)p.Y;

                //return (dx * dx + dy * dy <= radius * radius);
                return Math.Abs(dist(p) - radius) < 15;         
            }

            public double dist(Point p)  // compute the distance of point p to the current point
            {
                double distance;
                distance = Math.Sqrt((cx1 - p.X) * (cx1 - p.X) + (cy1 - p.Y) * (cy1 - p.Y));
                return distance;
            }
        }

        public class coluna
        {

        }

        public static bool isDrawing = false;

        public static bool drawLine = false;
        public static bool drawingLine = false;
        line tLine;

        public static bool drawRect = false;
        public static bool drawingRect = false;
        rect tRect;

        public static bool drawCircle = false;
        public static bool drawingCircle = false;
        circle tCircle;

        public static bool drawArcPtP = false;
        public static bool drawingArcPtP = false;        

        public static bool drawColuna = false;
        public static bool drawingColuna = false;
        coluna tColuna;

        public static bool measure = false;
        public static bool measuring = false;
        line tRuler;

        public static bool windowsCursor = false;
        int cursorMode = 0;
        Point aCursorPOS;

        public static bool isCartesian = true;
        Point cCursorPOS;

        public static bool isOrthoActivated = false;
        Point tempCursorPOSSTART;
        Point tempCursorPOSEND;

        public static bool isSnapActivated = true;
        float near = 15;

        public static bool movingScreen = false;
        line pointerOffset;

        double _hO = 0;
        double _vO = 0;

        float displayDpi;

        CoreCursorType cursorNormal = CoreCursorType.Arrow;
        CoreCursorType cursorHand = CoreCursorType.Hand;        

        void Display_DpiChanged(DisplayInformation sender, object args)
        {
            displayDpi = sender.LogicalDpi;

            // Manually call the ViewChanged handler to update DpiScale.
            scrollViewer_ViewChanged(null, null);
        }

        public static void cancelAll()
        {
            isDrawing = false;
            drawLine = false;
            drawingLine = false;
            drawRect = false;
            drawingRect = false;
            drawCircle = false;
            drawingCircle = false;
            drawArcPtP = false;
            drawingArcPtP = false;
            drawColuna = false;
            drawingColuna = false;

            measure = false;
            measuring = false;

            foreach (snap s in mainCanvas._mainCanvas.Snaps)
            {
                s.isVisible = false;
            }

            _mainCanvas.canvas.Invalidate();
        }

        public Point AbsoluteToCartesian(Point pos)
        {
            Point cPOS;

            cPOS.X = pos.X - (canvas.ActualWidth / 2);
            cPOS.Y = (canvas.ActualHeight / 2) - pos.Y;

            return cPOS;
        }

        public Point CartesianToAbsolute(Point pos)
        {
            Point aPOS;

            aPOS.X = (canvas.ActualWidth / 2) + pos.X;
            aPOS.Y = (canvas.ActualHeight / 2) - pos.Y;

            return aPOS;
        }

        private void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(canvas).Properties.IsLeftButtonPressed)
            {
                #region LINE
                if (drawLine == true)
                {
                    if (drawingLine == false)
                    {
                        tempCursorPOSSTART = new Point(aCursorPOS.X, aCursorPOS.Y);

                        tLine = new line();
                        tLine.x1 = (float)aCursorPOS.X;
                        tLine.y1 = (float)aCursorPOS.Y;
                        drawingLine = true;
                    }
                    else
                    {
                        drawingLine = true; // true = desenho contínuo

                        Lines.Add(new line
                        {
                            x1 = tLine.x1,
                            y1 = tLine.y1,
                            x2 = tLine.x2,
                            y2 = tLine.y2
                        });

                        // extremidades
                        Snaps.Add(new snap
                        {
                            type = 1,
                            rect = new Rect(new Point(tLine.x1 + near, tLine.y1 + near), new Point(tLine.x1 - near, tLine.y1 - near)),
                            isVisible = false
                        });

                        Snaps.Add(new snap
                        {
                            type = 1,
                            rect = new Rect(new Point(tLine.x2 + near, tLine.y2 + near), new Point(tLine.x2 - near, tLine.y2 - near)),
                            isVisible = false
                        });

                        // meio
                        Snaps.Add(new snap
                        {
                            type = 2,
                            rect = new Rect(new Point(tLine.x1 + (tLine.x2 - tLine.x1) / 2 + near, tLine.y1 + (tLine.y2 - tLine.y1) / 2 + near),
                                            new Point(tLine.x1 + (tLine.x2 - tLine.x1) / 2 - near, tLine.y1 + (tLine.y2 - tLine.y1) / 2 - near)),
                            isVisible = false
                        });

                        tLine.x1 = tLine.x2; //desenho sequencial
                        tLine.y1 = tLine.y2; //desenho sequencial

                        tempCursorPOSSTART = new Point(tLine.x2, tLine.y2);

                        canvas.Invalidate();
                    }
                }
                #endregion

                #region RECT
                if (drawRect == true)
                {
                    if (drawingRect == false)
                    {
                        tempCursorPOSSTART = new Point(aCursorPOS.X, aCursorPOS.Y);

                        tRect = new rect();
                        tRect.x1 = (float)aCursorPOS.X;
                        tRect.y1 = (float)aCursorPOS.Y;

                        drawingRect = true;
                    }
                    else
                    {
                        drawingRect = false;

                        Rects.Add(new rect
                        {
                            x1 = tRect.x1,
                            y1 = tRect.y1,
                            x2 = tRect.x2,
                            y2 = tRect.y2

                        });
                        
                        //extremidades
                        Snaps.Add(new snap
                        {
                            type = 1,
                            rect = new Rect(new Point(tRect.x1 + near, tRect.y1 + near), new Point(tRect.x1 - near, tRect.y1 - near)),
                            isVisible = false
                        });

                        Snaps.Add(new snap
                        {
                            type = 1,
                            rect = new Rect(new Point(tRect.x2 + near, tRect.y1 + near), new Point(tRect.x2 - near, tRect.y1 - near)),
                            isVisible = false
                        });

                        Snaps.Add(new snap
                        {
                            type = 1,
                            rect = new Rect(new Point(tRect.x2 + near, tRect.y2 + near), new Point(tRect.x2 - near, tRect.y2 - near)),
                            isVisible = false
                        });

                        Snaps.Add(new snap
                        {
                            type = 1,
                            rect = new Rect(new Point(tRect.x1 + near, tRect.y2 + near), new Point(tRect.x1 - near, tRect.y2 - near)),
                            isVisible = false
                        });

                        //meios
                        Snaps.Add(new snap
                        {
                            type = 2,
                            rect = new Rect(new Point(tRect.x1 + (tRect.x2 - tRect.x1) / 2 + near, tRect.y1 + near),
                                            new Point(tRect.x1 + (tRect.x2 - tRect.x1) / 2 - near, tRect.y1 - near)),
                            isVisible = false
                        });

                        Snaps.Add(new snap
                        {
                            type = 2,
                            rect = new Rect(new Point(tRect.x1 + near, tRect.y1 + (tRect.y2 - tRect.y1) / 2 + near),
                                            new Point(tRect.x1 - near, tRect.y2 - (tRect.y2 - tRect.y1) / 2 - near)),
                            isVisible = false
                        });

                        Snaps.Add(new snap
                        {
                            type = 2,
                            rect = new Rect(new Point(tRect.x1 + (tRect.x2 - tRect.x1) / 2 + near, tRect.y2 + near),
                                            new Point(tRect.x1 + (tRect.x2 - tRect.x1) / 2 - near, tRect.y2 - near)),
                            isVisible = false
                        });

                        Snaps.Add(new snap
                        {
                            type = 2,
                            rect = new Rect(new Point(tRect.x2 + near, tRect.y1 + (tRect.y2 - tRect.y1) / 2 + near),
                                            new Point(tRect.x2 - near, tRect.y2 - (tRect.y2 - tRect.y1) / 2 - near)),
                            isVisible = false
                        });

                        // centro geometrico
                        Snaps.Add(new snap
                        {
                            type = 3,
                            rect = new Rect(new Point(tRect.x1 + (tRect.x2 - tRect.x1) / 2 + near, tRect.y1 + (tRect.y2 - tRect.y1) / 2 + near),
                                            new Point(tRect.x1 + (tRect.x2 - tRect.x1) / 2 - near, tRect.y2 - (tRect.y2 - tRect.y1) / 2 - near)),
                            isVisible = false
                        });

                        drawRect = false; //true = desenho continuo
                        isDrawing = false;
                        canvas.Invalidate();
                    }
                }
                #endregion

                #region CIRCLE
                if (drawCircle == true)
                {
                    if (drawingCircle == false)
                    {
                        tempCursorPOSSTART = new Point(aCursorPOS.X, aCursorPOS.Y);

                        tCircle = new circle();
                        tCircle.cx1 = (float)aCursorPOS.X;
                        tCircle.cy1 = (float)aCursorPOS.Y;

                        drawingCircle = true;
                    }
                    else
                    {
                        drawingCircle = false;

                        float radius = Vector2.Distance(new Vector2(tCircle.cx1, tCircle.cy1), new Vector2((float)aCursorPOS.X, (float)aCursorPOS.Y));

                        Circles.Add(new circle
                        {
                            cx1 = tCircle.cx1,
                            cy1 = tCircle.cy1,
                            radius = radius
                        });

                        // centro geométrico
                        Snaps.Add(new snap
                        {
                            type = 3,
                            rect = new Rect(new Point(tCircle.cx1 + near, tCircle.cy1 + near), new Point(tCircle.cx1 - near, tCircle.cy1 - near)),
                            isVisible = false
                        });

                        drawCircle = false; //true = desenho continuo
                        isDrawing = false;
                        canvas.Invalidate();                        
                    }
                }
                #endregion

                #region RULER
                if (measure == true)
                {
                    if (measuring == false)
                    {
                        tempCursorPOSSTART = new Point(aCursorPOS.X, aCursorPOS.Y);

                        tRuler = new line();
                        tRuler.x1 = (float)aCursorPOS.X;
                        tRuler.y1 = (float)aCursorPOS.Y;

                        measuring = true;
                    }
                    else
                    {
                        measuring = false;

                        
                    }
                }
                #endregion
            }

            #region CAMERAMOVE
            if (e.GetCurrentPoint(canvas).Properties.IsMiddleButtonPressed)
            {
                if (movingScreen == false)
                {
                    cursorMode = 1;
                    canvas.Invalidate();

                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(cursorHand, 0);

                    pointerOffset = new line();
                    movingScreen = true;
                    pointerOffset.x1 = (float)aCursorPOS.X;
                    pointerOffset.y1 = (float)aCursorPOS.Y;
                }
            }
            #endregion            
        }
        
        private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            cCursorPOS = isCartesian ? AbsoluteToCartesian(aCursorPOS) : aCursorPOS;
            aCursorPOS = new Point(e.GetCurrentPoint(canvas).Position.X, e.GetCurrentPoint(canvas).Position.Y);            

            MainPage._MainPage.tBox_XYZ.Text = "X: " + cCursorPOS.X.ToString("0.0000", new CultureInfo("en-US")) +
                                               " | Y: " + cCursorPOS.Y.ToString("0.0000", new CultureInfo("en-US")) + 
                                               " | Z: 0.0000 ";

            #region SNAP
            if (isSnapActivated && isDrawing)
            {
                int size = 5;

                foreach (snap s in Snaps)
                {
                    if (s.rect.Contains(aCursorPOS))
                    {
                        s.isVisible = true;
                        Point centerPOS = new Point(s.rect.X + near, s.rect.Y + near);
                        Rect insideRect = new Rect(new Point(centerPOS.X - size, centerPOS.Y - size), new Point(centerPOS.X + size, centerPOS.Y + size));

                        if (insideRect.Contains(aCursorPOS))
                        {
                            aCursorPOS = centerPOS;
                        }
                    }
                    else if (!s.rect.Contains(aCursorPOS) && s.type != 3)
                    {
                        s.isVisible = false;
                    }
                }

                #region CENTRO GEOMETRICO
                foreach (circle c in Circles)
                {
                    if (c.InsideCircle(aCursorPOS))
                    {
                        float cx1 = c.cx1 - near;

                        Snaps.Find(p => p.rect.X == cx1).isVisible = true;
                    }
                }

                foreach (rect r in Rects)
                {
                    if (r.Contains(aCursorPOS, near))
                    {
                        float rx1 = r.x1 + (r.x2 - r.x1) / 2 - near;
                        float ry1 = r.y1 + (r.y2 - r.y1) / 2 - near;

                        Snaps.Find(p => p.rect.X == rx1 && p.rect.Y == ry1).isVisible = true;
                    }
                }
                #endregion
            }
            #endregion
            //bug após dar Zoom e clicar com a mousewheel e tentar mover
            #region CAMERAMOVE 
            if (movingScreen == true)
            {
                pointerOffset.x2 = (float)aCursorPOS.X;
                pointerOffset.y2 = (float)aCursorPOS.Y;

                if (pointerOffset.x2 > pointerOffset.x1)
                {
                    _hO = scrollViewer.HorizontalOffset - (pointerOffset.x2 - pointerOffset.x1);
                }
                else if (pointerOffset.x2 < pointerOffset.x1)
                {
                    _hO = scrollViewer.HorizontalOffset + (pointerOffset.x1 - pointerOffset.x2);
                }

                double hO = _hO;


                if (pointerOffset.y2 > pointerOffset.y1)
                {
                    _vO = scrollViewer.VerticalOffset - (pointerOffset.y2 - pointerOffset.y1);
                }
                else if (pointerOffset.y2 < pointerOffset.y1)
                {
                    _vO = scrollViewer.VerticalOffset + (pointerOffset.y1 - pointerOffset.y2);
                }

                double vO = _vO;

                scrollViewer.ChangeView(hO, vO, 1);
            }
            #endregion            

            #region ORTHO CALCULATIONS
            if (isOrthoActivated == true && isDrawing == true)
            {
                if (Math.Abs(aCursorPOS.X - tempCursorPOSSTART.X) > Math.Abs(aCursorPOS.Y - tempCursorPOSSTART.Y))
                {
                    tempCursorPOSEND = new Point(aCursorPOS.X, tempCursorPOSSTART.Y);
                }
                else
                {
                    tempCursorPOSEND = new Point(tempCursorPOSSTART.X, aCursorPOS.Y);
                }
            }
            else
            {
                tempCursorPOSEND = aCursorPOS;
            }
            #endregion            

            if (drawingLine == true)
            {
                tLine.x2 = (float)tempCursorPOSEND.X;
                tLine.y2 = (float)tempCursorPOSEND.Y;
            }

            if (drawingRect == true)
            {
                tRect.x2 = (float)tempCursorPOSEND.X;
                tRect.y2 = (float)tempCursorPOSEND.Y;
            }

            if (drawingCircle == true)
            {
                tCircle.rx2 = (float)tempCursorPOSEND.X;
                tCircle.ry2 = (float)tempCursorPOSEND.Y;
            }

            if (measuring == true)
            {
                tRuler.x2 = (float)tempCursorPOSEND.X;
                tRuler.y2 = (float)tempCursorPOSEND.Y;
            }

            canvas.Invalidate();
        }
        
        private void canvas_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            float tLineSWidth = 2;

            CanvasStrokeStyle strokeStyle = new CanvasStrokeStyle
            {
                DashStyle = CanvasDashStyle.Dot,
                DashCap = CanvasCapStyle.Round,
                StartCap = CanvasCapStyle.Round,
                EndCap = CanvasCapStyle.Round,
                LineJoin = CanvasLineJoin.Bevel,
            };

            CanvasStrokeStyle strokeStyle2 = new CanvasStrokeStyle
            {
                CustomDashStyle = new float[] { 15f, 10f }
            };

            CanvasTextFormat infoRectTextFormat = new CanvasTextFormat
            {
                FontSize = 12.0f,
                WordWrapping = CanvasWordWrapping.NoWrap
            };

            Color infoRectBGColor = Colors.Black;
            Color infoRectTextColor = Colors.DarkGray;

            var dir = CanvasSweepDirection.CounterClockwise;

            float cursorSize = 50;
            float insideCursorSize = 3;

            foreach (var region in args.InvalidatedRegions)
            {
                using (var drawingSession = sender.CreateDrawingSession(region))
                {
                    #region XYZ Axis
                    //Y
                    drawingSession.DrawLine(new Vector2((float)canvas.ActualWidth / 2, 0), new Vector2((float)canvas.ActualWidth / 2, (float)canvas.ActualHeight / 2), Colors.Green, 1);

                    drawingSession.DrawLine(new Vector2((float)canvas.ActualWidth / 2, (float)canvas.ActualHeight / 2), new Vector2((float)canvas.ActualWidth / 2, ((float)canvas.ActualHeight / 2) - 50), Colors.White, 1);

                    drawingSession.DrawLine(new Vector2(((float)canvas.ActualWidth / 2), ((float)canvas.ActualHeight / 2) - 70), new Vector2(((float)canvas.ActualWidth / 2) + 7, ((float)canvas.ActualHeight / 2) - 60), Colors.White, 1);// \
                    drawingSession.DrawLine(new Vector2(((float)canvas.ActualWidth / 2) + 14, ((float)canvas.ActualHeight / 2) - 70), new Vector2(((float)canvas.ActualWidth / 2) + 7, ((float)canvas.ActualHeight / 2) - 60), Colors.White, 1); // /
                    drawingSession.DrawLine(new Vector2(((float)canvas.ActualWidth / 2) + 7, ((float)canvas.ActualHeight / 2) - 50), new Vector2(((float)canvas.ActualWidth / 2) + 7, ((float)canvas.ActualHeight / 2) - 60), Colors.White, 1);// |
                    //X
                    drawingSession.DrawLine(new Vector2((float)canvas.ActualWidth / 2, (float)canvas.ActualHeight / 2), new Vector2((float)canvas.ActualWidth, (float)canvas.ActualHeight / 2), Colors.Red, 1);

                    drawingSession.DrawLine(new Vector2((float)canvas.ActualWidth / 2, (float)canvas.ActualHeight / 2), new Vector2(((float)canvas.ActualWidth /2) + 50, (float)canvas.ActualHeight / 2), Colors.White, 1);

                    drawingSession.DrawLine(new Vector2(((float)canvas.ActualWidth / 2) + 51, (float)canvas.ActualHeight / 2 - 21), new Vector2(((float)canvas.ActualWidth / 2) + 71, (float)canvas.ActualHeight / 2 - 1), Colors.White, 1);
                    drawingSession.DrawLine(new Vector2(((float)canvas.ActualWidth / 2) + 71, (float)canvas.ActualHeight / 2 - 21), new Vector2(((float)canvas.ActualWidth / 2) + 51, (float)canvas.ActualHeight / 2 - 1), Colors.White, 1);
                    //Z
                    //maybeoneday
                    #endregion

                    #region CURSOR
                    if (windowsCursor == false)
                    {
                        if (cursorMode == 0)
                        {
                            //vertical
                            drawingSession.DrawLine(new Vector2((float)aCursorPOS.X, (float)aCursorPOS.Y - cursorSize), new Vector2((float)aCursorPOS.X, (float)aCursorPOS.Y - insideCursorSize), Colors.White, 1);
                            drawingSession.DrawLine(new Vector2((float)aCursorPOS.X, (float)aCursorPOS.Y + insideCursorSize), new Vector2((float)aCursorPOS.X, (float)aCursorPOS.Y + cursorSize), Colors.White, 1);
                            //horizontal
                            drawingSession.DrawLine(new Vector2((float)aCursorPOS.X - cursorSize, (float)aCursorPOS.Y), new Vector2((float)aCursorPOS.X - insideCursorSize, (float)aCursorPOS.Y), Colors.White, 1);
                            drawingSession.DrawLine(new Vector2((float)aCursorPOS.X + insideCursorSize, (float)aCursorPOS.Y), new Vector2((float)aCursorPOS.X + cursorSize, (float)aCursorPOS.Y), Colors.White, 1);
                            //retangulo
                            drawingSession.DrawRectangle(new Rect(new Point(aCursorPOS.X - insideCursorSize, aCursorPOS.Y - insideCursorSize), new Point(aCursorPOS.X + insideCursorSize, aCursorPOS.Y + insideCursorSize)), Colors.White, 1);
                        }
                    }
                    else
                    {
                        Window.Current.CoreWindow.PointerCursor = new CoreCursor(cursorNormal, 0);
                    }
                    #endregion

                    #region LINE
                    if (drawingLine == true)
                    {
                        // Linha do desenho
                        Vector2 tLineP1 = new Vector2(tLine.x1, tLine.y1);
                        Vector2 tLineP2 = new Vector2(tLine.x2, tLine.y2);

                        drawingSession.DrawLine(tLineP1, tLineP2, Colors.White, 1);

                        // Linha paralela
                        float L = (float)(Math.Sqrt((tLineP1.X - tLineP2.X) * (tLineP1.X - tLineP2.X) + (tLineP1.Y - tLineP2.Y) * (tLineP1.Y - tLineP2.Y)));
                        float offsetPixels = 50;
                        float radius = Vector2.Distance(tLineP1, tLineP2);
                        float yAxis = -180;

                        if (tLine.y2 > tLine.y1)
                        {
                            yAxis = 180;
                            offsetPixels = -50;                            
                            dir = CanvasSweepDirection.Clockwise;
                        }

                        Vector2 pLineP1 = new Vector2(tLineP1.X + offsetPixels * (tLineP2.Y - tLineP1.Y) / L, tLineP1.Y + offsetPixels * (tLineP1.X - tLineP2.X) / L);
                        Vector2 pLineP2 = new Vector2(tLineP2.X + offsetPixels * (tLineP2.Y - tLineP1.Y) / L, tLineP2.Y + offsetPixels * (tLineP1.X - tLineP2.X) / L);

                        drawingSession.DrawLine(pLineP1, pLineP2, Colors.DarkGray, tLineSWidth, strokeStyle); // diagonal
                        drawingSession.DrawLine(tLineP1, pLineP1, Colors.DarkGray, tLineSWidth, strokeStyle); // ligação 1
                        drawingSession.DrawLine(tLineP2, pLineP2, Colors.DarkGray, tLineSWidth, strokeStyle); // ligação 2

                        // linha adjacente
                        drawingSession.DrawLine(tLineP1.X, tLineP1.Y, tLineP1.X + L, tLineP1.Y, Colors.DarkGray, tLineSWidth, strokeStyle);

                        // arco
                        CanvasPathBuilder pathBuilder = new CanvasPathBuilder(sender);
                        pathBuilder.BeginFigure(new Vector2(tLineP1.X + L, tLineP1.Y));

                        pathBuilder.AddArc(
                            tLineP2,
                            radius,
                            radius,
                            0, dir, CanvasArcSize.Small
                            );

                        pathBuilder.EndFigure(CanvasFigureLoop.Open);
                        
                        CanvasGeometry geometry = CanvasGeometry.CreatePath(pathBuilder);                        
                        drawingSession.DrawGeometry(geometry, Colors.DarkGray, 2, strokeStyle);

                        // InfoBox de medição do angulo                        
                        double angle = Math.Abs((Math.Atan2((tLineP1.Y) - tLineP2.Y, (tLineP1.X + radius) - tLineP2.X)) * 360 / Math.PI + yAxis);
                        double arcLenght = (angle * Math.PI * radius) / 180;

                        Vector2 midPoint = geometry.ComputePointOnPath((float)arcLenght/2);

                        CanvasTextLayout textLayout = new CanvasTextLayout(drawingSession, Math.Round(angle).ToString() + "°", infoRectTextFormat, 0.0f, 0.0f);

                        Rect infoRect = new Rect(midPoint.X - ((float)textLayout.DrawBounds.Width / 2) - 5, 
                                                       midPoint.Y - ((float)textLayout.DrawBounds.Height / 2) - 5,
                                                       textLayout.DrawBounds.Width + 10,
                                                       textLayout.DrawBounds.Height + 10
                                                       );

                        drawingSession.FillRectangle(infoRect, infoRectBGColor); // fundo
                        drawingSession.DrawTextLayout(textLayout, 
                                                      midPoint.X - ((float)textLayout.DrawBounds.Width / 2),
                                                      midPoint.Y - ((float)textLayout.DrawBounds.Height),
                                                      infoRectTextColor
                                                      ); 
                    }
                    #endregion

                    #region RECT
                    if (drawingRect == true)
                    {
                        drawingSession.DrawRectangle(new Rect(new Point(tRect.x1, tRect.y1), new Point(tRect.x2, tRect.y2)), Colors.White, 1);
                    }
                    #endregion

                    #region CIRCLE
                    if (drawingCircle == true)
                    {
                        // Linha do desenho
                        drawingSession.DrawLine(tCircle.cx1, tCircle.cy1, tCircle.rx2, tCircle.ry2, Colors.Orange, 1, strokeStyle2);

                        // Linha paralela
                        float L = (float)(Math.Sqrt((tCircle.cx1 - tCircle.rx2) * (tCircle.cx1 - tCircle.rx2) + (tCircle.cy1 - tCircle.ry2) * (tCircle.cy1 - tCircle.ry2)));

                        float offsetPixels = 50;

                        float x1p = tCircle.cx1 + offsetPixels * (tCircle.ry2 - tCircle.cy1) / L;
                        float x2p = tCircle.rx2 + offsetPixels * (tCircle.ry2 - tCircle.cy1) / L;
                        float y1p = tCircle.cy1 + offsetPixels * (tCircle.cx1 - tCircle.rx2) / L;
                        float y2p = tCircle.ry2 + offsetPixels * (tCircle.cx1 - tCircle.rx2) / L;

                        drawingSession.DrawLine(x1p, y1p, x2p, y2p, Colors.DarkGray, tLineSWidth, strokeStyle); // diagonal
                        drawingSession.DrawLine(tCircle.cx1, tCircle.cy1, x1p, y1p, Colors.DarkGray, tLineSWidth, strokeStyle); // ligação 1
                        drawingSession.DrawLine(tCircle.rx2, tCircle.ry2, x2p, y2p, Colors.DarkGray, tLineSWidth, strokeStyle); // ligação 2

                        drawingSession.DrawCircle(tCircle.cx1, tCircle.cy1, Vector2.Distance(new Vector2(tCircle.cx1, tCircle.cy1), new Vector2(tCircle.rx2, tCircle.ry2)), Colors.White, 1);
                    }
                    #endregion

                    #region RULER
                    if (measuring == true)
                    {
                        // Linha do desenho
                        Vector2 rLineP1 = new Vector2(tRuler.x1, tRuler.y1);
                        Vector2 rLineP2 = new Vector2(tRuler.x2, tRuler.y2);

                        drawingSession.DrawLine(rLineP1, rLineP2, Colors.Orange, 1, strokeStyle2);

                        // Linha paralela
                        float L = Convert.ToInt16(Math.Sqrt((rLineP1.X - rLineP2.X) * (rLineP1.X - rLineP2.X) + (rLineP1.Y - rLineP2.Y) * (rLineP1.Y - rLineP2.Y)));
                        float offsetPixels = 50;

                        Vector2 pLineP1 = new Vector2(rLineP1.X + offsetPixels * (rLineP2.Y - rLineP1.Y) / L, rLineP1.Y + offsetPixels * (rLineP1.X - rLineP2.X) / L);
                        Vector2 pLineP2 = new Vector2(rLineP2.X + offsetPixels * (rLineP2.Y - rLineP1.Y) / L, rLineP2.Y + offsetPixels * (rLineP1.X - rLineP2.X) / L);

                        drawingSession.DrawLine(pLineP1, pLineP2, Colors.DarkGray, tLineSWidth, strokeStyle); // diagonal
                        drawingSession.DrawLine(rLineP1, pLineP1, Colors.DarkGray, tLineSWidth, strokeStyle); // ligação 1
                        drawingSession.DrawLine(rLineP2, pLineP2, Colors.DarkGray, tLineSWidth, strokeStyle); // ligação 2

                        // InfoBox de medição do comprimento
                        float lenght = Vector2.Distance(rLineP1, rLineP2);

                        Vector2 midPoint = new Vector2(pLineP1.X + (pLineP2.X - pLineP1.X) / 2, pLineP1.Y + (pLineP2.Y - pLineP1.Y) / 2);

                        CanvasTextLayout textLayout = new CanvasTextLayout(drawingSession, lenght.ToString("0.0000", new CultureInfo("en-US")), infoRectTextFormat, 0.0f, 0.0f);

                        Rect infoRect = new Rect(midPoint.X - (textLayout.DrawBounds.Width / 2) - 5,
                                                                midPoint.Y - (textLayout.DrawBounds.Height / 2) - 5, 
                                                                textLayout.DrawBounds.Width + 10, 
                                                                textLayout.DrawBounds.Height + 10
                                                                );

                        drawingSession.FillRectangle(infoRect, infoRectBGColor); // fundo
                        drawingSession.DrawTextLayout(textLayout, 
                                                      midPoint.X - ((float)textLayout.DrawBounds.Width / 2), 
                                                      midPoint.Y - ((float)textLayout.DrawBounds.Height),
                                                      infoRectTextColor
                                                      );
                    }
                    #endregion

                    #region SNAP
                    if (isSnapActivated == true && isDrawing == true)
                    {
                        foreach (snap s in Snaps)
                        {
                            if (s.isVisible == true)
                            {
                                int size = 5;

                                Point centerPOS = new Point(s.rect.X + near, s.rect.Y + near);

                                if (s.type == 1)
                                {
                                    drawingSession.DrawRectangle(new Rect(new Point(centerPOS.X - size, centerPOS.Y - size), new Point(centerPOS.X + size, centerPOS.Y + size)), Colors.DeepSkyBlue, 2);
                                }
                                else if (s.type == 2)
                                {
                                    drawingSession.DrawLine(new Vector2((float)centerPOS.X - size, (float)centerPOS.Y + size), new Vector2((float)centerPOS.X + size, (float)centerPOS.Y + size), Colors.DeepSkyBlue, 2);//base
                                    drawingSession.DrawLine(new Vector2((float)centerPOS.X - size, (float)centerPOS.Y + size), new Vector2((float)centerPOS.X, (float)centerPOS.Y - size), Colors.DeepSkyBlue, 2);//lateral esq
                                    drawingSession.DrawLine(new Vector2((float)centerPOS.X + size, (float)centerPOS.Y + size), new Vector2((float)centerPOS.X, (float)centerPOS.Y - size), Colors.DeepSkyBlue, 2);//lateral dir
                                }
                                else if (s.type == 3)
                                {
                                    drawingSession.DrawLine(new Vector2((float)centerPOS.X, (float)centerPOS.Y - size), new Vector2((float)centerPOS.X, (float)centerPOS.Y + size), Colors.Red, 1);// |
                                    drawingSession.DrawLine(new Vector2((float)centerPOS.X - size, (float)centerPOS.Y), new Vector2((float)centerPOS.X + size, (float)centerPOS.Y), Colors.Red, 1);// -

                                    drawingSession.DrawLine(new Vector2((float)centerPOS.X - size, (float)centerPOS.Y - size), new Vector2((float)centerPOS.X + size, (float)centerPOS.Y + size), Colors.Red, 1);// \
                                    drawingSession.DrawLine(new Vector2((float)centerPOS.X + size, (float)centerPOS.Y - size), new Vector2((float)centerPOS.X - size, (float)centerPOS.Y + size), Colors.Red, 1);// /
                                }
                            }
                        }                        
                    }
                    #endregion

                    foreach (line l in Lines)
                    {
                        drawingSession.DrawLine(l.x1, l.y1, l.x2, l.y2, Colors.White, 1);
                    }

                    foreach (rect r in Rects)
                    {
                        drawingSession.DrawRectangle(new Rect(new Point(r.x1, r.y1), new Point(r.x2, r.y2)), Colors.White, 1);
                    }

                    foreach (circle c in Circles)
                    {
                        drawingSession.DrawCircle(new Vector2(c.cx1, c.cy1), c.radius, Colors.White, 1);
                    }
                }
            }
        }

        private void canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (movingScreen == true)
            {
                cursorMode = 0;

                movingScreen = false;
                Window.Current.CoreWindow.PointerCursor = null;

                canvas.Invalidate();
            }
        }

        private void canvas_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                cancelAll();
            }

            if (e.Key == VirtualKey.F3)
            {
                isSnapActivated = isSnapActivated ? false : true;
            }

            if (e.Key == VirtualKey.F8)
            {
                isOrthoActivated = isOrthoActivated ? false : true;
            }

            if (e.Key == VirtualKey.A || e.Key == VirtualKey.Z)
            {
                var currentZoom = scrollViewer.ZoomFactor;
                var newZoom = currentZoom;

                if (e.Key == VirtualKey.A)
                    newZoom /= 0.9f;
                else
                    newZoom *= 0.9f;

                newZoom = Math.Max(newZoom, scrollViewer.MinZoomFactor);
                newZoom = Math.Min(newZoom, scrollViewer.MaxZoomFactor);

                var currentPan = new Vector2((float)scrollViewer.HorizontalOffset,
                                             (float)scrollViewer.VerticalOffset);

                var centerOffset = new Vector2((float)scrollViewer.ViewportWidth,
                                               (float)scrollViewer.ViewportHeight) / 2;

                var newPan = ((currentPan + centerOffset) * newZoom / currentZoom) - centerOffset;

                scrollViewer.ChangeView(newPan.X, newPan.Y, newZoom);

                e.Handled = true;
            }
        }

        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Cancel out the display DPI, so our fractal always renders at 96 DPI regardless of display
            // configuration. This boosts performance on high DPI displays, at the cost of visual quality.
            // For even better performance (but lower quality) this value could be further reduced.
            float dpiAdjustment = 96 / displayDpi;

            // Adjust DPI to match the current zoom level.
            float dpiScale = dpiAdjustment * scrollViewer.ZoomFactor;

            // To boost performance during pinch-zoom manipulations, we only update DPI when it has
            // changed by more than 20%, or at the end of the zoom (when e.IsIntermediate reports false).
            // Smaller changes will just scale the existing bitmap, which is much faster than recomputing
            // the fractal at a different resolution. To trade off between zooming perf vs. smoothness,
            // adjust the thresholds used in this ratio comparison.
            var ratio = canvas.DpiScale / dpiScale;

            if (e == null || !e.IsIntermediate || ratio <= 0.8 || ratio >= 1.25)
            {
                canvas.DpiScale = dpiScale;
            }
        }

        private void control_Loaded(object sender, RoutedEventArgs e)
        {
            //Centraliza o scrollviewer
            scrollViewer.ChangeView((canvas.ActualWidth / 2) - scrollViewer.ActualWidth / 2, (canvas.ActualHeight / 2) - scrollViewer.ActualHeight / 2, 1);

            // Initialize the display DPI, and listen for events in case this changes.
            var display = DisplayInformation.GetForCurrentView();
            display.DpiChanged += Display_DpiChanged;
            Display_DpiChanged(display, null);

            // Set focus to our control, so it will receive keyboard input.
            canvas.Focus(FocusState.Programmatic);
        }

        private void control_Unloaded(object sender, RoutedEventArgs e)
        {
            // Explicitly remove references to allow the Win2D controls to get garbage collected
            canvas.RemoveFromVisualTree();
            canvas = null;

            DisplayInformation.GetForCurrentView().DpiChanged -= Display_DpiChanged;
        }

        private void canvas_CreateResources(CanvasVirtualControl sender, CanvasCreateResourcesEventArgs args)
        {
            // Don't bother reloading our shaders if it is only the DPI that changed.
            // That happens all the time due to ScrollViewer_ViewChanged adjusting canvas.DpiScale.
            if (args.Reason == CanvasCreateResourcesReason.DpiChanged)
                return;
        }

        private void canvas_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = null;

            aCursorPOS = new Point(e.GetCurrentPoint(canvas).Position.X, e.GetCurrentPoint(canvas).Position.Y);
        }

        private void canvas_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(cursorNormal, 0);
        }
    }
}
