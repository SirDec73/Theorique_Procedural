# Theorique Generation Procedurale

<details>
<summary>Details</summary>

  - [Introduction](#introduction)
    - [Grid](#grid)
    - [Cell](#cell)
    - [Random Service](#random-service)
    - [Procedural Generation Method](#procedural-generation-method)
  - [Simple Room Placement](#simple-room-placement)
  - [BSP](#bsp)
  - [Cellurar Automata](#cellurar-automata)
  - [Noise](#noise)

</details>

<br>

## Introduction

Ce github montre les bases de la génération procédural avec des exemples et un moyen de recréer des environnements simple avec différents scripts.

La génération procédurale est un outil nous permettant de générer du contenu basé sur des règles simple ou complexe, avec des paramètres prédéfinis, nous permettant de contrôlée l'aléatoire.

### Grid

La Grille représente notre espace de création.
```csharp
private readonly Cell[,] _gridArray;       
private readonly List<Cell> _cells;

public Vector3 OriginPosition { get; }
public float CellSize { get; }
public int Width { get; }
public int Lenght { get; }
public IReadOnlyList<Cell> Cells => _cells;
```
Elle possède une taille, une origine et des Cell représentants des cellules.

A l'initialisation, des cells vide sont créées sur l'entièreté de la grille

```csharp
public bool TryGetCellByCoordinates(Vector2Int coordinates, out Cell foundCell)
{
  var cellFound = TryGetCellByCoordinates(coordinates.x, coordinates.y, out Cell cell);
  foundCell = cell;

  return cellFound;
}
```
TryGetCellByCoordinates nous permet de récupérer, si elle existe, une cellule avec toute c'est donnée pour nous permettre de la modifier, récupérer des informations pour les comparer avec d'autre cellule, etc.


### Cell

La Cell (cellule) est un bloc qui vient entré dans la grille pour la completer et la faire exister.

```csharp
private readonly float _size;
private Tuple<GridObject, GridObjectController> _object;
        
public Vector2Int Coordinates { get; }
public bool ContainObject => _object != null;
public GridObject GridObject => _object.Item1;
public GridObjectController View => _object.Item2;
```
Chaque Cell a une taille, une coordonnée ainsi qu'un GridObject et GridObjectController.

Le GridObject nous permet d'accèder à des informations sur la cellule comme son nom ou son angle de rotation

Le GridObjectController nous permet d'appliquer des movements aux cells comme une MoveTo avec une localisation ou une Rotation avec un angle


### Random Service

Le RandomService est une méthode qui ce base sur un nombre (une seed) pour générer de l'aléatoire un évènement avec exactement la même seed aura toujours le même résultat à l'instant t

```csharp
public int   Range(int minInclusive,   int maxExclusive)
public float Range(float minInclusive, float maxInclusive)
public bool  Chance(float probability)
public T     Pick<T>(T[] array)
```
Range()  => Retourne un nombre dans la plage spécifié

Chance() => Génère un nombre floatant entre 0f et 1f et retourne true si le nombre est inférieur à la probabilité (plus la probabilité est élevé, plus "true" aura de chance de sortir)

Pick()   => Retourne un élément aléatoire dans une liste ou un array


### Procedural Generation Method

Un scriptableObject abstrait qui est la base avec laquelle on travail pour faire de la generation procedural. 

```csharp
[Header("Generation")] 
[SerializeField] protected int _maxSteps = 1000;

// Injected at runtime, not serialized
[NonSerialized] protected ProceduralGridGenerator GridGenerator;
[NonSerialized] protected RandomService RandomService;
[NonSerialized] private CancellationTokenSource _cancellationTokenSource;

protected VTools.Grid.Grid Grid => GridGenerator.Grid;
        
protected const string ROOM_TILE_NAME = "Room";
protected const string CORRIDOR_TILE_NAME = "Corridor";
protected const string GRASS_TILE_NAME = "Grass";
protected const string WATER_TILE_NAME = "Water";
protected const string ROCK_TILE_NAME = "Rock";
protected const string SAND_TILE_NAME = "Sand";
```
On indique le nombre de max step/generation que l'on veut réaliser.

On retrouve un RandomService, des constants pour les nom des prefabs et un ProceduralGridGenerator.

Dans ProceduralGridGenerator, un enfant de BaseGridGenerator, on retrouve la seed de notre random et la grid en elle même avec une fonction pour la générer

```csharp
protected abstract UniTask ApplyGeneration(CancellationToken cancellationToken);
```
C'est cette fonction que nous allons override pour faire notre generation procedural.

```csharp
protected bool CanPlaceRoom(RectInt room, int spacing)
```
Vérifie si une salle rectangulaire avec un spacing peut rentré dans la grille.

Il y a une vérification pour le dépassement de la grille mais aussi si une salle est déjà présente dans l'empalcement de la salle

```csharp
protected void AddTileToCell(Cell cell, string tileName, bool overrideExistingObjects)
```
Ajoute un visuel a une cell avec le tileName, un nom de prefab dans ProceduralGenerationMethod.

**ATTENTION** cell doit être une cellule présente dans la grille :
```csharp
if (Grid.TryGetCellByCoordinates(i, j, out var cell))
  AddTileToCell(cell, ROOM_TILE_NAME, false);
```

<br>

## Simple Room Placement

L'objectif de la SimpleRoomPlacement est de créer une salle de taille aléatoire et de regarder si la salle rentre dans la grille sans quel soit en conflit avec une autre salle.

### Get Started

```csharp
[CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
public class SimpleRoomPlacement : ProceduralGenerationMethod
```
Votre classe doit hériter de ProceduralGenerationMethod

CreateAssetMenu pour pouvoir créer un scriptable object de cette classe

Voici un exemple simple de création de salle :
```csharp
protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
  {
    // Declare variables here
    // ........
    for (int i = 0; i < _maxSteps; i++)
    {
        // Check for cancellation
        cancellationToken.ThrowIfCancellationRequested();


        // Your algorithm here
        // .......

        int spacing = 1;
        int minWidth = 5;
        int maxWidth = 12;
        int minLenght = 5;
        int maxLenght = 12;

        Vector2Int positionRoom = new Vector2Int(RandomService.Range(0, Grid.Width), RandomService.Range(0, Grid.Lenght));
        Vector2Int sizeRoom = new Vector2Int(RandomService.Range(minWidth, maxWidth), RandomService.Range(minLenght, maxLenght));

        RectInt room = new RectInt(positionRoom,sizeRoom);

        if(CanPlaceRoom(room, spacing))
        {
            CreateRoom(room);
        }

        // Waiting between steps to see the result.
        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
    }

    // Final ground building.
    BuildGround();
}
```

On créer des coordonnées et une taille aléatoire grâce au RandomService que l'on utilise pour créer un RectInt (une salle)

On vérifie si la salle passe dans la grille et si c'est le cas alors on créer la salle dans la grile

Après avoir fini les steps on créer le sol avec BuildGround pour mettre du sol là où il n'y a pas de salle

<br>
Dans le cas où l'on veut rajouter un couloir entre deux salle il est possible de le faire avec 
`CreateCorridor(Vector2 room1, Vector2 room2)`

room1 => centre de la salle numero 1

room2 => centre de la salle numero 2

<br>

## BSP

Le BSP (Binary Split Partition) est une manière plus obtimiser de créer un donjon avec des salles.

L'objectif est de séparer la grille plusieurs fois à des endroits différents pour pouvoir y insérer une salle.

![image](https://github.com/SirDec73/Theorique_Procedural/blob/main/ImageGit/BSP_Split.png)
![image](https://github.com/SirDec73/Theorique_Procedural/blob/main/ImageGit/BSP_AddRoom.png)

Chaque salles sont relier entre elle par leur séparation se qui permet de créer des couloirs plus facilement.

![image](https://github.com/SirDec73/Theorique_Procedural/blob/main/ImageGit/BSP_FirstCorridor.png)
![image](https://github.com/SirDec73/Theorique_Procedural/blob/main/ImageGit/BSP_AddCorridor.png)

### Get Started

<br>

## Cellurar Automata


<br>

## Noise
