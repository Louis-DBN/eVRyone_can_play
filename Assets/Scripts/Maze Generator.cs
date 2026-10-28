using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private MazeCell _mazeCellPrefab;
    
    [SerializeField]
    private int _mazeWidth;
    
    [SerializeField]
    private int _mazeDepth;

    private MazeCell[,] _mazeGrid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mazeGrid = new MazeCell[_mazeWidth, _mazeDepth];

        for (int i = 0; i < _mazeWidth; i++) {
            for (int j = 0; j < _mazeDepth; j++) {
                _mazeGrid[i, j] = Instantiate (_mazeCellPrefab, new Vector3(i, 0, j), Quaternion.identity);
            }
        }

        GenerateMaze(null, _mazeGrid[0, 0]);
        RemoveDuplicateWalls();

        CreateExit();
    }

    private void RemoveDuplicateWalls()
    {
        for (int i = 0; i < _mazeWidth - 1; i++)
        {
            for (int j = 0; j < _mazeDepth; j++)
            {
                _mazeGrid[i, j].ClearRightWall();
            }
        }

        for (int i = 0; i < _mazeWidth; i++)
        {
            for (int j = 0; j < _mazeDepth - 1; j++)
            {
                _mazeGrid[i, j].ClearFrontWall();
            }
        }
    }

    private void CreateExit() {
        int side = Random.Range(0, 4);
        int i;

        if (side < 2) {
            i = Random.Range(0, _mazeWidth);
        }
        else {
            i = Random.Range(0, _mazeDepth);
        }

        if(side == 0) {
            _mazeGrid[i, 0].ClearBackWall();
        }
        else if(side == 1) {
            _mazeGrid[i, _mazeDepth - 1].ClearFrontWall();
        }
        else if(side == 2) {
            _mazeGrid[0, i].ClearLeftWall();
        }
        else {
            _mazeGrid[_mazeWidth - 1, i].ClearRightWall();
        }
    }

    private void GenerateMaze(MazeCell previousCell, MazeCell currentCell) 
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);

        MazeCell nextCell;

        do {
            nextCell = GetNextUnvisitedCell(currentCell);

            if(nextCell != null) 
            {
                GenerateMaze(currentCell, nextCell);
            }
        } while (nextCell != null);
    }

    private MazeCell GetNextUnvisitedCell(MazeCell currentCell) 
    {
        var unvisitedCells = GetUnvisitedCells(currentCell);

        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell) 
    {
        int x = (int)currentCell.transform.position.x;
        int z = (int)currentCell.transform.position.z;

        if (x + 1 < _mazeWidth) 
        {
            var cellToRight = _mazeGrid[x + 1, z];

            if (cellToRight.IsVisited == false) 
            {
                yield return cellToRight;
            }
        }

        if (x - 1 >= 0)
        {
            var cellToLeft = _mazeGrid[x - 1, z];
            if (cellToLeft.IsVisited == false) 
            {
                yield return cellToLeft;
            }
        }

        if (z + 1 < _mazeDepth) 
        {
            var cellToFront = _mazeGrid[x, z + 1];

            if (cellToFront.IsVisited == false) 
            {
                yield return cellToFront;
            }
        }

        if (z - 1 >= 0) 
        {
            var cellToBack = _mazeGrid[x, z - 1];

            if (cellToBack.IsVisited == false) 
            {
                yield return cellToBack;
            }
        }
    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell) 
    {
        if (previousCell == null) {
            return;
        }

        if (previousCell.transform.position.x < currentCell.transform.position.x) {
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }
        
        if (previousCell.transform.position.x > currentCell.transform.position.x) {
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }
        
        if (previousCell.transform.position.z < currentCell.transform.position.z) {
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }
         
        if (previousCell.transform.position.z > currentCell.transform.position.z) {
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}