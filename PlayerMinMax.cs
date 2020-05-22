using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMinMax : AI
{
    public int limite = 2;


    private int ultimaColunma = -1;
    private List<List<int>> tablaValoracionesBasicas;
    private List<List<Piece>> miCopiaFinal = new List<List<Piece>>();
    private bool juguePrimero = false;


    public void Start()
    {
        miCopiaFinal = CopiarTablero(board);
        tablaValoracionesBasicas = new List<List<int>>();
    }

    public override int nextMove()
    {
        if (esPrimerTurno())
        {
            ultimaColunma = 3;
            miCopiaFinal = AñadirFichaAColumna(board, 3, Piece.PlayerTwo);
            juguePrimero = true;
            return 3;
        }
        else
        {
            InicializarTablaValoraciones();

            //Debug.Log("Ultima columna en la que ha echado el oponente " + UltimaColumnaEnLaQueHaEchadoElOponente());

            //Debug.Log("prueba si se guarda ultima columna " + ultimaColunma);

            Vector2 movimiento = MinimaxAB(board, 0, true, -Mathf.Infinity, Mathf.Infinity);

            Debug.Log("Columna que pasamos en nextMove " + movimiento.x);
            ultimaColunma = (int)movimiento.x;

            miCopiaFinal = AñadirFichaAColumna(board, (int)movimiento.x, Piece.PlayerTwo);

            return (int)movimiento.x;
        }
    }

    bool esPrimerTurno()
    {
        for (int x = 0; x < Config.numColumns; x++)
        {
            for (int y = 0; y < Config.numRows; y++)
            {
                if (board[x][y] != Piece.Empty)
                {
                    return false;
                }
            }
        }
        return true;
    }

    #region "Implementacion de MinMax con poda alfa beta"

    public Vector2 MinimaxAB(List<List<Piece>> estadoTablero, int profundidad, bool maximizamos, float alfa, float beta)
    {
        Vector2 valoracionColum, maxValoracion, minValoracion;
        List<int> posiblesMovimientos = PosiblesColumnas(estadoTablero);
        float alfaTemp = alfa, betaTemp = beta;

        Debug.Log("entroMinMax");

        if (profundidad == limite || GameOver(estadoTablero,Piece.PlayerTwo) || GameOver(estadoTablero,Piece.PlayerOne))
        {
            return Heuristica(estadoTablero, maximizamos);
        }
        else
        {
            if (maximizamos)
            {
                maxValoracion = new Vector2(0f, -Mathf.Infinity); //Inicializamos la maxima valoracion 
                Debug.Log("Maximizamos a profundida " + profundidad);

                for (int i = 0; i < posiblesMovimientos.Count; i++)
                {
                    Debug.Log("Maximizamos");
                    valoracionColum = MinimaxAB(AñadirFichaAColumna(estadoTablero, posiblesMovimientos[i], Piece.PlayerTwo), profundidad + 1, false, alfaTemp, betaTemp); //Aumentamos profundidad
                    valoracionColum.x = i; //not sure
                    Debug.Log("Resultado heuristica " + valoracionColum.y + " en columna " + valoracionColum.x + " en max");

                    if (maxValoracion.y < valoracionColum.y)
                    {                                                   //Comprobamos si nuestra ultima valoracion es mayor que la valoracion maxima
                        Debug.Log("la valoracion maxima cambia de " + maxValoracion.y + " a " + valoracionColum.y);
                        maxValoracion = valoracionColum;
                        Debug.Log("MaxValoracion = " + maxValoracion.x + " " + maxValoracion.y);
                    }


                    if (alfaTemp < valoracionColum.y)
                    {                                                    //Comprobamos si nuestra alfa es menor que la ultima valoracion
                        Debug.Log("la alfa cambia de " + alfaTemp + " a " + valoracionColum.y);
                        alfaTemp = valoracionColum.y;
                    }

                    if (betaTemp <= alfaTemp)
                    {                                                    //Comprobamos si tenemos que podar
                        Debug.Log("la beta es " + betaTemp + " que es menor o igual que alfa " + alfaTemp);
                        break;
                    }

                }


                return maxValoracion;  //Devolvemos la mejor columna y su valoracion
            }
            else
            {
                minValoracion = new Vector2(0f, Mathf.Infinity);
                Debug.Log("Minimizamos a profundida " + profundidad);

                for (int i = 0; i < posiblesMovimientos.Count; i++)
                {
                    Debug.Log("Minimizamos");
                    valoracionColum = MinimaxAB(AñadirFichaAColumna(estadoTablero, posiblesMovimientos[i], Piece.PlayerOne), profundidad + 1, true, alfaTemp, betaTemp); //Aumentamos profundidad
                    valoracionColum.x = i; //not sure
                    Debug.Log("Resultado heuristica " + valoracionColum.y + " en columna " + valoracionColum.x  + " en min");

                    if (minValoracion.y > valoracionColum.y)
                    {                                                       //Comprobamos si nuestra ultima valoracion es menor que la valoracion minima
                        Debug.Log("la valoracion minima cambia de " + minValoracion.y + " a " + valoracionColum.y);
                        minValoracion = valoracionColum;
                        Debug.Log("MaxValoracion = " + minValoracion.x + " " + minValoracion.y);
                    }

                    if (betaTemp > valoracionColum.y)
                    {                                                       //Comprobamos si nuestra beta es mayor que la ultima valoracion
                        Debug.Log("la beta cambia de " + betaTemp + " a " + valoracionColum.y);
                        betaTemp = valoracionColum.y;
                    }
                    if (betaTemp <= alfaTemp)
                    {                                                       //Comprobamos si tenemos que podar
                        Debug.Log("la beta es " + betaTemp + " que es menor o igual que alfa " + alfaTemp);
                        break;
                    }

                }


                return minValoracion;  //Devolvemos la mejor columna y su valoracion
            }
        }
    }


    private List<int> PosiblesColumnas(List<List<Piece>> tablero)
    {
        List<int> possibleMoves = new List<int>();
        for (int x = 0; x < Config.numColumns; x++)
        {
            for (int y = 0; y < Config.numRows; y++)
            {
                if (board[x][y] == Piece.Empty && !possibleMoves.Contains(x))
                {
                    possibleMoves.Add(x);
                }
            }
        }
        return possibleMoves;
    }

    private List<List<Piece>> AñadirFichaAColumna(List<List<Piece>> tablero, int columna, Piece player)
    {
        bool echada = false;
        List<List<Piece>> res = new List<List<Piece>>();


        res = CopiarTablero(tablero);

        /*for (int x = 0; x < Config.numColumns; x++)
        {
            res.Add(new List<Piece>());
            for (int y = 0; y < Config.numRows; y++)
            {
                res[x].Add(tablero[x][y]);
            }
        }*/


        for (int y = Config.numRows-1; y >= 0; y--)
        {
            if (res[columna][y] == Piece.Empty && !echada)
            {
                /*if (player == 1)
                {
                    res[columna][y] = Piece.PlayerOne;
                    echada = true;
                }

                if (player == 2)
                {
                    res[columna][y] = Piece.PlayerTwo;
                    echada = true;
                }*/

            res[columna][y] = player;
            echada = true;
                    
            }
        }

        return res;
    }

    private List<List<Piece>> CopiarTablero(List<List<Piece>> tablero)
    {
        List<List<Piece>> res = new List<List<Piece>>();

        for (int x = 0; x < Config.numColumns; x++)
        {
            res.Add(new List<Piece>());
            for (int y = 0; y < Config.numRows; y++)
            {
                res[x].Add(tablero[x][y]);
            }
        }

        return res;
    }

    #endregion

    #region"Implementacion de la heuristica"

    private Vector2 Heuristica(List<List<Piece>> tablero, bool isMax)
    {
        Vector2 res = new Vector2();
        int valorFichasPlayerOne = 0, valorFichasPlayerTwo = 0, valorAmenazasVerticalesPlayerOne = 0, valorAmenazasVerticalesPlayerTwo = 0, valorAmenazasDiagonalesPlayerOne = 0, valorAmenazasDiagonalesPlayerTwo = 0, valorAmenazasHorizontalesPlayerOne = 0, valorAmenazasHorizontalesPlayerTwo = 0;
        Debug.Log("Heuristica");

        if (!GameOver(tablero, Piece.PlayerOne) && !GameOver(tablero, Piece.PlayerTwo))
        {    //Si no hemos llegado con un posicion de fin de juego


            valorFichasPlayerTwo = ValoracionTablero(tablero, Piece.PlayerTwo);             //Valoramos el valor de cada ficha de player 2
            valorFichasPlayerOne = -ValoracionTablero(tablero, Piece.PlayerOne);            //Valoramos el valor de cada ficha de player 1 y le invertimos el valor
            Debug.Log("Valor fichas p2 " + valorFichasPlayerTwo);
            Debug.Log("Valor fichas p1 " + valorFichasPlayerOne);


            //Buscamos si existen amenazas verticales de victoria del player 1
            if (isNFichasEnVertical(3, tablero, Piece.PlayerOne))
            {
                Debug.Log("Entro a amenazas verticales de player 1");
                List<List<Vector2>> amenazasVerticales = new List<List<Vector2>>();

                amenazasVerticales = DevolverAmenazasFichasEnVertical(tablero, Piece.PlayerOne);    //Almacenamos todas las posiciones de dichas amenazas

                foreach (List<Vector2> amenaza in amenazasVerticales)                                //Recorremos todas las amenzas
                {
                    int valorAmenaza = 0;
                    for (int i = 0; i < amenaza.Count; i++)                                          //Sumamos sus valores
                    {
                        valorAmenaza += ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                    }
                    valorAmenazasVerticalesPlayerOne += valorAmenaza;
                }

                valorAmenazasVerticalesPlayerOne = -valorAmenazasVerticalesPlayerOne;               //Al ser el player enemigo, invertimos su valor
                Debug.Log("resultado " + valorAmenazasVerticalesPlayerOne);
            }

            //Buscamos si existen amenazas verticales de victoria del player 2
            if (isNFichasEnVertical(3, tablero, Piece.PlayerTwo))
            {
                Debug.Log("Entro a amenazas verticales de player 2");
                List<List<Vector2>> amenazasVerticales = new List<List<Vector2>>();

                amenazasVerticales = DevolverAmenazasFichasEnVertical(tablero, Piece.PlayerTwo);    //Almacenamos todas las posiciones de dichas amenazas

                foreach (List<Vector2> amenaza in amenazasVerticales)                                //Recorremos todas las amenzas
                {
                    int valorAmenaza = 0;
                    for (int i = 0; i < amenaza.Count; i++)                                          //Sumamos sus valores
                    {
                        valorAmenaza += ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                    }
                    valorAmenazasVerticalesPlayerTwo += valorAmenaza;
                }
                Debug.Log("resultado " + valorAmenazasVerticalesPlayerTwo);
            }

            //Buscamos si existen amenazas diagonales de victoria de player 1
            if (isNFichasEnDiagonal(3, tablero, Piece.PlayerOne))
            {
                Debug.Log("Entro a amenazas diagonales de player 1");
                List<List<Vector2>> amenazasDiagonales = new List<List<Vector2>>();

                amenazasDiagonales = DevolverAmenazasFichasEnDiagonal(tablero, Piece.PlayerOne);        //Almacenamos todas las posiciones de dichas amenazas

                foreach (List<Vector2> amenaza in amenazasDiagonales)                                //Recorremos todas las amenzas
                {
                    int valorAmenaza = 0;
                    for (int i = 0; i < amenaza.Count; i++)                                          //Sumamos sus valores
                    {

                        if (juguePrimero)
                        {
                            if ((int)amenaza[i].y % 2 == 0 && tablero[(int)amenaza[i].x][(int)amenaza[i].y] == Piece.Empty) //Compruebo si la amenaza va a caer en una fila par
                                valorAmenaza += 2 * ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                            else
                                valorAmenaza += ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                        }
                        else
                        {
                            if ((int)amenaza[i].y % 2 != 0 && tablero[(int)amenaza[i].x][(int)amenaza[i].y] == Piece.Empty) //Compruebo si la amenaza va a caer en una fila impar 
                                valorAmenaza += 2 * ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                            else
                                valorAmenaza += ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                        }

                        valorAmenaza = 2 * valorAmenaza;

                    }
                    valorAmenazasDiagonalesPlayerOne += valorAmenaza;
                }

                valorAmenazasDiagonalesPlayerOne = -valorAmenazasDiagonalesPlayerOne;
                Debug.Log("resultado " + valorAmenazasDiagonalesPlayerOne);
            }

            //Buscamos si existen amenazas diagonales de victoria de player 2
            if (isNFichasEnDiagonal(3, tablero, Piece.PlayerTwo))
            {
                Debug.Log("Entro a amenazas diagonales de player 2");
                List<List<Vector2>> amenazasDiagonales = new List<List<Vector2>>();

                amenazasDiagonales = DevolverAmenazasFichasEnDiagonal(tablero, Piece.PlayerOne);        //Almacenamos todas las posiciones de dichas amenazas

                foreach (List<Vector2> amenaza in amenazasDiagonales)                                //Recorremos todas las amenzas
                {
                    int valorAmenaza = 0;
                    for (int i = 0; i < amenaza.Count; i++)                                          //Sumamos sus valores
                    {

                        if (juguePrimero)
                        {
                            if ((int)amenaza[i].y % 2 != 0 && tablero[(int)amenaza[i].x][(int)amenaza[i].y] == Piece.Empty) //Compruebo si la amenaza va a caer en una fila par
                                valorAmenaza += 2 * ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                            else
                                valorAmenaza += ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                        }
                        else
                        {
                            if ((int)amenaza[i].y % 2 == 0 && tablero[(int)amenaza[i].x][(int)amenaza[i].y] == Piece.Empty) //Compruebo si la amenaza va a caer en una fila impar 
                                valorAmenaza += 2 * ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                            else
                                valorAmenaza += ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                        }

                        valorAmenaza = 2 * valorAmenaza;

                    }
                    valorAmenazasDiagonalesPlayerTwo += valorAmenaza;
                }
                Debug.Log("resultado " + valorAmenazasDiagonalesPlayerTwo);
            }

            if (isNFichasEnHorizontal(3, tablero, Piece.PlayerOne))
            {
                Debug.Log("Entro a amenazas horizontales de player 1");
                List<List<Vector2>> amenazasHorizontales = new List<List<Vector2>>();

                amenazasHorizontales = DevolverAmenazasFichasEnHorizontal(tablero, Piece.PlayerOne);        //Almacenamos todas las posiciones de dichas amenazas

                foreach (List<Vector2> amenaza in amenazasHorizontales)                                //Recorremos todas las amenzas
                {
                    int valorAmenaza = 0;
                    for (int i = 0; i < amenaza.Count; i++)                                          //Sumamos sus valores
                    {

                        if (juguePrimero)
                        {
                            if ((int)amenaza[i].y % 2 == 0 && tablero[(int)amenaza[i].x][(int)amenaza[i].y] == Piece.Empty) //Compruebo si la amenaza va a caer en una fila par
                                valorAmenaza += 2 * ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                            else
                                valorAmenaza += ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                        }
                        else
                        {
                            if ((int)amenaza[i].y % 2 != 0 && tablero[(int)amenaza[i].x][(int)amenaza[i].y] == Piece.Empty) //Compruebo si la amenaza va a caer en una fila impar 
                                valorAmenaza += 2 * ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                            else
                                valorAmenaza += ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                        }

                        valorAmenaza = 5 * valorAmenaza;

                    }
                    valorAmenazasHorizontalesPlayerOne += valorAmenaza;
                }
                valorAmenazasHorizontalesPlayerOne = -valorAmenazasHorizontalesPlayerOne;
                Debug.Log("resultado " + valorAmenazasHorizontalesPlayerOne);
            }

            if (isNFichasEnHorizontal(3, tablero, Piece.PlayerTwo))
            {
                Debug.Log("Entro a amenazas Horizontales de player 2");
                List<List<Vector2>> amenazasHorizontales = new List<List<Vector2>>();

                amenazasHorizontales = DevolverAmenazasFichasEnHorizontal(tablero, Piece.PlayerTwo);        //Almacenamos todas las posiciones de dichas amenazas

                foreach (List<Vector2> amenaza in amenazasHorizontales)                                //Recorremos todas las amenzas
                {
                    int valorAmenaza = 0;
                    for (int i = 0; i < amenaza.Count; i++)                                          //Sumamos sus valores
                    {

                        if (juguePrimero)
                        {
                            if ((int)amenaza[i].y % 2 != 0 && tablero[(int)amenaza[i].x][(int)amenaza[i].y] == Piece.Empty) //Compruebo si la amenaza va a caer en una fila par
                                valorAmenaza += 2 * ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                            else
                                valorAmenaza += ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                        }
                        else
                        {
                            if ((int)amenaza[i].y % 2 == 0 && tablero[(int)amenaza[i].x][(int)amenaza[i].y] == Piece.Empty) //Compruebo si la amenaza va a caer en una fila impar 
                                valorAmenaza += 2 * ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                            else
                                valorAmenaza += ValorCasilla((int)amenaza[i].x, (int)amenaza[i].y);
                        }

                        valorAmenaza = 5 * valorAmenaza;

                    }
                    valorAmenazasHorizontalesPlayerTwo += valorAmenaza;
                    Debug.Log("resultado " + valorAmenazasHorizontalesPlayerTwo);
                }
            }


            res.x = -1;
            res.y = valorAmenazasDiagonalesPlayerTwo + valorAmenazasDiagonalesPlayerOne + valorAmenazasVerticalesPlayerTwo + valorAmenazasVerticalesPlayerOne + valorFichasPlayerTwo + valorFichasPlayerOne + valorAmenazasHorizontalesPlayerOne + valorAmenazasHorizontalesPlayerTwo;
        }
        else
        {
            if (GameOver(tablero, Piece.PlayerOne))
            {
                res.x = -1;
                res.y = -1000000000000f;
            }

            if (GameOver(tablero, Piece.PlayerTwo))
            {
                res.x = -1;
                res.y = 1000000000000f;
            }
        }


        return res;
    }


    int ValoracionTablero(List<List<Piece>> tablero, Piece jugador)
    {
        int res = 0;

        for (int x = 0; x < Config.numColumns; x++)
        {
            for (int y = 0; y < Config.numRows; y++)
            {
                if (tablero[x][y] == jugador)
                {
                    res += tablaValoracionesBasicas[x][y];
                }
            }
        }

        return res;
    }

    int ValorCasilla(int x,int y)
    {
        return tablaValoracionesBasicas[x][y];
    }

    int ContarFichasEnTablero(List<List<Piece>> tablero)
    {
        int res = 0;

        for (int x = 0; x < Config.numColumns; x++)
        {
            for (int y = 0; y < Config.numRows; y++)
            {
                if (tablero[x][y] == Piece.PlayerOne || board[x][y] == Piece.PlayerTwo)
                {
                    res++;
                }
            }
        }

        return res;
    }


    private void InicializarTablaValoraciones()
    {
        tablaValoracionesBasicas = new List<List<int>>();
        /*tablaValoracionesBasicas.Add(new List<int> {    1,   1,   10,   100,   10,   1,    1 });
        tablaValoracionesBasicas.Add(new List<int> {    1,   1,   10,   100,   10,   1,    1 });
        tablaValoracionesBasicas.Add(new List<int> {   10,  10,  100,  1000,  100,  10,   10 });
        tablaValoracionesBasicas.Add(new List<int> {   10,  10, 1000,  1000, 1000,  10,   10 });
        tablaValoracionesBasicas.Add(new List<int> {  100, 100, 1000, 10000, 1000, 100,  100 });
        tablaValoracionesBasicas.Add(new List<int> { 1000, 100, 1000, 10000, 1000, 100, 1000 });*/

        tablaValoracionesBasicas.Add(new List<int> {   1,   1,   10,   10,  100,  1000 });
        tablaValoracionesBasicas.Add(new List<int> {   1,   1,   10,  100,  100,   100 });
        tablaValoracionesBasicas.Add(new List<int> {  10,  10,  100, 1000, 1000,  1000 });
        tablaValoracionesBasicas.Add(new List<int> { 100, 100, 1000, 1000, 1000, 10000 });
        tablaValoracionesBasicas.Add(new List<int> {  10,  10,  100, 1000, 1000,  1000 });
        tablaValoracionesBasicas.Add(new List<int> {   1,   1,   10,  100,  100,   100 });
        tablaValoracionesBasicas.Add(new List<int> {   1,   1,   10,   10,  100,  1000 });
    }

    private int numeroDeFilasRestantes(List<List<Piece>> tablero,int col)
    {
        int res = 0;
        for (int y = 0; y < Config.numRows; y++)
        {
            if (tablero[col][y] == Piece.Empty)
            {
                    res++;
            }
        }
        return res;
    }

    private int UltimaColumnaEnLaQueHaEchadoElOponente()
    {
        int res = -1; ;

        for (int x = 0; x < Config.numColumns; x++)
        {
            for (int y = 0; y < Config.numRows; y++)
            {
                if (board[x][y] != miCopiaFinal[x][y])
                    res = x;
            }
        }

        return res;
    }

    bool GameOver(List<List<Piece>> tablero,Piece pieza)
    {
        if (isNFichasEnHorizontal(4, tablero, pieza))
            return true;
        if (isNFichasEnVertical(4, tablero, pieza))
            return true;
        if (isNFichasEnDiagonal(4, tablero, pieza))
            return true;

        return false;
    }

    bool isNFichasEnHorizontal(int n,List<List<Piece>> tablero,Piece pieza)
    {
        for (int y = 0; y < Config.numRows; y++)
        {
            int contadorCeldasConsecutivas = 0;

            for(int x = 0; x < Config.numColumns; x++)
            {
                if (tablero[x][y] == pieza)
                    contadorCeldasConsecutivas++;
                else
                    contadorCeldasConsecutivas = 0;

                if(contadorCeldasConsecutivas == n)
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool isNFichasEnVertical(int n,List<List<Piece>> tablero, Piece pieza)
    {
        for (int x = 0; x < Config.numColumns; x++)
        {
            int contadorCeldasConsecutivas = 0;
            for (int y = Config.numRows - 1; y >= 0; y--)
            {
                if (tablero[x][y] == pieza)
                    contadorCeldasConsecutivas++;
                else
                    contadorCeldasConsecutivas = 0;

                if (contadorCeldasConsecutivas == n)
                    return true;
            }
        }

        return false;
    }

    bool isNFichasEnDiagonal(int n,List<List<Piece>> tablero,Piece pieza)
    {
        for (int x = 0; x < Config.numColumns; x++)
        {
            for (int y = 0; y < Config.numRows; y++) 
            {

                //Primera ficha - Diagonal inferior derecha
                if (tablero[x][y] == pieza) 
                {
                    if (x + 1 < Config.numColumns && y + 1 < Config.numRows) //Comprobamos siguiente ficha abajo derecha existe
                    {
                        if (tablero[x + 1][y + 1] == pieza)                     //Segunda ficha
                        {
                            if (x + 2 < Config.numColumns && y + 2 < Config.numRows)    //Comprobamos siguiente ficha 2 abajo 2 derecha existe
                            {
                                if (tablero[x + 2][y + 2] == pieza)                         //Tercera ficha
                                {
                                    if(n == 4)                                                      //Queremos comprobar 4 seguidas y no 3
                                    {
                                        if (x + 3 < Config.numColumns && y + 3 < Config.numRows)    //Comprobamos siguiente ficha 3 abajo 3 derecha existe
                                        {
                                            if (tablero[x + 3][y + 3] == pieza)                     //Cuarta ficha
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                //Primera ficha - Digagonal superior derecha
                if (tablero[x][y] == pieza) 
                {
                    if (x - 1 >= 0 && y + 1 < Config.numRows) //Comprobamos siguiente ficha arriba derecha existe
                    {
                        if (tablero[x - 1][y + 1] == pieza)         //Segunda ficha
                        {
                            if (x - 2 >= 0 && y + 2 < Config.numRows)    //Comprobamos siguiente ficha 2 arriba 2 derecha existe
                            {
                                if (tablero[x - 2][y + 2] == pieza)         //Tercera ficha
                                {
                                    if (n == 4)                                    //Queremos comprobar 4 seguidas y no 3
                                    {
                                        if (x - 3 >= 0 && y + 3 < Config.numRows)    //Comprobamos siguiente ficha 3 abajo 3 derecha existe
                                        {
                                            if (tablero[x - 3][y + 3] == pieza)                     //Cuarta ficha
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }


    #region "Devolver posiciones de amenaza"

    List<List<Vector2>> DevolverAmenazasFichasEnHorizontal(List<List<Piece>> tablero, Piece pieza)
    {
        List<List<Vector2>> listaPosiciones = new List<List<Vector2>>();

        for (int y = 0; y < Config.numRows; y++)
        {
            int contadorCeldasConsecutivas = 0;
            List<Vector2> conjuntoPosiciones = new List<Vector2>();

            for (int x = 0; x < Config.numColumns; x++)
            {
                if (tablero[x][y] == pieza)
                {
                    contadorCeldasConsecutivas++;
                    conjuntoPosiciones.Add(new Vector2(x, y));
                }
                else if( tablero[x][y] == Piece.Empty)

                {
                    
                    if(x == 0)
                    {
                        if(tablero[x+1][y] != pieza  || tablero[x + 1][y] != Piece.Empty)
                        {
                            contadorCeldasConsecutivas = 0;
                            conjuntoPosiciones.Clear();
                        }
                        else
                        {
                            conjuntoPosiciones.Add(new Vector2(x, y));
                        }
                    }
                    else if(x == 6)
                    {
                        if (tablero[x - 1][y] != pieza || tablero[x - 1][y] != Piece.Empty)
                        {
                            contadorCeldasConsecutivas = 0;
                            conjuntoPosiciones.Clear();
                        }
                        else
                        {
                            conjuntoPosiciones.Add(new Vector2(x, y));

                        }
                    }
                    else
                    {
                        if((tablero[x - 1][y] != pieza && tablero[x + 1][y] != pieza) && (tablero[x - 1][y] == Piece.Empty && tablero[x + 1][y] == Piece.Empty))
                        {
                            contadorCeldasConsecutivas = 0;
                            conjuntoPosiciones.Clear();
                        }
                        else
                        {

                            conjuntoPosiciones.Add(new Vector2(x, y));

                        }
                    }
                }
                else
                {
                    contadorCeldasConsecutivas = 0;
                    conjuntoPosiciones.Clear();
                }
                    

                if (contadorCeldasConsecutivas == 3)
                {
                    listaPosiciones.Add(conjuntoPosiciones);
                    conjuntoPosiciones.RemoveAt(0);
                    contadorCeldasConsecutivas--;
                }
            }
        }


        return listaPosiciones;
    }

    List<List<Vector2>> DevolverAmenazasFichasEnVertical(List<List<Piece>> tablero, Piece pieza)
    {
        List<List<Vector2>> listaPosiciones = new List<List<Vector2>>();

        for (int x = 0; x < Config.numColumns; x++)
        {
            int contadorCeldasConsecutivas = 0;
            List<Vector2> conjuntoPosiciones = new List<Vector2>();

            for (int y = Config.numRows - 1; y >= 0; y--)
            {
                if (tablero[x][y] == pieza)
                {
                    contadorCeldasConsecutivas++;
                    conjuntoPosiciones.Add(new Vector2(x, y));
                }
                else
                {
                    contadorCeldasConsecutivas = 0;
                    conjuntoPosiciones.Clear();
                }

                if (contadorCeldasConsecutivas == 3)
                {
                    listaPosiciones.Add(conjuntoPosiciones);
                }
                    
            }
        }

        return listaPosiciones;
    }
    List<List<Vector2>> DevolverAmenazasFichasEnDiagonal(List<List<Piece>> tablero, Piece pieza)
    {
        List<List<Vector2>> listaPosiciones = new List<List<Vector2>>();

        for (int x = 0; x < 4; x++)
        {

            List<Vector2> conjuntoPosiciones = new List<Vector2>();

            for (int y = 0; y < Config.numRows; y++)
            {

                //Primera ficha - Diagonal inferior derecha (3 seguidas - ultima vacia)
                if (tablero[x][y] == pieza)
                {
                    if (x + 1 < Config.numColumns && y + 1 < Config.numRows) //Comprobamos siguiente ficha abajo derecha existe
                    {
                        if (tablero[x + 1][y + 1] == pieza)                     //Segunda ficha
                        {
                            if (x + 2 < Config.numColumns && y + 2 < Config.numRows)    //Comprobamos siguiente ficha 2 abajo 2 derecha existe
                            {
                                if (tablero[x + 2][y + 2] == pieza)                         //Tercera ficha
                                {
                                    if (x + 3 < Config.numColumns && y + 3 < Config.numRows)    //Comprobamos siguiente ficha 3 abajo 3 derecha existe
                                    {
                                        if (tablero[x + 3][y + 3] == Piece.Empty)
                                        {
                                            conjuntoPosiciones.Add(new Vector2(x, y));
                                            conjuntoPosiciones.Add(new Vector2(x + 1, y + 1));
                                            conjuntoPosiciones.Add(new Vector2(x + 2, y + 2));
                                            conjuntoPosiciones.Add(new Vector2(x + 3, y + 3));

                                            if(x - 1 >= 0 && y - 1 >= 0)
                                            {
                                                if(tablero[x - 1][y - 1] == Piece.Empty)
                                                    conjuntoPosiciones.Add(new Vector2(x - 1, y - 1));
                                            }


                                            listaPosiciones.Add(conjuntoPosiciones);
                                            conjuntoPosiciones.Clear();
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

                //Primera ficha - Digagonal superior derecha (3 seguidas - ultima vacia)
                if (tablero[x][y] == pieza)
                {
                    if (x + 1 < Config.numColumns && y - 1 >= 0) //Comprobamos siguiente ficha arriba derecha existe
                    {
                        if (tablero[x + 1][y - 1] == pieza)         //Segunda ficha
                        {
                            if (x + 2 < Config.numColumns && y - 2 >= 0)    //Comprobamos siguiente ficha 2 arriba 2 derecha existe
                            {
                                if (tablero[x + 2][y - 2] == pieza)         //Tercera ficha
                                {
                                    if (x + 3 < Config.numColumns && y - 3 >= 0)    //Comprobamos siguiente ficha 3 arriba 3 derecha existe
                                    {
                                        if (tablero[x + 3][y - 3] == Piece.Empty)
                                        {
                                            conjuntoPosiciones.Add(new Vector2(x, y));
                                            conjuntoPosiciones.Add(new Vector2(x + 1, y - 1));
                                            conjuntoPosiciones.Add(new Vector2(x + 2, y - 2));
                                            conjuntoPosiciones.Add(new Vector2(x + 3, y - 3));

                                            if (x - 1 >= 0 && y + 1 < Config.numRows)
                                            {
                                                if (tablero[x - 1][y + 1] == Piece.Empty)
                                                    conjuntoPosiciones.Add(new Vector2(x - 1, y + 1));
                                            }
                                            listaPosiciones.Add(conjuntoPosiciones);
                                            conjuntoPosiciones.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //Primera ficha - Diagonal inferior derecha (primera vacia - 3 seguidas)
                if (tablero[x][y] == Piece.Empty)
                {
                    if (x + 1 < Config.numColumns && y + 1 < Config.numRows) //Comprobamos siguiente ficha abajo derecha existe
                    {
                        if (tablero[x + 1][y + 1] == pieza)                     //Segunda ficha
                        {
                            if (x + 2 < Config.numColumns && y + 2 < Config.numRows)    //Comprobamos siguiente ficha 2 abajo 2 derecha existe
                            {
                                if (tablero[x + 2][y + 2] == pieza)                         //Tercera ficha
                                {
                                    if (x + 3 < Config.numColumns && y + 3 < Config.numRows)    //Comprobamos siguiente ficha 3 abajo 3 derecha existe
                                    {
                                        if (tablero[x + 3][y + 3] == pieza)
                                        {
                                            conjuntoPosiciones.Add(new Vector2(x, y));
                                            conjuntoPosiciones.Add(new Vector2(x + 1, y + 1));
                                            conjuntoPosiciones.Add(new Vector2(x + 2, y + 2));
                                            conjuntoPosiciones.Add(new Vector2(x + 3, y + 3));

                                            if (x + 4 < Config.numColumns && y + 4 < Config.numRows)
                                            {
                                                if (tablero[x + 4][y + 4] == Piece.Empty)
                                                    conjuntoPosiciones.Add(new Vector2(x + 4, y + 4));
                                            }
                                            listaPosiciones.Add(conjuntoPosiciones);
                                            conjuntoPosiciones.Clear();
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

                //Primera ficha - Digagonal superior derecha (primera vacia - 3 seguidas)
                if (tablero[x][y] == Piece.Empty)
                {
                    if (x + 1 < Config.numColumns && y - 1 >= 0) //Comprobamos siguiente ficha arriba derecha existe
                    {
                        if (tablero[x + 1][y - 1] == pieza)         //Segunda ficha
                        {
                            if (x + 2 < Config.numColumns && y - 2 >= 0)    //Comprobamos siguiente ficha 2 arriba 2 derecha existe
                            {
                                if (tablero[x + 2][y - 2] == pieza)         //Tercera ficha
                                {
                                    if (x + 3 < Config.numColumns && y - 3 >= 0)    //Comprobamos siguiente ficha 3 arriba 3 derecha existe
                                    {
                                        if (tablero[x + 3][y - 3] == pieza)
                                        {
                                            conjuntoPosiciones.Add(new Vector2(x, y));
                                            conjuntoPosiciones.Add(new Vector2(x + 1, y - 1));
                                            conjuntoPosiciones.Add(new Vector2(x + 2, y - 2));
                                            conjuntoPosiciones.Add(new Vector2(x + 3, y - 3));

                                            if (x + 4 < Config.numColumns && y - 4 >= 0)
                                            {
                                                if (tablero[x + 4][y - 4] == Piece.Empty)
                                                    conjuntoPosiciones.Add(new Vector2(x + 4, y - 4));
                                            }

                                            listaPosiciones.Add(conjuntoPosiciones);
                                            conjuntoPosiciones.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //Primera ficha - Diagonal inferior derecha (primera pieza - vacia - 2 seguidas)
                if (tablero[x][y] == pieza)
                {
                    if (x + 1 < Config.numColumns && y + 1 < Config.numRows) //Comprobamos siguiente ficha abajo derecha existe
                    {
                        if (tablero[x + 1][y + 1] == Piece.Empty)                     //Segunda ficha
                        {
                            if (x + 2 < Config.numColumns && y + 2 < Config.numRows)    //Comprobamos siguiente ficha 2 abajo 2 derecha existe
                            {
                                if (tablero[x + 2][y + 2] == pieza)                         //Tercera ficha
                                {
                                    if (x + 3 < Config.numColumns && y + 3 < Config.numRows)    //Comprobamos siguiente ficha 3 abajo 3 derecha existe
                                    {
                                        if (tablero[x + 3][y + 3] == pieza)
                                        {
                                            conjuntoPosiciones.Add(new Vector2(x, y));
                                            conjuntoPosiciones.Add(new Vector2(x + 1, y + 1));
                                            conjuntoPosiciones.Add(new Vector2(x + 2, y + 2));
                                            conjuntoPosiciones.Add(new Vector2(x + 3, y + 3));
                                            listaPosiciones.Add(conjuntoPosiciones);
                                            conjuntoPosiciones.Clear();
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

                //Primera ficha - Digagonal superior derecha (primera pieza - vacia - 2 seguidas)
                if (tablero[x][y] == pieza)
                {
                    if (x + 1 < Config.numColumns && y - 1 >= 0) //Comprobamos siguiente ficha arriba derecha existe
                    {
                        if (tablero[x + 1][y - 1] == Piece.Empty)         //Segunda ficha
                        {
                            if (x + 2 < Config.numColumns && y - 2 >= 0)    //Comprobamos siguiente ficha 2 arriba 2 derecha existe
                            {
                                if (tablero[x + 2][y - 2] == pieza)         //Tercera ficha
                                {
                                    if (x + 3 < Config.numColumns && y - 3 >= 0)    //Comprobamos siguiente ficha 3 arriba 3 derecha existe
                                    {
                                        if (tablero[x + 3][y - 3] == pieza)
                                        {
                                            conjuntoPosiciones.Add(new Vector2(x, y));
                                            conjuntoPosiciones.Add(new Vector2(x + 1, y - 1));
                                            conjuntoPosiciones.Add(new Vector2(x + 2, y - 2));
                                            conjuntoPosiciones.Add(new Vector2(x + 3, y - 3));
                                            listaPosiciones.Add(conjuntoPosiciones);
                                            conjuntoPosiciones.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //Primera ficha - Diagonal inferior derecha (2 seguidas - vacia - ultima pieza)
                if (tablero[x][y] == pieza)
                {
                    if (x + 1 < Config.numColumns && y + 1 < Config.numRows) //Comprobamos siguiente ficha abajo derecha existe
                    {
                        if (tablero[x + 1][y + 1] == pieza)                     //Segunda ficha
                        {
                            if (x + 2 < Config.numColumns && y + 2 < Config.numRows)    //Comprobamos siguiente ficha 2 abajo 2 derecha existe
                            {
                                if (tablero[x + 2][y + 2] == Piece.Empty)                         //Tercera ficha
                                {
                                    if (x + 3 < Config.numColumns && y + 3 < Config.numRows)    //Comprobamos siguiente ficha 3 abajo 3 derecha existe
                                    {
                                        if (tablero[x + 3][y + 3] == pieza)
                                        {
                                            conjuntoPosiciones.Add(new Vector2(x, y));
                                            conjuntoPosiciones.Add(new Vector2(x + 1, y + 1));
                                            conjuntoPosiciones.Add(new Vector2(x + 2, y + 2));
                                            conjuntoPosiciones.Add(new Vector2(x + 3, y + 3));
                                            listaPosiciones.Add(conjuntoPosiciones);
                                            conjuntoPosiciones.Clear();
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

                //Primera ficha - Digagonal superior derecha (2 seguidas - vacia - ultima pieza)
                if (tablero[x][y] == pieza)
                {
                    if (x + 1 < Config.numColumns && y - 1 >= 0) //Comprobamos siguiente ficha arriba derecha existe
                    {
                        if (tablero[x + 1][y - 1] == pieza)         //Segunda ficha
                        {
                            if (x + 2 < Config.numColumns && y - 2 >= 0)    //Comprobamos siguiente ficha 2 arriba 2 derecha existe
                            {
                                if (tablero[x + 2][y - 2] == Piece.Empty)         //Tercera ficha
                                {
                                    if (x + 3 < Config.numColumns && y - 3 >= 0)    //Comprobamos siguiente ficha 3 arriba 3 derecha existe
                                    {
                                        if (tablero[x + 3][y - 3] == pieza)
                                        {
                                            conjuntoPosiciones.Add(new Vector2(x, y));
                                            conjuntoPosiciones.Add(new Vector2(x + 1, y - 1));
                                            conjuntoPosiciones.Add(new Vector2(x + 2, y - 2));
                                            conjuntoPosiciones.Add(new Vector2(x + 3, y - 3));
                                            listaPosiciones.Add(conjuntoPosiciones);
                                            conjuntoPosiciones.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        return listaPosiciones;

    }

    #endregion
    #endregion

}

