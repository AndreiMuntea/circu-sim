using circu_sim.CircuitLogic;
using circu_sim.GraphicComponents;
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

            var connectorComponent = CreateConnectorComponent(switchComponent);
            PictureBoxBoard.Controls.Add(connectorComponent);

            SwitchToConnector.Add(switchComponent, connectorComponent);
        }

        private SwitchComponent CreateSwitchComponent()
        {
            var inputPosition = GetNodePosition(CurrentCircuit.Inputs, CircuitNodeType.Input);
            var inputIdentifier = GetNodeIdentifier(CircuitNodeType.Input, inputPosition);
            var inputNode = CurrentCircuit.InsertNode(CircuitNodeType.Input, inputIdentifier);
            var inputLocation = GetSwitchLocation(inputPosition);

            SwitchComponent switchComponent = new(
                inputNode,
                Properties.Resources.Switch_On,
                Properties.Resources.Switch_Off,
                inputLocation,
                GetOnOffComponenSize(),
                inputPosition
            );

            switchComponent.MouseClick += PictureBoxSwitch_MouseClick;
            switchComponent.MouseDoubleClick += PictureBoxSwitch_MouseClick;
            switchComponent.MouseMove += Line_MouseMove;

            Label switchLabel = CreateInputOutputLabel(inputIdentifier, inputLocation);
            PictureBoxBoard.Controls.Add(switchLabel);

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
        }

        private BulbComponent CreateBulbComponent()
        {
            var outputPosition = GetNodePosition(CurrentCircuit.Outputs, CircuitNodeType.Output);
            var outputIdentifier = GetNodeIdentifier(CircuitNodeType.Output, outputPosition);
            var outputNode = CurrentCircuit.InsertNode(CircuitNodeType.Output, outputIdentifier);
            var outputLocation = GetBulbLocation(outputPosition);

            BulbComponent bulbComponent = new(
                outputNode,
                Properties.Resources.Bulb_On,
                Properties.Resources.Bulb_Off,
                outputLocation,
                GetOnOffComponenSize(),
                outputPosition
            );

            bulbComponent.MouseClick += PictureBoxBulb_MouseClick;
            bulbComponent.MouseMove += Line_MouseMove;

            Label bulbLabel = CreateInputOutputLabel(outputIdentifier, outputLocation);
            PictureBoxBoard.Controls.Add(bulbLabel);

            return bulbComponent;
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
