using UnityEngine;

public class GridManager : MonoBehaviour
{
    private GridData<bool> _isCellBlocked;
    private GridData<SpriteRenderer> _cellSpriteRenderersData;
    private static readonly Vector2Int _invalidCellIndex = new Vector2Int(-1 , -1);
    
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GridInfo gridInfo;

    public GridData<bool> IsCellBlocked
    {
        get => _isCellBlocked;
        set => _isCellBlocked = value;
    }

    public GridData<SpriteRenderer> CellSpriteRenderersData
    {
        get => _cellSpriteRenderersData;
        set => _cellSpriteRenderersData = value;
    }

    private void Awake()
    {
        CellSpriteRenderersData = new GridData<SpriteRenderer>(gridInfo);
        IsCellBlocked = new GridData<bool>(gridInfo);
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        for(int col = 0; col < gridInfo.Cols; col++)
        {
            for(int row = 0; row < gridInfo.Rows; row++)
            {
                Vector2 cellWorldPos = CellToWorld(col , row);
                GameObject cell = Instantiate(cellPrefab , cellWorldPos , Quaternion.identity , transform);
                
                CellSpriteRenderersData.SetValue(col , row , cell.GetComponentInChildren<SpriteRenderer>());
                CellSpriteRenderersData.GetValue(col , row).color = (col + row) % 2  == 0 ? Color.white : Color.black;
            }
        }
    }

    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        Vector2 localPosition = (worldPosition / gridInfo.CellSize - transform.position);
        
        int col = Mathf.FloorToInt(localPosition.x);
        int row = Mathf.FloorToInt(localPosition.y);
        
        if(col < 0 || col >= gridInfo.Cols || row < 0 || row >= gridInfo.Rows)
        {
            return _invalidCellIndex;
        }

        return new Vector2Int(col , row);
    }

    public Vector2 CellToWorld(int col , int row)
    {
        var x = (col * gridInfo.CellSize) + transform.position.x;
        var y = row * gridInfo.CellSize  + transform.position.y;
        return new Vector2(x , y);
    }
}