using System;
using System.Threading;
using Spectre.Console;


class Program
{
  static int timer = 600; // Countdown timer in seconds

  static int[] field;
  static int score = 0;
  static int width = 10;
  static int height = 20;
  static int[] currentTetromino;
  static int[] nextTetromino;
  static int currentX, currentY;

  static void Main(string[] args)
  {
    field = new int[width * height];
    NewTetromino();
    currentX = width / 2;
    currentY = 0;
    int counter = 0;
    int threshold = 200; // Adjust this value to get the desired Tetromino speed

    Thread timerThread = new Thread(new ThreadStart(Countdown));
    timerThread.Start();

    Console.Clear();
    while (true)
    {

      if (Console.KeyAvailable)
      {
        var key = Console.ReadKey(true).Key;
        switch (key)
        {
          case ConsoleKey.LeftArrow:
            MoveLeft();
            break;
          case ConsoleKey.RightArrow:
            MoveRight();
            break;
          case ConsoleKey.UpArrow:
            Rotate();
            break;
          case ConsoleKey.DownArrow:
            MoveDown();
            break;
          case ConsoleKey.Spacebar:
            Drop();
            break;
        }
      }

      // Increment counter
      counter++;

      // Move the Tetromino down every 'threshold' updates
      if (counter >= threshold)
      {

        currentY++;
        if (IsCollision())
        {
          // If the Tetromino hit something, move it back up, merge it into the field, and create a new Tetromino
          currentY--;
          Merge();
          ClearLines();
          NewTetromino();
          currentX = width / 2;
          currentY = 0;
          if (IsCollision())
          {
            // If the new Tetromino collides with something, the game is over
            break;
          }
          // Reset counter
          counter = 0;
        }
      }

      // Draw the game field and the current Tetromino
      Draw();

      // Wait a bit before the next iteration
      Thread.Sleep(1000 / 60); // 20 updates per second
    }
  }


  static void MoveLeft()
  {
    currentX--;
    if (IsCollision())
    {
      currentX++;
    }
  }

  static void MoveRight()
  {
    currentX++;
    if (IsCollision())
    {
      currentX--;
    }
  }

  static void Rotate()
  {
    int[] rotatedTetromino = new int[16];

    for (int px = 0; px < 4; px++)
    {
      for (int py = 0; py < 4; py++)
      {
        rotatedTetromino[px * 4 + py] = currentTetromino[py * 4 + (3 - px)];
      }
    }

    currentTetromino = rotatedTetromino;

    if (IsCollision())
    {
      // If the rotation would cause a collision, rotate it back
      for (int px = 0; px < 4; px++)
      {
        for (int py = 0; py < 4; py++)
        {
          rotatedTetromino[px * 4 + py] = currentTetromino[py * 4 + px];
        }
      }

      currentTetromino = rotatedTetromino;
    }
  }

  static void Draw()
  {
    // Create a new canvas
    var canvas = new Canvas(width, height);

    // Draw game field
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        if (field[y * width + x] != 0)
        {

           // Check the boundaries before drawing
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                   // Draw a block at (x, y)
                    canvas.SetPixel(x, y, Color.White);
                }
        }
      }
    }

    // Draw current Tetromino
    for (int px = 0; px < 4; px++)
    {
      for (int py = 0; py < 4; py++)
      {
        if (currentTetromino[py * 4 + px] != 0)
        {
          // Draw a block at (currentX + px, currentY + py)
          canvas.SetPixel(currentX + px, currentY + py, Color.Green);
        }
      }
    }

// Draw next Tetromino
for (int i = 0; i < 16; i++)
{
    int x = i % 4;
    int y = i / 4;
    if (nextTetromino[i] != 0)
    {
        // Calculate the position on the canvas
        int canvasX = width + 2 + x;
        int canvasY = 2 + y;

        // Check the boundaries before drawing
        if (canvasX >= 0 && canvasX < canvas.Width && canvasY >= 0 && canvasY < canvas.Height)
        {
            canvas.SetPixel(canvasX +50, canvasY, Color.Red);
        }
    }
}

    // Create a new panel with the canvas as its content
    var panel = new Panel(canvas)
    {
        Border = BoxBorder.Rounded, // Set the border style
        Padding = new Padding(1), // Set the padding
    };

    // Render the panel
    AnsiConsole.Render(panel);

    // Clear the previous score and time
    Console.SetCursorPosition(width + 20, 0);
    Console.Write(new string(' ', 20));
    Console.SetCursorPosition(width + 20, 1);
    Console.Write(new string(' ', 20));

    // Show score and time
    Console.SetCursorPosition(width + 20, 0);
    Console.WriteLine("Score: " + score);
    Console.SetCursorPosition(width + 20, 1);
    Console.WriteLine("Time: " + timer);
  }

  static void Countdown()
  {
    while (timer > 0)
    {
      Thread.Sleep(1000);
      timer--;
    }
    // game over
    Console.WriteLine("Game Over");
    Environment.Exit(0);
  }

  static void MoveDown()
  {
    currentY++;
    if (IsCollision())
    {
      currentY--;
      Merge();
      NewTetromino();
      currentX = width / 2;
      currentY = 0;
      if (IsCollision())
      {
        // game over
        Console.WriteLine("Game Over");
        Environment.Exit(0);
      }
    }
  }

  static void Drop()
  {
    while (!IsCollision())
    {
      currentY++;
    }
    currentY--;
    Merge();
    NewTetromino();
    currentX = width / 2;
    currentY = 0;
    if (IsCollision())
    {
      // game over
      Console.WriteLine("Game Over");
      Environment.Exit(0);
    }
  }

  static bool IsCollision()
  {
    for (int px = 0; px < 4; px++)
    {
      for (int py = 0; py < 4; py++)
      {
        if (currentTetromino[py * 4 + px] != 0)
        {
          if (currentX + px < 0 || currentX + px >= width || currentY + py < 0 || currentY + py >= height)
          {
            return true; // Collision with wall
          }
          if (field[(currentY + py) * width + currentX + px] != 0)
          {
            return true; // Collision with other block
          }
        }
      }
    }
    return false;
  }

  static void Merge()
  {
    for (int px = 0; px < 4; px++)
    {
      for (int py = 0; py < 4; py++)
      {
        if (currentTetromino[py * 4 + px] != 0)
        {
          field[(currentY + py) * width + currentX + px] = 1;
        }
      }
    }
  }

  static Random random = new Random();

  static int[][] Tetrominoes = new int[][]
  {
    new int[] { 0, 0, 0, 0,
                0, 1, 1, 1,
                0, 1, 0, 0,
                0, 0, 0, 0 }, // I
    new int[] { 0, 0, 1, 0,
                0, 0, 1, 0,
                0, 0, 1, 0,
                0, 0, 1, 0 }, // J
    new int[] { 0, 0, 0, 0,
                0, 0, 1, 0,
                0, 1, 1, 1,
                0, 0, 0, 0 }, // T
    new int[] { 0, 1, 0, 0,
                0, 1, 0, 0,
                0, 1, 0, 0,
                0, 1, 0, 0 }, // L
    new int[] { 0, 0, 0, 0,
                0, 0, 1, 1,
                0, 1, 1, 0,
                0, 0, 0, 0 }, // S
    new int[] { 0, 0, 0, 0,
                0, 1, 1, 0,
                0, 0, 1, 1,
                0, 0, 0, 0 }, // Z
    new int[] { 0, 0, 0, 0,
                0, 1, 1, 0,
                0, 1, 1, 0,
                0, 0, 0, 0 }  // O
  };

  static void NewTetromino()
  {
    currentTetromino = nextTetromino ?? Tetrominoes[new Random().Next(0, Tetrominoes.Length)];
    nextTetromino = Tetrominoes[new Random().Next(0, Tetrominoes.Length)];
  }

  static void ClearLines()
  {
    for (int i = 0; i < height; i++)
    {
      if (IsLineFull(i))
      {
        RemoveLine(i);
        score += 100; // Increase score when a line is cleared
      }
    }
  }

  static bool IsLineFull(int i)
  {
    for (int j = 0; j < width; j++)
    {
      if (field[i * width + j] == 0)
      {
        return false;
      }
    }
    return true;
  }

  static void RemoveLine(int i)
  {
    for (int y = i; y > 0; y--)
    {
      for (int j = 0; j < width; j++)
      {
        field[y * width + j] = field[(y - 1) * width + j];
      }
    }
  }
}