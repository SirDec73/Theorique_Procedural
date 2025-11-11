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
  - [Remerciements](#remerciements)

</details>

<br>

## Introduction

Ce GitHub montre les bases de la **génération procédurale** avec des exemples et un moyen de recréer des environnements simples avec différents scripts.

La génération procédurale est **un outil** nous permettant de générer du contenu basé sur **des règles simples ou complexes**, avec des paramètres prédéfinis, nous permettant de **contrôler l'aléatoire**.

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
Elle possède une taille, une origine et des cellules représentant des cellules.

À l'initialisation, des cells vides sont créées sur l'entièreté de la grille.

```csharp
public bool TryGetCellByCoordinates(Vector2Int coordinates, out Cell foundCell)
{
  var cellFound = TryGetCellByCoordinates(coordinates.x, coordinates.y, out Cell cell);
  foundCell = cell;

  return cellFound;
}
```
TryGetCellByCoordinates nous permet de récupérer, si elle existe, une cellule avec toutes ses données pour nous permettre de la modifier, de récupérer des informations pour les comparer avec d'autres cellules, etc.


### Cell

La Cell (cellule) est un bloc qui vient entrer dans la grille pour la compléter et la faire exister.

```csharp
private readonly float _size;
private Tuple<GridObject, GridObjectController> _object;
        
public Vector2Int Coordinates { get; }
public bool ContainObject => _object != null;
public GridObject GridObject => _object.Item1;
public GridObjectController View => _object.Item2;
```
Chaque Cell a une taille, une coordonnée ainsi qu'un GridObject et un GridObjectController.

Le GridObject nous permet d'accéder à des informations sur la cellule comme son nom ou son angle de rotation.

Le GridObjectController nous permet d'appliquer des mouvements aux cells, comme un MoveTo avec une localisation ou une Rotation avec un angle.


### Random Service

Le RandomService est une méthode qui se base sur un nombre (une seed) pour générer de l'aléatoire. Un évènement avec exactement la même seed aura toujours le même résultat à l'instant t.

```csharp
public int   Range(int minInclusive,   int maxExclusive)
public float Range(float minInclusive, float maxInclusive)
public bool  Chance(float probability)
public T     Pick<T>(T[] array)
```
Range()  => Retourne un nombre dans la plage spécifiée.

Chance() => Génère un nombre flottant entre 0f et 1f et retourne true si le nombre est inférieur à la probabilité (plus la probabilité est élevée, plus "true" aura de chances de sortir).

Pick()   => Retourne un élément aléatoire dans une liste ou un array.

### Procedural Generation Method

Un scriptableObject abstrait qui est la base avec laquelle on travaille pour faire de la génération procédurale. 

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
On indique le nombre de max steps/generation que l'on veut réaliser.

On retrouve un RandomService, des constants pour le nom des prefabs et un ProceduralGridGenerator.

Dans ProceduralGridGenerator, un enfant de BaseGridGenerator, on retrouve la seed de notre random et la grid en elle-même avec une fonction pour la générer.

```csharp
protected abstract UniTask ApplyGeneration(CancellationToken cancellationToken);
```
C'est cette fonction que nous allons override pour faire notre génération procédurale.

```csharp
protected bool CanPlaceRoom(RectInt room, int spacing)
```
Vérifie si une salle rectangulaire avec un spacing peut rentrer dans la grille.

Il y a une vérification pour le dépassement de la grille, mais aussi si une salle est déjà présente dans l'emplacement de la salle.

```csharp
protected void AddTileToCell(Cell cell, string tileName, bool overrideExistingObjects)
```
Ajoute un visuel à une cell avec le tileName, un nom de prefab dans ProceduralGenerationMethod.

**ATTENTION** cell doit être une cellule présente dans la grille :

```csharp
if (Grid.TryGetCellByCoordinates(i, j, out var cell))
  AddTileToCell(cell, ROOM_TILE_NAME, false);
```

<br>

## Simple Room Placement

L'objectif de la SimpleRoomPlacement est de créer une salle de taille aléatoire et de regarder si la salle rentre dans la grille sans qu'elle soit en conflit avec une autre salle.

### Get Started

```csharp
[CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
public class SimpleRoomPlacement : ProceduralGenerationMethod
```

Votre classe doit hériter de ProceduralGenerationMethod.

CreateAssetMenu pour pouvoir créer un scriptable object de cette classe.

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

On crée des coordonnées et une taille aléatoire grâce au RandomService que l'on utilise pour créer un RectInt (une salle).

On vérifie si la salle passe dans la grille et, si c'est le cas, alors on crée la salle dans la grille.

Après avoir fini les steps, on crée le sol avec BuildGround pour mettre du sol là où il n'y a pas de salle.

<br>

Dans le cas où l'on veut rajouter un couloir entre deux salles, il est possible de le faire avec : ``CreateCorridor(Vector2 room1, Vector2 room2)``.

room1 => centre de la salle numéro 1

room2 => centre de la salle numéro 2

<br>

## BSP

Le BSP (Binary Space Partition) est une manière plus optimisée de créer un donjon avec des salles.

L'objectif est de séparer la grille plusieurs fois à des endroits différents pour pouvoir y insérer une salle.

Ces séparations donnent des Childs, puis des Leafs s'il s'agit du dernier enfant.

![image](https://github.com/SirDec73/Theorique_Procedural/blob/main/ImageGit/BSP_Split.png)
![image](https://github.com/SirDec73/Theorique_Procedural/blob/main/ImageGit/BSP_AddRoom.png)

Chaque salle est reliée entre elle par sa séparation, ce qui permet de créer des couloirs plus facilement.

![image](https://github.com/SirDec73/Theorique_Procedural/blob/main/ImageGit/BSP_FirstCorridor.png)
![image](https://github.com/SirDec73/Theorique_Procedural/blob/main/ImageGit/BSP_AddCorridor.png)

### Get Started

```csharp
[CreateAssetMenu(menuName = "Procedural Generation Method/Binary Space Partition")]
public class BinarySpacePartition : ProceduralGenerationMethod
```

Votre classe doit hériter de ProceduralGenerationMethod.

CreateAssetMenu pour pouvoir créer un scriptable object de cette classe.

```csharp
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
```
Tout d'abord, on vient créer notre nœud racine : ``BSP_Node Root = new BSP_Node(minSize, rectInt, RandomService);``.

C'est à partir de cette node que nous allons pouvoir la séparer et créer des enfants.

Ensuite, on vient générer les nodes : ``GenerateNodes(_partition, Root);``.

Cette fonction vient récursivement séparer les nodes pour en créer de nouvelles qui vont elles-mêmes se faire séparer un nombre de fois égal à _partition ;

Ensuite, on vient générer les salles : ``GenerateNodesRooms(Root);``

On vient récursivement récupérer les leafs pour créer des salles adaptées à des paramètres comme la taille minimum ou maximum de la salle ou le spacing pour éviter que les salles soient collées entre elles.

Pour finir, on vient ajouter les couloirs des salles : ``GenerateNodesCorridor(Root);``

La fonction vient récursivement récupérer les parents des leafs pour relier le centre des 2 leafs entre eux par un couloir.

Le couloir est créé en 2 parties (horizontale et verticale) et l'ordre d'appel de ces 2 parties est tiré aléatoirement.

<br>

## Cellular Automata

Le Cellular Automata est une suite de plusieurs cellules soit activées soit désactivées, générée aléatoirement, appelée aussi le "White Noise" ou "Bruit Blanc".

Il est similaire au jeu de la vie de Conway : à chaque génération, les cellules peuvent changer d'état selon les règles que l'on applique.

Par exemple, si une cellule est désactivée et entourée de minimum 4 cellules actives, alors la cellule devient elle aussi active.

Il est possible de modifier active et desactive par d'autres valeurs comme des integers et de modifier les règles pour changer ses valeurs et avoir, si l'on fait une île, une séparation de la mer, le sable, l'herbe, les montagnes, etc.


### Get Started

```csharp
[CreateAssetMenu(menuName = "Procedural Generation Method/Cellurar Automata")]
public class CellurarAutomata : ProceduralGenerationMethod
```

Votre classe doit hériter de ProceduralGenerationMethod.

CreateAssetMenu pour pouvoir créer un scriptable object de cette classe.

```csharp
[SerializeField] float noiseDensity = 0.5f;
[SerializeField] int nbGeneration = 4;
[SerializeField] int CountChange = 4;

protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
{
    var time = DateTime.Now;

    for (int i = 0; i < Grid.Lenght; i++)
    {
        for (int j = 0; j < Grid.Width; j++)
        {
            bool isGround = RandomService.Chance(noiseDensity);
            if (isGround)
            {
                CreateGroundCell(i, j);
            }
            else
            {
                CreateWaterCell(i, j);
            }
        }

        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);

    }

    Debug.Log($"Generation Map completed in {(DateTime.Now - time).TotalSeconds: 0.00} seconds.");

    for (int i = 0; i < nbGeneration; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();

        List <((string,Sprite),int,int)> tmpGrid = new List<((string, Sprite), int, int)>();

        for (int y = 0; y < Grid.Lenght; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                ((string, Sprite), int, int) tile = (CheckAndGetNewTile(x, y),x,y);
                if (IsGroundName(tile.Item2,tile.Item3,tile.Item1.Item1))
                    continue;
                tmpGrid.Add(tile);
            }
            foreach (var tmp in tmpGrid)
            {
                AssignNewType(tmp.Item2, tmp.Item3, tmp.Item1);
            }
            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            tmpGrid.Clear();
        }
    }
}
```

noiseDensity => Densité de l'herbe (0 => pas d'herbe, 1 => que de l'herbe)

CountChange => valeur de règle pour la modification d'une cellule

Pour commencer, on vient faire une boucle pour créer un white noise avec l'aide de notre noiseDensity.

Puis on boucle sur le nombre de générations où l'on vient appliquer notre règle de CountChange qui s'effectue dans ``CheckAndGetNewTile(x, y)``.

**ATTENTION**

Dans cet exemple, chaque génération de ligne vient impacter directement les lignes suivantes pendant cette même génération.

Si vous ne souhaitez pas que cela arrive :

1. Déplacer ``List <((string, Sprite), int, int)> tmpGrid = new List<((string, Sprite), int, int)>();`` au-dessus de la boucle de génération.
2. Déplacer la boucle foreach en dessous de la boucle de génération
3. Retirer le nettoyage de la grille ``tmpGrid.Clear();``.

<br>

## Noise

Cette partie a été réalisée à l'aide de FastNoiseLite : ``https://github.com/Auburn/FastNoiseLite``.

Le Noise est un moyen de générer du pixel noir et blanc aléatoire manipulable.

Il existe plusieurs types de noise, comme le "white noise" qui génère des pixels strictement noir ou blanc (0 ou 1).

Le "Gradient Noise" lui génère des pixels noirs, blancs, gris clair, gris foncé, etc. Ses valeurs varient de 0f à1 f et peuvent être comparées à des ondes.

Grâce aux valeurs récupérées, on peut facilement créer un terrain avec la variation des numéros.

Le "Fractal Noise" permet à partir du Gradient Noise d'en générer plusieurs puis de les associer entre eux pour avoir un nouveau noise.

L'**"Octave"** est le nom d'un Gradient Noise utilisé par le Fractal Noise.

Pour garder une homogénéité entre les octaves et ne pas perdre le contrôle, on diminue la fréquence des prochaines octaves grâce à la lacunarité et on baisse l'amplitude avec la persistance pour garder un lien avec la première octave générée.

### Get Started

```csharp
[CreateAssetMenu(menuName = "Procedural Generation Method/Noise")]
public class Noise : ProceduralGenerationMethod
```
Votre classe doit hériter de ProceduralGenerationMethod.

CreateAssetMenu pour pouvoir créer un scriptable object de cette classe.

```csharp
public class Noise : ProceduralGenerationMethod
{
    protected FastNoiseLite noise;

    [Header("NOISE DATA")]
    [SerializeField]                        protected FastNoiseLite.NoiseType noiseType;
    [SerializeField, Range(0.001f, 0.1f)]   protected float frequency = 0.010f;
    [SerializeField, Range(0.001f, 2f)]     protected float amplitude = 1f;

    [Header("FRACTAL NOISE")]
    [SerializeField]                        protected FastNoiseLite.FractalType fractalType;
    [SerializeField, Range(1, 10)]          protected int nbOctave = 1;
    [SerializeField, Range(0.01f, 10f)]     protected float lacunarity = 2f;
    [SerializeField, Range(0.001f, 10f)]    protected float persistance = 0.5f;

    [Header("HEIGHTS")]
    [SerializeField, Range(-1f, 1f)]        protected float sandHeight = -0.2f;
    [SerializeField, Range(-1f, 1f)]        protected float grassHeight = 0f;
    [SerializeField, Range(-1f, 1f)]        protected float rockHeight = 0.6f;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        noise = SetupNoise();

        // Gather noise data
        float noiseData;

        for (int x = 0; x < Grid.Lenght; x++)
        {
            for (int y = 0; y < Grid.Width; y++)
            {
                noiseData = GetNoiseData(x, y);

                string type;
                if (noiseData >= rockHeight)
                {
                    type = ROCK_TILE_NAME;
                }
                else if (noiseData >= grassHeight)
                {
                    type = GRASS_TILE_NAME;
                }
                else if (noiseData >= sandHeight)
                {
                    type = SAND_TILE_NAME;
                }
                else 
                { 
                    type = WATER_TILE_NAME;
                }

                AssignNewType(x,y, type);
            }
            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }
    }
```

noiseType => Type de bruit de base

frequency => Fais varier la taille des motifs.

Amplitude => Fais varier la hauteur des ondes.


fractalType => Type des octaves

nbOctave => Nombre d'octave appliqué

lacunarity => Fais varier la fréquence des prochaines octaves.

persistance Diminue l'amplitude des octaves.


sandHeight => Hauteur du début d'apparition du sable

grassHeight => Hauteur du début d'apparition de l'herbe

rockHeight => Hauteur du début d'apparition de la montagne


Ce qui se trouve en dessous de sandHeight, c'est de l'eau. 

On commence par setup le noise avec les paramètres renseignés ``noise = SetupNoise();`` qu'on vient renseigner dans une grille.

Puis, selon la valeur dans la grille, on vient appliquer un élément (Water, Sand, Grass, Rock).

## Remerciements

Je tiens à remercier RUTKOWSKI Yona pour m'avoir enseigné (à moi et à toute ma promotion) les grandes bases de la génération procédurale !

