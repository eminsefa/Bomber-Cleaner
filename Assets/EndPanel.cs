using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class EndPanel : MonoBehaviour
{
    private const string WinText = "CONGRATS!";
    private const string LoseText = "GAMEOVER!";
    [SerializeField] private Transform dimed;
    [SerializeField] private TextMeshPro endText;
    [SerializeField] private List<SpriteRenderer> stars;

    public void SetPanel(bool win, int score,float scale)
    {
        endText.text = win ? WinText : LoseText;
        
        for (int i = 0; i < score; i++) stars[i].color=Color.white;
        
        dimed.SetParent(null);
        dimed.localScale=Vector3.one*50;
        transform.DOScale(scale, 0.5f)
            .SetEase(Ease.OutBack);
    }
}
