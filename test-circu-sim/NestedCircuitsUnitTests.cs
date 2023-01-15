using circu_sim.CircuitLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CircuitSimulatorTests
{
    [TestClass]
    public class NestedCircuitTest
    {
        [TestMethod]
        public void NestedCircuitDefaultConstructorTest()
        {
            string circuitId = Guid.NewGuid().ToString();
            NestedCircuit circuit = new(circuitId);

            Assert.AreEqual(circuit.Circuits.Count, 0);
            Assert.AreEqual(circuit.Inputs.Count, 0);
            Assert.AreEqual(circuit.Outputs.Count, 0);
            Assert.AreEqual(circuit.Links.Count, 0);
            Assert.AreEqual(circuit.IsElementary, false);

            circuit.Dispose();
        }

        [TestMethod]
        public void NestedCircuitAddRemoveNodesTest()
        {
            string circuitId = Guid.NewGuid().ToString();
            NestedCircuit circuit = new(circuitId);

            CircuitNode i0 = circuit.InsertNode(CircuitNodeType.Input, "Input_0");
            Assert.AreEqual(circuit.Inputs.Count, 1);
            Assert.AreEqual(circuit.Outputs.Count, 0);

            Assert.AreEqual(i0.Identifier, "Input_0");
            Assert.AreEqual(i0.CircuitIdentifier, circuitId);


            CircuitNode i1 = circuit.InsertNode(CircuitNodeType.Input, "Input_1");
            Assert.AreEqual(circuit.Inputs.Count, 2);
            Assert.AreEqual(circuit.Outputs.Count, 0);

            Assert.AreEqual(i1.Identifier, "Input_1");
            Assert.AreEqual(i1.CircuitIdentifier, circuitId);


            CircuitNode o0 = circuit.InsertNode(CircuitNodeType.Output, "Output_0");
            Assert.AreEqual(circuit.Inputs.Count, 2);
            Assert.AreEqual(circuit.Outputs.Count, 1);

            Assert.AreEqual(o0.Identifier, "Output_0");
            Assert.AreEqual(o0.CircuitIdentifier, circuitId);

            // We should find all nodes.
            Assert.IsNotNull(circuit.SearchNode(i0.CircuitIdentifier, i0.Identifier));
            Assert.IsNotNull(circuit.SearchNode(i1.CircuitIdentifier, i1.Identifier));
            Assert.IsNotNull(circuit.SearchNode(o0.CircuitIdentifier, o0.Identifier));

            // Delete i0.
            circuit.DeleteNode(i0);
            Assert.AreEqual(circuit.Inputs.Count, 1);
            Assert.AreEqual(circuit.Outputs.Count, 1);

            // We shouldn't find i0
            Assert.IsNull(circuit.SearchNode(i0.CircuitIdentifier, i0.Identifier));
            Assert.IsNotNull(circuit.SearchNode(i1.CircuitIdentifier, i1.Identifier));
            Assert.IsNotNull(circuit.SearchNode(o0.CircuitIdentifier, o0.Identifier));

            // Delete i1
            circuit.DeleteNode(i1);
            Assert.AreEqual(circuit.Inputs.Count, 0);
            Assert.AreEqual(circuit.Outputs.Count, 1);

            // Find only o0
            Assert.IsNull(circuit.SearchNode(i0.CircuitIdentifier, i0.Identifier));
            Assert.IsNull(circuit.SearchNode(i1.CircuitIdentifier, i1.Identifier));
            Assert.IsNotNull(circuit.SearchNode(o0.CircuitIdentifier, o0.Identifier));

            // Delete o0
            circuit.DeleteNode(o0);
            Assert.AreEqual(circuit.Inputs.Count, 0);
            Assert.AreEqual(circuit.Outputs.Count, 0);

            // No nodes left
            // We shouldn't find i0
            Assert.IsNull(circuit.SearchNode(i0.CircuitIdentifier, i0.Identifier));
            Assert.IsNull(circuit.SearchNode(i1.CircuitIdentifier, i1.Identifier));
            Assert.IsNull(circuit.SearchNode(o0.CircuitIdentifier, o0.Identifier));

            circuit.Dispose();
        }

        [TestMethod]
        public void NestedCircuitAddRemoveCircuitsTest()
        {
            string circuitId = Guid.NewGuid().ToString();
            NestedCircuit circuit = new(circuitId);

            Assert.AreEqual(circuit.Circuits.Count, 0);

            AndCircuit andCircuit = new("AND_CIRCUIT");
            circuit.InsertCircuit(andCircuit);
            Assert.AreEqual(circuit.Circuits.Count, 1);

            circuit.InsertCircuit(andCircuit);
            Assert.AreEqual(circuit.Circuits.Count, 1);

            OrCircuit orCircuit = new("OR_CIRCUIT");
            circuit.InsertCircuit(orCircuit);
            Assert.AreEqual(circuit.Circuits.Count, 2);

            circuit.RemoveCircuit(andCircuit);
            Assert.AreEqual(circuit.Circuits.Count, 1);

            circuit.RemoveCircuit(orCircuit);
            Assert.AreEqual(circuit.Circuits.Count, 0);

            circuit.Dispose();
            andCircuit.Dispose();
            orCircuit.Dispose();
        }

        [TestMethod]
        public void NestedCircuitLinkTest()
        {
            string circuitId = Guid.NewGuid().ToString();
            NestedCircuit circuit = new(circuitId);

            CircuitNode i1 = circuit.InsertNode(CircuitNodeType.Input, Guid.NewGuid().ToString());
            CircuitNode i2 = circuit.InsertNode(CircuitNodeType.Input, Guid.NewGuid().ToString());
            Assert.AreEqual(circuit.Inputs.Count, 2);

            CircuitNode o1 = circuit.InsertNode(CircuitNodeType.Output, Guid.NewGuid().ToString());
            Assert.AreEqual(circuit.Outputs.Count, 1);

            AndCircuit andCircuit = new("AND_CIRCUIT");
            circuit.InsertCircuit(andCircuit);
            Assert.AreEqual(circuit.Circuits.Count, 1);

            circuit.Link(i1, andCircuit.Inputs[0]);
            circuit.Link(i2, andCircuit.Inputs[1]);
            circuit.Link(andCircuit.Outputs[0], o1);
            Assert.AreEqual(circuit.Links.Count, 3);

            i1.SignalHandler(this, new Signal(true, circuitId));
            Assert.IsFalse(o1.CurrentState);
            i2.SignalHandler(this, new Signal(true, circuitId));
            Assert.IsTrue(o1.CurrentState);

            // This should remove all links
            circuit.RemoveCircuit(andCircuit);
            Assert.AreEqual(circuit.Links.Count, 0);

            // Output should be unsignaled now
            Assert.IsFalse(o1.CurrentState);

            // Inputs should be signaled
            Assert.IsTrue(i1.CurrentState);
            Assert.IsTrue(i2.CurrentState);

            circuit.Dispose();
            andCircuit.Dispose();
        }

        [TestMethod]
        public void NestedCircuitXML()
        {
            string circuitId = Guid.NewGuid().ToString();
            NestedCircuit circuit = new(circuitId);

            CircuitNode i1 = circuit.InsertNode(CircuitNodeType.Input, Guid.NewGuid().ToString());
            CircuitNode i2 = circuit.InsertNode(CircuitNodeType.Input, Guid.NewGuid().ToString());
            Assert.AreEqual(circuit.Inputs.Count, 2);

            CircuitNode o1 = circuit.InsertNode(CircuitNodeType.Output, Guid.NewGuid().ToString());
            Assert.AreEqual(circuit.Outputs.Count, 1);

            AndCircuit andCircuit = new("AND_CIRCUIT");
            circuit.InsertCircuit(andCircuit);
            Assert.AreEqual(circuit.Circuits.Count, 1);

            circuit.Link(i1, andCircuit.Inputs[0]);
            circuit.Link(i2, andCircuit.Inputs[1]);
            circuit.Link(andCircuit.Outputs[0], o1);
            Assert.AreEqual(circuit.Links.Count, 3);

            // Create a mirror xml circuit
            NestedCircuit xmlCircuit = CircuitCreator.FromXml(circuit.ToXML()) as NestedCircuit;
            Assert.IsNotNull(xmlCircuit);
            Assert.AreEqual(xmlCircuit.ToXML().ToString(), circuit.ToXML().ToString());

            xmlCircuit.Inputs[0].SignalHandler(this, new Signal(true, circuitId));
            Assert.IsFalse(xmlCircuit.Outputs[0].CurrentState);
            xmlCircuit.Inputs[1].SignalHandler(this, new Signal(true, circuitId));
            Assert.IsTrue(xmlCircuit.Outputs[0].CurrentState);

            // This should remove all links
            xmlCircuit.RemoveCircuit(xmlCircuit.Circuits[0]);
            Assert.AreEqual(xmlCircuit.Links.Count, 0);

            // Output should be unsignaled now
            Assert.IsFalse(xmlCircuit.Outputs[0].CurrentState);

            // Inputs should be signaled
            Assert.IsTrue(xmlCircuit.Inputs[0].CurrentState);
            Assert.IsTrue(xmlCircuit.Inputs[1].CurrentState);

            circuit.Dispose();
            andCircuit.Dispose();
            xmlCircuit.Dispose();
        }

    }
}