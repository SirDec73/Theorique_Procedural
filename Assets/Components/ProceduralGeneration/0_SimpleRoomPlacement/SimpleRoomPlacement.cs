using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        
        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Declare variables here
            // ........

            int spacing = 1;
            int minWidth = 4;
            int maxWidth = 11;
            int minLenght = 4;
            int maxLenght = 11;

            int nbRoom = 0;

            for (int i = 0; i < _maxSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Your algorithm here
                // .......

                if (nbRoom >= _maxRooms)
                    break;


                Vector2Int positionRoom = new Vector2Int(RandomService.Range(0, Grid.Width), RandomService.Range(0, Grid.Lenght));
                Vector2Int sizeRoom = new Vector2Int(RandomService.Range(minWidth, maxWidth), RandomService.Range(minLenght, maxLenght));

                RectInt room = new RectInt(positionRoom,sizeRoom);

                Debug.Log(i);

                if(CanPlaceRoom(room, spacing))
                {
                    CreateRoom(room);
                    nbRoom++;
                }

                // Waiting between steps to see the result.
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }
            
            // Final ground building.
            BuildGround();
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
}