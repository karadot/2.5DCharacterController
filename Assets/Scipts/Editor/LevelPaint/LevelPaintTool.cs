using Scipts.LevelData;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.XR;

namespace Scipts.Editor
{
    [EditorTool("Level Paint Tool", typeof(LevelDataHolder))]
    public class LevelPaintTool : EditorTool
    {
        private Grid _selectedGrid;
        private LevelDataHolder _levelDataHolder;

        private bool isMouseDown = false;

        private Vector3Int _lastProcessedCell;

        private int _activePaintObjectIndex;

        private LevelPaintData[] _paintDatas;
        private int _activeDataIndex;
        private GUIContent[] _paintDataGUIContents;

        private GridLineData[][] gridLines = new GridLineData[2][];

        public override void OnToolGUI(EditorWindow window)
        {
            if (window is not SceneView sceneView)
                return;
            if (!_selectedGrid || !_levelDataHolder) return;
            Event current = Event.current;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var mousePosition = Event.current.mousePosition;
            if (!_isPositioning && !_isResizing && !_settingsAreaRect.Contains(mousePosition) &&
                !_resizeAreaRect.Contains(mousePosition))
            {
                Plane gridPlane = new Plane(Vector3.back, 0);
                var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                gridPlane.Raycast(ray, out float enter);
                var getClosestGrid = GetClosestGridPos(ray.origin + ray.direction * enter);
                var closestGridWorldPos = GetWorldPos(getClosestGrid);
                Handles.DrawSolidDisc(closestGridWorldPos, Vector3.forward, .5f);
                Handles.color = Color.red;
                Handles.DrawSolidDisc(ray.origin + ray.direction * enter, Vector3.forward, .2f);
                if (Event.current.button == 0)
                {
                    switch (Event.current.type)
                    {
                        case EventType.MouseDown:
                            _lastProcessedCell = getClosestGrid;
                            DoPaint(closestGridWorldPos);
                            break;
                        case EventType.MouseDrag:
                            if (_lastProcessedCell != getClosestGrid)
                            {
                                _lastProcessedCell = getClosestGrid;
                                DoPaint(closestGridWorldPos);
                            }

                            break;
                    }
                }
            }

            Handles.color = Color.blue;

            var camPos = sceneView.camera.transform.position;
            camPos.z = 0;
            var offset = _selectedGrid.CellToWorld(_selectedGrid.WorldToCell(camPos));
            for (int i = 0; i < 101; i++)
            {
                Handles.DrawLine(gridLines[0][i].StartPoint + offset, gridLines[0][i].EndPoint + offset);
                Handles.DrawLine(gridLines[1][i].StartPoint + offset, gridLines[1][i].EndPoint + offset);
            }


            DrawSettings();
        }

        void DoPaint(Vector3 closestGridWorldPos)
        {
            if (_deleteMode)
            {
                ClearObjectAtWorldPos(closestGridWorldPos);
            }
            else
            {
                ClearObjectAtWorldPos(closestGridWorldPos);
                var spawnedItem = PrefabUtility.InstantiatePrefab(
                    _paintDatas[_activeDataIndex].PaintObjects[_activePaintObjectIndex],
                    _levelDataHolder.transform) as GameObject;
                var ts = spawnedItem.transform;
                ts.position = closestGridWorldPos;
                ts.localScale = _selectedGrid.cellSize;
                EditorUtility.SetDirty(_levelDataHolder.gameObject);
            }
        }

        void ClearObjectAtWorldPos(Vector3 worldPos)
        {
            var parentTs = _levelDataHolder.transform;
            for (int i = parentTs.childCount - 1; i >= 0; i--)
            {
                if (parentTs.GetChild(i).position.Equals(worldPos))
                {
                    DestroyImmediate(parentTs.GetChild(i).gameObject);
                    EditorUtility.SetDirty(_levelDataHolder.gameObject);
                }
            }
        }

        #region Settings

        private bool _deleteMode;
        private GameObject _spawnItem;

        private Rect _settingsAreaRect;
        private Vector2 _scrollValue;

        private Rect _resizeAreaRect;
        private bool _isResizing;
        private Vector2 _resizeStartMousePos, _resizeStartAreaPos;

        private Rect _positionRect = new Rect();
        private bool _isPositioning;
        private Vector2 _moveStartMousePos, _moveStartAreaPos;

        public void DrawSettings()
        {
            var mousePosition = Event.current.mousePosition;
            if (Event.current.button == 0)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (_resizeAreaRect.Contains(mousePosition))
                        {
                            _resizeStartMousePos = mousePosition;
                            _resizeStartAreaPos = _resizeAreaRect.position;
                            _isResizing = true;
                            return;
                        }

                        if (_positionRect.Contains(mousePosition))
                        {
                            Debug.Log("Mouse Over");
                            _moveStartMousePos = mousePosition;
                            _moveStartAreaPos = _positionRect.position;
                            _isPositioning = true;
                            return;
                        }

                        break;
                    case EventType.MouseUp:
                        _isResizing = false;
                        _isPositioning = false;
                        break;
                    case EventType.MouseDrag:
                        if (_isResizing)
                        {
                            _resizeAreaRect.x = _resizeStartAreaPos.x + (mousePosition.x - _resizeStartMousePos.x);
                            _resizeAreaRect.y = _resizeStartAreaPos.y + (mousePosition.y - _resizeStartMousePos.y);
                            _settingsAreaRect.width = _resizeAreaRect.x - _settingsAreaRect.x + _resizeRectSize;
                            _settingsAreaRect.height = _resizeAreaRect.y - _settingsAreaRect.y + _resizeRectSize;
                        }

                        if (_isPositioning)
                        {
                            _settingsAreaRect.x = _moveStartAreaPos.x + (mousePosition.x - _moveStartMousePos.x);
                            _settingsAreaRect.y = _moveStartAreaPos.y + (mousePosition.y - _moveStartMousePos.y);
                        }

                        break;
                }
            }

            Handles.BeginGUI();
            //DrawDraggableSettings(0);
            DrawDraggableSettings();
            GUI.Button(_resizeAreaRect, "X");
            UpdateResizeRect();
            Handles.EndGUI();
        }

        private int _labelHeight = 23;

        void DrawDraggableSettings()
        {
            var style = new GUIStyle(GUI.skin.box);
            var styleBlack = new GUIStyle(GUI.skin.box);
            styleBlack.fontSize = 21;
            styleBlack.fontStyle = FontStyle.Bold;
            styleBlack.normal.scaledBackgrounds = new[] {Texture2D.blackTexture};


            GUILayout.BeginArea(_settingsAreaRect, style);
            _positionRect = _settingsAreaRect;
            _positionRect.height = _labelHeight;
            var currBGColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;
            GUILayout.BeginVertical(styleBlack);
            GUILayout.Label("Paint Options", GUILayout.MinHeight(_labelHeight),
                GUILayout.MaxHeight(_labelHeight));
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Theme");
            _activeDataIndex = EditorGUILayout.Popup(_activeDataIndex, _paintDataGUIContents);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = currBGColor;
            _deleteMode = EditorGUILayout.Toggle("Delete Mode", _deleteMode);
            _scrollValue = GUILayout.BeginScrollView(_scrollValue);
            GUILayout.BeginVertical();
            for (int i = 0; i < _paintDatas[_activeDataIndex].PaintObjects.Count; i++)
            {
                if (GUILayout.Button(_paintDatas[_activeDataIndex].PaintObjects[i].gameObject.name))
                    _activePaintObjectIndex = i;
            }

            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        #endregion


        public override void OnActivated()
        {
            _levelDataHolder = target as LevelDataHolder;
            if (_levelDataHolder != null) _selectedGrid = _levelDataHolder.GetComponent<Grid>();
            _paintDatas = Resources.LoadAll<LevelPaintData>("");
            _paintDataGUIContents = new GUIContent[_paintDatas.Length];
            for (int i = 0; i < _paintDatas.Length; i++)
            {
                _paintDataGUIContents[i] = new GUIContent(_paintDatas[i].name);
            }

            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Entering Platform Tool"), .1f);
            _settingsAreaRect = new Rect(50, 5, 200, 200);
            UpdateResizeRect();
            gridLines[0] = new GridLineData[101];
            gridLines[1] = new GridLineData[101];

            for (int i = 0; i < 101; i++)
            {
                var gridIndex = i - 50;
                gridLines[0][i] = new GridLineData
                {
                    StartPoint = GetWorldPosForLines(new Vector3Int(gridIndex, -50, 0)),
                    EndPoint = GetWorldPosForLines(new Vector3Int(gridIndex, 50, 0))
                };
                gridLines[1][i] = new GridLineData
                {
                    StartPoint = GetWorldPosForLines(new Vector3Int(-50, gridIndex, 0)),
                    EndPoint = GetWorldPosForLines(new Vector3Int(50, gridIndex, 0))
                };
            }
        }

        private readonly float _resizeRectSize = 25f;

        void UpdateResizeRect()
        {
            _resizeAreaRect = new Rect(_settingsAreaRect.x + _settingsAreaRect.width - _resizeRectSize,
                _settingsAreaRect.y + _settingsAreaRect.height - _resizeRectSize, _resizeRectSize,
                _resizeRectSize);
        }

        public override void OnWillBeDeactivated()
        {
            _levelDataHolder = null;
            _selectedGrid = null;
            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Exiting Platform Tool"), .1f);
        }

        Vector3 GetWorldPos(Vector3Int gridPos)
        {
            return _selectedGrid.GetCellCenterWorld(gridPos);
        }

        Vector3 GetWorldPosForLines(Vector3Int gridPos)
        {
            return _selectedGrid.GetCellCenterWorld(gridPos) + _selectedGrid.cellSize * .5f;
        }

        Vector3Int GetClosestGridPos(Vector3 worldPos)
        {
            return _selectedGrid.WorldToCell(worldPos);
        }
    }
}

public struct GridLineData
{
    public Vector3 StartPoint, EndPoint;
}