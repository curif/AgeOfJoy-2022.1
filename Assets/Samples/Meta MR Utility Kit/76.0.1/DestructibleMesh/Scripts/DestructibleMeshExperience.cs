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

using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;
using Unity.Collections;
using TMPro;

namespace Meta.XR.MRUtilityKitSamples.DestructibleMesh
{
    [MetaCodeSample("MRUKSample-DestructibleMesh")]
    public class DestructibleMeshExperience : MonoBehaviour
    {
        public DestructibleGlobalMeshSpawner destructibleGlobalMeshSpawner;
        public GameObject ExceptionUI;
        public TextMeshProUGUI ExceptionMessage;
        private DestructibleMeshComponent _destructibleMeshComponent;
        private List<GameObject> _globalMeshSegments = new();
        private OVRCameraRig _cameraRig;

        private void Awake()
        {
            _cameraRig = FindAnyObjectByType<OVRCameraRig>();
        }

        private void OnEnable()
        {
            destructibleGlobalMeshSpawner.OnSegmentationCompleted += AddBarycentricCoordinates;
            destructibleGlobalMeshSpawner.OnDestructibleMeshCreated.AddListener(OnDestructibleMeshCreated);
        }

        private void OnDisable()
        {
            destructibleGlobalMeshSpawner.OnSegmentationCompleted -= AddBarycentricCoordinates;
            destructibleGlobalMeshSpawner.OnDestructibleMeshCreated.RemoveListener(OnDestructibleMeshCreated);
        }

        private void Start()
        {
            GlobalMeshSanityCheck();
        }

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                TryDestroyMeshSegment();
            }

            if (OVRInput.GetDown(OVRInput.Button.One) ||
                OVRInput.GetDown(OVRInput.Button.Three))
            {
                GlobalMeshSanityCheck();
                destructibleGlobalMeshSpawner.AddDestructibleGlobalMesh(MRUK.Instance.GetCurrentRoom());
            }

            if (OVRInput.GetDown(OVRInput.Button.Two) ||
                OVRInput.GetDown(OVRInput.Button.Four))
            {
                destructibleGlobalMeshSpawner.RemoveDestructibleGlobalMesh(MRUK.Instance.GetCurrentRoom());
            }

            if ((OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) ||
                 OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger)) &&
                _destructibleMeshComponent)
            {
                _destructibleMeshComponent.DebugDestructibleMeshComponent();
            }
        }

        private void OnDestructibleMeshCreated(DestructibleMeshComponent destructibleMeshComponent)
        {
            _destructibleMeshComponent = destructibleMeshComponent;
            destructibleMeshComponent.GetDestructibleMeshSegments(_globalMeshSegments);
            foreach (var globalMeshSegment in _globalMeshSegments)
            {
                globalMeshSegment.AddComponent<MeshCollider>();
            }
        }

        private void GlobalMeshSanityCheck()
        {
            if (OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Meta_Quest_3 ||
                OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Meta_Quest_3S ||
                OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Meta_Link_Quest_3S ||
                OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Meta_Link_Quest_3)
            {
                if (MRUK.Instance != null && MRUK.Instance.GetCurrentRoom() != null &&
                    MRUK.Instance.GetCurrentRoom().GlobalMeshAnchor == null)
                {
                    ExceptionUI.SetActive(true);
                    ExceptionMessage.text =
                        "No Global Mesh was found for the current room. Make sure to have correctly set up the space through a room capture.";
                }
                else
                {
                    ExceptionUI.SetActive(false);
                }
            }
            else
            {
                ExceptionUI.SetActive(true);
                ExceptionMessage.text =
                    "Destructible meshes are only supported on Quest 3 and Quest 3S devices.";
            }
        }

        private void TryDestroyMeshSegment()
        {
            var ray = GetControllerRay();
            if (!Physics.Raycast(ray, out var hit))
            {
                return;
            }

            var hitObject = hit.collider.gameObject;
            if (_globalMeshSegments.Contains(hitObject) && hitObject != _destructibleMeshComponent.ReservedSegment)
            {
                // The DestroySegment function is preferred when destroying mesh segments
                // as it takes care of destroying the assets instantiated.
                _destructibleMeshComponent.DestroySegment(hitObject);
            }
        }

        private static DestructibleMeshComponent.MeshSegmentationResult AddBarycentricCoordinates(
            DestructibleMeshComponent.MeshSegmentationResult meshSegmentationResult)
        {
            var newSegments = new List<DestructibleMeshComponent.MeshSegment>();

            foreach (var segment in meshSegmentationResult.segments)
            {
                var newSegment = AddBarycentricCoordinatesToMeshSegment(segment);
                newSegments.Add(newSegment);
            }

            var newReservedSegment = AddBarycentricCoordinatesToMeshSegment(meshSegmentationResult.reservedSegment);

            return new DestructibleMeshComponent.MeshSegmentationResult()
            {
                segments = newSegments,
                reservedSegment = newReservedSegment
            };
        }

        private static DestructibleMeshComponent.MeshSegment AddBarycentricCoordinatesToMeshSegment(
            DestructibleMeshComponent.MeshSegment segment)
        {
            using var vs = new NativeArray<Vector3>(segment.positions, Allocator.Temp);
            using var ts = new NativeArray<int>(segment.indices, Allocator.Temp);
            var vertices = new Vector3[ts.Length];
            var idx = new int[ts.Length];
            var barCoord = new Color[ts.Length];

            for (var i = 0; i < ts.Length; i += 3)
            {
                vertices[i + 0] = vs[ts[i + 0]];
                vertices[i + 1] = vs[ts[i + 1]];
                vertices[i + 2] = vs[ts[i + 2]];
                barCoord[i + 0] = new Color(1, 0, 0, 0); // Barycentric coordinates for vertex 1
                barCoord[i + 1] = new Color(0, 1, 0, 0); // Barycentric coordinates for vertex 2
                barCoord[i + 2] = new Color(0, 0, 1, 0); // Barycentric coordinates for vertex 3
            }

            for (var i = 0; i < ts.Length; i++)
            {
                idx[i] = i;
            }

            var newSegment = new DestructibleMeshComponent.MeshSegment()
            {
                indices = idx,
                positions = vertices,
                colors = barCoord
            };
            return newSegment;
        }

        private Ray GetControllerRay()
        {
            Vector3 rayOrigin;
            Vector3 rayDirection;
            switch (OVRInput.activeControllerType)
            {
                case OVRInput.Controller.Touch:
                case OVRInput.Controller.RTouch:
                    rayOrigin = _cameraRig.rightHandOnControllerAnchor.position;
                    rayDirection = _cameraRig.rightHandOnControllerAnchor.forward;
                    break;
                case OVRInput.Controller.LTouch:
                    rayOrigin = _cameraRig.leftHandOnControllerAnchor.position;
                    rayDirection = _cameraRig.leftHandOnControllerAnchor.forward;
                    break;
                // hands
                default:
                {
                    var rightHand = _cameraRig.rightHandAnchor.GetComponentInChildren<OVRHand>();
                    // can be null if running in Editor with Meta Linq app and the headset is put off
                    if (rightHand != null)
                    {
                        rayOrigin = rightHand.PointerPose.position;
                        rayDirection = rightHand.PointerPose.forward;
                    }
                    else
                    {
                        rayOrigin = _cameraRig.centerEyeAnchor.position;
                        rayDirection = _cameraRig.centerEyeAnchor.forward;
                    }

                    break;
                }
            }

            return new Ray(rayOrigin, rayDirection);
        }
    }
}
