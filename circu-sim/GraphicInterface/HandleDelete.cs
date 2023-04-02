using circu_sim.GraphicComponents;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace circu_sim
{
    public partial class CircuitSimulator
    {
        private bool IsDeleteInProgress;

        private void PictureBoxDeleteButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (sender is not PictureBox)
            {
                return;
            }

            IsDeleteInProgress = true;
            SetMouseCursorToDelete();

            ClearCurrentLine();
        }

        private void PictureBoxClearButton_MouseClick(object? sender, MouseEventArgs e)
        {
            if (sender is not PictureBox)
            {
                return;
            }

            ClearBoard();

            ClearCurrentLine();
            ClearDeleteInProgress();
        }

        private void ClearBoard()
        {
            DeleteAllComponents();

            CreateNewCircuit();

            ClearDeleteInProgress();
        }

        private void DeleteAllComponents()
        {
            while (PictureBoxBoard.Controls.Count > 0)
            {
                var component = PictureBoxBoard.Controls[0];

                if (component is SwitchComponent switchComponent)
                {
                    DeleteSwitch(switchComponent);
                }

                else if (component is BulbComponent bulbComponent)
                {
                    DeleteBulb(bulbComponent);
                }

                else if (component is OnOffComponent circuitNode)
                {
                    DeleteCircuitNode(circuitNode);
                }

                else if (component is DoubleBufferedLabel circuit)
                {
                    DeleteCircuit(circuit);
                }
            }
        }

        private void DeleteSwitch(SwitchComponent SwitchComponent)
        {
            DeleteConnector(SwitchComponent);
            DeleteOnOffComponentLabel(SwitchComponent);

            CurrentCircuit.DeleteNode(SwitchComponent.Node);

            DeleteLines(GetLinksToSwitch(SwitchComponent));

            UnregisterSwitchEvents(SwitchComponent);

            Dispose(SwitchComponent);
        }

        private void DeleteConnector(SwitchComponent SwitchComponent)
        {
            var connectorComponent = GetConnectorBySwitch(SwitchComponent);
            SwitchToConnector.Remove(SwitchComponent);

            UnregisterConnectorEvents(connectorComponent);

            Dispose(connectorComponent);
        }

        private void DeleteBulb(BulbComponent BulbComponent)
        {
            DeleteOnOffComponentLabel(BulbComponent);

            CurrentCircuit.DeleteNode(BulbComponent.Node);

            DeleteLines(GetLinksToBulb(BulbComponent));

            UnregisterBulbEvents(BulbComponent);

            Dispose(BulbComponent);
        }

        private void DeleteOnOffComponentLabel(OnOffComponent OnOffComponent)
        {
            var onOffComponentLabel = GetLabelByOnOffComponent(OnOffComponent);
            OnOffComponentToLabel.Remove(OnOffComponent);

            UnregisterOnOffComponentLabelEvents(onOffComponentLabel);

            Dispose(onOffComponentLabel);
        }

        private void DeleteCircuitNode(OnOffComponent CircuitNode)
        {
            UnregisterCircuitNodeEvents(CircuitNode);

            DeleteLines(GetLinksToCircuitNode(CircuitNode));

            Dispose(CircuitNode);
        }

        private void DeleteCircuit(DoubleBufferedLabel Circuit)
        {
            var displayCircuit = GetDisplayCircuit(Circuit);
            if (displayCircuit is not null)
            {
                foreach (var input in displayCircuit.ConnectorInputs)
                {
                    DeleteCircuitNode(input);
                }
                foreach (var output in displayCircuit.ConnectorOutputs)
                {
                    DeleteCircuitNode(output);
                }

                DisplayCircuits.Remove(displayCircuit);
                CurrentCircuit.RemoveCircuit(displayCircuit.LogicCircuit);
            }

            UnregisterCircuitEvents(Circuit);

            Dispose(Circuit);
        }

        private void DeleteLines(List<Line> Lines)
        {
            foreach (var line in Lines)
            {
                DeleteLine(line);
            }
        }

        private void DeleteLine(Line Line)
        {
            AllLines.Remove(Line);
            PictureBoxBoard.Invalidate();

            var inputNode = Line.StartingComponent.Node;
            var outputNode = Line.StoppingComponent?.Node;

            if (outputNode != null)
            {
                CurrentCircuit.Unlink(inputNode, outputNode);
            }
        }

        private void UnregisterSwitchEvents(SwitchComponent SwitchComponent)
        {
            SwitchComponent.MouseClick -= PictureBoxSwitch_MouseClick;
            SwitchComponent.MouseDoubleClick -= PictureBoxSwitch_MouseClick;
            SwitchComponent.MouseMove -= Line_MouseMove;
        }

        private void UnregisterConnectorEvents(PictureBox ConnectorComponent)
        {
            ConnectorComponent.MouseClick -= PictureBoxConnector_MouseClick;
            ConnectorComponent.MouseMove -= Line_MouseMove;
        }

        private void UnregisterBulbEvents(BulbComponent BulbComponent)
        {
            BulbComponent.MouseClick -= PictureBoxBulb_MouseClick;
            BulbComponent.MouseMove -= Line_MouseMove;
        }

        private void UnregisterOnOffComponentLabelEvents(TextBox OnOffComponentLabel)
        {
            OnOffComponentLabel.TextChanged -= OnOffComponentLabel_TextChanged;
            OnOffComponentLabel.MouseMove -= Line_MouseMove;
        }

        private void UnregisterCircuitNodeEvents(OnOffComponent CircuitNode)
        {
            CircuitNode.MouseMove -= Line_MouseMove;
            CircuitNode.MouseClick -= CircuitInputNode_OnMouseClick;
            CircuitNode.MouseClick -= CircuitOutputNode_OnMouseClick;
        }

        private void UnregisterCircuitEvents(DoubleBufferedLabel Circuit)
        {
            Circuit.MouseDown -= Circuit_OnMouseDown;
            Circuit.MouseMove -= Line_MouseMove;
        }

        private void Dispose(Control Component)
        {
            PictureBoxBoard.Controls.Remove(Component);
            Component.Dispose();
        }

        private void ClearDeleteInProgress()
        {
            IsDeleteInProgress = false;
            SetMouseCursorToDefault();

            // Run a garbage collection on demand after delete.
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForFullGCComplete();
        }
    }
}
