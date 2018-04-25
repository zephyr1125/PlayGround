
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class TileManager : Singleton<TileManager>
{
    public float DefaultTemperature;

    public Canvas Canvas;

    public Vector2Int Size;

    public GameObject TilePrefab;

    private Tile[,] _tiles;

    public bool IsCenterContinue;

    public float SimulateInterval;

    private Coroutine _routine;

    private bool _isSimulating;

    public float SourceTemperature;

    public void Start()
    {
        CreateBoard();
    }
    
    public void CreateBoard()
    {
        _tiles = new Tile[Size.x,Size.y];
        for (int j = 0; j < Size.y; j++)
        {
            for (int i = 0; i < Size.x; i++)
            {
                _tiles[i,j] = Instantiate(TilePrefab).GetComponent<Tile>();
                _tiles[i,j].Init(new Vector2Int(i,j));
                _tiles[i, j].gameObject.name = "Tile(" + i + "," + j + ")";
                _tiles[i,j].GetComponent<Transform>().SetParent(Canvas.transform);
                _tiles[i,j].transform.localPosition = new Vector3(i*32 - Size.x*16, j*32 - Size.y*16);
            }
        }

        _isSimulating = false;
    }

    public void OnChangeIsCenterContinue(bool value)
    {
        IsCenterContinue = value;
        Debug.Log(value);
    }

    public void OnChangeSimulateInterval(float value)
    {
        SimulateInterval = value;
        Debug.Log(value);
    }

    public void OnChangeSourceTemperature(string temperature)
    {
        var temp = float.Parse(temperature);
        SourceTemperature = temp;
    }

    public void OnStep()
    {
        //先统一存入上一tick温度
        foreach (var tile in _tiles)
        {
            tile.SavePreTemperature();
        }
        //然后根据上一tick温度进行本tick模拟
        foreach (var tile in _tiles)
        {
            tile.Step();
        }
    }

    public void OnSimulate()
    {
        _isSimulating = !_isSimulating;
        
        if (_isSimulating)
        {
            _routine = StartCoroutine(CoSimulate());
        }
        else
        {
            StopCoroutine(_routine);
        }
    }

    private IEnumerator CoSimulate()
    {
        while (true)
        {
            OnStep();
            
            yield return new WaitForSeconds(SimulateInterval);
        }
    }
    
    public Tile[] GetNeighbours(Vector2Int pos)
    {
        var result = new List<Tile>();
        
        result.Add(GetTile(new Vector2Int(pos.x-1, pos.y)));
        result.Add(GetTile(new Vector2Int(pos.x+1, pos.y)));
        result.Add(GetTile(new Vector2Int(pos.x, pos.y-1)));
        result.Add(GetTile(new Vector2Int(pos.x, pos.y+1)));

        return result.FindAll(tile => tile != null).ToArray();
    }

    private Tile GetTile(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= Size.x)
        {
            return null;
        }

        if (pos.y < 0 || pos.y >= Size.y)
        {
            return null;
        }

        return _tiles[pos.x, pos.y];
    }
}
