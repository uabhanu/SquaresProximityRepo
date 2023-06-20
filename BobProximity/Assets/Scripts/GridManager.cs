using UnityEngine;

public class GridManager : MonoBehaviour
{
    private GridData<bool> _isCellBlockedData;
    private GridData<GameObject> _coinOnTheCellData;
    private GridData<int> _coinValueData;
    private GridData<int> _playerIndexData;
    private GridData<SpriteRenderer> _cellSpriteRenderersData;
    private readonly Vector2Int _invalidCellIndex = new Vector2Int(-1 , -1);
    
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GridInfo gridInfo;

    public GridData<bool> IsCellBlockedData
    {
        get => _isCellBlockedData;
        set => _isCellBlockedData = value;
    }
    
    public GridData<GameObject> CoinOnTheCellData
    {
        get => _coinOnTheCellData;
        set => _coinOnTheCellData = value;
    }
    
    public GridData<int> CoinValueData
    {
        get => _coinValueData;
        set => _coinValueData = value;
    }
    
    public GridData<int> PlayerIndexData
    {
        get => _playerIndexData;
        set => _playerIndexData = value;
    }

    public GridData<SpriteRenderer> CellSpriteRenderersData
    {
        get => _cellSpriteRenderersData;
        set => _cellSpriteRenderersData = value;
    }
    
    public GridInfo GridInfo => gridInfo;

    public Vector2Int InvalidCellIndex => _invalidCellIndex;

    private void Awake()
    {
        CellSpriteRenderersData = new GridData<SpriteRenderer>(GridInfo);
        CoinOnTheCellData = new GridData<GameObject>(GridInfo);
        CoinValueData = new GridData<int>(GridInfo);
        IsCellBlockedData = new GridData<bool>(GridInfo);
        PlayerIndexData = new GridData<int>(GridInfo);
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        for(int col = 0; col < GridInfo.Cols; col++)
        {
            for(int row = 0; row < GridInfo.Rows; row++)
            {
                Vector2 cellWorldPos = CellToWorld(col , row);
                GameObject cell = Instantiate(cellPrefab , cellWorldPos , Quaternion.identity , transform);
                CellSpriteRenderersData.SetValue(col , row , cell.GetComponentInChildren<SpriteRenderer>());
            }
        }
    }
    
    public Vector2 CellToWorld(int col , int row)
    {
        var x = (col * GridInfo.CellSize) + transform.position.x;
        var y = row * GridInfo.CellSize  + transform.position.y;
        return new Vector2(x , y);
    }

    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        Vector2 localPosition = (worldPosition / GridInfo.CellSize - transform.position);
        
        int col = Mathf.FloorToInt(localPosition.x);
        int row = Mathf.FloorToInt(localPosition.y);
        
        if(col < 0 || col >= GridInfo.Cols || row < 0 || row >= GridInfo.Rows)
        {
            return InvalidCellIndex;
        }

        return new Vector2Int(col , row);
    }
}