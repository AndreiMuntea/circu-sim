using System.Windows.Forms;

namespace circu_sim
{
    public partial class CircuitSimulator
    {
        private void TableLayoutPanelMenu_MouseEnter(object sender, System.EventArgs e)
        {
            SetMouseCursorToDefault();
        }

        private void TableLayoutPanelActionButtons_MouseEnter(object sender, System.EventArgs e)
        {
            SetMouseCursorToDefault();
        }

        private void PictureBoxBoard_MouseEnter(object sender, System.EventArgs e)
        {
            if (IsDeleteInProgress)
            {
                SetMouseCursorToDelete();
            }
        }

        private void SetMouseCursorToDefault()
        {
            Cursor = Cursors.Default;
        }

        private void SetMouseCursorToDelete()
        {
            Cursor = Cursors.No;
        }
    }
}
