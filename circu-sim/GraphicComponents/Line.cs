using System;
using System.Drawing;
using System.Windows.Forms;

namespace circu_sim.GraphicComponents
{
    public class Line
    {
        public OnOffComponent StartingComponent { get; set; }
        public OnOffComponent? StoppingComponent { get; set; }

        private readonly PictureBox Connector;

        private PointF StartingPoint;
        private PointF StoppingPoint;

        public PointF MousePoint { get; set; }

        public Line(OnOffComponent StartingComponent, PictureBox Connector)
        {
            this.StartingComponent = StartingComponent;
            this.Connector = Connector;
        }

        private void SetStartingPoint()
        {
            StartingPoint = new PointF(
                Connector.Right - 1,
                Connector.Top + (Connector.Height / 2)
            );
        }

        private void SetStoppingPoint()
        {
            if (StoppingComponent == null)
            {
                return;
            }

            StoppingPoint = new PointF(
                StoppingComponent.Left,
                StoppingComponent.Top + (StoppingComponent.Height / 2) - 1
            );
        }

        public void GenerateLine(PaintEventArgs Canvas)
        {
            SetStartingPoint();
            SetStoppingPoint();

            //
            // If the Starting Component is signaled, draw the line with green,
            // otherwise draw it black
            //
            string colorHex = StartingComponent.Node.CurrentState ? "#308B5B"      // green
                                                                  : "#4D4D4D";     // black

            if (StoppingComponent != null)
            {
                DrawLine(Canvas, StartingPoint, StoppingPoint, colorHex);
            }
            else
            {
                DrawLine(Canvas, StartingPoint, MousePoint, colorHex);
            }
        }

        private static void DrawLine(PaintEventArgs Canvas, PointF From, PointF To, string ColorHex)
        {
            Canvas.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Pen LinePen = new(ColorTranslator.FromHtml(ColorHex), 5);
            Canvas.Graphics.DrawLine(LinePen, From.X, From.Y, To.X, To.Y);
            LinePen.Dispose();
        }

        public bool IsLineNearPoint(PointF Point)
        {
            if (StoppingComponent == null)
            {
                return false;
            }

            //
            // We calculate the distance from our point to the line
            // given by the points - StartingPoint and StoppingPoint.
            //
            double distance = ComputePointToLineDistance(StartingPoint, StoppingPoint, Point);

            // If the distance is smaller than a delta, we consider the point to be near the line.
            const double delta = 10;

            return (distance < delta);
        }

        private static double ComputePointToLineDistance(PointF A, PointF B, PointF Point)
        {
            //
            // Equation of line AB: a * x + b * y + c = 0 (where a,b,c are coefficients).
            //
            // OR
            //                      (y - yA)    (x - xA)
            // Equation of line AB: --------  = ---------
            //                      (yB - yA)   (xB - xA)
            //
            // <=>
            //
            // (y - yA)    (x - xA)
            // --------  = ---------
            // (yB - yA)   (xB - xA)
            //
            // <=>
            //
            // (y - yA) * (xB - xA) = (x - xA) * (yB - yA)                                  <=>
            //
            // (x - xA) * (yB - yA) - (y - yA) * (xB - xA) = 0                              <=>
            //
            // x * (yB - yA) - xA * (yB - yA) - y * (xB - xA) + yA * (xB - xA) = 0          <=>
            //
            // x * (yB - yA) - y * (xB - xA) - xA * (yB - yA) + yA * (xB - xA) = 0          <=>
            //
            // x * (yB - yA) + y * (xA - xB) + xA * (yA - yB) + yA * (xB - xA) = 0          <=>
            //
            // x * (yB - yA) + y * (xA - xB) + xA * yA - xA * yB + yA * xB - yA * xA = 0    <=>
            //
            // x * (yB - yA) + y * (xA - xB) - xA * yB + yA * xB = 0                        <=>
            //
            // x * (yB - yA) + y * (xA - xB) + yA * xB - xA * yB = 0
            //     |-------|       |-------|   |---------------|
            // x *     a     + y *     b     +        c
            //
            //
            // We replace the coefficients => 
            //
            // a = yB - yA
            // b = xA - xB
            // c = yA * xB - xA * yB
            //
            double xA = A.X;
            double yA = A.Y;

            double xB = B.X;
            double yB = B.Y;

            double a = yB - yA;
            double b = xA - xB;
            double c = yA * xB - xA * yB;

            //
            // The distance from a point M (xM, yM) to the line AB (a * x + b * y + c = 0) has the following formula:
            //     |a * xM + b * yM + c|
            // d = ---------------------
            //      sqrt(a ^ 2 + b ^ 2)
            //
            return Math.Abs(a * Point.X + b * Point.Y + c) / Math.Sqrt(a * a + b * b);
        }
    }
}
