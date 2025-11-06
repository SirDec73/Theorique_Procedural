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

[CreateAssetMenu(menuName = "Procedural Generation Method/Binary Space Partition")]
public class BinarySpacePartition : ProceduralGenerationMethod
{
    [SerializeField] int _partition;

    //[NonSerialized]int spacing = 1;
    [NonSerialized]int minSize = 5;
    //[NonSerialized]int maxSize = 12;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        List<BSP_Node> nodes = new List<BSP_Node>();

        cancellationToken.ThrowIfCancellationRequested();

        RectInt rectInt = new RectInt(0,0,Grid.Width,Grid.Lenght);

        BSP_Node Root = new BSP_Node(minSize, rectInt, RandomService);


        Debug.Log("=== GenerateNodes ===");
        GenerateNodes(_partition,Root);


        Debug.Log("=== GenerateRooms ===");
        GenerateNodesRooms(Root);
        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);


        Debug.Log("=== GenerateCorridor ===");
        GenerateNodesCorridor(Root);


        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);


        BuildGround();
    }

    public void GenerateNodes(int nbCut, BSP_Node nodeToSplit)
    {
        (BSP_Node, BSP_Node) childrenNode;
        if (nbCut > 0)
        {
            childrenNode = nodeToSplit.SplitNode();
            if(childrenNode.Item1 == null)
                return;
            GenerateNodes(nbCut - 1, childrenNode.Item1);
            GenerateNodes(nbCut - 1, childrenNode.Item2);
        }
    }
    public void GenerateNodesRooms(BSP_Node rootNode)
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

    private void GenerateNodesCorridor(BSP_Node node1)
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

        node1.AssociateRoom = node1.ChildLeaf.Item1.Room;
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
