using circu_sim.CircuitLogic;
using System.Drawing;
using System.Windows.Forms;

namespace circu_sim.GraphicComponents
{
    public class OnOffComponent : PictureBox
    {
        private Image OnImage { get; set; }
        private Image OffImage { get; set; }
        public CircuitNode Node { get; set; }

        public OnOffComponent(CircuitNode Node, Image OnImage, Image OffImage, Point Location, Size Size)
        {
            this.Node = Node;
            this.OnImage = OnImage;
            this.OffImage = OffImage;
            this.Location = Location;

            this.Size = Size;
            SizeMode = PictureBoxSizeMode.Zoom;

            this.Node.Subscribers += StateChangedHandler;

            EvaluateImage();
        }

        protected void StateChangedHandler(object? Sender, Signal ReceivedSignal)
        {
            EvaluateImage();
        }

        public void EvaluateImage()
        {
            Image = Node.CurrentState ? OnImage : OffImage;
        }

        public new void Dispose()
        {
            base.Dispose();

            Node.Subscribers -= StateChangedHandler;

            OnImage.Dispose();
            OffImage.Dispose();
        }
    }

    public class SwitchComponent : OnOffComponent
    {
        public const string OnOffComponentType = "Switch";
        public int Position { get; set; }

        public SwitchComponent(
            CircuitNode Node,
            Image OnImage,
            Image OffImage,
            Point Location,
            Size Size,
            int Position
        ) : base(Node, OnImage, OffImage, Location, Size)
        {
            this.Position = Position;

            Anchor = AnchorStyles.Left | AnchorStyles.Top;
        }
    }

    public class BulbComponent : OnOffComponent
    {
        public const string OnOffComponentType = "Bulb";
        public int Position { get; set; }

        public BulbComponent(
            CircuitNode Node,
            Image OnImage,
            Image OffImage,
            Point Location,
            Size Size,
            int Position
        ) : base(Node, OnImage, OffImage, Location, Size)
        {
            this.Position = Position;

            Anchor = AnchorStyles.Right | AnchorStyles.Top;
        }
    }
}
