namespace DreamScape.ImgGeneration;

using Godot;
using System;
using System.Collections.Generic;



/// <summary>
/// uses the FastNoiseLite library to generate noise that is the n used to
/// create an abstract img
/// </summary>
public partial class ImgGenerator : Node2D
{
    public const float VisibleTime = 12f;
    public const float NeighborDelay = 0.2f;
    public const float AlphaChangeAmt = 0.1f;
    public const float AlphaChangeDelay = 0.5f;

    private readonly string[] LightColors = new string[] {
        "#FFD700", "#FFA500", "#FF8C00", "#FF7F50", "#FF6347", "#FF4500", "#FF0000", "#000000", "#2F4F4F", "#708090", "#696969", "#A9A9A9", "#808080", "#C0C0C0"
         };
    private readonly string[] DarkColors = new string[] { "#000000", "#2F4F4F", "#708090", "#696969", "#A9A9A9", "#808080", "#C0C0C0" };

    private FastNoiseLite noiseGenerator;
    private Color[] colorPalette;
    private Dictionary<Vector2, Tile> tiles;
    private Dictionary<Vector2, Tile> fadingTiles;

    private Vector2I cellSize;

    public ImgGenerator(Vector2 ImgSize)
    {
        noiseGenerator = new FastNoiseLite();
        tiles = new Dictionary<Vector2, Tile>();
        cellSize = new Vector2I(32, 32);
        this.ImgSize = ImgSize;
        colorPalette = new Color[8];
        RandomizePalette(false);

        //center img based on size
        Position = new Vector2((-ImgSize.X * cellSize.X / 2) + (cellSize.X * .5f), (-ImgSize.Y * cellSize.Y / 2) + (cellSize.Y * .5f));

        Generating = false;
        Generated = false;
        Fading = false;
    }

    public bool Generating { get; private set; }
    public bool Generated { get; private set; }
    public bool Fading { get; set; }

    /// <summary>
    /// the noise generator used to create the img
    /// </summary>
    public FastNoiseLite NoiseGenerator { get => noiseGenerator; set => noiseGenerator = value; }

    /// <summary>
    /// determines how many tile will be used to make the width a
    /// nd height of  the dream scape img
    /// </summary>
    public Vector2 ImgSize { get; set; }


    /// <summary>
    /// generates a dream scape img form the noise map and other settings
    /// </summary>
    public void Generate()
    {
        if (!Generating && !Generated && !Fading)
        {
            Generating = true;
            GD.Print("Generating new img");

            for (int x = 0; x < ImgSize.X; x++)
            {
                for (int y = 0; y < ImgSize.Y; y++)
                {
                    Vector2 tileCords = new Vector2(x * cellSize.X, y * cellSize.Y);
                    Vector2 cords = new Vector2(x, y);

                    Color tileColor = GetColorFromHeight(noiseGenerator.GetNoise2D(x, y));
                    Tile tile = new Tile(this, tileCords, cellSize, tileColor, AlphaChangeAmt, AlphaChangeDelay, NeighborDelay, VisibleTime);
                    tile.FindNeighbors(tiles);

                    tiles.Add(cords, tile);
                    AddChild(tile);
                }
            }

            Generating = false;
            Generated = true;
        }
    }

    /// <summary>
    /// clears the current img and generates a new one
    /// </summary>
    public void Clear()
    {
        if (Generated && !Fading)
        {
            for (int x = 0; x < ImgSize.X; x++)
            {
                for (int y = 0; y < ImgSize.Y; y++)
                {
                    Tile tile = tiles[new Vector2(x, y)];
                    RemoveChild(tile);
                    tile.QueueFree();
                }
            }
            tiles.Clear();
            Generated = false;
        }
    }

    /// <summary>
    /// starts the fade effect on a random tile
    /// </summary>
    public void StartFadeEffect()
    {
        if (!Fading && Generated)
        {
            Fading = true;
            fadingTiles = new Dictionary<Vector2, Tile>(tiles);

            GD.Print($"starting Fade");

            RandomNumberGenerator rng = new RandomNumberGenerator();
            int xCoord = rng.RandiRange(0, (int)ImgSize.X - 1);
            int yCoord = rng.RandiRange(0, (int)ImgSize.Y - 1);

            Vector2 cord = new Vector2(xCoord, yCoord);

            if (tiles.ContainsKey(cord))
            {
                tiles[cord].StartFadeEffect(true);
            }
            else
            {
                GD.PrintErr($"Tile at {cord} not found");
            }
        }

    }

    /// <summary>
    /// used to remove a tile from the fading tiles list if it's done
    /// </summary>
    public void RemoveTileFromFadingList(Vector2 coord)
    {
        fadingTiles.Remove(coord);
    }

    /// <summary>
    /// runs through all currently tiles that aren't done fading in/out
    /// </summary>
    /// <returns></returns>
    public bool IsFadeEffectDone()
    {
        foreach (var tile in fadingTiles.Values)
        {
            if (tile.IsFading)
            {
                return false;
            }
        }
        return true;

    }

    /// <summary>
    /// gets tile color from current value in noise generator
    /// </summary>
    private Color GetColorFromHeight(float height)
    {
        if (height < -0.75f)
        {
            return colorPalette[0];
        }
        else if (height < -0.5f)
        {
            return colorPalette[1];
        }
        else if (height < -0.25f)
        {
            return colorPalette[2];
        }
        else if (height < 0.0f)
        {
            return colorPalette[3];
        }
        else if (height < 0.25f)
        {
            return colorPalette[4];
        }
        else if (height < 0.5f)
        {

            return colorPalette[5];
        }
        else if (height < 0.75f)
        {
            return colorPalette[6];
        }
        else if (height < 1f)
        {
            return colorPalette[7];
        }
        return new Color(height, height, height, 0);
    }

    /// <summary>
    /// randomizes the color palate of the img
    /// </summary>
    public void RandomizePalette(bool darkPalette)
    {
        string[] colors = darkPalette ? DarkColors : LightColors;
        RandomNumberGenerator rng = new RandomNumberGenerator();

        for (int i = 0; i < colorPalette.Length; i++)
        {
            int colorIndex = rng.RandiRange(0, colors.Length - 1);
            Color color = new Color(colors[colorIndex]);
            color.A = 0;
            colorPalette[i] = color;
        }
    }


}
