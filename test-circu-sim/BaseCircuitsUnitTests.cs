using circu_sim.CircuitLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace CircuitSimulatorTests
{
    [TestClass]
    public class BaseCircuitTest
    {
        [TestMethod]
        public void NotCircuitConstructorTest()
        {
            string circuitId = "My Circuit";
            NotCircuit circuit = new(circuitId);

            // Check Basic Information
            Assert.AreEqual(circuit.Identifier, circuitId);
            Assert.AreEqual(circuit.IsElementary, true);

            // Check Inputs
            Assert.AreEqual(circuit.Inputs.Count, 1);

            {
                Assert.AreEqual(circuit.Inputs[0].Identifier, "NOT_Input0");
                Assert.AreEqual(circuit.Inputs[0].CircuitIdentifier, circuitId);
                Assert.AreEqual(circuit.Inputs[0].CurrentState, false);
                // Each node in circuit should have circuit as source
                Assert.AreEqual(circuit.Inputs[0].Sources.Count, 1);
                Assert.AreEqual(circuit.Inputs[0].Sources[circuitId], false);
            }

            // Check Outputs
            Assert.AreEqual(circuit.Outputs.Count, 1);
            {
                Assert.AreEqual(circuit.Outputs[0].Identifier, "NOT_Output0");
                Assert.AreEqual(circuit.Outputs[0].CircuitIdentifier, circuitId);
                Assert.AreEqual(circuit.Outputs[0].CurrentState, true);
                // Each node in circuit should have circuit as source
                Assert.AreEqual(circuit.Outputs[0].Sources.Count, 1);
                Assert.AreEqual(circuit.Outputs[0].Sources[circuitId], true);
            }

            // Dispose
            circuit.Dispose();
        }

        [TestMethod]
        public void NotCircuitXMLSerializationTest()
        {
            string circuitId = "My Circuit";
            NotCircuit circuit = new(circuitId);

            // Check XML
            //
            // <Root>
            //  <Identifier>_circuit_id</Identifier>
            //  <Inputs>1</Inputs>
            //  <Outputs>1</Outputs>
            //  <AssemblyType>circu_sim.CircuitLogic.NotCircuit</AssemblyType>
            //  <IsElementary>true</IsElementary>
            // </Root>
            //
            System.Xml.Linq.XElement xmlRootCircuit = circuit.ToXML();
            Assert.AreEqual(xmlRootCircuit.Name, "Root");

            System.Xml.Linq.XElement xmlIdentifier = xmlRootCircuit.Element("Identifier");
            Assert.IsNotNull(xmlIdentifier);
            Assert.AreEqual(xmlIdentifier.Value, circuitId);

            System.Xml.Linq.XElement xmlInputs = xmlRootCircuit.Element("Inputs");
            Assert.IsNotNull(xmlInputs);
            Assert.AreEqual(int.Parse(xmlInputs.Value), 1);

            System.Xml.Linq.XElement xmlOutputs = xmlRootCircuit.Element("Outputs");
            Assert.IsNotNull(xmlOutputs);
            Assert.AreEqual(int.Parse(xmlOutputs.Value), 1);

            System.Xml.Linq.XElement xmlAssemblyType = xmlRootCircuit.Element("AssemblyType");
            Assert.IsNotNull(xmlAssemblyType);
            Assert.AreEqual(xmlAssemblyType.Value, "circu_sim.CircuitLogic.NotCircuit");

            System.Xml.Linq.XElement xmlIsElementary = xmlRootCircuit.Element("IsElementary");
            Assert.IsNotNull(xmlIsElementary);
            Assert.AreEqual(bool.Parse(xmlIsElementary.Value), true);

            // Check that we can create a circuit from xml.
            Assembly assembly = Assembly.GetAssembly(typeof(BaseCircuit));
            Assert.IsNotNull(assembly);

            Type circuitType = assembly.GetType(xmlAssemblyType.Value);
            Assert.IsNotNull(circuitType);

            BaseCircuit circuitCreated = Activator.CreateInstance(circuitType, xmlIdentifier.Value) as BaseCircuit;
            Assert.IsNotNull(circuitCreated);

            Assert.AreEqual(circuitCreated.Identifier, circuit.Identifier);
            Assert.AreEqual(circuitCreated.ToXML().ToString(), circuit.ToXML().ToString());

            circuit.Dispose();
            circuitCreated.Dispose();
        }

        [TestMethod]
        public void NotCircuitSignal()
        {
            string circuitId = "My Circuit";
            NotCircuit circuit = new(circuitId);

            // not false = true
            circuit.Inputs[0].SignalHandler(null, new Signal(false, circuitId));
            Assert.AreEqual(circuit.Inputs[0].CurrentState, false);
            Assert.AreEqual(circuit.Outputs[0].CurrentState, true);

            // not true = false
            circuit.Inputs[0].SignalHandler(null, new Signal(true, circuitId));
            Assert.AreEqual(circuit.Inputs[0].CurrentState, true);
            Assert.AreEqual(circuit.Outputs[0].CurrentState, false);

            circuit.Dispose();
        }

        [TestMethod]
        public void OrCircuitConstructorTest()
        {
            string circuitId = "My Circuit";
            OrCircuit circuit = new(circuitId);

            // Check Basic Information
            Assert.AreEqual(circuit.Identifier, circuitId);
            Assert.AreEqual(circuit.IsElementary, true);

            // Check Inputs
            Assert.AreEqual(circuit.Inputs.Count, 2);

            {
                Assert.AreEqual(circuit.Inputs[0].Identifier, "OR_Input0");
                Assert.AreEqual(circuit.Inputs[0].CircuitIdentifier, circuitId);
                Assert.AreEqual(circuit.Inputs[0].CurrentState, false);
                // Each node in circuit should have circuit as source
                Assert.AreEqual(circuit.Inputs[0].Sources.Count, 1);
                Assert.AreEqual(circuit.Inputs[0].Sources[circuitId], false);

                Assert.AreEqual(circuit.Inputs[1].Identifier, "OR_Input1");
                Assert.AreEqual(circuit.Inputs[1].CircuitIdentifier, circuitId);
                Assert.AreEqual(circuit.Inputs[1].CurrentState, false);
                // Each node in circuit should have circuit as source
                Assert.AreEqual(circuit.Inputs[1].Sources.Count, 1);
                Assert.AreEqual(circuit.Inputs[1].Sources[circuitId], false);
            }

            // Check Outputs
            Assert.AreEqual(circuit.Outputs.Count, 1);
            {
                Assert.AreEqual(circuit.Outputs[0].Identifier, "OR_Output0");
                Assert.AreEqual(circuit.Outputs[0].CircuitIdentifier, circuitId);
                Assert.AreEqual(circuit.Outputs[0].CurrentState, false);
                // Each node in circuit should have circuit as source
                Assert.AreEqual(circuit.Outputs[0].Sources.Count, 1);
                Assert.AreEqual(circuit.Outputs[0].Sources[circuitId], false);
            }

            // Dispose
            circuit.Dispose();
        }

        [TestMethod]
        public void OrCircuitXMLSerializationTest()
        {
            string circuitId = "My Circuit";
            OrCircuit circuit = new(circuitId);

            // Check XML
            //
            // <Root>
            //  <Identifier>_circuit_id</Identifier>
            //  <Inputs>2</Inputs>
            //  <Outputs>1</Outputs>
            //  <AssemblyType>circu_sim.CircuitLogic.OrCircuit</AssemblyType>
            //  <IsElementary>true</IsElementary>
            // </Root>
            //
            System.Xml.Linq.XElement xmlRootCircuit = circuit.ToXML();
            Assert.AreEqual(xmlRootCircuit.Name, "Root");

            System.Xml.Linq.XElement xmlIdentifier = xmlRootCircuit.Element("Identifier");
            Assert.IsNotNull(xmlIdentifier);
            Assert.AreEqual(xmlIdentifier.Value, circuitId);

            System.Xml.Linq.XElement xmlInputs = xmlRootCircuit.Element("Inputs");
            Assert.IsNotNull(xmlInputs);
            Assert.AreEqual(int.Parse(xmlInputs.Value), 2);

            System.Xml.Linq.XElement xmlOutputs = xmlRootCircuit.Element("Outputs");
            Assert.IsNotNull(xmlOutputs);
            Assert.AreEqual(int.Parse(xmlOutputs.Value), 1);

            System.Xml.Linq.XElement xmlAssemblyType = xmlRootCircuit.Element("AssemblyType");
            Assert.IsNotNull(xmlAssemblyType);
            Assert.AreEqual(xmlAssemblyType.Value, "circu_sim.CircuitLogic.OrCircuit");

            System.Xml.Linq.XElement xmlIsElementary = xmlRootCircuit.Element("IsElementary");
            Assert.IsNotNull(xmlIsElementary);
            Assert.AreEqual(bool.Parse(xmlIsElementary.Value), true);

            // Check that we can create a circuit from xml.
            Assembly assembly = Assembly.GetAssembly(typeof(BaseCircuit));
            Assert.IsNotNull(assembly);

            Type circuitType = assembly.GetType(xmlAssemblyType.Value);
            Assert.IsNotNull(circuitType);

            BaseCircuit circuitCreated = Activator.CreateInstance(circuitType, xmlIdentifier.Value) as BaseCircuit;
            Assert.IsNotNull(circuitCreated);

            Assert.AreEqual(circuitCreated.Identifier, circuit.Identifier);
            Assert.AreEqual(circuitCreated.ToXML().ToString(), circuit.ToXML().ToString());

            circuit.Dispose();
            circuitCreated.Dispose();
        }

        [TestMethod]
        public void OrCircuitSignal()
        {
            string circuitId = "My Circuit";
            OrCircuit circuit = new(circuitId);

            // false || false = false
            circuit.Inputs[0].SignalHandler(null, new Signal(false, circuitId));
            circuit.Inputs[1].SignalHandler(null, new Signal(false, circuitId));
            Assert.AreEqual(circuit.Inputs[0].CurrentState, false);
            Assert.AreEqual(circuit.Inputs[1].CurrentState, false);
            Assert.AreEqual(circuit.Outputs[0].CurrentState, false);

            // false || true = true
            circuit.Inputs[0].SignalHandler(null, new Signal(false, circuitId));
            circuit.Inputs[1].SignalHandler(null, new Signal(true, circuitId));
            Assert.AreEqual(circuit.Inputs[0].CurrentState, false);
            Assert.AreEqual(circuit.Inputs[1].CurrentState, true);
            Assert.AreEqual(circuit.Outputs[0].CurrentState, true);

            // true || false = true
            circuit.Inputs[0].SignalHandler(null, new Signal(true, circuitId));
            circuit.Inputs[1].SignalHandler(null, new Signal(false, circuitId));
            Assert.AreEqual(circuit.Inputs[0].CurrentState, true);
            Assert.AreEqual(circuit.Inputs[1].CurrentState, false);
            Assert.AreEqual(circuit.Outputs[0].CurrentState, true);

            // true || true = true
            circuit.Inputs[0].SignalHandler(null, new Signal(true, circuitId));
            circuit.Inputs[1].SignalHandler(null, new Signal(true, circuitId));
            Assert.AreEqual(circuit.Inputs[0].CurrentState, true);
            Assert.AreEqual(circuit.Inputs[1].CurrentState, true);
            Assert.AreEqual(circuit.Outputs[0].CurrentState, true);

            circuit.Dispose();
        }

        [TestMethod]
        public void AndCircuitConstructorTest()
        {
            string circuitId = "My Circuit";
            AndCircuit circuit = new(circuitId);

            // Check Basic Information
            Assert.AreEqual(circuit.Identifier, circuitId);
            Assert.AreEqual(circuit.IsElementary, true);

            // Check Inputs
            Assert.AreEqual(circuit.Inputs.Count, 2);

            {
                Assert.AreEqual(circuit.Inputs[0].Identifier, "AND_Input0");
                Assert.AreEqual(circuit.Inputs[0].CircuitIdentifier, circuitId);
                Assert.AreEqual(circuit.Inputs[0].CurrentState, false);
                // Each node in circuit should have circuit as source
                Assert.AreEqual(circuit.Inputs[0].Sources.Count, 1);
                Assert.AreEqual(circuit.Inputs[0].Sources[circuitId], false);

                Assert.AreEqual(circuit.Inputs[1].Identifier, "AND_Input1");
                Assert.AreEqual(circuit.Inputs[1].CircuitIdentifier, circuitId);
                Assert.AreEqual(circuit.Inputs[1].CurrentState, false);
                // Each node in circuit should have circuit as source
                Assert.AreEqual(circuit.Inputs[1].Sources.Count, 1);
                Assert.AreEqual(circuit.Inputs[1].Sources[circuitId], false);
            }

            // Check Outputs
            Assert.AreEqual(circuit.Outputs.Count, 1);
            {
                Assert.AreEqual(circuit.Outputs[0].Identifier, "AND_Output0");
                Assert.AreEqual(circuit.Outputs[0].CircuitIdentifier, circuitId);
                Assert.AreEqual(circuit.Outputs[0].CurrentState, false);
                // Each node in circuit should have circuit as source
                Assert.AreEqual(circuit.Outputs[0].Sources.Count, 1);
                Assert.AreEqual(circuit.Outputs[0].Sources[circuitId], false);
            }

            circuit.Dispose();
        }

        [TestMethod]
        public void AndCircuitXMLSerializationTest()
        {
            string circuitId = "My Circuit";
            AndCircuit circuit = new(circuitId);

            // Check XML
            //
            // <Root>
            //  <Identifier>_circuit_id</Identifier>
            //  <Inputs>2</Inputs>
            //  <Outputs>1</Outputs>
            //  <AssemblyType>circu_sim.CircuitLogic.AndCircuit</AssemblyType>
            //  <IsElementary>true</IsElementary>
            // </Root>
            //
            System.Xml.Linq.XElement xmlRootCircuit = circuit.ToXML();
            Assert.AreEqual(xmlRootCircuit.Name, "Root");

            System.Xml.Linq.XElement xmlIdentifier = xmlRootCircuit.Element("Identifier");
            Assert.IsNotNull(xmlIdentifier);
            Assert.AreEqual(xmlIdentifier.Value, circuitId);

            System.Xml.Linq.XElement xmlInputs = xmlRootCircuit.Element("Inputs");
            Assert.IsNotNull(xmlInputs);
            Assert.AreEqual(int.Parse(xmlInputs.Value), 2);

            System.Xml.Linq.XElement xmlOutputs = xmlRootCircuit.Element("Outputs");
            Assert.IsNotNull(xmlOutputs);
            Assert.AreEqual(int.Parse(xmlOutputs.Value), 1);

            System.Xml.Linq.XElement xmlAssemblyType = xmlRootCircuit.Element("AssemblyType");
            Assert.IsNotNull(xmlAssemblyType);
            Assert.AreEqual(xmlAssemblyType.Value, "circu_sim.CircuitLogic.AndCircuit");

            System.Xml.Linq.XElement xmlIsElementary = xmlRootCircuit.Element("IsElementary");
            Assert.IsNotNull(xmlIsElementary);
            Assert.AreEqual(bool.Parse(xmlIsElementary.Value), true);

            // Check that we can create a circuit from xml.
            Assembly assembly = Assembly.GetAssembly(typeof(BaseCircuit));
            Assert.IsNotNull(assembly);

            Type circuitType = assembly.GetType(xmlAssemblyType.Value);
            Assert.IsNotNull(circuitType);

            BaseCircuit circuitCreated = Activator.CreateInstance(circuitType, xmlIdentifier.Value) as BaseCircuit;
            Assert.IsNotNull(circuitCreated);

            Assert.AreEqual(circuitCreated.Identifier, circuit.Identifier);
            Assert.AreEqual(circuitCreated.ToXML().ToString(), circuit.ToXML().ToString());

            circuit.Dispose();
            circuitCreated.Dispose();
        }

        [TestMethod]
        public void AndCircuitSignal()
        {
            string circuitId = "My Circuit";
            AndCircuit circuit = new(circuitId);

            // false && false = false
            circuit.Inputs[0].SignalHandler(null, new Signal(false, circuitId));
            circuit.Inputs[1].SignalHandler(null, new Signal(false, circuitId));
            Assert.AreEqual(circuit.Inputs[0].CurrentState, false);
            Assert.AreEqual(circuit.Inputs[1].CurrentState, false);
            Assert.AreEqual(circuit.Outputs[0].CurrentState, false);

            // false && true = false
            circuit.Inputs[0].SignalHandler(null, new Signal(false, circuitId));
            circuit.Inputs[1].SignalHandler(null, new Signal(true, circuitId));
            Assert.AreEqual(circuit.Inputs[0].CurrentState, false);
            Assert.AreEqual(circuit.Inputs[1].CurrentState, true);
            Assert.AreEqual(circuit.Outputs[0].CurrentState, false);

            // true && false = false
            circuit.Inputs[0].SignalHandler(null, new Signal(true, circuitId));
            circuit.Inputs[1].SignalHandler(null, new Signal(false, circuitId));
            Assert.AreEqual(circuit.Inputs[0].CurrentState, true);
            Assert.AreEqual(circuit.Inputs[1].CurrentState, false);
            Assert.AreEqual(circuit.Outputs[0].CurrentState, false);

            // true && true = true
            circuit.Inputs[0].SignalHandler(null, new Signal(true, circuitId));
            circuit.Inputs[1].SignalHandler(null, new Signal(true, circuitId));
            Assert.AreEqual(circuit.Inputs[0].CurrentState, true);
            Assert.AreEqual(circuit.Inputs[1].CurrentState, true);
            Assert.AreEqual(circuit.Outputs[0].CurrentState, true);

            circuit.Dispose();
        }
    }
}