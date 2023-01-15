using circu_sim.GraphicComponents;
using System.Collections.Generic;
using System.Windows.Forms;

namespace circu_sim
{
    public partial class CircuitSimulator
    {
        private Line? CurrentLine;
        private readonly List<Line> AllLines = new();

        private void PictureBoxConnector_MouseClick(object? sender, MouseEventArgs e)
        {
            if (sender is not PictureBox pictureBoxConnector)
            {
                return;
            }

            SwitchComponent switchComponent = GetSwitchByConnector(pictureBoxConnector);

            if (IsDeleteInProgress)
            {
                DeleteSwitch(switchComponent);
                ClearDeleteInProgress();

                return;
            }

            CurrentLine = new Line(switchComponent, pictureBoxConnector);
        }

        private void PictureBoxBulb_MouseClick(object? sender, MouseEventArgs e)
        {
            if (sender is not BulbComponent bulbComponent)
            {
                return;
            }

            if (IsDeleteInProgress)
            {
                DeleteBulb(bulbComponent);
                ClearDeleteInProgress();

                return;
            }

            CreateLink(bulbComponent);

            ClearCurrentLine();
        }

        private void CircuitOutputNode_OnMouseClick(object? Sender, MouseEventArgs e)
        {
            if (Sender is not OnOffComponent outputNode)
            {
                return;
            }

            if (IsDeleteInProgress)
            {
                return;
            }

            CurrentLine = new Line(outputNode, outputNode);
        }

        private void CircuitInputNode_OnMouseClick(object? Sender, MouseEventArgs e)
        {
            if (Sender is not OnOffComponent inputNode)
            {
                return;
            }

            if (IsDeleteInProgress)
            {
                return;
            }

            CreateLink(inputNode);

            ClearCurrentLine();
        }

        private void CreateLink(OnOffComponent StoppingComponent)
        {
            if (CurrentLine == null)
            {
                return;
            }

            var startingComponent = CurrentLine.StartingComponent;
            var existingLine = GetExistingLine(startingComponent, StoppingComponent);
            if (existingLine != null)
            {
                return;
            }

            CurrentLine.StoppingComponent = StoppingComponent;
            AllLines.Add(CurrentLine);

            var inputNode = startingComponent.Node;
            var outputNode = StoppingComponent.Node;

            CurrentCircuit.Link(inputNode, outputNode);
        }

        private void Line_MouseMove(object? sender, MouseEventArgs e)
        {
            if (CurrentLine == null)
            {
                return;
            }

            CurrentLine.MousePoint = PictureBoxBoard.PointToClient(Cursor.Position);

            PictureBoxBoard.Invalidate();
        }

        private void PictureBoxBoard_MouseClick(object? sender, MouseEventArgs e)
        {
            Line? line = GetClickedLine(e.Location);

            if (line != null && IsDeleteInProgress)
            {
                DeleteLine(line);
            }

            ClearCurrentLine();
            ClearDeleteInProgress();
        }

        private void PictureBoxBoard_Paint(object? sender, PaintEventArgs e)
        {
            foreach (Line line in AllLines)
            {
                line.GenerateLine(e);
            }

            CurrentLine?.GenerateLine(e);
        }

        private void ClearCurrentLine()
        {
            CurrentLine = null;
            PictureBoxBoard.Invalidate();
        }
    }
}
