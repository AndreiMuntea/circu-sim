using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace circu_sim.CircuitLogic
{
    public class NestedCircuit : BaseCircuit
    {
        public List<BaseCircuit> Circuits;
        public List<Tuple<CircuitNode, CircuitNode>> Links;

        public NestedCircuit(string Identifier) : base(Identifier, false)
        {
            Circuits = new();
            Links = new();
        }

        public override void Dispose()
        {
            base.Dispose();

            // Remove the circuits
            while (Circuits.Count > 0)
            {
                RemoveCircuit(Circuits[0]);
            }

            // Remove the links
            while (Links.Count > 0)
            {
                Unlink(Links[0].Item1, Links[0].Item2);
            }

            GC.SuppressFinalize(this);
        }

        public CircuitNode InsertNode(CircuitNodeType NodeType, string NodeIdentifier)
        {
            CircuitNode? node = new(NodeIdentifier, Identifier);
            if (NodeType == CircuitNodeType.Input)
            {
                AddNode(ref Inputs, node);
            }
            else if (NodeType == CircuitNodeType.Output)
            {
                AddNode(ref Outputs, node);
            }
            Evaluate();
            return node;
        }
        public void DeleteNode(CircuitNode Node)
        {
            // We can only erase input/output nodes from this circuit
            if (Node.CircuitIdentifier == Identifier)
            {
                // First erase all links to and from this node
                UnlinkReferences(Node);
                // Then remove the actual node
                RemoveNode(ref Inputs, Node);
                RemoveNode(ref Outputs, Node);
            }
            Evaluate();
        }
        public CircuitNode? SearchNode(string CircuitIdentifier, string NodeIdentifier)
        {
            CircuitNode? foundNode = null;
            BaseCircuit? foundCircuit = Identifier.Equals(CircuitIdentifier, StringComparison.Ordinal) ? this
                                                                                    : null;
            foreach (BaseCircuit? circuit in Circuits)
            {
                if (circuit.Identifier.Equals(CircuitIdentifier, StringComparison.Ordinal))
                {
                    foundCircuit = circuit;
                }
            }
            if (null != foundCircuit)
            {
                foundNode = FindNode(ref foundCircuit.Inputs, NodeIdentifier);
                foundNode ??= FindNode(ref foundCircuit.Outputs, NodeIdentifier);
            }
            return foundNode;
        }
        public void InsertCircuit(BaseCircuit Circuit)
        {
            BaseCircuit? circuit = Circuits.Find(x => x.Identifier.Equals(Circuit.Identifier, StringComparison.Ordinal));
            if (circuit == null)
            {
                Circuits.Add(Circuit);
            }
            Evaluate();
        }
        public void RemoveCircuit(BaseCircuit Circuit)
        {
            BaseCircuit? circuit = Circuits.Find(x => x.Identifier.Equals(Circuit.Identifier, StringComparison.Ordinal));
            if (circuit != null)
            {
                // Unlink all links to this circuit's nodes.
                foreach (CircuitNode? node in Circuit.Outputs)
                {
                    UnlinkReferences(node);
                }
                foreach (CircuitNode? node in circuit.Inputs)
                {
                    UnlinkReferences(node);
                }
                // And now erase the circuit
                Circuits.Remove(circuit);
                circuit.Dispose();
            }
            Evaluate();
        }
        public void Link(CircuitNode Source, CircuitNode Destination)
        {
            // If a link between Source --> Destination exists, stop
            Tuple<CircuitNode, CircuitNode>? existingLinkSrcDst = Links.Find(
                x => x.Item1.CircuitIdentifier.Equals(Source.CircuitIdentifier, StringComparison.Ordinal) &&
                     x.Item1.Identifier.Equals(Source.Identifier, StringComparison.Ordinal) &&
                     x.Item2.CircuitIdentifier.Equals(Destination.CircuitIdentifier, StringComparison.Ordinal) &&
                     x.Item2.Identifier.Equals(Destination.Identifier, StringComparison.Ordinal)
                );
            if (existingLinkSrcDst != null)
            {
                return;
            }

            // If a link between Destination --> Source exists, stop
            Tuple<CircuitNode, CircuitNode>? existingLinkDstSrc = Links.Find(
                x => x.Item1.CircuitIdentifier.Equals(Destination.CircuitIdentifier, StringComparison.Ordinal) &&
                     x.Item1.Identifier.Equals(Destination.Identifier, StringComparison.Ordinal) &&
                     x.Item2.CircuitIdentifier.Equals(Source.CircuitIdentifier, StringComparison.Ordinal) &&
                     x.Item2.Identifier.Equals(Source.Identifier, StringComparison.Ordinal)
                );
            if (existingLinkDstSrc != null)
            {
                return;
            }

            // No link between the two nodes, create one.
            Tuple<CircuitNode, CircuitNode>? link = new(Source, Destination);
            Links.Add(link);

            Destination.AddSource(Source.Identifier, Source.CurrentState);
            Source.Subscribers += Destination.SignalHandler;

            Evaluate();
        }
        public void Unlink(CircuitNode Source, CircuitNode Destination)
        {
            Tuple<CircuitNode, CircuitNode>? existingLinkSrcDst = Links.Find(
                x => x.Item1.CircuitIdentifier.Equals(Source.CircuitIdentifier, StringComparison.Ordinal) &&
                     x.Item1.Identifier.Equals(Source.Identifier, StringComparison.Ordinal) &&
                     x.Item2.CircuitIdentifier.Equals(Destination.CircuitIdentifier, StringComparison.Ordinal) &&
                     x.Item2.Identifier.Equals(Destination.Identifier, StringComparison.Ordinal)
                );
            if (existingLinkSrcDst != null)
            {
                Source.Subscribers -= Destination.SignalHandler;
                Destination.RemoveSource(Source.Identifier);

                Links.Remove(existingLinkSrcDst);
            }
            Evaluate();
        }
        public override void Evaluate()
        {
            // Reevaluate the circuits as a change occured
            foreach (BaseCircuit? circuit in Circuits)
            {
                circuit.Evaluate();
            }
        }
        public override XElement ToXML()
        {
            XElement? root = base.ToXML();

            // Inputs
            XElement? xmlInputs = new("InputsList");
            foreach (CircuitNode? input in Inputs)
            {
                XElement? xmlInput = new("Identifier", input.Identifier);
                xmlInputs.Add(xmlInput);
            }
            root.Add(xmlInputs);

            // Outputs
            XElement? xmlOutputs = new("OutputsList");
            foreach (CircuitNode? output in Outputs)
            {
                XElement? xmlOutput = new("Identifier", output.Identifier);
                xmlOutputs.Add(xmlOutput);
            }
            root.Add(xmlOutputs);

            // Circuits
            XElement? xmlCircuits = new("Circuits");
            foreach (BaseCircuit? circuit in Circuits)
            {
                xmlCircuits.Add(circuit.ToXML());
            }
            root.Add(xmlCircuits);

            // Links
            XElement? xmlLinks = new("Links");
            foreach (Tuple<CircuitNode, CircuitNode>? link in Links)
            {
                XElement? xmlLink = new("Link");
                xmlLink.Add(new XElement("SrcCircuitId", link.Item1.CircuitIdentifier));
                xmlLink.Add(new XElement("SrcNodeId", link.Item1.Identifier));

                xmlLink.Add(new XElement("DstCircuitId", link.Item2.CircuitIdentifier));
                xmlLink.Add(new XElement("DstNodeId", link.Item2.Identifier));

                xmlLinks.Add(xmlLink);
            }
            root.Add(xmlLinks);

            return root;
        }
        protected void UnlinkReferences(CircuitNode Node)
        {
            // Remove links where this node is source
            List<Tuple<CircuitNode, CircuitNode>>? listSrc = Links.FindAll(
                 x => x.Item1.CircuitIdentifier == Node.CircuitIdentifier &&
                      x.Item1.Identifier == Node.Identifier);
            foreach (Tuple<CircuitNode, CircuitNode>? link in listSrc)
            {
                Unlink(link.Item1, link.Item2);
            }

            // Remove links where this node is destination
            List<Tuple<CircuitNode, CircuitNode>>? listDst = Links.FindAll(
                 x => x.Item2.CircuitIdentifier == Node.CircuitIdentifier &&
                      x.Item2.Identifier == Node.Identifier);
            foreach (Tuple<CircuitNode, CircuitNode>? link in listDst)
            {
                Unlink(link.Item1, link.Item2);
            }
        }
    }

    public static class CircuitCreator
    {
        public static void SerializeCircuitToFile(BaseCircuit Circuit, string CircuitName)
        {
            // Get the circuits directory
            string circuitsPath = GetCircuitDirectoryPath();

            // Check circuit name length
            if (CircuitName.Length == 0 || CircuitName.Length > 16)
            {
                throw new FileNotFoundException("Circuit name should have between 1 and 16 characters!");
            }

            // Check that circuit name contains only letters, numbers, spaces and underscore
            // a-z <-- downcase letters
            // A-Z <-- upcase letters
            // 0-9 <-- numbers
            // _   <-- underscore
            //     <-- space
            if (!Regex.IsMatch(CircuitName, @"^[a-zA-Z0-9_ ]+$"))
            {
                throw new FileNotFoundException("Circuit name can contain only letters, numbers, spaces or underscore!");
            }

            // Append 'CircuitName'
            string circuitPath = Path.Combine(circuitsPath, CircuitName + ".xml");

            // Now serialize the circuit in xml format.
            Circuit.ToXML().Save(circuitPath);
        }

        public static Tuple<BaseCircuit, string> DeserializeCircuitFromFile(string XmlCircuitPath)
        {
            // Get the circuits directory
            string circuitsPath = GetCircuitDirectoryPath();

            // Get the folder only
            string? currentDir = Path.GetDirectoryName(XmlCircuitPath);
            string? circuitName = Path.GetFileNameWithoutExtension(XmlCircuitPath);
            if (currentDir == null || circuitName == null)
            {
                throw new FileNotFoundException("Can't find circuits directory path!");
            }

            // Check if circuit is in circuits folder
            if (!currentDir.Equals(circuitsPath, StringComparison.Ordinal))
            {
                throw new FileNotFoundException("Circuit from outside of circuits folder!");
            }

            // Deserialize the circuit
            XElement xmlCircuit = XElement.Load(XmlCircuitPath);
            BaseCircuit? circuit = FromXml(xmlCircuit);
            if (circuit == null)
            {
                throw new InvalidOperationException("Invalid XML for circuit!");
            }

            // Return the circuit and its name
            return new Tuple<BaseCircuit, string>(circuit, circuitName);
        }
        public static List<Tuple<BaseCircuit, string>> DeserializeAllCircuits()
        {
            List<Tuple<BaseCircuit, string>> circuits = new();

            // Get the circuits directory
            DirectoryInfo circuitsPath = new(GetCircuitDirectoryPath());

            // Go through all xml files in top level of circuits directory
            var files = circuitsPath.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly).OrderBy(x => x.CreationTime);
            foreach (FileInfo? file in files)
            {
                if (null != file)
                {
                    Tuple<BaseCircuit, string> circuit = DeserializeCircuitFromFile(file.FullName);
                    circuits.Add(circuit);
                }
            }

            // Return the deserialized circuits
            return circuits;
        }

        public static string GetCircuitDirectoryPath()
        {
            // Location of the executable
            string currentDir = Path.GetFullPath(Environment.CurrentDirectory);

            // Append 'Circuits'
            string directoryPath = Path.Combine(currentDir, "Circuits");

            // Create the folder if it doesn't exist
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        public static BaseCircuit? FromXml(XElement Root)
        {
            bool result = true;

            string identifier = "";
            int inputs = 0;
            int outputs = 0;
            bool isElementary = false;
            string assemblyType = "";

            // Get common serialized items
            result = result && GetString(Root, "Identifier", ref identifier);
            result = result && GetInt(Root, "Inputs", ref inputs);
            result = result && GetInt(Root, "Outputs", ref outputs);
            result = result && GetBool(Root, "IsElementary", ref isElementary);
            result = result && GetString(Root, "AssemblyType", ref assemblyType);
            if (!result)
            {
                return null;
            }

            // If the circuit is elementary, we are done
            if (isElementary)
            {
                return CreateBaseCircuit(identifier, assemblyType);
            }

            return CreateNestedCircuit(Root, identifier, assemblyType);
        }

        private static BaseCircuit? CreateBaseCircuit(string Identifier, string AssemblyType)
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(BaseCircuit));
            if (assembly == null)
            {
                return null;
            }

            Type? circuitType = assembly.GetType(AssemblyType);
            if (circuitType == null)
            {
                return null;
            }

            return Activator.CreateInstance(circuitType, Identifier) as BaseCircuit;
        }
        private static BaseCircuit? CreateNestedCircuit(XElement Root, string Identifier, string AssemblyType)
        {
            bool result = true;
            List<XElement>? xmlInputs = new();
            List<XElement>? xmlOutputs = new();
            List<XElement>? xmlCircuits = new();
            List<XElement>? xmlLinks = new();

            // Get required fields from nested circuit
            result = result && GetElementsList(Root, "InputsList", "Identifier", ref xmlInputs);
            result = result && GetElementsList(Root, "OutputsList", "Identifier", ref xmlOutputs);
            result = result && GetElementsList(Root, "Circuits", "Root", ref xmlCircuits);
            result = result && GetElementsList(Root, "Links", "Link", ref xmlLinks);
            if (!result)
            {
                return null;
            }

            // Create the nested circuit
            if (CreateBaseCircuit(Identifier, AssemblyType) is not NestedCircuit circuit)
            {
                return null;
            }

            // Add inputs and outputs
            InsertNodes(xmlInputs, CircuitNodeType.Input, ref circuit);
            InsertNodes(xmlOutputs, CircuitNodeType.Output, ref circuit);

            // Add all sub-circuits.
            if (!InsertCircuits(xmlCircuits, ref circuit))
            {
                return null;
            }

            // Add links.
            if (!InsertLinks(xmlLinks, ref circuit))
            {
                return null;
            }

            // All good return circuit.
            return circuit;
        }

        private static void InsertNodes(List<XElement> Inputs, CircuitNodeType NodeType, ref NestedCircuit Circuit)
        {
            foreach (XElement? input in Inputs)
            {
                Circuit.InsertNode(NodeType, input.Value);
            }
        }
        private static bool InsertLinks(List<XElement> Links, ref NestedCircuit Circuit)
        {
            foreach (XElement? link in Links)
            {
                bool result = true;
                CircuitNode? src = null;
                CircuitNode? dst = null;

                result = result && GetNode(link, "SrcCircuitId", "SrcNodeId", ref Circuit, ref src);
                result = result && GetNode(link, "DstCircuitId", "DstNodeId", ref Circuit, ref dst);
                if (result == false || src == null || dst == null)
                {
                    return false;
                }

                Circuit.Link(src, dst);
            }
            return true;
        }
        private static bool InsertCircuits(List<XElement> Circuits, ref NestedCircuit Circuit)
        {
            foreach (XElement? xmlCircuit in Circuits)
            {
                BaseCircuit? childCircuit = FromXml(xmlCircuit);
                if (childCircuit == null)
                {
                    return false;
                }
                Circuit.InsertCircuit(childCircuit);
            }
            return true;
        }
        private static bool GetString(XElement Root, string Name, ref string Element)
        {
            XElement? xmlElement = Root.Element(Name);
            if (xmlElement == null)
            {
                return false;
            }
            Element = xmlElement.Value;
            return true;
        }
        private static bool GetInt(XElement Root, string Name, ref int Element)
        {
            XElement? xmlElement = Root.Element(Name);
            if (xmlElement == null)
            {
                return false;
            }
            return int.TryParse(xmlElement.Value, out Element);
        }
        private static bool GetBool(XElement Root, string Name, ref bool Element)
        {
            XElement? xmlElement = Root.Element(Name);
            if (xmlElement == null)
            {
                return false;
            }
            return bool.TryParse(xmlElement.Value, out Element);
        }
        private static bool GetElementsList(XElement Root, string NodeName, string ElementName, ref List<XElement> Elements)
        {
            XElement? xmlNode = Root.Element(NodeName);
            if (xmlNode == null)
            {
                return false;
            }

            IEnumerable<XElement>? xmlElements = xmlNode.Elements(ElementName);
            if (xmlElements == null)
            {
                return false;
            }

            Elements = xmlElements.ToList();
            return true;
        }
        private static bool GetNode(XElement Link, string CircuitId, string NodeId, ref NestedCircuit Circuit, ref CircuitNode? Element)
        {
            string srcCircuitId = "";
            string srcNodeId = "";
            bool result = true;

            result = result && GetString(Link, CircuitId, ref srcCircuitId);
            result = result && GetString(Link, NodeId, ref srcNodeId);
            if (!result)
            {
                return false;
            }

            Element = Circuit.SearchNode(srcCircuitId, srcNodeId);
            return true;
        }

    }
}