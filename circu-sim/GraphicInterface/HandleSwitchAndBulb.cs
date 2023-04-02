using circu_sim.CircuitLogic;
using circu_sim.GraphicComponents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace circu_sim
{
    public partial class CircuitSimulator
    {
        private const int MaxComponents = 10;

        private NestedCircuit CurrentCircuit = new(GenerateGuid());
        private readonly Dictionary<SwitchComponent, PictureBox> SwitchToConnector = new();

        private void PictureBoxSwitchIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (CurrentCircuit.Inputs.Count >= MaxComponents)
            {
                return;
            }

            var switchComponent = CreateSwitchComponent();
            PictureBoxBoard.Controls.Add(switchComponent);

            var switchComponentLabel = CreateOnOffComponentLabel(switchComponent);
            PictureBoxBoard.Controls.Add(switchComponentLabel);
            PictureBoxBoard.Controls.SetChildIndex(switchComponentLabel, 0);

            var connectorComponent = CreateConnectorComponent(switchComponent);
            PictureBoxBoard.Controls.Add(connectorComponent);

            SwitchToConnector.Add(switchComponent, connectorComponent);
        }

        private SwitchComponent CreateSwitchComponent()
        {
            var inputPosition = GetNodePosition(CurrentCircuit.Inputs, CircuitNodeType.Input);
            var inputIdentifier = GetNodeIdentifier(CircuitNodeType.Input, inputPosition);
            var inputNode = CurrentCircuit.InsertNode(CircuitNodeType.Input, inputIdentifier);

            SwitchComponent switchComponent = new(
                inputNode,
                Properties.Resources.Switch_On,
                Properties.Resources.Switch_Off,
                GetSwitchLocation(inputPosition),
                GetOnOffComponenSize(),
                inputPosition
            );

            switchComponent.MouseClick += PictureBoxSwitch_MouseClick;
            switchComponent.MouseDoubleClick += PictureBoxSwitch_MouseClick;
            switchComponent.MouseMove += Line_MouseMove;

            return switchComponent;
        }

        private PictureBox CreateConnectorComponent(SwitchComponent SwitchComponent)
        {
            PictureBox connectorComponent = new()
            {
                Image = Properties.Resources.Connector,
                Location = GetConnectorLocation(SwitchComponent),
                Size = GetConnectorSize(SwitchComponent),
                SizeMode = PictureBoxSizeMode.Zoom,
            };

            connectorComponent.MouseClick += PictureBoxConnector_MouseClick;
            connectorComponent.MouseMove += Line_MouseMove;

            return connectorComponent;
        }

        private void PictureBoxBulbIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (CurrentCircuit.Outputs.Count >= MaxComponents)
            {
                return;
            }

            var bulbComponent = CreateBulbComponent();
            PictureBoxBoard.Controls.Add(bulbComponent);

            var bulbComponentLabel = CreateOnOffComponentLabel(bulbComponent);
            PictureBoxBoard.Controls.Add(bulbComponentLabel);
            PictureBoxBoard.Controls.SetChildIndex(bulbComponentLabel, 0);
        }

        private BulbComponent CreateBulbComponent()
        {
            var outputPosition = GetNodePosition(CurrentCircuit.Outputs, CircuitNodeType.Output);
            var outputIdentifier = GetNodeIdentifier(CircuitNodeType.Output, outputPosition);
            var outputNode = CurrentCircuit.InsertNode(CircuitNodeType.Output, outputIdentifier);

            BulbComponent bulbComponent = new(
                outputNode,
                Properties.Resources.Bulb_On,
                Properties.Resources.Bulb_Off,
                GetBulbLocation(outputPosition),
                GetOnOffComponenSize(),
                outputPosition
            );

            bulbComponent.MouseClick += PictureBoxBulb_MouseClick;
            bulbComponent.MouseMove += Line_MouseMove;

            return bulbComponent;
        }

        private TextBox CreateOnOffComponentLabel(OnOffComponent OnOffComponent)
        {
            TextBox onOffComponentLabel = new()
            {
                Text = OnOffComponent.Node.Identifier,
                Font = GetInputOutputLabelFont(),
                ForeColor = ColorTranslator.FromHtml("#4D4D4D"),
                BackColor = ColorTranslator.FromHtml("#D8D8D3"),
                BorderStyle= BorderStyle.None,
                Location = new Point(OnOffComponent.Location.X, OnOffComponent.Location.Y - 25)
            };
            onOffComponentLabel.BringToFront();

            onOffComponentLabel.Size = TextRenderer.MeasureText(onOffComponentLabel.Text, onOffComponentLabel.Font);
            onOffComponentLabel.TextChanged += OnOffComponentLabel_TextChanged;

            return onOffComponentLabel;
        }

        private void OnOffComponentLabel_TextChanged(object? sender, EventArgs e)
        {
            if (sender is not TextBox onOffComponentLabel)
            {
                return;
            }

            onOffComponentLabel.Size = TextRenderer.MeasureText(onOffComponentLabel.Text, onOffComponentLabel.Font);
        }

        private void PictureBoxSwitch_MouseClick(object? sender, MouseEventArgs e)
        {
            if (sender is not SwitchComponent switchComponent)
            {
                return;
            }

            if (IsDeleteInProgress)
            {
                DeleteSwitch(switchComponent);
                ClearDeleteInProgress();

                return;
            }

            CurrentCircuit.ToggleNode(switchComponent.Node);

            ClearCurrentLine();
        }
    }
}
