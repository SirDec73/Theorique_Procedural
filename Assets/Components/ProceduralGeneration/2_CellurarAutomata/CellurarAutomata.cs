using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

public enum TypeTile
{
    None,
    Water,
    Ground
}

[CreateAssetMenu(menuName = "Procedural Generation Method/Cellurar Automata")]
public class CellurarAutomata : ProceduralGenerationMethod
{
    [SerializeField] float noiseDensity = 0.5f;
    [SerializeField] int nbGeneration = 4;
    [SerializeField] int CountChange = 4;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {

        GenerateNoise();


        for (int i = 0; i < nbGeneration; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<List<string>> tmpGrid = new List<List<string>>();

            for (int y = 0; y < Grid.Lenght; y++)
            {
                tmpGrid.Add(new List<string>());
                for (int x = 0; x < Grid.Width; x++)
                { 
                    string tile = CheckAndAssignNewTile(x, y);
                    tmpGrid[y].Add(tile);
                }
            }

            for (int y = 0; y < Grid.Lenght; y++)
            {
                for (int x = 0; x < Grid.Width; x++)
                {
                    AssignNewType(x, y, tmpGrid[y][x]);
                }
            }

            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }

    }

    private string CheckAndAssignNewTile(int x, int y)
    {

        Dictionary<TypeTile, int> types = new Dictionary<TypeTile, int>() {
            {TypeTile.None,0 },
            {TypeTile.Ground,0 },
            {TypeTile.Water,0 }
        };

        types[CheckTileName(x - 1, y - 1)] += 1;
        types[CheckTileName(x - 1, y)] += 1;
        types[CheckTileName(x - 1, y + 1)] += 1;

        types[CheckTileName(x, y + 1)] += 1;
        types[CheckTileName(x, y - 1)] += 1;

        types[CheckTileName(x + 1, y - 1)] += 1;
        types[CheckTileName(x + 1, y)] += 1;
        types[CheckTileName(x + 1, y + 1)] += 1;

        if (types[TypeTile.Ground] >= CountChange)
        {
            return GRASS_TILE_NAME;
        }
        else if (types[TypeTile.Water] >= CountChange)
        {
            return WATER_TILE_NAME;
        }
        else
        {
            Grid.TryGetCellByCoordinates(x, y, out var cell);
            return cell.GridObject.Template.Name;
        }

    }

    TypeTile CheckTileName(int posX,int posY)
    {
        if (Grid.TryGetCellByCoordinates(posX, posY, out Cell cell))
        {
            if (cell.GridObject.Template.Name == GRASS_TILE_NAME)
            {
                return TypeTile.Ground;
            }
            else
            {
                return TypeTile.Water;
            }
        }
        return TypeTile.None;
    }


    void GenerateNoise()
    {
        for (int i = 0; i < Grid.Lenght; i++)
        {
            for (int j = 0; j < Grid.Width; j++)
            {
                bool isGround = RandomService.Chance(noiseDensity);
                if (isGround)
                {
                    CreateGroundCell(i, j);
                }
                else
                {
                    CreateWaterCell(i, j);
                }
            }
        }
    }

    void CreateWaterCell(int posX, int posY)
    {
        Grid.TryGetCellByCoordinates(posX, posY, out var cell);
        AddTileToCell(cell, WATER_TILE_NAME, true);
    }
    void CreateGroundCell(int posX, int posY)
    {
        Grid.TryGetCellByCoordinates(posX, posY, out var cell);
        AddTileToCell(cell, GRASS_TILE_NAME, true);
    }

    void AssignNewType(int posX, int posY, string tileName)
    {
        Grid.TryGetCellByCoordinates(posX, posY, out var cell);
        AddTileToCell(cell, tileName, true);
    }

}
