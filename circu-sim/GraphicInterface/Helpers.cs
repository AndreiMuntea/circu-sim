using circu_sim.CircuitLogic;
using circu_sim.GraphicComponents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace circu_sim
{
    public partial class CircuitSimulator
    {
        private void CreateNewCircuit()
        {
            CurrentCircuit.Dispose();
            CurrentCircuit = new(GenerateGuid());
        }

        private static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        private SwitchComponent GetSwitchByConnector(PictureBox Connector)
        {
            return SwitchToConnector.First(x => x.Value == Connector).Key;
        }

        private PictureBox GetConnectorBySwitch(SwitchComponent Switch)
        {
            return SwitchToConnector.First(x => x.Key == Switch).Value;
        }

        private TextBox GetLabelByOnOffComponent(OnOffComponent OnOffComponent)
        {
            return OnOffComponentToLabel.First(x => x.Key == OnOffComponent).Value;
        }

        private static string GetNodeIdentifier(CircuitNodeType NodeType, int NodeIndex)
        {
            return NodeType.ToString() + NodeIndex;
        }

        private static int GetNodePosition(List<CircuitNode> Nodes, CircuitNodeType NodeType)
        {
            for (var position = 0; position < Nodes.Count; position++)
            {
                var identifier = GetNodeIdentifier(NodeType, position);
                if (!Nodes.Any(x => x.Identifier == identifier))
                {
                    return position;
                }
            }

            return Nodes.Count;
        }

        private Line? GetClickedLine(Point MousePosition)
        {
            return AllLines.FirstOrDefault(x => x.IsLineNearPoint(MousePosition));
        }

        private List<Line> GetLinksToSwitch(SwitchComponent SwitchComponent)
        {
            return AllLines.FindAll(x => x.StartingComponent == SwitchComponent);
        }

        private List<Line> GetLinksToBulb(BulbComponent BulbComponent)
        {
            return AllLines.FindAll(x => x.StoppingComponent == BulbComponent);
        }

        private List<Line> GetLinksToCircuitNode(OnOffComponent CircuitNode)
        {
            return AllLines.FindAll(x => x.StartingComponent == CircuitNode || x.StoppingComponent == CircuitNode);
        }

        private Line? GetExistingLine(OnOffComponent StartingComponent, OnOffComponent StoppingComponent)
        {
            return AllLines.FirstOrDefault(x => x.StartingComponent == StartingComponent && x.StoppingComponent == StoppingComponent);
        }

        private CircuitDisplayComponent? GetDisplayCircuit(Label Label)
        {
            return DisplayCircuits.FirstOrDefault(x => x.CircuitLabel.Equals(Label));
        }
    }
}
