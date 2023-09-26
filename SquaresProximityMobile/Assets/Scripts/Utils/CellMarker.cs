using Managers;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    public class CellMarker : MonoBehaviour
    {
        private GridManager _gridManager;
        private Vector2Int _currentIndex;
        
        [SerializeField] private GameObject indexLabelTMPObj;
        [SerializeField] private TextMeshPro indexLabelTextTMP;

        private void Awake()
        {
            _gridManager = FindObjectOfType<GridManager>();
        }

        private void Update()
        {
            #if UNITY_EDITOR
            
                _currentIndex = _gridManager.WorldToCell(transform.position);
                
                if(EditorApplication.isPlaying)
                {
                    indexLabelTMPObj.SetActive(true);
                    indexLabelTextTMP.text = _currentIndex.ToString();

                    Color cellColor = GetCellColor(_currentIndex);
                    indexLabelTextTMP.color = GetContrastingColor(cellColor);
                }
            
            #endif
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
}