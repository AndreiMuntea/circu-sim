using circu_sim.CircuitLogic;
using circu_sim.GraphicComponents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace circu_sim
{
    public partial class CircuitSimulator
    {
        private readonly BindingList<Tuple<BaseCircuit, string>> AllCircuits = new();
        private readonly List<CircuitDisplayComponent> DisplayCircuits = new();

        private void PictureBoxSaveButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (sender is not PictureBox)
            {
                return;
            }

            SaveCircuit();

            ClearBoard();
        }

        private void SaveCircuit()
        {
            try
            {
                string circuitFolder = CircuitCreator.GetCircuitDirectoryPath();
                Microsoft.Win32.SaveFileDialog dialog = new()
                {
                    FileName = "Circuit",
                    DefaultExt = ".xml",
                    Filter = "XML Documents (.xml)|*.xml",
                    InitialDirectory = circuitFolder
                };

                // If this is true, it means Save was pressed, not cancel, not close.
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    string? circuitPath = Path.GetDirectoryName(dialog.FileName);
                    var circuitToSave = CurrentCircuit?.Clone();
                    if (circuitPath == null || circuitToSave == null || !Path.GetFullPath(circuitPath).Equals(circuitFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new FileNotFoundException("The circuit can only be saved in dedicated folder!");
                    }
                    else
                    {
                        string circuitName = Path.GetFileNameWithoutExtension(dialog.FileName);
                        CircuitCreator.SerializeCircuitToFile(circuitToSave, circuitName);

                        AllCircuits.Add(new Tuple<BaseCircuit, string>(circuitToSave, circuitName));
                    }
                }
            }
            catch (Exception ex)
            {
                string failMessage = "Cannot save circuit! Exception: " + ex.Message;
                MessageBox.Show(
                    failMessage,
                    "Cannot save circuit!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void DataGridViewAllCircuits_MouseClick(object? Sender, EventArgs e)
        {
            if (Sender is not DataGridView dataGridViewAllCircuits)
            {
                return;
            }

            if (dataGridViewAllCircuits.SelectedRows == null)
            {
                return;
            }

            var selectedIndex = dataGridViewAllCircuits.CurrentCell.RowIndex;
            if (selectedIndex < 0 || selectedIndex >= AllCircuits.Count)
            {
                return;
            }
            var selectedCircuit = AllCircuits[selectedIndex];

            var logicCircuit = selectedCircuit.Item1.Clone();
            if (logicCircuit == null)
            {
                return;
            }

            AddDisplayCircuit(logicCircuit, selectedCircuit.Item2);
        }

        private void AddDisplayCircuit(BaseCircuit LogicCircuit, string CircuitName)
        {
            var displayCircuit = new CircuitDisplayComponent(
                LogicCircuit,
                CircuitName,
                Properties.Resources.Connector,
                GetCircuitSize(LogicCircuit),
                GetCircuitConnectorSize(),
                GetCircuitLabelFont()
            );
            var displayLocation = new Point(
                PictureBoxBoard.Width / 2 - displayCircuit.CircuitLabel.Width / 2,
                PictureBoxBoard.Height / 2 - displayCircuit.CircuitLabel.Height / 2
            );
            displayCircuit.SetLocation(displayLocation);

            foreach (var control in displayCircuit.ConnectorInputs)
            {
                PictureBoxBoard.Controls.Add(control);
                control.MouseMove += Line_MouseMove;
                control.MouseClick += CircuitInputNode_OnMouseClick;
            }
            foreach (var control in displayCircuit.ConnectorOutputs)
            {
                PictureBoxBoard.Controls.Add(control);
                control.MouseMove += Line_MouseMove;
                control.MouseClick += CircuitOutputNode_OnMouseClick;
            }

            PictureBoxBoard.Controls.Add(displayCircuit.CircuitLabel);
            DisplayCircuits.Add(displayCircuit);
            CurrentCircuit.InsertCircuit(displayCircuit.LogicCircuit);

            displayCircuit.BringToFront();
            displayCircuit.CircuitLabel.MouseDown += Circuit_OnMouseDown;
            displayCircuit.CircuitLabel.MouseMove += Line_MouseMove;
        }

        private void Circuit_OnMouseDown(object? Sender, MouseEventArgs e)
        {
            if (Sender is not DoubleBufferedLabel circuitLabel)
            {
                return;
            }

            if (CurrentLine is not null)
            {
                ClearCurrentLine();
                return;
            }

            var displayCircuit = GetDisplayCircuit(circuitLabel);
            if (displayCircuit is null)
            {
                return;
            }
            displayCircuit.BringToFront();

            if (IsDeleteInProgress)
            {
                DeleteCircuit(circuitLabel);
                ClearDeleteInProgress();
                return;
            }

            var location = PictureBoxBoard.PointToClient(new Point(e.X, e.Y));
            displayCircuit.SetLocation(location);

            circuitLabel.DoDragDrop(displayCircuit, DragDropEffects.All);

            PictureBoxBoard.Refresh();
        }

        private void PictureBoxBoard_DragEnter(object? Sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CircuitDisplayComponent)))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void PictureBoxBoard_DragOver(object? Sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CircuitDisplayComponent)))
            {
                var displayCircuit = (CircuitDisplayComponent)e.Data.GetData(typeof(CircuitDisplayComponent));

                var location = PictureBoxBoard.PointToClient(new Point(e.X, e.Y));
                displayCircuit.SetLocation(location);

                PictureBoxBoard.Refresh();
            }
        }

        private void PictureBoxBoard_DragDrop(object? Sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CircuitDisplayComponent)))
            {
                var displayCircuit = (CircuitDisplayComponent)e.Data.GetData(typeof(CircuitDisplayComponent));

                var location = PictureBoxBoard.PointToClient(new Point(e.X, e.Y));
                displayCircuit.SetLocation(location);

                PictureBoxBoard.Refresh();
            }
        }
    }
}
