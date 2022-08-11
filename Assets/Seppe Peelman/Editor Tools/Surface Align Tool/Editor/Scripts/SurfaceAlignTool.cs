#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor;
using System.Reflection;
using System;
using UnityEditor.SceneManagement;

namespace SeppePeelman.EditorTools.SurfaceAlignTool
{
    [EditorTool("Surface Align Tool")]
    public class SurfaceAlignTool : EditorTool
    {
        [SerializeField] Texture2D _toolIcon;
        GUIContent _iconContent;

        private SurfaceAlignToolSettings _settings;

        private Transform _activeTransform;
        private List<Transform> _activeTransformChildren = new List<Transform>();
        private List<TransformLayer> _activeTransformChildrenLayers = new List<TransformLayer>();
        private SphereCollider _activeTransformCollider;
        private Renderer _activeTransformRenderer;

        private object _sceneOverlayWindow;
        private MethodInfo _showSceneViewOverlay;

        private float _previousGuiHotcontrol;

        private const string SETTINGS_NAME = "SurfaceAlignTool_Settings";
        private const int IGNORE_RAYCAST_LAYER = 2;



        void OnEnable()
        {
            _iconContent = new GUIContent()
            {
                image = _toolIcon,
                text = "Surface Align Tool",
                tooltip = "Surface Align Tool"
            };

            _settings = Resources.Load(SETTINGS_NAME) as SurfaceAlignToolSettings;
#if UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanged += ToolManager_activeToolChanged;
#else
            UnityEditor.EditorTools.EditorTools.activeToolChanged += ToolManager_activeToolChanged;
#endif

            EnableSettingsWindow();

            _previousGuiHotcontrol = GUIUtility.hotControl;
        }

        private void OnDisable()
        {

#if UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanged -= ToolManager_activeToolChanged;
#else
            UnityEditor.EditorTools.EditorTools.activeToolChanged -= ToolManager_activeToolChanged;
#endif
        }

        private void ToolManager_activeToolChanged()
        {
#if UNITY_2020_2_OR_NEWER
            if (ToolManager.IsActiveTool(this))
#else
            if (UnityEditor.EditorTools.EditorTools.IsActiveTool(this))
#endif
            {
                _settings = Resources.Load(SETTINGS_NAME) as SurfaceAlignToolSettings;
                return;
            }
            if (_activeTransformCollider != null)
            {
                DestroyImmediate(_activeTransformCollider.gameObject);
                ResetLayer();
                _activeTransform = null;
            }
        }

        public override GUIContent toolbarIcon
        {
            get { return _iconContent; }
        }

        private void SetActiveTransform(Transform transform)
        {
            if (_activeTransform != transform)
            {
                DestroyCollider();
                ResetLayer();

                _activeTransform = transform;

                if (_activeTransform == null) { return; }

                GameObject tempColliderObject = new GameObject("SurfaceAlignTool_Temp_Collider");
                tempColliderObject.transform.localScale = Vector3.one;
                tempColliderObject.transform.SetParent(_activeTransform);
                tempColliderObject.transform.localPosition = Vector3.zero;
                tempColliderObject.transform.localEulerAngles = Vector3.zero;
                tempColliderObject.hideFlags = HideFlags.HideAndDontSave;

                _activeTransformCollider = tempColliderObject.AddComponent<SphereCollider>();
                _activeTransformCollider.center = Vector3.zero;
                _activeTransformCollider.radius = _settings.SnapRadius;

                _activeTransformChildren.Clear();
                FillActiveTransformChildrenListRecursive(_activeTransform);

                _activeTransformRenderer = _activeTransform.GetComponent<Renderer>();
                if (_activeTransformRenderer != null)
                {
                    Vector3 boundsExtents = _activeTransformRenderer.bounds.extents;
                    float largestExtent = 0f;
                    if (boundsExtents.x > largestExtent) { largestExtent = boundsExtents.x; }
                    if (boundsExtents.y > largestExtent) { largestExtent = boundsExtents.y; }
                    if (boundsExtents.z > largestExtent) { largestExtent = boundsExtents.z; }

                    _activeTransformCollider.radius += largestExtent;
                }

                Physics.autoSyncTransforms = true;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            //If we're not in the scene view, exit.
            if (!(window is SceneView)) { return; }

            //If we're not the active tool, exit.
#if UNITY_2020_2_OR_NEWER
            if (!ToolManager.IsActiveTool(this))
#else
            if (!UnityEditor.EditorTools.EditorTools.IsActiveTool(this))
#endif
            {
                DestroyCollider();
                ResetLayer();
                return;
            }

            if (_settings == null) { return; }

            // Get selected object
            SetActiveTransform(Selection.activeTransform);
            if (!_activeTransform) { return; }
            if (!_activeTransformCollider) { return; }

            if (_showSceneViewOverlay != null && _sceneOverlayWindow != null) { _showSceneViewOverlay.Invoke(null, new object[] { _sceneOverlayWindow }); };

            KeepColliderScale();

            EditorGUI.BeginChangeCheck();

            Quaternion handleRotation = _activeTransform.rotation;
            if (UnityEditor.Tools.pivotRotation == PivotRotation.Global)
            {
                handleRotation = Quaternion.Euler(Vector3.zero);
            }

            Vector3 targetPosition = Handles.PositionHandle(_activeTransform.position, handleRotation);
            Quaternion targetRotation = _activeTransform.rotation;

            Handles.color = Color.yellow;
            Handles.FreeMoveHandle(1, _activeTransform.position, .25f * HandleUtility.GetHandleSize(_activeTransform.position), Vector3.one, Handles.SphereHandleCap);

            Vector3 mouseRaycastNormal = Vector3.zero;

            // Handle mouse control (FreeMoveHandle)
            if (GUIUtility.hotControl == 1) 
            {
                if (_previousGuiHotcontrol != 1)
                {
                    SetupLayer();
                    _previousGuiHotcontrol = GUIUtility.hotControl;
                }
                targetPosition = GetCurrentMousePositionInScene(ref mouseRaycastNormal);
                targetRotation = _activeTransform.rotation;
            }
            else if (GUIUtility.hotControl != _previousGuiHotcontrol)
            {
                if (_previousGuiHotcontrol == 1)
                {
                    ResetLayer();
                }
                _previousGuiHotcontrol = GUIUtility.hotControl;
            }

            // Check colliders in range
            Collider[] hitColliders = Physics.OverlapSphere(targetPosition, _activeTransformCollider.radius);
            List<Collider> validColliders = new List<Collider>();

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].transform != _activeTransform && !_activeTransformChildren.Contains(hitColliders[i].transform) && hitColliders[i] != _activeTransformCollider)
                {
                    validColliders.Add(hitColliders[i]);
                }
            }

            // Snap and align to collider
            if (validColliders.Count > 0)
            {
                if (GUIUtility.hotControl == 1) // Using mouse
                {
                    targetRotation = GetTargetRotation(mouseRaycastNormal);
                    targetPosition += mouseRaycastNormal * _settings.DepthOffset;

                    if (_settings.AddObjectBoundsToDepthOffset && _activeTransformRenderer != null)
                    {
#if UNITY_2021_2_OR_NEWER
                        targetPosition += mouseRaycastNormal * GetUpAxisBoundsExtents(_activeTransformRenderer.localBounds);
#else
                        if(_activeTransformRenderer.GetType() == typeof(MeshRenderer))
                        {
                            Mesh mesh = _activeTransformRenderer.GetComponent<MeshFilter>().sharedMesh;
                            if (mesh != null) { targetPosition += mouseRaycastNormal * GetUpAxisBoundsExtents(mesh.bounds); }
                        }
                        else if(_activeTransformRenderer.GetType() == typeof(SpriteRenderer))
                        {
                            Sprite sprite = _activeTransformRenderer.GetComponent<SpriteRenderer>().sprite;
                            if (sprite != null) { targetPosition += mouseRaycastNormal * GetUpAxisBoundsExtents(sprite.bounds); }
                        }
#endif
                    }
                }
                else // Using handles
                {
                    List<NormalPoint> colliderPoints = new List<NormalPoint>();
                    int closestPointIndex = 0;
                    float closestDistance = float.MaxValue;

                    NormalPoint closestPoint = null;

                    for (int i = 0; i < validColliders.Count; i++)
                    {
                        colliderPoints.Add(GetClosestNormalPointOnCollider(validColliders[i], targetPosition));

                        float distanceToPoint = Vector3.Distance(colliderPoints[i].Position, _activeTransform.position);

                        if (distanceToPoint < closestDistance)
                        {
                            closestDistance = distanceToPoint;
                            closestPointIndex = i;
                        }
                    }

                    if (closestPointIndex < colliderPoints.Count)
                    {
                        closestPoint = colliderPoints[closestPointIndex];
                    }

                    targetRotation = GetTargetRotation(closestPoint.Normal);
                    targetPosition = closestPoint.Position;
                    targetPosition += closestPoint.Normal * _settings.DepthOffset;

                    if (_settings.AddObjectBoundsToDepthOffset && _activeTransformRenderer != null)
                    {
#if UNITY_2021_2_OR_NEWER
                        targetPosition += closestPoint.Normal * GetUpAxisBoundsExtents(_activeTransformRenderer.localBounds);
#else
                        if(_activeTransformRenderer.GetType() == typeof(MeshRenderer))
                        {
                            Mesh mesh = _activeTransformRenderer.GetComponent<MeshFilter>().sharedMesh;
                            if (mesh != null) { targetPosition += closestPoint.Normal * GetUpAxisBoundsExtents(mesh.bounds); }
                        }
                        else if(_activeTransformRenderer.GetType() == typeof(SpriteRenderer))
                        {
                            Sprite sprite = _activeTransformRenderer.GetComponent<SpriteRenderer>().sprite;
                            if (sprite != null) { targetPosition += closestPoint.Normal * GetUpAxisBoundsExtents(sprite.bounds); }
                        }
#endif
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_activeTransform, "Move " + _activeTransform.name);
                _activeTransform.position = targetPosition;
                _activeTransform.rotation = targetRotation;
                window.Repaint();
            }
        }

        private Vector3 GetCurrentMousePositionInScene(ref Vector3 normal)
        {
            Vector3 mousePosition = Event.current.mousePosition;

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            int layerMask = ~LayerMask.GetMask("Ignore Raycast");
            float range = _settings.RaycastDistance;
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range, layerMask))
            {
                normal = hit.normal;
                return hit.point;
            }
            else
            {
                return ray.origin + (ray.direction * range);
            }
        }

        private NormalPoint GetClosestNormalPointOnCollider(Collider targetCollider, Vector3 fromPosition)
        {
            Vector3 closestPoint = Vector3.zero;
            Vector3 surfaceNormal = Vector3.zero;
            float surfacePenetrationDepth;

            if (targetCollider.GetType() == typeof(MeshCollider) && targetCollider.transform.localScale == Vector3.one)
            {
                MeshCollider meshCollider = targetCollider.GetComponent<MeshCollider>();
                var closestPointCalculator = new BaryCentricDistance(meshCollider.sharedMesh, targetCollider.transform);
                BaryCentricDistance.Result result = closestPointCalculator.GetClosestTriangleAndPoint(fromPosition);
                fromPosition = result.closestPoint;
            }

            if (Physics.ComputePenetration(targetCollider, targetCollider.transform.position, targetCollider.transform.rotation, _activeTransformCollider, fromPosition, _activeTransform.rotation, out surfaceNormal, out surfacePenetrationDepth))
            {
                closestPoint = fromPosition + (surfaceNormal * (_activeTransformCollider.radius - surfacePenetrationDepth));
                surfaceNormal = -surfaceNormal;
            }


            if (closestPoint == Vector3.zero)
            {
                closestPoint = fromPosition;
                surfaceNormal = _activeTransform.up;
            }

            return new NormalPoint(closestPoint, surfaceNormal.normalized);
        }

        private Quaternion GetTargetRotation(Vector3 normal)
        {
            Vector3 upAxis = _activeTransform.up;
            switch (_settings.UpAxis)
            {
                case UpAxis.X:
                    upAxis = _activeTransform.right;
                    break;
                case UpAxis.Y:
                    upAxis = _activeTransform.up;
                    break;
                case UpAxis.Z:
                    upAxis = _activeTransform.forward;
                    break;
                case UpAxis.NegativeX:
                    upAxis = -_activeTransform.right;
                    break;
                case UpAxis.NegativeY:
                    upAxis = -_activeTransform.up;
                    break;
                case UpAxis.NegativeZ:
                    upAxis = -_activeTransform.forward;
                    break;
            }

            return Quaternion.FromToRotation(upAxis, normal) * _activeTransform.rotation;
        }

        private float GetUpAxisBoundsExtents(Bounds bounds)
        {
            float extents = 0;
            switch (_settings.UpAxis)
            {
                case UpAxis.X:
                    extents = bounds.extents.x * _activeTransform.lossyScale.x;
                    break;
                case UpAxis.Y:
                    extents = bounds.extents.y * _activeTransform.lossyScale.y;
                    break;
                case UpAxis.Z:
                    extents = bounds.extents.z * _activeTransform.lossyScale.z;
                    break;
                case UpAxis.NegativeX:
                    extents = bounds.extents.x * _activeTransform.lossyScale.x;
                    break;
                case UpAxis.NegativeY:
                    extents = bounds.extents.y * _activeTransform.lossyScale.y;
                    break;
                case UpAxis.NegativeZ:
                    extents = bounds.extents.z * _activeTransform.lossyScale.z;
                    break;
            }

            return extents;
        }

        private void KeepColliderScale()
        {
            if (!_activeTransform || !_activeTransformCollider) { return; }
            if (_activeTransform.localScale.x != 0 && _activeTransform.localScale.y != 0 && _activeTransform.localScale.z != 0)
            {
                _activeTransformCollider.transform.localScale = new Vector3((1f / _activeTransform.lossyScale.x), (1f / _activeTransform.lossyScale.y), (1f / _activeTransform.lossyScale.z));
            }
        }

        private void DestroyCollider()
        {
            if (_activeTransformCollider != null)
            {
                DestroyImmediate(_activeTransformCollider.gameObject);
            }
        }

        private void SetupLayer()
        {
            if (_activeTransform != null)
            {
                _activeTransformChildrenLayers.Clear();
                SetLayersRecursive(_activeTransform, IGNORE_RAYCAST_LAYER);
                _activeTransformCollider.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void ResetLayer()
        {
            if (_activeTransform != null)
            {
                foreach (TransformLayer transformLayer in _activeTransformChildrenLayers)
                {
                    if (transformLayer.Transform != null)
                    {
                        transformLayer.Transform.gameObject.hideFlags = HideFlags.None;
                        transformLayer.Transform.gameObject.layer = transformLayer.Layer;
                    }
                }
                if (_activeTransformCollider)
                {
                    _activeTransformCollider.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
        }

        private void SetLayersRecursive(Transform t, int layer)
        {
            _activeTransformChildrenLayers.Add(new TransformLayer(t, t.gameObject.layer));
            t.gameObject.hideFlags = HideFlags.DontSave;
            t.gameObject.layer = layer;

            for (int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                SetLayersRecursive(child, layer);
            }
        }

        private void FillActiveTransformChildrenListRecursive(Transform t)
        {
            _activeTransformChildren.Add(t);

            for (int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                FillActiveTransformChildrenListRecursive(child);
            }
        }

        private void EnableSettingsWindow()
        {
#if UNITY_2020_1_OR_NEWER
            Assembly unityEditor = Assembly.GetAssembly(typeof(UnityEditor.SceneView));
            Type overlayWindowType = unityEditor.GetType("UnityEditor.OverlayWindow");
            Type sceneViewOverlayType = unityEditor.GetType("UnityEditor.SceneViewOverlay");
            Type windowFuncType = sceneViewOverlayType.GetNestedType("WindowFunction");
            Delegate windowFunc = Delegate.CreateDelegate(windowFuncType, this.GetType().GetMethod(nameof(DoOverlayUI), BindingFlags.Static | BindingFlags.NonPublic));
            Type windowDisplayOptionType = sceneViewOverlayType.GetNestedType("WindowDisplayOption");
            _sceneOverlayWindow = Activator.CreateInstance(overlayWindowType,
                            EditorGUIUtility.TrTextContent("Surface Align Tool Settings     .", (string)null, (Texture)null), // Title
                            windowFunc, // Draw function of the window
                            int.MaxValue, // Priority of the window
                            (UnityEngine.Object)_settings, // Unity Obect that will be passed to the drawing function
                            Enum.Parse(windowDisplayOptionType, "OneWindowPerTarget") //SceneViewOverlay.WindowDisplayOption.OneWindowPerTarget
                        );
            _showSceneViewOverlay = sceneViewOverlayType.GetMethod("ShowWindow", BindingFlags.Static | BindingFlags.Public);
#endif
        }

#if UNITY_2020_1_OR_NEWER
        private static void DoOverlayUI(UnityEngine.Object settingsObject, SceneView sceneView)
        {
            SurfaceAlignToolSettings settings = (SurfaceAlignToolSettings)settingsObject;
            GUILayout.Space(10);
            settings.UpAxis = (UpAxis)EditorGUILayout.EnumPopup("Up Axis:", settings.UpAxis);
            settings.SnapRadius = EditorGUILayout.FloatField("Snap Radius:", settings.SnapRadius);
            settings.RaycastDistance = EditorGUILayout.FloatField("Ray Distance:", settings.RaycastDistance);
            settings.DepthOffset = EditorGUILayout.FloatField("Depth Offset:", settings.DepthOffset);
            settings.AddObjectBoundsToDepthOffset = EditorGUILayout.Toggle("Offset Bounds:", settings.AddObjectBoundsToDepthOffset);
        }
#endif
    }

    [System.Serializable]
    public class NormalPoint
    {
        public Vector3 Position;
        public Vector3 Normal;

        public NormalPoint(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }
    }

    [System.Serializable]
    public class TransformLayer
    {
        public Transform Transform;
        public int Layer;

        public TransformLayer(Transform transform, int layer)
        {
            Transform = transform;
            Layer = layer;
        }
    }
}
#endif