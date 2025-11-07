using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Procedural Generation Method/Terrain Noise")]
public class TerrainNoise : Noise
{

    [Header("Terrain")]
    [SerializeField] GameObject prefabTerrain;

    [SerializeField] int HEIGHT;

    Terrain terrain;
    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        noise = SetupNoise();

        terrain = prefabTerrain.GetComponent<Terrain>();

        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        Instantiate(terrain);

        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
    }


    TerrainData GenerateTerrain(TerrainData td)
    {
        td.size = new Vector3(Grid.Width, HEIGHT, Grid.Lenght);


        td.SetHeights(0, 0, GenerateHeight());
        return td;

    }

    float[,] GenerateHeight()
    {
        float[,] noiseData = new float[Grid.Width,Grid.Lenght];

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Lenght; y++) 
            {
                noiseData[x,y] = GetNoiseData(x, y);
                //noiseData[x,y] = CalculateHeight(x, y, noiseCoordinate);
            }
        }

        return noiseData;
    }

    //float CalculateHeight(int x, int y, float scale)
    //{
    //    float xCoord = (float)x / Grid.Width * 100 * scale;
    //    float yCoord = (float)y / Grid.Lenght * 100 * scale;
    //
    //    return Mathf.PerlinNoise(xCoord, yCoord);
    //
    //}
}