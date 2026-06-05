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
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;

namespace Meta.XR.MRUtilityKitSamples.SpaceMap
{
    [MetaCodeSample("MRUKSample-SpaceMap")]
    public class TerrainFlattener : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter meshToFlatten;

        [SerializeField]
        private float terrainHeight = -0.1f;

        public void FlattenMesh()
        {
            if (!meshToFlatten)
            {
                Debug.Log("No mesh to flatten");
                return;
            }

            var vertices = meshToFlatten.mesh.vertices;
            var triangles = meshToFlatten.mesh.triangles;
            var localSpaceVertBuffer = new List<Vector3>();
            for (var i = 0; i < triangles.Length - 2; i += 3)
            {
                var worldPos1 = meshToFlatten.transform.TransformPoint(vertices[triangles[i]]);
                var worldPos2 = meshToFlatten.transform.TransformPoint(vertices[triangles[i + 1]]);
                var worldPos3 = meshToFlatten.transform.TransformPoint(vertices[triangles[i + 2]]);

                foreach (var room in MRUK.Instance.Rooms)
                {
                    if (room.IsPositionInRoom(worldPos1) || room.IsPositionInRoom(worldPos2) || room.IsPositionInRoom(worldPos3))
                    {
                        worldPos1.y = terrainHeight;
                        vertices[triangles[i]] = meshToFlatten.transform.InverseTransformPoint(worldPos1);

                        worldPos2.y = terrainHeight;
                        vertices[triangles[i + 1]] = meshToFlatten.transform.InverseTransformPoint(worldPos2);

                        worldPos3.y = terrainHeight;
                        vertices[triangles[i + 2]] = meshToFlatten.transform.InverseTransformPoint(worldPos3);

                        localSpaceVertBuffer.Add(vertices[triangles[i]]);
                        localSpaceVertBuffer.Add(vertices[triangles[i + 1]]);
                        localSpaceVertBuffer.Add(vertices[triangles[i + 2]]);
                    }
                }
            }

            // once verts have been flattened, check for affected triangles
            var tolerance = 0.001f;
            foreach (var t in triangles)
            {
                var sourceVert = vertices[t];
                for (var k = 0; k < localSpaceVertBuffer.Count; k++)
                {
                    var flatVert = localSpaceVertBuffer[k];

                    // if this vert has the same position (when viewed top down),
                    // then it's assumed to be a vert with an identical position and should also be flattened
                    // notice the coordinate frame isn't world space (to avoid transform math)
                    if (Mathf.Abs(sourceVert.x - flatVert.x) <= tolerance &&
                        Mathf.Abs(sourceVert.y - flatVert.y) <= tolerance)
                    {
                        vertices[t].z = flatVert.z;
                        break;
                    }
                }
            }

            meshToFlatten.mesh.vertices = vertices;

            meshToFlatten.mesh.RecalculateNormals();
            meshToFlatten.mesh.RecalculateTangents();
            meshToFlatten.mesh.RecalculateBounds();

            if (meshToFlatten.gameObject.GetComponent<MeshCollider>())
            {
                meshToFlatten.gameObject.GetComponent<MeshCollider>().sharedMesh = meshToFlatten.mesh;
            }
        }
    }
}
