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

[CreateAssetMenu(menuName = "Procedural Generation Method/BSP Above You")]
public class BSP_AboveYou : ProceduralGenerationMethod
{
    [SerializeField] int spacing = 1;

    [SerializeField] int nodeLimitSize = 6;

    [SerializeField] int minRoomSize = 3;
    [SerializeField] int maxRoomSize = 5;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Vector2Int limitRoomSize = new Vector2Int(minRoomSize,maxRoomSize);
        RectInt rectInt = new RectInt(0,0,Grid.Width,Grid.Lenght);

        BSP_Node_AboveYou Root = new BSP_Node_AboveYou(limitRoomSize, nodeLimitSize, rectInt, Grid, RandomService);


        Debug.Log("=== GenerateNodes ===");
        GenerateNodes(_maxSteps,Root);


        Debug.Log("=== GenerateRooms ===");
        GenerateNodesRooms(Root);
        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);


        Debug.Log("=== GenerateCorridor ===");
        GenerateNodesCorridor(Root);


        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);


        BuildGround();
    }

    public void GenerateNodes(int nbCut, BSP_Node_AboveYou nodeToSplit)
    {
        (BSP_Node_AboveYou, BSP_Node_AboveYou) childrenNode;
        if (nbCut > 0)
        {
            childrenNode = nodeToSplit.SplitNode();
            if(childrenNode.Item1 == null)
                return;
            GenerateNodes(nbCut - 1, childrenNode.Item1);
            GenerateNodes(nbCut - 1, childrenNode.Item2);
        }
    }
    public void GenerateNodesRooms(BSP_Node_AboveYou rootNode)
    {
        if (rootNode.ChildLeaf.Item1 != null)
        {
            GenerateNodesRooms(rootNode.ChildLeaf.Item1);
            GenerateNodesRooms(rootNode.ChildLeaf.Item2);
            return;
        }
        RectInt roomData = rootNode.GenerateRoomData();
        CreateRoom(roomData);
    }

    private void GenerateNodesCorridor(BSP_Node_AboveYou node1)
    {
        if(node1.ChildLeaf.Item1 != null)
        {
            GenerateNodesCorridor(node1.ChildLeaf.Item1);
            GenerateNodesCorridor(node1.ChildLeaf.Item2);
        }
        else
        {
            return;
        }

        Vector2 room1 = node1.ChildLeaf.Item1.Room.center;
        Vector2 room2 = node1.ChildLeaf.Item2.Room.center;

        int distanceX = (int)(room2.x - room1.x);
        int distanceY = (int)(room2.y - room1.y);

        int directionX = distanceX > 0 ? 1 : -1;
        int directionY = distanceY > 0 ? 1 : -1;

        CreateDogLegsCorridor(room1, distanceX, distanceY, directionX, directionY);

        node1.Room = node1.ChildLeaf.Item1.Room;
    }

    void CreateDogLegsCorridor(Vector2 room, int distanceX, int distanceY, int directionX, int directionY)
    {
        bool horizontalFirst = RandomService.Chance(0.5f);
        if (horizontalFirst)
        {
            CreateHorizontalCorridor(room, distanceX, directionX);
            CreateVerticalCorridor(room, distanceY, directionY, distanceX);
        }
        else
        {
            CreateVerticalCorridor(room, distanceY, directionY);
            CreateHorizontalCorridor(room, distanceX, directionX,distanceY);
        }
    }

    void CreateHorizontalCorridor(Vector2 room, int distanceX, int directionX, int directionY = 0)
    {
        for (int x = 0; x < Mathf.Abs(distanceX); x++)
        {
            Grid.TryGetCellByCoordinates((int)room.x + x * directionX, (int)room.y + directionY, out var cell);
            AddTileToCell(cell, CORRIDOR_TILE_NAME, false);
        }
    }
    void CreateVerticalCorridor(Vector2 room, int distanceY, int directionY, int distanceX = 0)
    {
        for (int y = 0; y < Mathf.Abs(distanceY); y++)
        {
            Grid.TryGetCellByCoordinates((int)room.x + distanceX, (int)room.y + y * directionY, out var cell);
            AddTileToCell(cell, CORRIDOR_TILE_NAME, false);
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
