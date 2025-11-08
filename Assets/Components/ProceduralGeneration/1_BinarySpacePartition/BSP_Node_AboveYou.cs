using UnityEngine;
using VTools.RandomService;

public class BSP_Node_AboveYou
{
    // Extern DATA
    VTools.Grid.Grid _grid;
    readonly RandomService _randomService;

    // Node DATA
    RectInt _node;
    int _nodeLimitSize;
    (BSP_Node_AboveYou, BSP_Node_AboveYou) _childLeaf;

    // Room DATA
    RectInt _room;
    Vector2Int _roomLimitSize;
    const int _spacing = 1;

    // GETTER ___ SETTER
    public RectInt Node { get { return _node; } }
    public (BSP_Node_AboveYou, BSP_Node_AboveYou) ChildLeaf { get { return _childLeaf; } }
    public RectInt Room { get { return _room; } set { _room = value; } }


    public BSP_Node_AboveYou(Vector2Int roomLimitSize, int nodeLimitSize, RectInt nodeRect, VTools.Grid.Grid grid, RandomService rs)
    {
        _roomLimitSize = roomLimitSize;
        _nodeLimitSize = nodeLimitSize; 
        _node = nodeRect;
        _grid = grid;
        _randomService = rs;
    }

    public (BSP_Node_AboveYou, BSP_Node_AboveYou) SplitNode()
    {
        (RectInt, RectInt) children;

        bool isHorizontalCut = _randomService.Chance(0.5f);

        int positionCut;

        (int, int) minDistance;

        if (isHorizontalCut)
        {
            minDistance = CanHorizontalCut();

            if (minDistance.Item1 < 0)
            {
                minDistance = CanVerticalCut();
                if (minDistance.Item1 < 0)
                {
                    Debug.Log($"IMPOSSIBLE TO SPLIT NODE !");
                    return (null,null);
                }
                Debug.Log($"isHorizontalCut = {!isHorizontalCut}");
                positionCut = _randomService.Range(minDistance.Item1, minDistance.Item2+1);
                children = GenerateSplitVertical(positionCut);
            }
            else
            {
                Debug.Log($"isHorizontalCut = {isHorizontalCut}");
                positionCut = _randomService.Range(minDistance.Item1, minDistance.Item2+1);
                children = GenerateSplitHorizontal(positionCut);
            }
        }
        else
        {
            minDistance = CanVerticalCut();

            if (minDistance.Item1 < 0)
            {
                minDistance = CanHorizontalCut();
                if (minDistance.Item1 < 0)
                {
                    Debug.Log($"IMPOSSIBLE TO SPLIT NODE !");
                    return (null, null);
                }
                Debug.Log($"isHorizontalCut = {!isHorizontalCut}");
                positionCut = _randomService.Range(minDistance.Item1, minDistance.Item2 + 1);
                children = GenerateSplitHorizontal(positionCut);
            }
            else
            {
                Debug.Log($"isHorizontalCut = {isHorizontalCut}");
                positionCut = _randomService.Range(minDistance.Item1, minDistance.Item2+1);
                children = GenerateSplitVertical(positionCut);
            }
        }

        _childLeaf.Item1 = new BSP_Node_AboveYou(_roomLimitSize, _nodeLimitSize, children.Item1, _grid, _randomService);
        _childLeaf.Item2 = new BSP_Node_AboveYou(_roomLimitSize, _nodeLimitSize, children.Item2, _grid, _randomService);

        Debug.LogWarning($"CutPosition = {positionCut}");
        Debug.LogWarning($"Child 1 = {children.Item1}");
        Debug.LogWarning($"Child2 = {children.Item2}");

        return _childLeaf;
    }


    public (int, int) CanHorizontalCut()
    {
        int minDistanceLeft = _node.y + _nodeLimitSize + _spacing * 2;
        int minDistanceRight = _node.y + _node.height - (_nodeLimitSize + _spacing * 2);
        if (minDistanceLeft > minDistanceRight)
        {
            return (-1, -1);
        }
        return (minDistanceLeft, minDistanceRight);
    }
    public (int, int) CanVerticalCut()
    {
        int minDistanceBottom = _node.x + _nodeLimitSize + _spacing * 2;
        int minDistanceTop = _node.x + _node.width - (_nodeLimitSize + _spacing * 2);
        if (minDistanceBottom > minDistanceTop)
        {
            return (-1, -1);
        }
        return (minDistanceBottom, minDistanceTop);
    }

    (RectInt, RectInt) GenerateSplitHorizontal(int positionCut)
    {
        return (new RectInt(_node.x, _node.y, _node.width, positionCut - _node.y),
                new RectInt(_node.x, positionCut, _node.width, _node.y + _node.height - positionCut));
    }
    (RectInt, RectInt) GenerateSplitVertical(int positionCut)
    {
        return (new RectInt(_node.x, _node.y, positionCut - _node.x, _node.height),
                new RectInt(positionCut, _node.y, _node.x + _node.width - positionCut, _node.height));
    }

    public RectInt GenerateRoomData()
    {
        int sizeX = _randomService.Range(_roomLimitSize.x, _roomLimitSize.y);
        int sizeY = _randomService.Range(_roomLimitSize.x, _roomLimitSize.y);

        int startPositionX = _node.position.x + _randomService.Range(0, _node.width - sizeX - _spacing);
        int startPositionY = _node.position.y + _randomService.Range(0, _node.height - sizeY - _spacing);

        _room = new RectInt(startPositionX, startPositionY, sizeX, sizeY);
        //_associateRoom = new RectInt(rectInt.position.x, rectInt.position.y, rectInt.width, rectInt.height);

        Debug.Log("associateRoom : " + _room);

        return _room;
    }

}
