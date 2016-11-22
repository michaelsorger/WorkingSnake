using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnakeGame
{
    public class World
    {        
        int[][] snakeWorld;
        public Dictionary<int, Snake> snakeDict;
        public Dictionary<int, Food> foodDict;
        public int pixelsPerCell = 5;
        public World(int wid,int hei)
        {
            width = wid;
            height = hei;
            foodDict = new Dictionary<int, Food>();
            snakeDict = new Dictionary<int, Snake>();
        }

        // Width of the world in cells (not pixels)
        public int width
        {
            get;
            private set;
        }

        // Height of the world in cells (not pixels)
        public int height
        {
            get;
            private set;
        }

        /// <summary>
        /// Updates the game world for one time tick (frame).
        /// Moves snakes forward in their direction.
        /// Eats food.
        /// Kills snakes, and recycles them.
        /// </summary>
        public void Update()
        {
            //some questions to consider
            //how do all the snakes in the world get updated at this point, does the world care about all the clients?
            //maybe foreach control made from a client, update all the snakes accordingly? what can we do to make this not slow?
            //check if food is eaten during movement? or after all snake movement is done?
            //check for all snakes collision? helper function? called something like killSnakeHeadsAgainstWallOrSnake();

            //psudo code
            //updateSnakeLocation();
            //updateSnakeSize();? or does the world not care?
            //killSnakeHeadsAgainstWallOrSnake(); - calls collideWith from snake class?
        }
        //draw
        public void Draw(PaintEventArgs e)
        {
            using (System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(Color.Black))
            {
                lock (foodDict)
                {
                    foreach (KeyValuePair<int, Food> foodToDraw in foodDict)
                    {
                        foodToDraw.Value.Draw(e,pixelsPerCell);
                    }
                }
                lock (snakeDict)
                {
                    foreach(KeyValuePair<int,Snake> snakeToDraw in snakeDict)
                    {
                        snakeToDraw.Value.Draw(e, pixelsPerCell);
                    }
                }
            }
            
        }
    }
}
