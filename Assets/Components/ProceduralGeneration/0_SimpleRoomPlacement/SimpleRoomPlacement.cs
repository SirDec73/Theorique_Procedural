using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Unity.VisualScripting;
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
            int minWidth = 5;
            int maxWidth = 12;
            int minLenght = 5;
            int maxLenght = 12;

            int nbRoom = 0;

            List<RectInt> rooms = new List<RectInt>();

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
                    rooms.Add(room);
                    nbRoom++;
                }

                // Waiting between steps to see the result.
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }

            rooms.Sort((a,b) => a.xMin.CompareTo(b.xMin));

            for(int i = 0; i < nbRoom-1; i++)
            {
                Vector2 centerRoom1 = rooms[i].center;
                Vector2 centerRoom2 = rooms[i+1].center;
                CreateCorridor(centerRoom1, centerRoom2);
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
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

        private void CreateCorridor(Vector2 room1, Vector2 room2)
        {
            int distanceX = (int)Mathf.Round(room2.x - room1.x);
            int distanceY = (int)Mathf.Round(room2.y - room1.y);

            int directionX = distanceX > 0 ? 1 : -1;
            int directionY = distanceY > 0 ? 1 : -1;


            for (int x = 0; x < Mathf.Abs(distanceX); x++)
            {
                Grid.TryGetCellByCoordinates((int)room1.x + x*directionX, (int)room1.y, out var cell);
                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
            for (int y = 0; y < Mathf.Abs(distanceY); y++)
            {
                Grid.TryGetCellByCoordinates((int)room1.x + (int)distanceX, (int)room1.y + y * directionY, out var cell);
                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
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