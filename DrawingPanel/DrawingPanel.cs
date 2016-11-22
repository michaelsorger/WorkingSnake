using SnakeGame;
using System.Drawing;
using System.Windows.Forms;

namespace SnakeGame
{
    /// <summary>
    /// This is a helper class for drawing a world
    /// We can place one of these panels in our GUI, alongside other controls like buttons
    /// Anything drawn within this panel will use a local coordinate system
    /// </summary>
    public class DrawingPanel : Panel
    {

        /// We need a reference to the world, so we can draw the objects in it
        private World world;


        public DrawingPanel()
        {
            // Setting this property to true prevents flickering
            this.DoubleBuffered = true;
        }

        /// <summary>
        /// Pass in a reference to the world, so we can draw the objects in it
        /// </summary>
        /// <param name="_world"></param>
        public void SetWorld(World _world)
        {
            world = _world;
        }

        /// <summary>
        /// Override the behavior when the panel is redrawn
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // If we don't have a reference to the world yet, nothing to draw.
            if (world == null)
                return;

            using (System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(Color.Black))
            {


                // Turn on anti-aliasing for smooth round edges
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw the top wall
                Rectangle topWall = new Rectangle(0, 0, world.width * world.pixelsPerCell, world.pixelsPerCell);
                e.Graphics.FillRectangle(drawBrush, topWall);

                // Draw the right wall
                Rectangle rightWall = new Rectangle((world.width - 1) * world.pixelsPerCell, 0, world.pixelsPerCell, world.height * world.pixelsPerCell);
                e.Graphics.FillRectangle(drawBrush, rightWall);

                // Draw the left wall
                Rectangle leftWall = new Rectangle(0, 0, world.pixelsPerCell, world.height * world.pixelsPerCell);
                e.Graphics.FillRectangle(drawBrush, leftWall);

                // Draw the bottom wall
                Rectangle bottomWall = new Rectangle(0, (world.height * world.pixelsPerCell) - world.pixelsPerCell, world.width * world.pixelsPerCell, world.pixelsPerCell);
                e.Graphics.FillRectangle(drawBrush, bottomWall);
            }

            // Draw the "world" (just food) within this panel by using the PaintEventArgs
            world.Draw(e);

        }
    }
}

