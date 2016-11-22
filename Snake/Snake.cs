using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace SnakeGame
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Point
    {
        [JsonProperty]
        int x;

        [JsonProperty]
        int y;

        public Point(int X, int Y)
        {
            x = X;
            y = Y;
        }
        public int getX()
        {
            return x;
        }
        public int getY()
        {
            return y;
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class Food
    {
        [JsonProperty]
        private int ID;

        [JsonProperty]
        Point loc;

        [JsonProperty]
        int cellSize;

        public int getID()
        {
            return ID;
        }
        public Food(int foodID, SnakeGame.Point location)
        {
            ID = foodID;
            loc = location; 
        }

        //draw
        /// <summary>
        /// Helper method for DrawingPanel
        /// Given the PaintEventArgs that comes from DrawingPanel, draw the contents of the world on to the panel.
        /// </summary>
        /// <param name="e"></param>
        public void Draw(PaintEventArgs e,int pixelsPerCell)
        {
            using (System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(Color.Black))
            {
                // Draw the single dot that represents the food
                Rectangle dotBounds = new Rectangle(loc.getX() * pixelsPerCell, loc.getY() * pixelsPerCell, pixelsPerCell, pixelsPerCell);
                e.Graphics.FillEllipse(drawBrush, dotBounds);
            }
        }

        public bool FoodIsDead(Food someFood)
        {
            //returns true when points are -1
            Point deadRep = new Point(-1, -1);
            if (someFood.loc == deadRep)
                return true;
            else
                return false;
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class Snake
    {
        //dictionary to map ID to color
        Dictionary<int, Color> snakeColorDict = new Dictionary<int, Color>();

        [JsonProperty]
        private int ID;

        [JsonProperty]
        string name;

        [JsonProperty]
        int headX;

        [JsonProperty]
        int tailY;

        [JsonProperty]
        List<Point> vertices;

        Color snakeColor;
        public int getID()
        {
            return ID;
        }
        public string getName()
        {
            return name;
        }
        public Snake(string id)
        {
            Int32.TryParse(id, out ID);
            Random rand = new Random();
            int r = rand.Next(255);
            int g = rand.Next(255);
            int b = rand.Next(255);
            snakeColor = Color.FromArgb(r, g, b);

        }

        public void Draw(PaintEventArgs e, int pixelsPerCell)
        {
            using (System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(snakeColor))
            {
                Rectangle rect;
                // Draw the snake
                for(int i = 1; i < vertices.Count; i++)
                {
                    if(vertices[i-1].getX() == vertices[i].getX()) //vertical case
                    {
                        if(vertices[i-1].getY() < vertices[i].getY())//going down
                        {
                             rect = new Rectangle(vertices[i - 1].getX() * pixelsPerCell, vertices[i - 1].getY() * pixelsPerCell, pixelsPerCell, (vertices[i].getY() - vertices[i - 1].getY())*pixelsPerCell + pixelsPerCell);
                        }
                        else //going Up
                        {
                             rect = new Rectangle(vertices[i].getX() * pixelsPerCell, vertices[i].getY() * pixelsPerCell, pixelsPerCell, (vertices[i - 1].getY() - vertices[i].getY())*pixelsPerCell + pixelsPerCell);
                        }
                        
                    }
                   
                    else //horizontal case
                    {
                        if(vertices[i-1].getX() < vertices[i].getX()) //going right
                        {
                             rect = new Rectangle(vertices[i - 1].getX() * pixelsPerCell, vertices[i - 1].getY() * pixelsPerCell, (vertices[i].getX() - vertices[i - 1].getX())*pixelsPerCell + pixelsPerCell,pixelsPerCell);
                        }
                        else //going left
                        {
                             rect = new Rectangle(vertices[i].getX() * pixelsPerCell, vertices[i].getY() * pixelsPerCell, (vertices[i - 1].getX() - vertices[i].getX())*pixelsPerCell + pixelsPerCell, pixelsPerCell);
                        }
                    }
                    e.Graphics.FillRectangle(drawBrush,rect);
                }                
            }
        }

        public bool SnakeIsDead(Snake someSnake)
        {
            //returns true when points are -1
            Point deadRep = new Point(-1, -1);
            if (someSnake.vertices[0] == deadRep && someSnake.vertices[1] == deadRep)
                return true;
            else
                return false;
        }

        public int snakeScore(Snake someSnake)
        {
            int score = 0;
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                score += distanceFormula(vertices[i].getX(), vertices[i + 1].getX(), vertices[i].getY(), vertices[i + 1].getY());
            }
            return score;
        }
        private int distanceFormula(int x1, int x2, int y1, int y2)
        {
            return (int)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
    }
}
