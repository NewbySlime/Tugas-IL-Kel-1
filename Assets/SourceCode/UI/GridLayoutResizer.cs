using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GridLayoutGroup))]
public class GridLayoutResizer: UIBehaviour{
  [SerializeField]
  private bool _ScaleCellSize;
  [SerializeField]
  private bool _ScaleSpacing;

  [SerializeField]
  private Vector2 _ScaleFactor = Vector2.one;

  private RectTransform _rect_transform;
  private GridLayoutGroup _grid_layout;

  private Vector2 _base_scale_cell;
  private Vector2 _base_spacing;

  private float _ratio_x;
  private float _ratio_y;

  public bool IsInitialized{private set; get;} = false;


  protected override void Start(){
    base.Start();

    _rect_transform = GetComponent<RectTransform>();
    _grid_layout = GetComponent<GridLayoutGroup>();

    _base_scale_cell = _grid_layout.cellSize;
    _base_spacing = _grid_layout.spacing;

    _ratio_x = _grid_layout.cellSize.x/_rect_transform.rect.width;
    _ratio_y = _grid_layout.cellSize.y/_rect_transform.rect.height;

    IsInitialized = true;
    OnRectTransformDimensionsChange();
  }


  protected override void OnRectTransformDimensionsChange(){
    base.OnRectTransformDimensionsChange();
    if(!IsInitialized)
      return;

    if(_ScaleCellSize){
      Vector2 _new_size = new Vector2(_rect_transform.rect.width, _rect_transform.rect.height) * new Vector2(_ratio_x, _ratio_y);

      Vector2 _diff_size = _new_size - _base_scale_cell;
      _grid_layout.cellSize = _diff_size*_ScaleFactor + _base_scale_cell; 
    }

    Vector2 _diff_ratio = _grid_layout.cellSize/_base_scale_cell;
    if(_ScaleSpacing)
      _grid_layout.spacing = _diff_ratio * _base_spacing;
  }
}