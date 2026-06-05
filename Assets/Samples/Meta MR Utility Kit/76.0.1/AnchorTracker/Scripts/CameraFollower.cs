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

using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.KeyboardTracker
{
    [MetaCodeSample("MRUKSample-KeyboardTracker")]
    public class CameraFollower : MonoBehaviour
    {
        [SerializeField] Camera _camera;

        [SerializeField] float _distance = 1;

        [SerializeField]
        [Tooltip("How quickly this object will move to the target position.")]
        [Range(0, 1)]
        float _stiffness = .1f;

        void Update()
        {
            var targetPosition = _camera.transform.position + _camera.transform.forward * _distance;
            var targetRotation = Quaternion.LookRotation(_camera.transform.forward);

            transform.SetPositionAndRotation(
                position: Vector3.Lerp(transform.position, targetPosition, _stiffness),
                rotation: Quaternion.Slerp(transform.rotation, targetRotation, _stiffness));
        }
    }
}
