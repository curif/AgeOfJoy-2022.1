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

using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.KeyboardTracker
{
    [MetaCodeSample("MRUKSample-KeyboardTracker")]
    public class Bounded3DVisualizer : MonoBehaviour
    {
        [SerializeField]
        LineRenderer _lineRenderer;

        [field: SerializeField]
        [Tooltip("The transform to which the keyboard's detected position and scale should be applied.")]
        public Transform BoxTransform
        {
            get; private set;
        }

        [SerializeField]
        GameObject _axes;

        MRUKTrackable _trackable;

        OVRPassthroughLayer _passthroughLayer;

        readonly HashSet<string> _logOnce = new();

        void LogOnce(string msg)
        {
            if (_logOnce.Add(msg))
            {
                Debug.Log(msg);
            }
        }

        void Update()
        {
            if (_passthroughLayer)
            {
                if (BoxTransform)
                {
                    // Only draw the box when we're using surface projected passthrough
                    BoxTransform.gameObject.SetActive(_passthroughLayer.isActiveAndEnabled);
                }

                if (_axes)
                {
                    _axes.SetActive(!_passthroughLayer.isActiveAndEnabled);
                }
            }
        }

        public void Initialize(OVRPassthroughLayer passthroughLayer, MRUKTrackable trackable)
        {
            if (trackable == null)
                throw new ArgumentNullException(nameof(trackable));

            _passthroughLayer = passthroughLayer;
            _trackable = trackable;

            if (_trackable.VolumeBounds == null)
            {
                LogOnce($"Trackable {_trackable} has no Bounded3D component. Ignoring.");
                return;
            }

            var box = _trackable.VolumeBounds.Value;
            LogOnce($"Bounded3D volume: {box}");

            if (_lineRenderer)
            {
                var min = -box.extents;
                _lineRenderer.positionCount = 10;

                _lineRenderer.SetPosition(0, min);
                _lineRenderer.SetPosition(1, min + new Vector3(box.size.x, 0, 0));
                _lineRenderer.SetPosition(2, min + new Vector3(box.size.x, box.size.y, 0));
                _lineRenderer.SetPosition(3, min + new Vector3(0, box.size.y, 0));

                _lineRenderer.SetPosition(4, min);
                _lineRenderer.SetPosition(5, min + new Vector3(0, 0, box.size.z));

                _lineRenderer.SetPosition(6, min + new Vector3(box.size.x, 0, box.size.z));
                _lineRenderer.SetPosition(7, min + new Vector3(box.size.x, box.size.y, box.size.z));
                _lineRenderer.SetPosition(8, min + new Vector3(0, box.size.y, box.size.z));
                _lineRenderer.SetPosition(9, min + new Vector3(0, 0, box.size.z));
            }

            if (BoxTransform)
            {
                BoxTransform.localScale = box.size;

                // Register the owning object with passthrough layer
                var meshFilter = BoxTransform.GetComponentInChildren<MeshFilter>();
                if (meshFilter)
                {
                    if (passthroughLayer)
                    {
                        passthroughLayer.AddSurfaceGeometry(meshFilter.gameObject, true);
                    }
                }
                else
                {
                    Debug.LogWarning($"Property '{nameof(BoxTransform)}' named '{BoxTransform.name}' has no {nameof(MeshFilter)}. Ignoring passthrough layer.");
                }
            }
            else
            {
                Debug.LogWarning($"Property '{nameof(BoxTransform)}' not set; ignoring passthrough layer.");
            }
        }
    }
}
