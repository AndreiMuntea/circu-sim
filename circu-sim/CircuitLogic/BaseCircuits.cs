using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace circu_sim.CircuitLogic
{
    //
    // This will be the signal that will be sent by a source
    // and received by a consumer.
    //
    public class Signal : EventArgs
    {
        public bool SignaledState { get; set; }
        public string SignalSourceId { get; set; }
        public Signal(bool State, string SignalSourceId)
        {
            SignaledState = State;
            this.SignalSourceId = SignalSourceId;
        }
    }

    public enum CircuitNodeType
    {
        Input,
        Output
    };

    public class CircuitNode
    {
        public string Identifier { get; private set; }
        public string CircuitIdentifier { get; private set; }
        public Dictionary<string, bool> Sources { get; private set; }
        public EventHandler<Signal>? Subscribers { get; set; }
        public bool CurrentState { get; private set; }

        public CircuitNode(string Identifier, string CircuitIdentifier)
        {
            this.Identifier = Identifier;
            this.CircuitIdentifier = CircuitIdentifier;
            Sources = new();
            Subscribers = null;
            CurrentState = false;
        }

        public void AddSource(string SourceId, bool InitialState)
        {
            // Don't connect twice to the same source.
            if (!Sources.ContainsKey(SourceId))
            {
                // Preserve the source state.
                Sources[SourceId] = InitialState;
            }
            // We need to reevaluate the current state counting the new source.
            Evaluate();
        }
        public void RemoveSource(string SourceId)
        {
            // We can do this only if we are connected to the source
            Sources.Remove(SourceId);
            // Reevaluate based on the removed connection.
            Evaluate();
        }

        public void SignalHandler(object? Sender, Signal ReceivedSignal)
        {
            // We are not connected to this source.
            if (!Sources.ContainsKey(ReceivedSignal.SignalSourceId))
            {
                return;
            }

            // Update the state of the node.
            Sources[ReceivedSignal.SignalSourceId] = ReceivedSignal.SignaledState;

            // Reevaluate the current state and notify subscribers if needed.
            Evaluate();
        }

        public void Evaluate()
        {
            // Compute the new state. True if at least one source is signaled.
            bool newState = false;
            foreach (KeyValuePair<string, bool> source in Sources)
            {
                newState = newState || source.Value;
            }

            // We only notify if there is a change.
            bool changeRequired = CurrentState != newState;

            // Save the new state.
            CurrentState = newState;

            // Notify if the node state was changed.
            if (changeRequired)
            {
                Signal? signal = new(CurrentState, Identifier);
                Subscribers?.Invoke(this, signal);
            }
        }
    }

    public abstract class BaseCircuit : IDisposable
    {
        public List<CircuitNode> Inputs;
        public List<CircuitNode> Outputs;
        public string Identifier;
        public bool IsElementary;

        public BaseCircuit(string Identifier, bool isElementary)
        {
            Inputs = new();
            Outputs = new();
            this.Identifier = Identifier;
            IsElementary = isElementary;
        }

        public virtual void Dispose()
        {
            while (Outputs.Count > 0)
            {
                RemoveNode(ref Outputs, Outputs[0]);
            }
            while (Inputs.Count > 0)
            {
                RemoveNode(ref Inputs, Inputs[0]);
            }
            GC.SuppressFinalize(this);
        }

        protected static CircuitNode? FindNode(ref List<CircuitNode> NodesList, string Identifier)
        {
            return NodesList.Find(x => x.Identifier.Equals(Identifier, StringComparison.Ordinal));
        }
        public abstract void Evaluate();

        protected void AddNode(ref List<CircuitNode> NodesList, CircuitNode Node)
        {
            // A node can be added only once.
            CircuitNode? node = FindNode(ref NodesList, Node.Identifier);
            if (node == null)
            {
                // Add the new node to the list.
                NodesList.Add(Node);

                // Register as source to the node.
                Node.AddSource(Identifier, false);

                // Subscribe to the state change notification.
                Node.Subscribers += SignalHandler;
            }
            // Evaluate based on changes.
            Evaluate();
        }
        protected void RemoveNode(ref List<CircuitNode> NodesList, CircuitNode Node)
        {
            // We can only remove the node if in list
            CircuitNode? node = FindNode(ref NodesList, Node.Identifier);
            if (node != null)
            {
                // We are no longer interested in the node state.
                Node.Subscribers -= SignalHandler;

                // Remove us as a source.
                Node.RemoveSource(Identifier);

                // Erase the node from list.
                NodesList.Remove(Node);
            }
            // Evaluate based on changes.
            Evaluate();
        }
        public void ToggleNode(CircuitNode Node)
        {
            if (Node.Sources.ContainsKey(Identifier))
            {
                bool newState = !Node.Sources[Identifier];
                Node.SignalHandler(this, new Signal(newState, Identifier));
            }
        }

        protected void SignalHandler(object? Sender, Signal ReceivedSignal)
        {
            // A change occured. Reevaluate the circuit.
            Evaluate();
        }

        public virtual XElement ToXML()
        {
            XElement? Root = new("Root");

            string? assemblyType = GetType().AssemblyQualifiedName;
            assemblyType = assemblyType?.Split(',').First();

            XElement? xmlIdentifier = new("Identifier", Identifier);
            Root.Add(xmlIdentifier);

            XElement? xmlInputs = new("Inputs", Inputs.Count);
            Root.Add(xmlInputs);

            XElement? xmlOutputs = new("Outputs", Outputs.Count);
            Root.Add(xmlOutputs);

            XElement? xmlType = new("AssemblyType", assemblyType);
            Root.Add(xmlType);

            XElement? xmlIsElementary = new("IsElementary", IsElementary);
            Root.Add(xmlIsElementary);

            return Root;
        }

        public BaseCircuit? Clone()
        {
            // Serialize the circuit
            var serializedCircuit = ToXML().ToString();

            // Create a new GUID as identifier to replace the old circuit GUID.
            serializedCircuit = serializedCircuit.Replace(Identifier, Guid.NewGuid().ToString());

            // Create a new circuit
            return CircuitCreator.FromXml(XElement.Parse(serializedCircuit));
        }
    }

    public class NotCircuit : BaseCircuit
    {
        public NotCircuit(string Identifier) : base(Identifier, true)
        {
            CircuitNode? input0 = new("NOT_Input0", this.Identifier);
            AddNode(ref Inputs, input0);

            CircuitNode? output0 = new("NOT_Output0", this.Identifier);
            AddNode(ref Outputs, output0);
        }
        public override void Evaluate()
        {
            // Sanity check
            if (Inputs.Count != 1 || Outputs.Count != 1)
            {
                return;
            }

            // Output state for NOT logic gate is !(Inputs[0])
            bool state = !Inputs[0].CurrentState;
            Signal? signal = new(state, Identifier);

            // Notify the output node with the change.
            Outputs[0].SignalHandler(this, signal);
        }
    }

    public class AndCircuit : BaseCircuit
    {
        public AndCircuit(string Identifier) : base(Identifier, true)
        {
            CircuitNode? input0 = new("AND_Input0", this.Identifier);
            AddNode(ref Inputs, input0);

            CircuitNode? input1 = new("AND_Input1", this.Identifier);
            AddNode(ref Inputs, input1);

            CircuitNode? output0 = new("AND_Output0", this.Identifier);
            AddNode(ref Outputs, output0);
        }
        public override void Evaluate()
        {
            // Sanity check
            if (Inputs.Count != 2 || Outputs.Count != 1)
            {
                return;
            }

            // Output state for AND logic gate is (Inputs[0] && Inputs[1])
            bool state = Inputs[0].CurrentState && Inputs[1].CurrentState;
            Signal? signal = new(state, Identifier);

            // Notify the output node with the change.
            Outputs[0].SignalHandler(this, signal);
        }
    }

    public class OrCircuit : BaseCircuit
    {
        public OrCircuit(string Identifier) : base(Identifier, true)
        {
            CircuitNode? input0 = new("OR_Input0", this.Identifier);
            AddNode(ref Inputs, input0);

            CircuitNode? input1 = new("OR_Input1", this.Identifier);
            AddNode(ref Inputs, input1);

            CircuitNode? output0 = new("OR_Output0", this.Identifier);
            AddNode(ref Outputs, output0);
        }
        public override void Evaluate()
        {
            // Sanity check
            if (Inputs.Count != 2 || Outputs.Count != 1)
            {
                return;
            }

            // Output state for OR logic gate is (Inputs[0] || Inputs[1])
            bool state = Inputs[0].CurrentState || Inputs[1].CurrentState;
            Signal? signal = new(state, Identifier);

            // Notify the output node with the change.
            Outputs[0].SignalHandler(this, signal);
        }
    }
}
