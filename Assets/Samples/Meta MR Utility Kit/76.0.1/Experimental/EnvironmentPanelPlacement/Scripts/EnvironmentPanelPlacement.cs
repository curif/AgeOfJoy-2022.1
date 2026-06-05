/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.EnvironmentPanelPlacement
{
    [MetaCodeSample("MRUKSample-EnvironmentPanelPlacement")]
    public class EnvironmentPanelPlacement : MonoBehaviour
    {
        [SerializeField] private EnvironmentRaycastManager _raycastManager;
        [SerializeField] private Transform _centerEyeAnchor;
        [SerializeField] private Transform _raycastAnchor;
        [SerializeField] private OVRInput.RawButton _grabButton = OVRInput.RawButton.RIndexTrigger | OVRInput.RawButton.RHandTrigger;
        [SerializeField] private OVRInput.RawAxis2D _scaleAxis = OVRInput.RawAxis2D.RThumbstick;
        [SerializeField] private OVRInput.RawAxis2D _moveAxis = OVRInput.RawAxis2D.RThumbstick;
        [SerializeField] private Transform _panel;
        [SerializeField] private float _panelAspectRatio = 0.823f;
        [SerializeField] private GameObject _panelGlow;
        [SerializeField] private LineRenderer _raycastVisualizationLine;
        [SerializeField] private Transform _raycastVisualizationNormal;

        private readonly RollingAverage _rollingAverageFilter = new RollingAverage();
        private Pose? _targetPose;
        private Vector3 _positionVelocity;
        private float _rotationVelocity;
        private bool _isGrabbing;
        private float _distanceFromController;
        private Pose? _environmentPose;
        private EnvironmentRaycastHitStatus _currentEnvHitStatus;
        private OVRSpatialAnchor _spatialAnchor;

        private IEnumerator Start()
        {
            // Wait until headset starts tracking
            enabled = false;
            while (!OVRPlugin.userPresent || !OVRManager.isHmdPresent)
            {
                yield return null;
            }
            yield return null;
            enabled = true;

            // Place the panel in front of the user
            var position = _centerEyeAnchor.position + _centerEyeAnchor.forward;
            var forward = Vector3.ProjectOnPlane(_centerEyeAnchor.position - position, Vector3.up).normalized;
            _panel.position = position;
            _panel.rotation = Quaternion.LookRotation(forward);

            // Create the OVRSpatialAnchor and make it a parent of the panel.
            // This will prevent the panel front drifting after headset lock/unlock.
            _spatialAnchor = new GameObject(nameof(OVRSpatialAnchor)).AddComponent<OVRSpatialAnchor>();
            _spatialAnchor.transform.SetPositionAndRotation(_panel.position, _panel.rotation);
            _panel.SetParent(_spatialAnchor.transform);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                _isGrabbing = false;
                _targetPose = null;
            }
        }

        private void Update()
        {
            if (!Application.isFocused)
            {
                return;
            }

            VisualizeRaycast();
            if (_isGrabbing)
            {
                UpdateTargetPose();
                if (OVRInput.GetUp(_grabButton))
                {
                    _panelGlow.SetActive(false);
                    _isGrabbing = false;
                    _environmentPose = null;

                    // If the existing OVRSpatialAnchor if further than 3 meters away from the current panel position, delete it and create a new one:
                    // https://developers.meta.com/horizon/documentation/unity/unity-spatial-anchors-best-practices#tips-for-using-spatial-anchors
                    if (_panel.localPosition.magnitude > 3f)
                    {
                        _spatialAnchor.EraseAnchorAsync();
                        DestroyImmediate(_spatialAnchor);

                        var parent = _panel.parent;
                        _panel.SetParent(null);
                        parent.SetPositionAndRotation(_panel.position, _panel.rotation);
                        _spatialAnchor = parent.gameObject.AddComponent<OVRSpatialAnchor>();
                        _panel.SetParent(parent);
                    }
                }
            }
            else
            {
                // Animate scale with right thumbstick
                const float scaleSpeed = 1.5f;
                var panelScale = _panel.localScale.x;
                panelScale *= 1f + OVRInput.Get(_scaleAxis).y * scaleSpeed * Time.deltaTime;
                panelScale = Mathf.Clamp(panelScale, 0.2f, 1.5f);
                _panel.localScale = new Vector3(panelScale, panelScale * _panelAspectRatio, 1f);

                // Detect grab gesture and update grab indicator
                bool didHitPanel = Physics.Raycast(GetRaycastRay(), out var hit) && hit.transform == _panel;
                _panelGlow.SetActive(didHitPanel);
                if (didHitPanel && OVRInput.GetDown(_grabButton))
                {
                    _isGrabbing = true;
                    _distanceFromController = Vector3.Distance(_raycastAnchor.position, _panel.position);
                }
            }
            AnimatePanelPose();
        }

        private Ray GetRaycastRay()
        {
            return new Ray(_raycastAnchor.position + _raycastAnchor.forward * 0.1f, _raycastAnchor.forward);
        }

        private void UpdateTargetPose()
        {
            // Animate manual placement position with right thumbstick
            const float moveSpeed = 2.5f;
            _distanceFromController += OVRInput.Get(_moveAxis).y * moveSpeed * Time.deltaTime;
            _distanceFromController = Mathf.Clamp(_distanceFromController, 0.3f, float.MaxValue);

            // Try place the panel onto environment
            var newEnvPose = TryGetEnvironmentPose();
            if (newEnvPose.HasValue)
            {
                _environmentPose = newEnvPose.Value;
            }
            else if (_currentEnvHitStatus == EnvironmentRaycastHitStatus.HitPointOutsideOfCameraFrustum)
            {
                _environmentPose = null;
            }
            var manualPlacementPosition = _raycastAnchor.position + _raycastAnchor.forward * _distanceFromController;
            var panelForward = Vector3.ProjectOnPlane(_centerEyeAnchor.position - manualPlacementPosition, Vector3.up).normalized;
            var manualPlacementPose = new Pose(manualPlacementPosition, Quaternion.LookRotation(panelForward));
            // If environment pose is available and the panel is closer to it than to the user, place the panel onto environment to create a magnetism effect
            bool chooseEnvPose = _environmentPose.HasValue && Vector3.Distance(manualPlacementPose.position, _environmentPose.Value.position) / Vector3.Distance(manualPlacementPose.position, _centerEyeAnchor.position) < 0.5;
            _targetPose = chooseEnvPose ? _environmentPose.Value : manualPlacementPose;
        }

        private Pose? TryGetEnvironmentPose()
        {
            var ray = GetRaycastRay();
            if (!_raycastManager.Raycast(ray, out var hit) || hit.normalConfidence < 0.5f)
            {
                return null;
            }
            bool isCeiling = Vector3.Dot(hit.normal, Vector3.down) > 0.7f;
            if (isCeiling)
            {
                return null;
            }
            const float sizeTolerance = 0.2f;
            var panelSize = new Vector3(_panel.localScale.x, _panel.localScale.y, 0f) * (1f - sizeTolerance);
            bool isVerticalSurface = Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.3f;
            if (isVerticalSurface)
            {
                // If the surface is vertical, stick the panel to the surface
                if (_raycastManager.PlaceBox(ray, panelSize, Vector3.up, out var result))
                {
                    // Apply the rolling average filter to smooth the normal
                    var smoothedNormal = _rollingAverageFilter.UpdateRollingAverage(result.normal);
                    return new Pose(result.point, Quaternion.LookRotation(smoothedNormal, Vector3.up));
                }
            }
            else
            {
                // Position the panel upright and check collisions with environment
                var position = hit.point + Vector3.up * _panel.localScale.y * 0.5f;
                var halfExtents = panelSize * 0.5f;
                var forward = Vector3.ProjectOnPlane(_centerEyeAnchor.position - position, Vector3.up).normalized;
                var orientation = Quaternion.LookRotation(forward, Vector3.up);
                const float collisionCheckOffset = 0.1f;
                if (!_raycastManager.CheckBox(position + Vector3.up * collisionCheckOffset, halfExtents, orientation))
                {
                    return new Pose(position, orientation);
                }
            }
            return null;
        }

        private void AnimatePanelPose()
        {
            if (!_targetPose.HasValue)
            {
                return;
            }

            const float smoothTime = 0.13f;
            _panel.position = Vector3.SmoothDamp(_panel.position, _targetPose.Value.position, ref _positionVelocity, smoothTime);

            float angle = Quaternion.Angle(_panel.rotation, _targetPose.Value.rotation);
            if (angle > 0f)
            {
                float dampedAngle = Mathf.SmoothDampAngle(angle, 0f, ref _rotationVelocity, smoothTime);
                float t = 1f - dampedAngle / angle;
                _panel.rotation = Quaternion.SlerpUnclamped(_panel.rotation, _targetPose.Value.rotation, t);
            }
        }

        private void VisualizeRaycast()
        {
            var ray = GetRaycastRay();
            bool hasHit = RaycastPanelOrEnvironment(ray, out var hit) || hit.status == EnvironmentRaycastHitStatus.HitPointOccluded;
            bool hasNormal = hit.normalConfidence > 0f;
            _raycastVisualizationLine.enabled = hasHit;
            _raycastVisualizationNormal.gameObject.SetActive(hasHit && hasNormal);
            if (hasHit)
            {
                _raycastVisualizationLine.SetPosition(0, ray.origin);
                _raycastVisualizationLine.SetPosition(1, hit.point);

                if (hasNormal)
                {
                    _raycastVisualizationNormal.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
                }
            }
        }

        private bool RaycastPanelOrEnvironment(Ray ray, out EnvironmentRaycastHit envHit)
        {
            if (Physics.Raycast(ray, out var physicsHit) && physicsHit.transform == _panel)
            {
                envHit = new EnvironmentRaycastHit
                {
                    status = EnvironmentRaycastHitStatus.Hit,
                    point = physicsHit.point,
                    normal = physicsHit.normal,
                    normalConfidence = 1f
                };
                return true;
            }
            bool envHitResult = _raycastManager.Raycast(ray, out envHit);
            _currentEnvHitStatus = envHit.status;
            return envHitResult;
        }

        private class RollingAverage
        {
            private List<Vector3> _normals;
            private int _currentRollingAverageIndex;

            public Vector3 UpdateRollingAverage(Vector3 current)
            {
                if (_normals == null)
                {
                    const int filterSize = 10;
                    _normals = Enumerable.Repeat(current, filterSize).ToList();
                }
                _currentRollingAverageIndex++;
                _normals[_currentRollingAverageIndex % _normals.Count] = current;
                Vector3 result = default;
                foreach (var normal in _normals)
                {
                    result += normal;
                }
                return result.normalized;
            }
        }
    }
}
