using TMPro;
using UnityEngine;

public class TextRenderer : MonoBehaviour
{
    private GridManager _gridManager;
    private Vector2Int _currentIndex;
    
    [SerializeField] private TextMeshPro valueLabelTextTMP;

    private void Awake()
    {
        _gridManager = FindObjectOfType<GridManager>();
    }

    private void Update()
    {
        _currentIndex = _gridManager.WorldToCell(transform.position);

        if(_currentIndex != null && _gridManager.CellSpriteRenderersData != null)
        {
            //TODO Still getting NullReferenceException so this is for some other time
            Color cellColor = GetCellColor(_currentIndex);
            valueLabelTextTMP.color = GetContrastingColor(cellColor);   
        }
    }

    private Color GetCellColor(Vector2Int cellIndex)
    {
        return _gridManager.CellSpriteRenderersData.GetValue(cellIndex.x , cellIndex.y).color;
    }

    private Color GetContrastingColor(Color color)
    {
        float luminance = (0.299f * color.r + 0.587f * color.g + 0.114f * color.b);
        return luminance > 0.5f ? Color.black : Color.white;
    }
}
