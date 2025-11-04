using UnityEngine;
using VTools.RandomService;

public class BSP_Node
{
    int _spacing;
    RectInt rectInt;

    readonly RandomService _randomService;

    BSP_Node _parent;
    (BSP_Node,BSP_Node) _childLeaf;

    RectInt _associateRoom;

    public BSP_Node(int spacing, RectInt size, RandomService rs, BSP_Node parent = null)
    {
        _spacing = spacing;
        rectInt = size;
        _randomService = rs;
        _parent = parent;
    }

    public (BSP_Node, BSP_Node) SplitNode()
    {
        RectInt child1, child2;

        bool isHorizontalCut = _randomService.Chance(0.5f);

        int positionCut;


        if (isHorizontalCut)
        {
            positionCut = _randomService.Range(rectInt.y + rectInt.height/3 + _spacing, rectInt.y + rectInt.height * 2 / 3 + _spacing);


            child1 = new RectInt(rectInt.x, rectInt.y, rectInt.width, positionCut - rectInt.y);
            child2 = new RectInt(rectInt.x, positionCut, rectInt.width, rectInt.y + rectInt.height - positionCut);
        }
        else
        {
            positionCut = _randomService.Range(rectInt.x + rectInt.width/3 + _spacing, rectInt.x + rectInt.width * 2 / 3 + _spacing);
            child1 = new RectInt(rectInt.x, rectInt.y, positionCut - rectInt.x, rectInt.height);
            child2 = new RectInt(positionCut, rectInt.y, rectInt.x + rectInt.width - positionCut, rectInt.height);
        }

        _childLeaf.Item1 = new BSP_Node(_spacing, child1,_randomService,this);
        _childLeaf.Item2 = new BSP_Node(_spacing, child2,_randomService,this);

        Debug.Log($"isHorizontalCut = {isHorizontalCut}");
        Debug.LogWarning($"CutPosition = {positionCut}");
        Debug.LogWarning($"Child 1 = {child1}");
        Debug.LogWarning($"Child2 = {child2}");

        return _childLeaf;
    }


    


}
