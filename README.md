# Theorique Generation Procedurale

<details>
<summary>Details</summary>

  - [Introduction](#introduction)
    - [Grid](#grid)
    - [Cell](#cell)
    - [Procedural Generation Method](#procedural-generation-method)
  - [Simple Room Placement](#simple-room-placement)
  - [BSP](#BSP)
  - [Cellurar Automata](#cellurar-automata)
  - [Noise](#noise)

</details>

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
TryGetCellByCoordinates nous permet de récupérer, si elle existe, une cellule avec toute c'est donnée pour nous permettre de la modifier

### Cell

### Procedural Generation Method


## Simple Room Placement


## BSP


## Cellurar Automata


## Noise
