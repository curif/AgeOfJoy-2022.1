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

using UnityEngine;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;

namespace Meta.XR.MRUtilityKitSamples.KeyboardTracker
{
    [MetaCodeSample("MRUKSample-KeyboardTracker")]
    public sealed class KeyboardManager : MonoBehaviour
    {
        [SerializeField]
        GameObject _prefab;

        [SerializeField]
        OVRPassthroughLayer _passthroughUnderlay;

        [SerializeField]
        OVRPassthroughLayer _passthroughOverlay;

        public void OnTrackableAdded(MRUKTrackable trackable)
        {
            Debug.Log($"Detected new {trackable.TrackableType} with {trackable.name}");

            if (trackable.TrackableType != OVRAnchor.TrackableType.Keyboard)
            {
                // We only care about keyboards
                return;
            }

            // Instantiate the prefab
            var newGameObject = Instantiate(_prefab, trackable.transform);

            // Hook everything up
            var boundaryVisualizer = newGameObject.GetComponentInChildren<Bounded3DVisualizer>();
            if (boundaryVisualizer)
            {
                boundaryVisualizer.Initialize(_passthroughOverlay, trackable);
            }
        }

        public void OnTrackableRemoved(MRUKTrackable trackable)
        {
            Debug.Log($"Removing GameObject '{trackable.name}'");
            Destroy(trackable.gameObject);
        }

        void Update()
        {
            // Toggle between full passthrough and surface-projected passthrough
            if (OVRInput.GetDown(OVRInput.RawButton.A))
            {
                if (_passthroughOverlay.isActiveAndEnabled)
                {
                    _passthroughOverlay.gameObject.SetActive(false);
                    _passthroughUnderlay.gameObject.SetActive(true);
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                }
                else
                {
                    _passthroughOverlay.gameObject.SetActive(true);
                    _passthroughUnderlay.gameObject.SetActive(false);
                    Camera.main.clearFlags = CameraClearFlags.Skybox;
                }
            }
        }
    }
}
