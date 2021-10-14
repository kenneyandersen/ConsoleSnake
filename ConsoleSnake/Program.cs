using System;
using System.Threading;

namespace ConsoleSnake
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Console Snake");
            var snakeGame = new ConsoleSnake();
            Console.CursorVisible = false;
            Console.SetWindowSize(120, 50);
            snakeGame.Start();
            Console.CursorVisible = true;
        }
    }

    class ConsoleSnake
    {
        bool doExit = false;
        const int maxSnakeLength = 30;
        const int mapWidth = 50;
        const int mapHeight = 20;
        const int snakeStartLength = 3;
        const int baseSnakeSpeed = 500; // 500 ms, between moves
        const int maxSnakeSpeed = 50;
        const int snakeSpeedSubtractPerLength = 50; // 10 ms subtract per link in snake
        Random random = new Random();
        // game state
        DateTime gameStartTime;
        int snakeHeading; // 0 = Up, 1=Left, 2=Right, 3=Down
        int currentSnakeLength; // how many links do the snake currently have
        int[] snakePositionX = new int[maxSnakeLength]; // x-koordinates for each link of the snake
        int[] snakePositionY = new int[maxSnakeLength]; // y-koordinates for each link of the snake
        DateTime lastSnakeMove = DateTime.MinValue;
        bool gameover;
        bool win;

        bool foodSpawned;
        bool foodEaten;
        int foodX;
        int foodY;

        
        public void Start()
        {
            doExit = false;
            ResetGame();
            RunGameLoop();
        }


        void ResetGame()
        {
            gameStartTime = DateTime.Now;
            lastSnakeMove = DateTime.Now;
            gameover = false;
            win = false;
            // initialize snake
            currentSnakeLength = snakeStartLength;
            snakeHeading = 1; // 0 = Up, 1=Left, 2=Right, 3=Down
            snakePositionX[0] = mapWidth / 2;
            snakePositionY[0] = mapHeight / 2;
            // set rest of snake to be at the spot to the left of the head
            for(int i = 1; i < currentSnakeLength; i++)
            {
                snakePositionX[i] = snakePositionX[0] + 1;
                snakePositionY[i] = snakePositionY[0];
            }
            // init food
            foodSpawned = false;
            foodEaten = false;
        }

        void RunGameLoop()
        {
            while(!doExit)
            {
                UpdateState();
                DrawFrame();
                Thread.Sleep(50);
            }
        }


        void UpdateState()
        {
            HandleKeyInput();
            if (!gameover && !win)
            {
                SpawnFood();
                UpdateSnakePosition();
                CheckForCollision();
                CheckWinCondition();
            }
        }

        void HandleKeyInput()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Q:
                        doExit=true;
                        break;
                    case ConsoleKey.UpArrow:
                        snakeHeading = 0;
                        break;
                    case ConsoleKey.DownArrow:
                        snakeHeading = 3;
                        break;
                    case ConsoleKey.LeftArrow:
                        snakeHeading = 1;
                        break;
                    case ConsoleKey.RightArrow:
                        snakeHeading = 2;
                        break;
                    case ConsoleKey.R:
                        ResetGame();
                        break;
                }
            }
        }

        void UpdateSnakePosition()
        {
            // calculate interval between snakemove
            int snakeMoveInterval = baseSnakeSpeed - snakeSpeedSubtractPerLength * currentSnakeLength;
            if (snakeMoveInterval < maxSnakeSpeed)
            {
                // make sure we do not get over max speed
                snakeMoveInterval = maxSnakeSpeed; 
            }
            // check if it is time to move the snake
            if (DateTime.Now < lastSnakeMove + TimeSpan.FromMilliseconds(snakeMoveInterval))
            {
                return; // not time to move the snake
            }
            lastSnakeMove = DateTime.Now;
            
            if(foodEaten && currentSnakeLength < maxSnakeLength)
            {
                currentSnakeLength += 1; // increase length of snake
                foodEaten = false; // reset the food eaten marker, until we eat again
            }

            // each part of the tail of the snake moves one forward
            for (int i = currentSnakeLength-1; i > 0; i--)
            {
                snakePositionX[i] = snakePositionX[i-1];
                snakePositionY[i] = snakePositionY[i-1];
            }
            // find the new position of the head (position now has the position that the head had)
            int headX = snakePositionX[1];
            int headY = snakePositionY[1];
            switch (snakeHeading)
            {
                // 0 = Up, 1=Left, 2=Right, 3=Down
                case 0:
                    headY -= 1;
                    break;
                case 1:
                    headX -= 1;
                    break;
                case 2:
                    headX += 1;
                    break;
                case 3:
                    headY += 1;
                    break;
            }
            snakePositionX[0] = headX;
            snakePositionY[0] = headY;
        }

        void SpawnFood()
        {
            if (!foodSpawned)
            {
                foodX = random.Next(0, mapWidth);
                foodY = random.Next(0, mapHeight);
                foodSpawned = true;
            }
        }

        void CheckForCollision()
        {
            // check for collision with the end of the map
            for(int i = 0; i< currentSnakeLength; i++)
            {
                if (snakePositionX[i] < 0 
                    || snakePositionX[i] > mapWidth-1
                    || snakePositionY[i] < 0
                    || snakePositionY[i] > mapHeight-1)
                {
                    gameover = true;
                }
            }
            // check for head collision with tail. compare head coordinates with the tail coordinates
            for(int i = 1; i < currentSnakeLength; i++)
            {
                if(snakePositionX[i] == snakePositionX[0] && snakePositionY[i] == snakePositionY[0])
                {
                    // collision with tail
                    gameover = true;
                }
            }

            // check for collision with food, and eat food if necessary
            if (foodSpawned)
            {
                if (snakePositionX[0]==foodX && snakePositionY[0]==foodY)
                {
                    // the head is at the food
                    foodEaten = true;
                    foodSpawned = false;
                }
            }
        }

        void CheckWinCondition()
        {
            if (currentSnakeLength == maxSnakeLength)
            {
                win = true;
            }
        }

        //
        // -----------------
        //   Draw screen code below
        // -----------------
        //

        private int mapLeft = 3; // offset of where to draw the map
        private int mapTop = 4; // offset of where to draw the map
        void DrawFrame()
        {
            Console.Clear();
            DrawMapArea();
            DrawStatsArea();
            DrawFood();
            DrawSnake();
            
            if (gameover)
            {
                DrawGameOver();
            }
            if (win)
            {
                DrawWin();
            }
        }

        void DrawStatsArea()
        {
            // gametime
            Console.SetCursorPosition(0, 0);
            Console.Write("GameTime: ");
            Console.Write((DateTime.Now - gameStartTime).ToString());
            // points
            Console.SetCursorPosition(0, 1);
            Console.Write("Point: ");
            Console.Write(currentSnakeLength);
            Console.Write("/");
            Console.Write(maxSnakeLength);
            // key help
            Console.SetCursorPosition(0, 2);
            Console.Write("Q: Quit - R: Reset game");
        }

        void DrawMapArea()
        {
            for (int i = mapLeft-1; i < mapWidth + mapLeft +1; i++)
            {
                // draw top bar of map
                Console.SetCursorPosition(i, mapTop - 1);
                Console.Write("#");
                // draw bottom bar of map
                Console.SetCursorPosition(i, mapTop + mapHeight + 1);
                Console.Write("#");
            }
            for(int i = mapTop-1; i < mapTop+mapHeight+1; i++)
            {
                Console.SetCursorPosition(mapLeft - 1, i);
                Console.Write("#");
                Console.SetCursorPosition(mapLeft + mapWidth + 1, i);
                Console.Write("#");
            }
        }

        void DrawSnake()
        {
            for(int i = 0; i < currentSnakeLength; i++)
            {
                int x = snakePositionX[i];
                int y = snakePositionY[i];
                // adjust coordinates to screen map
                x += mapLeft;
                y += mapTop;
                if (x < 0 || x > Console.WindowWidth || y < 0 || y > Console.WindowHeight)
                {
                    continue; // a precaution for the game not to crash if snake ends up outside the screenarea
                }
                Console.SetCursorPosition(x, y);
                if (i == 0)
                {
                    // the head, we use O instead of *
                    Console.Write("O");
                } else
                {
                    Console.Write("*");
                }
                
            }
        }

        void DrawFood()
        {
            if (foodSpawned)
            {
                Console.SetCursorPosition(foodX + mapLeft, foodY + mapTop);
                Console.Write("@");
            }
        }

        void DrawGameOver()
        {
            Console.SetCursorPosition(mapWidth / 2 + mapLeft - 4, mapHeight / 2 + mapTop);
            Console.Write("Game Over");
        }

        void DrawWin()
        {
            Console.SetCursorPosition(mapWidth / 2 + mapLeft - 8, mapHeight / 2 + mapTop);
            Console.Write("Sie haben gewonnen!");
        }
    }
}
