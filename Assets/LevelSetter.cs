using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class LevelSetter : MonoBehaviour
{
    [SerializeField] private Transform border;
    [SerializeField] private GameObject menuButton;
    [SerializeField] private TextMeshPro levelText;
    [SerializeField] private TextMeshPro bombCountText;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameManager gameManager;

    public class Cell
    {
        public Transform CellTr;
        public int CellType; //0 is empty, 1 is filled, 2 is bombed

        public void Exploded()
        {
            CellTr.GetChild(2).DOScale(1.2f, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    for (int i = 0; i < CellTr.childCount; i++)
                    {
                        var active = i == 0;
                        CellTr.GetChild(i).gameObject.SetActive(active);
                    }
                });
        }

        public void Bombed()
        {
            CellTr.GetChild(2).gameObject.SetActive(true);
            CellType = 2;
        }
    }

    public void SetLevel(LevelData data, ref Dictionary<int, Cell> gridCellDictionary)
    {
        var gridWidth = data.GridWidth;
        var gridHeight = data.GridHeight;
        var filled=data.FilledGrids;
        bombCountText.text = data.BombCount.ToString();
        border.localScale = new Vector3(3f * gridWidth + 0.1f, 3f * gridHeight + 0.1f, 1);
        levelText.text = "Level-" + data.LevelNumber;
        
        var iCount = 0;
        for (int i = -gridHeight + 1; i < gridHeight; i += 2)
        {
            var jCount = 0;
            for (int j = -gridWidth + 1; j < gridWidth; j += 2)
            {
                var spawnPos = new Vector3(j / 2f, i / 2f, 0);
                var gridNumber = iCount * gridWidth + jCount;
                var cellType = data.Grid[gridNumber];
                var p = Instantiate(cellPrefab, spawnPos, Quaternion.identity, gameManager.transform);
                p.transform.GetChild(cellType).gameObject.SetActive(true);
                var cell = new Cell {CellTr = p.transform, CellType = cellType};
                gridCellDictionary.Add(gridNumber, cell);
                jCount++;
            }

            iCount++;
        }
        SetUI();
    }

    private void SetUI()
    {
        var cam = Camera.main;
        var buttonPos = cam.ScreenToWorldPoint(new Vector2(50, cam.pixelHeight - 125));
        buttonPos.z = 0;
        menuButton.transform.position = buttonPos;
        var levelPos = cam.ScreenToWorldPoint(new Vector2(cam.pixelWidth / 2f, cam.pixelHeight - 125));
        levelPos.z = 0;
        levelText.transform.position = levelPos;
        var bombPos = cam.ScreenToWorldPoint(new Vector2(cam.pixelWidth - 50, cam.pixelHeight - 125));
        bombPos.z = 0;
        bombCountText.transform.parent.transform.position = bombPos;
    }

    public void SetBombCountText(int count)
    {
        bombCountText.text = count.ToString();
    }
}