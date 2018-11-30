using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace CursorChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            string exeLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exeLocation) + Path.DirectorySeparatorChar;
            string configLocation = exeDirectory + "ImageEditorLocation.txt";
            string tempImageLocation = exeDirectory + "image.png";

            string consolewrite = exeLocation +
                "\nPress F1 to scan. Press F2 to wait 3 seconds then scan." +
                "\nPress F3 to open the current screen in an image editor." +
                "\nPress F4 to convert RGB to hex. Press F5 to convert hex to RGB. Type \"back\" while converting to cancel." +
                "\nPress F6 to clear console.\n";
            Console.WriteLine(consolewrite);

            string imageEditorLocation = File.Exists(configLocation) ? File.ReadAllText(exeDirectory + "ImageEditorLocation.txt") : null;
            if (!File.Exists(imageEditorLocation))
                imageEditorLocation = null;

            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            string format = "Location (X,Y): {0},{1}. Color (R,G,B): {2},{3},{4}. Hex: {5}";

            while (true)
            {
                var key = Console.ReadKey();
                // Get color
                if (key.Key == ConsoleKey.F1)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Update(ref bmp);
                    Point pos = Cursor.Position;
                    Color pixel = bmp.GetPixel(pos.X, pos.Y);
                    string hex = GetHex(pixel);
                    Console.WriteLine(format,
                        pos.X,
                        pos.Y,
                        pixel.R,
                        pixel.G,
                        pixel.B,
                        hex);
                }
                // Wait 3 seconds then get color
                else if (key.Key == ConsoleKey.F2)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Point pos = Cursor.Position;
                    for (int i = 3; i > 0; i--)
                    {
                        Console.WriteLine("{0} at {1},{2}", i, pos.X, pos.Y);
                        Thread.Sleep(1000);
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }
                    Update(ref bmp);
                    Color pixel = bmp.GetPixel(pos.X, pos.Y);
                    string hex = GetHex(pixel);
                    Console.WriteLine(format,
                        pos.X,
                        pos.Y,
                        pixel.R,
                        pixel.G,
                        pixel.B,
                        hex);
                }
                else if (key.Key == ConsoleKey.F3)
                {
                    if (imageEditorLocation == null)
                        continue;

                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.WriteLine("Opening " + imageEditorLocation);
                    Update(ref bmp);
                    bmp.Save(tempImageLocation);
                    Process.Start(imageEditorLocation, tempImageLocation);
                }
                // RGB to hex
                else if (key.Key == ConsoleKey.F4)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("R,G,B: ");
                    string[] input = Console.ReadLine().Replace(" ", "").Split(',');
                    if (input.Length > 0)
                        if (input[0].ToLower() == "back")
                        {
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            ClearCurrentConsoleLine();
                            continue;
                        }
                    int[] rgb = new int[3];
                    if (input.Length == 3)
                    {
                        bool calc = true;
                        for (int i = 0; i < input.Length && calc; i++)
                        {
                            if (int.TryParse(input[i], out int value) && value <= 255 && value >= 0)
                                rgb[i] = value;
                            else
                                calc = false;
                        }
                        if (calc)
                            Console.WriteLine("Hex: {0}", GetHex(Color.FromArgb(rgb[0], rgb[1], rgb[2])));
                        else
                            Console.WriteLine("Error: Invalid RGB format");
                    }
                }
                // Hex to RGB
                else if (key.Key == ConsoleKey.F5)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("Hex: ");
                    string hex = Console.ReadLine().Replace(" ", "").Replace("#", "");
                    if (hex == "back")
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        ClearCurrentConsoleLine();
                        continue;
                    }
                    Color? color = GetRGB(hex);
                    if (color == null)
                        Console.WriteLine("Error: Invalid hex format");
                    else
                        Console.WriteLine("RGB: {0},{1},{2}", color.Value.R, color.Value.G, color.Value.B);
                }
                // Clear console
                else if (key.Key == ConsoleKey.F6)
                {
                    Console.Clear();
                    Console.WriteLine(consolewrite);
                }
                else
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                }
            }
        }

        static void Update(ref Bitmap bmp)
        {
            var gfxScreenshot = Graphics.FromImage(bmp);
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.Left, Screen.PrimaryScreen.Bounds.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            gfxScreenshot.Dispose();
        }

        static string GetHex(Color color)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        static Color? GetRGB(string hex)
        {
            if (hex.Length == 6)
            {
                try
                {
                    int[] rgb = new int[3];
                    for (int i = 0, h = 0; i < 3; i++, h += 2)
                        rgb[i] = Convert.ToInt32(hex[h].ToString() + hex[h + 1].ToString(), 16);
                    return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
                }
                catch (FormatException)
                {
                    return null;
                }
            }
            else return null;
        }
        
        static void ClearCurrentConsoleLine()
        {
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
        }
    }
}
