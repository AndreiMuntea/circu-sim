using circu_sim.CircuitLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace circu_sim.GraphicComponents
{
    public class DoubleBufferedLabel : Label
    {
        public DoubleBufferedLabel()
        {
            DoubleBuffered = true;
        }
    }

    public class CircuitDisplayComponent : IDisposable
    {
        public BaseCircuit LogicCircuit { get; private set; }
        public int IOCount { get; private set; }

        public DoubleBufferedLabel CircuitLabel { get; private set; }

        public Image ConnectorImageInput { get; private set; }
        public Image ConnectorImageOutput { get; private set; }

        public Image ConnectorImage { get; private set; }
        public Size ConnectorSize { get; set; }

        public List<OnOffComponent> ConnectorInputs { get; private set; }
        public List<OnOffComponent> ConnectorOutputs { get; private set; }

        public CircuitDisplayComponent(BaseCircuit LogicCircuit, string CircuitName, Image Connector, Size LabelSize, Size ConnectorSize, Font LabelFont)
        {
            this.LogicCircuit = LogicCircuit;

            //
            // from: "o--->" to "<---o"
            //
            this.ConnectorImage = Connector;
            this.ConnectorSize = ConnectorSize;

            ConnectorImageInput = new Bitmap(Connector, ConnectorSize);
            ConnectorImageInput.RotateFlip(RotateFlipType.RotateNoneFlipX);

            ConnectorImageOutput = new Bitmap(Connector, ConnectorSize);

            //
            // Inputs are on left and outputs on right.
            // Compute the height based on whichever is bigger.
            //
            IOCount = Math.Max(this.LogicCircuit.Inputs.Count, this.LogicCircuit.Outputs.Count);
            IOCount = Math.Max(1, IOCount);

            CircuitLabel = new DoubleBufferedLabel
            {
                BackColor = ColorTranslator.FromHtml("#4D4D4D"),
                Dock = DockStyle.None,

                Font = LabelFont,
                Text = CircuitName,
                TextAlign = ContentAlignment.MiddleCenter,

                Size = LabelSize,

                Anchor = AnchorStyles.Top
            };

            //
            // Create inputs. Note that this are not linked to the CircuitLabel itself
            // until SetLocation() is called.
            //
            ConnectorInputs = new();
            foreach (var input in this.LogicCircuit.Inputs)
            {
                var connector = new OnOffComponent(input,
                                                   ConnectorImageInput,
                                                   ConnectorImageInput,
                                                   new Point(0, 0),
                                                   ConnectorImageInput.Size);
                ConnectorInputs.Add(connector);
            }

            //
            // Create outputs. Note that this are not linked to the CircuitLabel itself
            // until SetLocation() is called.
            //
            ConnectorOutputs = new();
            foreach (var output in this.LogicCircuit.Outputs)
            {
                var connector = new OnOffComponent(output,
                                                   ConnectorImageOutput,
                                                   ConnectorImageOutput,
                                                   new Point(0, 0),
                                                   ConnectorImageOutput.Size);
                ConnectorOutputs.Add(connector);
            }
        }

        public void Dispose()
        {
            foreach (var connector in ConnectorInputs)
            {
                connector.Dispose();
            }
            ConnectorInputs.Clear();

            foreach (var connector in ConnectorOutputs)
            {
                connector.Dispose();
            }
            ConnectorOutputs.Clear();

            CircuitLabel.Dispose();

            ConnectorImageInput.Dispose();
            ConnectorImageOutput.Dispose();

            LogicCircuit.Dispose();
            GC.SuppressFinalize(this);
        }

        public void SetLocation(Point Location)
        {
            CircuitLabel.Location = Location;

            //
            // The connectors will be distributed at an equal distance
            //
            int distance = CircuitLabel.Height / IOCount;

            //
            // Now set the location of input.
            // Top left corner of circuit label - width of connector image
            //
            Point location = CircuitLabel.Location;
            location.X -= ConnectorSize.Width;

            foreach (var connector in ConnectorInputs)
            {
                connector.Location = location;
                location.Y += distance;
            }

            //
            // Now set the location of output.
            // Top left corner of circuit label + width of circuit to get to top right corner
            //
            location = CircuitLabel.Location;
            location.X += CircuitLabel.Width;

            foreach (var connector in ConnectorOutputs)
            {
                connector.Location = location;
                location.Y += distance;
            }
        }

        public void BringToFront()
        {
            foreach (var connector in ConnectorInputs)
            {
                connector.BringToFront();
            }
            foreach (var connector in ConnectorOutputs)
            {
                connector.BringToFront();
            }
            CircuitLabel.BringToFront();
        }

        public void Resize(Size NewLabelSize, Size NewConnectorSize, Font NewLabelFont)
        {
            CircuitLabel.Size = NewLabelSize;
            CircuitLabel.Font = NewLabelFont;
            ConnectorSize = NewConnectorSize;

            ConnectorImageInput = new Bitmap(ConnectorImage, ConnectorSize);
            ConnectorImageInput.RotateFlip(RotateFlipType.RotateNoneFlipX);
            foreach (var connector in ConnectorInputs)
            {
                connector.Size = NewConnectorSize;
                connector.Image = ConnectorImageInput;
            }

            ConnectorImageOutput = new Bitmap(ConnectorImage, ConnectorSize);
            foreach (var connector in ConnectorOutputs)
            {
                connector.Size = NewConnectorSize;
                connector.Image = ConnectorImageOutput;
            }

            SetLocation(CircuitLabel.Location);
        }
    }
}
