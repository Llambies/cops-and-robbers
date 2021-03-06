using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //Inicializar matriz a 0's
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                matriu[i, j] = 0;
            }
        }

        //Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            for (int j = 0; j < Constants.NumTiles; j++)
            {
                if (i % 8 != 0)
                {
                    if (i - 1 >= 0)
                    {
                        matriu[i - 1, j] = 1;
                    }
                }
                if (i - 8 >= 0)
                {
                    matriu[i - 8, j] = 1;
                }
                if ((i + 1) % 8 != 0)
                {
                    if (i + 1 <= 63)
                    {
                        matriu[i + 1, j] = 1;
                    }
                }
                if (i + 8 <= 63)
                {
                    matriu[i + 8, j] = 1;
                }

            }
        }

        //Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            if (i % 8 != 0)
            {
                if (i - 1 >= 0)
                {
                    //izquierda
                    tiles[i].adjacency.Add(tiles[i - 1].numTile);
                }
                else
                {
                    tiles[i].adjacency.Add(-1);
                }
            }

            if (i - 8 >= 0)
            {
                //abajo
                tiles[i].adjacency.Add(tiles[i - 8].numTile);
            }
            else
            {
                tiles[i].adjacency.Add(-1);
            }

            if ((i + 1) % 8 != 0)
            {
                if (i + 1 <= 63)
                {
                    //derecha
                    tiles[i].adjacency.Add(tiles[i + 1].numTile);
                }
                else
                {
                    tiles[i].adjacency.Add(-1);
                }
            }

            if (i + 8 <= 63)
            {
                //abajo
                tiles[i].adjacency.Add(tiles[i + 8].numTile);
            }
            else
            {
                tiles[i].adjacency.Add(-1);
            }

        }

    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {

        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */

        /*
        int destino = Random.Range(0, 63);

        bool valorNoValido = true;
        while(valorNoValido)
        {
            if (tiles[destino].selectable)
            {
                valorNoValido = false;
            }
            else
            {
                destino = Random.Range(0, 63);
            }
        }

        clickedTile = destino;

        robber.GetComponent<RobberMove>().MoveToTile(tiles[destino]);
        robber.GetComponent<RobberMove>().currentTile = destino;
        */

        int mayorDistancia = 0;
        int mayorDistanciaCop0 = 0;
        int mayorDistanciaCop1 = 0;
        int posMayor = 0;

        List<int> casillasAdyacentes = new List<int>();


        foreach (Tile t in tiles)
        {
            if (t.selectable)
            {
                casillasAdyacentes.Add(t.numTile);
            }
        }

        ResetTiles();

        List<int> distanciasCop0 = new List<int>();

        clickedCop = 0;
        FindSelectableTiles(true);

        for (int i = 0; i < tiles.Length; i++)
        {
            if (casillasAdyacentes.Contains(tiles[i].numTile))
            {
                distanciasCop0.Add(tiles[i].distance);
            }
        }

        ResetTiles();

        List<int> distanciasCop1 = new List<int>();

        clickedCop = 1;
        FindSelectableTiles(true);

        for (int i = 0; i < tiles.Length; i++)
        {
            if (casillasAdyacentes.Contains(tiles[i].numTile))
            {
                distanciasCop1.Add(tiles[i].distance);
            }
        }

        ResetTiles();

        for (int i = 0; i < casillasAdyacentes.Count; i++)
        {
            if (mayorDistancia < (distanciasCop0[i] + distanciasCop1[i]))
            {
                mayorDistancia = (distanciasCop0[i] + distanciasCop1[i]);
                mayorDistanciaCop0 = distanciasCop0[i];
                mayorDistanciaCop1 = distanciasCop1[i];
                posMayor = i;
            }
            else if (mayorDistancia == (distanciasCop0[i] + distanciasCop1[i]))
            {
                if (distanciasCop0[i] < distanciasCop1[i])
                {
                    if (mayorDistanciaCop0 < distanciasCop0[i])
                    {
                        mayorDistanciaCop0 = distanciasCop0[i];
                        mayorDistanciaCop1 = distanciasCop1[i];
                        posMayor = i;
                    }
                }
                else if (distanciasCop0[i] > distanciasCop1[i])
                {
                    if (mayorDistanciaCop1 < distanciasCop1[i])
                    {
                        mayorDistanciaCop0 = distanciasCop0[i];
                        mayorDistanciaCop1 = distanciasCop1[i];
                        posMayor = i;
                    }
                }
            }
        }


        FindSelectableTiles(false);
        robber.GetComponent<RobberMove>().MoveToTile(tiles[casillasAdyacentes[posMayor]]);
        robber.GetComponent<RobberMove>().currentTile = casillasAdyacentes[posMayor];
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {

        int indexcurrentTile;

        if (cop == true)
        {
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        }
        else
        {
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;
        }


        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        List<int> gentes = new List<int>();
        for (int i = 0; i < cops.Length; i++)
        {
            gentes.Add(cops[i].GetComponent<CopMove>().currentTile);
        }

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO: Implementar BFS. Los nodos seleccionables los ponemos como selectable=true
        //Tendrás que cambiar este código por el BFS
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            tiles[i].selectable = false;
            tiles[i].visited = false;
        }

        Tile tileActual = tiles[indexcurrentTile];
        tileActual.visited = true;


        for (int i = 0; i < tileActual.adjacency.Count; i++)
        {
            if (tileActual.adjacency[i] >= 0)
            {
                tiles[tileActual.adjacency[i]].parent = tileActual;
                nodes.Enqueue(tiles[tileActual.adjacency[i]]);


            }
        }

        while (nodes.Count > 0)
        {

            Tile aux = nodes.Dequeue();

            if (!aux.visited)
            {
                if (gentes.Contains(aux.numTile))
                {
                    aux.visited = true;
                    aux.distance = aux.parent.distance + 1;
                }
                else
                {
                    aux.visited = true;
                    aux.distance = aux.parent.distance + 1;

                    for (int i = 0; i < aux.adjacency.Count; i++)
                    {
                        if (aux.adjacency[i] >= 0)
                        {

                            if (!tiles[aux.adjacency[i]].visited)
                            {
                                tiles[aux.adjacency[i]].parent = aux;
                                nodes.Enqueue(tiles[aux.adjacency[i]]);

                            }
                        }
                    }
                }

            }
        }

        foreach (Tile t in tiles)
        {
            if (t.distance <= 2)
            {
                if (!gentes.Contains(t.numTile))
                {
                    t.selectable = true;
                }

            }
        }
    }


}
    
   
    

    

   

       

