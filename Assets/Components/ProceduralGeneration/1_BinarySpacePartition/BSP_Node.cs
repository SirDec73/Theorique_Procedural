using UnityEngine;
using VTools.RandomService;

public class BSP_Node
{
    int _minRoomSize;
    RectInt rectInt;
    const int _spacing = 1;

    readonly RandomService _randomService;

    BSP_Node _parent;
    (BSP_Node,BSP_Node) _childLeaf;
    public (BSP_Node, BSP_Node) ChildLeaf { get { return _childLeaf; } }

    RectInt _associateRoom;
    public RectInt Room { get { return _associateRoom; } }
    public RectInt AssociateRoom { set { _associateRoom = value; } }

    public BSP_Node(int minRoomSize, RectInt size, RandomService rs, BSP_Node parent = null)
    {
        _minRoomSize = minRoomSize;
        rectInt = size;
        _randomService = rs;
        _parent = parent;
    }

    public (BSP_Node, BSP_Node) SplitNode()
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

        _childLeaf.Item1 = new BSP_Node(_minRoomSize, children.Item1,_randomService,this);
        _childLeaf.Item2 = new BSP_Node(_minRoomSize, children.Item2,_randomService,this);

        Debug.LogWarning($"CutPosition = {positionCut}");
        Debug.LogWarning($"Child 1 = {children.Item1}");
        Debug.LogWarning($"Child2 = {children.Item2}");

        return _childLeaf;
    }


    public (int, int) CanHorizontalCut()
    {
        int minDistanceLeft = rectInt.y + _minRoomSize + _spacing * 2;
        int minDistanceRight = rectInt.y + rectInt.height - (_minRoomSize + _spacing * 2);
        if (minDistanceLeft > minDistanceRight)
        {
            return (-1, -1);
        }
        return (minDistanceLeft, minDistanceRight);
    }
    public (int, int) CanVerticalCut()
    {
        int minDistanceBottom = rectInt.x + _minRoomSize + _spacing * 2;
        int minDistanceTop = rectInt.x + rectInt.width - (_minRoomSize + _spacing * 2);
        if (minDistanceBottom > minDistanceTop)
        {
            return (-1, -1);
        }
        return (minDistanceBottom, minDistanceTop);
    }

    (RectInt, RectInt) GenerateSplitHorizontal(int positionCut)
    {
        return (new RectInt(rectInt.x, rectInt.y, rectInt.width, positionCut - rectInt.y),
                new RectInt(rectInt.x, positionCut, rectInt.width, rectInt.y + rectInt.height - positionCut));
    }
    (RectInt, RectInt) GenerateSplitVertical(int positionCut)
    {
        return (new RectInt(rectInt.x, rectInt.y, positionCut - rectInt.x, rectInt.height),
                new RectInt(positionCut, rectInt.y, rectInt.x + rectInt.width - positionCut, rectInt.height));
    }

    public RectInt GenerateRoomData()
    {
        int sizeX = _randomService.Range(_minRoomSize, rectInt.width - 2 *_spacing);
        int sizeY = _randomService.Range(_minRoomSize, rectInt.height - 2 * _spacing);

        int startPositionX = rectInt.position.x + _randomService.Range(0, rectInt.width - sizeX - 2 * _spacing);
        int startPositionY = rectInt.position.y + _randomService.Range(0, rectInt.height - sizeY - 2 * _spacing);

        _associateRoom = new RectInt(startPositionX + _spacing, startPositionY + _spacing, sizeX, sizeY);
        //_associateRoom = new RectInt(rectInt.position.x, rectInt.position.y, rectInt.width, rectInt.height);

        Debug.Log("associateRoom : " + _associateRoom);

        return _associateRoom;
    }

}
