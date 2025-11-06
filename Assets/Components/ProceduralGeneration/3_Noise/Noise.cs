using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using VTools.Grid;

[CreateAssetMenu(menuName = "Procedural Generation Method/Noise")]
public class Noise : ProceduralGenerationMethod
{
    protected FastNoiseLite noise;

    [Header("NOISE DATA")]
    [SerializeField]                        protected FastNoiseLite.NoiseType noiseType;
    [SerializeField, Range(0.001f, 0.1f)]   protected float frequency = 0.010f;
    [SerializeField, Range(0.001f, 2f)]     protected float amplitude = 0.010f;

    [Header("FRACTAL NOISE")]
    [SerializeField]                        protected FastNoiseLite.FractalType fractalType;
    [SerializeField, Range(1, 10)]          protected int nbOctave = 1;
    [SerializeField, Range(0.01f, 10f)]     protected float lacunarity = 2f;
    [SerializeField, Range(0.001f, 10f)]    protected float persistance = 0.5f;

    [Header("HEIGHTS")]
    [SerializeField, Range(-1f, 1f)]        protected float sandHeight = -0.2f;
    [SerializeField, Range(-1f, 1f)]        protected float grassHeight = 0f;
    [SerializeField, Range(-1f, 1f)]        protected float rockHeight = 0.6f;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        noise = SetupNoise();

        // Gather noise data
        float noiseData;

        for (int x = 0; x < Grid.Lenght; x++)
        {
            for (int y = 0; y < Grid.Width; y++)
            {
                noiseData = GetNoiseData(x, y);

                string type;
                if (noiseData >= rockHeight)
                {
                    type = ROCK_TILE_NAME;
                }
                else if (noiseData >= grassHeight)
                {
                    type = GRASS_TILE_NAME;
                }
                else if (noiseData >= sandHeight)
                {
                    type = SAND_TILE_NAME;
                }
                else 
                { 
                    type = WATER_TILE_NAME;
                }

                AssignNewType(x,y, type);
            }
            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }
    }

    protected FastNoiseLite SetupNoise()
    {
        noise = new FastNoiseLite(GridGenerator.Seed);

        noise.SetNoiseType(noiseType);
        noise.SetFrequency(frequency);


        noise.SetFractalOctaves(nbOctave);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(persistance);
        noise.SetFractalType(fractalType);

        return noise;
    }

    protected float GetNoiseData(int x, int y) 
    {
        float noiseData = noise.GetNoise(x, y) * amplitude;
        return Mathf.Clamp(noiseData,-1,1);
    }

    protected void AssignNewType(int posX, int posY, string tileName)
    {
        Grid.TryGetCellByCoordinates(posX, posY, out var cell);
        AddTileToCell(cell, tileName, true);
    }
}
