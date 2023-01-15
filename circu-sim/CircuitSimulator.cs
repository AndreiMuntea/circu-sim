using circu_sim.CircuitLogic;
using System;
using System.Windows.Forms;

namespace circu_sim
{
    public partial class CircuitSimulator : Form
    {
        public CircuitSimulator()
        {
            InitializeComponent();

            InitializePictureBoxBoard();
            InitializeDataGridViewAllCircuits();

            SetFont();
        }

        private void InitializePictureBoxBoard()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void InitializeDataGridViewAllCircuits()
        {
            AllCircuits.Add(new Tuple<BaseCircuit, string>(new AndCircuit(GenerateGuid()), "AND"));
            AllCircuits.Add(new Tuple<BaseCircuit, string>(new OrCircuit(GenerateGuid()), "OR"));
            AllCircuits.Add(new Tuple<BaseCircuit, string>(new NotCircuit(GenerateGuid()), "NOT"));

            foreach (var circuit in CircuitCreator.DeserializeAllCircuits())
            {
                AllCircuits.Add(circuit);
            }

            DataGridViewAllCircuits.AutoGenerateColumns = false;
            DataGridViewAllCircuits.ColumnCount = 1;
            DataGridViewAllCircuits.Columns[0].DataPropertyName = "Item2";
            DataGridViewAllCircuits.DataSource = AllCircuits;
        }
    }
}
