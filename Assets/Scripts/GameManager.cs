using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour, IPointerDownHandler
{
    private SceneHandler _sceneHandler;
    private LevelData _levelData;
    private int _gridWidth;
    private int _gridHeight;
    private int _bombCount;
    private int _levelNumber;
    private float _endPanelScale;
    private bool _levelEnded;
    private bool _buttonClicked;
    private List<int> _bombedGrids = new List<int>();
    private List<int> _filledGrids = new List<int>();
    private List<int> _filledDestroyableGrids = new List<int>();
    private List<GameButton> _gameButtons = new List<GameButton>();
    private Dictionary<int, LevelSetter.Cell> _gridCellDictionary = new Dictionary<int, LevelSetter.Cell>();
    [SerializeField] private EndPanel endPanel;
    [SerializeField] private LevelSetter levelManager;
    

    private void Start()
    {
        _sceneHandler = SceneHandler.Instance; //It is okay to do here since Scene Handler is never destroyed
        _sceneHandler.SetCameraSizeForGrid();
        foreach (var b in FindObjectsOfType<GameButton>())
        {
            _gameButtons.Add(b);
            b.ButtonClicked += ButtonClicked;
        }

        SetLevel(_sceneHandler.GetCurrentLevelData());
    }

    private void OnDisable()
    {
        foreach (var b in _gameButtons) b.ButtonClicked -= ButtonClicked;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_levelEnded || _bombCount <= 0) return;
        if (eventData.pointerEnter == null) return;
        var clikedCell = eventData.pointerEnter.transform;
        var data = _gridCellDictionary.First(x => x.Value.CellTr == clikedCell);
        var grid = data.Key;
        var cell = data.Value;
        if (cell.CellType == 0) InsertBomb(grid, cell);
    }

    private void InsertBomb(int grid, LevelSetter.Cell p)
    {
        p.Bombed();
        _bombCount--;
        _bombedGrids.Add(grid);
        levelManager.SetBombCountText(_bombCount);
        if (_bombCount == 0) StartCoroutine(EndLevel());
        else if (CheckIfLevelEnds()) StartCoroutine(EndLevel(true));
    }

    private bool CheckIfLevelEnds()
    {
        var levelCompleted = true;
        foreach (var gridNumber in _filledGrids.Except(_filledDestroyableGrids))
        {
            var gridIsDestroyable = false;
            var neighbours = GridHelper.GetNeighbours(_levelData,gridNumber);
            for (int i = 0; i < neighbours.Count; i++)
            {
                if (_gridCellDictionary[neighbours[i]].CellType == 2)
                {
                    gridIsDestroyable = true;
                    break;
                }
            }
            if (!gridIsDestroyable) levelCompleted = false;
            else _filledDestroyableGrids.Add(gridNumber);
        }
        return levelCompleted;
    }

    private IEnumerator EndLevel(bool completed = false)
    {
        if(!completed) completed = CheckIfLevelEnds();
        _levelEnded = true;
        foreach (var gridNumber in _bombedGrids)
        {
            _gridCellDictionary[gridNumber].Exploded();
        }

        foreach (var gridNumber in _filledDestroyableGrids)
        {
            _gridCellDictionary[gridNumber].Exploded();
            _filledGrids.Remove(gridNumber);
        }

        yield return new WaitForSeconds(0.75f);
        if (!completed)
        {
            foreach (var gridNumber in _filledGrids)
            {
                _gridCellDictionary[gridNumber].CellTr.DOScale(1.1f, 0.2f)
                    .SetLoops(6, LoopType.Yoyo);
            }
        }
        var t = completed ? 0.5f : 2f;
        yield return new WaitForSeconds(t);
        var score = completed ? _bombCount > 0 ? _bombCount > 1 ? 3 : 2 : 1 : 0; //funny
        endPanel.SetPanel(completed, score,_endPanelScale);
        if (completed) LevelDataHandler.LevelEnded(_levelNumber, score);
    }

    private void SetLevel(LevelData data)
    {
        _levelData = data;
        _gridWidth = data.GridWidth;
        _gridHeight = data.GridHeight;
        _levelNumber = data.LevelNumber;
        _filledGrids = data.FilledGrids.ToList();
        _bombCount = data.BombCount;
        var smallerScale = _gridWidth > _gridHeight ? _gridHeight : _gridWidth;
        _endPanelScale= smallerScale / 4f;
        levelManager.SetLevel(_levelData, ref _gridCellDictionary);
    }

    private void ButtonClicked(GameButton.ButtonTypeEnum buttonType)
    {
        if (_buttonClicked) return;
        _buttonClicked = true;
        endPanel.transform.SetParent(transform);
        levelManager.transform.SetParent(transform);
        transform.DOScale(0, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                switch (buttonType)
                {
                    case GameButton.ButtonTypeEnum.MenuButton:
                        _sceneHandler.LoadMenu();
                        break;
                    case GameButton.ButtonTypeEnum.ReplayButton:
                        _sceneHandler.LoadLevel(_levelData.LevelNumber);
                        break;
                }
            });
    }
}