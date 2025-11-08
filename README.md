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
Vérifie si une salle rectangulaire peut rentré dans la grille

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


<br>

## BSP


<br>

## Cellurar Automata


<br>

## Noise
