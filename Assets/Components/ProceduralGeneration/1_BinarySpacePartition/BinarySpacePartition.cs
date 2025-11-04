using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/Binary Space Partition")]
public class BinarySpacePartition : ProceduralGenerationMethod
{
    [SerializeField] int _partition;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        List<BSP_Node> nodes = new List<BSP_Node>();

        int spacing = 1;
        int minWidth = 5;
        int maxWidth = 12;
        int minLenght = 5;
        int maxLenght = 12;

        cancellationToken.ThrowIfCancellationRequested();

        RectInt rectInt = new RectInt(0,0,Grid.Width,Grid.Lenght);

        BSP_Node Root = new BSP_Node(spacing, rectInt, RandomService);

        GenerateNodes(2,Root);


        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);


        BuildGround();
    }

    public void GenerateNodes(int nbCut, BSP_Node nodeToSplit)
    {
        (BSP_Node, BSP_Node) childrenNode;
        if (nbCut > 0)
        {
            childrenNode = nodeToSplit.SplitNode();
            GenerateNodes(nbCut - 1, childrenNode.Item1);
            GenerateNodes(nbCut - 1, childrenNode.Item2);
        }
    }

    private void CreateRoom(RectInt room)
    {
        for (int i = room.xMin; i < room.xMax; i++)
        {
            for (int j = room.yMin; j < room.yMax; j++)
            {
                if (Grid.TryGetCellByCoordinates(i, j, out var cell))
                    AddTileToCell(cell, ROOM_TILE_NAME, false);
            }
        }
    }

    private void BuildGround()
    {
        var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");

        // Instantiate ground blocks
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                {
                    Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                    continue;
                }

                GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
            }
        }
    }

}
