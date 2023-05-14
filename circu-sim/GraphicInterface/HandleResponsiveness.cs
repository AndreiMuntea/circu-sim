using circu_sim.CircuitLogic;
using circu_sim.GraphicComponents;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace circu_sim
{
    public partial class CircuitSimulator
    {
        private void PictureBoxBoard_ClientSizeChanged(object sender, EventArgs e)
        {
            ReplaceControls();
            SetFont();
        }

        private void ReplaceControls()
        {
            foreach (var control in PictureBoxBoard.Controls)
            {
                if (control is SwitchComponent switchComponent)
                {
                    switchComponent.Size = GetOnOffComponenSize();
                    switchComponent.Location = GetSwitchLocation(switchComponent.Position);

                    var connector = GetConnectorBySwitch(switchComponent);
                    connector.Size = GetConnectorSize(switchComponent);
                    connector.Location = GetConnectorLocation(switchComponent);

                    var switchComponentLabel = GetLabelByOnOffComponent(switchComponent);
                    switchComponentLabel.Size = GetOnOffComponentLabelSize(switchComponentLabel);
                    switchComponentLabel.Location = GetOnOffComponentLabelLocation(switchComponent);
                }

                else if (control is BulbComponent bulbComponent)
                {
                    bulbComponent.Size = GetOnOffComponenSize();
                    bulbComponent.Location = GetBulbLocation(bulbComponent.Position);

                    var bulbComponentLabel = GetLabelByOnOffComponent(bulbComponent);
                    bulbComponentLabel.Size = GetOnOffComponentLabelSize(bulbComponentLabel);
                    bulbComponentLabel.Location = GetOnOffComponentLabelLocation(bulbComponent);
                }

                else if (control is Label labelCircuit)
                {
                    var circuit = GetDisplayCircuit(labelCircuit);
                    if (circuit == null)
                    {
                        return;
                    }

                    circuit.Resize(GetCircuitSize(circuit.LogicCircuit), GetCircuitConnectorSize(), GetCircuitLabelFont());
                }
            }

            PictureBoxBoard.Invalidate();
        }

        private Point GetSwitchLocation(int OnOffComponentPosition)
        {
            int leftPosition = (int)(0.03 * PictureBoxBoard.Width);
            int topPosition = GetOnOffComponentTopLocation(OnOffComponentPosition);

            return new Point(leftPosition, topPosition);
        }

        private Point GetBulbLocation(int OnOffComponentPosition)
        {
            int leftPosition = (int)(0.97 * PictureBoxBoard.Width) - GetOnOffComponenSize().Width;
            int topPosition = GetOnOffComponentTopLocation(OnOffComponentPosition);

            return new Point(leftPosition, topPosition);
        }

        private int GetOnOffComponentTopLocation(int OnOffComponentPosition)
        {
            int initialPosition = (int)(0.03 * PictureBoxBoard.Height);
            int padding = (int)(0.1 * PictureBoxBoard.Height);

            return initialPosition + (padding * OnOffComponentPosition);
        }

        private Size GetOnOffComponenSize()
        {
            int height = (int)(0.06 * Size.Height);
            return new Size(height, height);
        }

        private static Point GetConnectorLocation(SwitchComponent SwitchComponent)
        {
            return new Point(
                SwitchComponent.Right - 1,
                SwitchComponent.Top + ((SwitchComponent.Height - GetConnectorSize(SwitchComponent).Height) / 2)
            );
        }

        private static Size GetConnectorSize(SwitchComponent SwitchComponent)
        {
            return new Size(SwitchComponent.Size.Width / 2, SwitchComponent.Size.Height / 2);
        }

        private Point GetOnOffComponentLabelLocation(OnOffComponent OnOffComponent)
        {
            int padding = (int)(0.025 * PictureBoxBoard.Height);
            return new Point(OnOffComponent.Location.X, OnOffComponent.Location.Y - padding);
        }

        private static Size GetOnOffComponentLabelSize(TextBox OnOffComponentLabel)
        {
            return TextRenderer.MeasureText(OnOffComponentLabel.Text, OnOffComponentLabel.Font);
        }

        private static int GetCircuitIOCount(BaseCircuit Circuit)
        {
            var ioCount = Math.Max(Circuit.Inputs.Count, Circuit.Outputs.Count);
            return Math.Max(1, ioCount);
        }

        private Size GetCircuitSize(BaseCircuit Circuit)
        {
            var ioCount = GetCircuitIOCount(Circuit);

            int height = (int)(0.03 * Size.Height);
            int width = (int)(0.09 * Size.Width);

            return new Size(width, height * ioCount);
        }

        private Size GetCircuitConnectorSize()
        {
            int height = (int)(0.03 * Size.Height);
            return new Size(height, height);
        }

        private int GetFontSize()
        {
            var minFontSize = 8;
            var maxFontSize = 30;

            var fontSize = TableLayoutPanelComponents.Width / 22;
            if (fontSize < minFontSize)
            {
                fontSize = minFontSize;
            }
            if (fontSize > maxFontSize)
            {
                fontSize = maxFontSize;
            }

            return fontSize;
        }

        private Font GetLabelMenuFont()
        {
            var labelFont = new Font("Microsoft JhengHei", GetFontSize(), FontStyle.Bold);
            return labelFont;
        }

        private Font GetCircuitLabelFont()
        {
            var labelFont = GetLabelMenuFont();
            var labelCircuit = new Font(labelFont.FontFamily, labelFont.Size - 4, FontStyle.Bold);

            return labelCircuit;
        }

        private Font GetOnOffComponentLabelFont()
        {
            var labelFont = new Font("Microsoft JhengHei", GetFontSize() - 4, FontStyle.Bold);
            return labelFont;
        }

        private void SetFont()
        {
            var labelMenuFont = GetLabelMenuFont();

            LabelComponents.Font = labelMenuFont;
            LabelCircuits.Font = labelMenuFont;

            DataGridViewAllCircuits.Font = labelMenuFont;
            DataGridViewAllCircuits.DefaultCellStyle.Font = labelMenuFont;

            foreach (var control in PictureBoxBoard.Controls)
            {
                if (control is TextBox onOffComponentLabel)
                {
                    onOffComponentLabel.Font = GetOnOffComponentLabelFont();
                }
            }
        }
    }
}
