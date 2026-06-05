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
using System.Linq;
using System.Threading.Tasks;
using Meta.XR.MRUtilityKit;
using Meta.XR.MRUtilityKit.SceneDecorator;
using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.SceneDecoratorSample
{
    [MetaCodeSample("MRUKSample-SceneDecorator")]
    public class SceneDecoratorSampleController : MonoBehaviour
    {
        [SerializeField] private SceneDecorator Decorator;
        [SerializeField] private EffectMesh EffectMesh;
        [SerializeField] private EffectMesh EffectMesh_GlobalMesh;
        [SerializeField] public List<InspectorDecoration> Decorations;
        [SerializeField] public SpaceMapGPU SpaceMapGPU;

        private bool _meshVisibility;
        private readonly Dictionary<KeyCode, Action> keysPressed = new();
        private bool keyPressed;

        [Serializable]
        public struct InspectorDecoration
        {
            public DecorationStyle Style;
            public SceneDecoration Decoration;
        }

        [Serializable]
        public enum DecorationStyle
        {
            None,
            Floor1,
            Floor2,
            Walls,
            Desk,
            Everything
        }

        private DecorationStyle currentDecoration;

        private void Update()
        {
            for (var i = 0; i < keysPressed.Count; i++)
            {
                var kv = keysPressed.ElementAt(i);
                if (Input.GetKeyDown(kv.Key) && !keyPressed)
                {
                    keyPressed = true;
                    kv.Value();
                }

                if (Input.GetKeyUp(kv.Key))
                {
                    keyPressed = false;
                }
            }
        }


        private async Task Start()
        {
            currentDecoration = DecorationStyle.None;

            await MRUK.Instance.LoadSceneFromDevice();
            keysPressed.Add(KeyCode.S, ToggleMesh);
            keysPressed.Add(KeyCode.D, ClearDecorations);
            keysPressed.Add(KeyCode.F, DecorationFloor1);
            keysPressed.Add(KeyCode.G, DecorationFloor2);
            keysPressed.Add(KeyCode.H, DecorationWalls);
            keysPressed.Add(KeyCode.J, DecorationDesk);
            keysPressed.Add(KeyCode.K, AllDecorations);
        }

        private void OnEnable()
        {
            MRUK.Instance.RoomCreatedEvent.AddListener(ReceiveCreatedRoom);
            MRUK.Instance.RoomRemovedEvent.AddListener(ReceiveRemovedRoom);
            MRUK.Instance.RoomUpdatedEvent.AddListener(ReceiveUpdatedRoom);
            MRUK.Instance.SceneLoadedEvent.AddListener(UpdateAfterEvent);

        }

        private void OnDisable()
        {
            MRUK.Instance.RoomCreatedEvent.RemoveListener(ReceiveCreatedRoom);
            MRUK.Instance.RoomRemovedEvent.RemoveListener(ReceiveRemovedRoom);
            MRUK.Instance.RoomUpdatedEvent.RemoveListener(ReceiveUpdatedRoom);
            MRUK.Instance.SceneLoadedEvent.RemoveListener(UpdateAfterEvent);
        }

        private void ReceiveCreatedRoom(MRUKRoom room)
        {
            RegisterAnchorUpdates(room);
            UpdateAfterEvent();
        }

        private void ReceiveUpdatedRoom(MRUKRoom room)
        {
            UpdateAfterEvent();
        }

        private void ReceiveRemovedRoom(MRUKRoom room)
        {
            UnregisterAnchorUpdates(room);
            UpdateAfterEvent();
        }

        private void UnregisterAnchorUpdates(MRUKRoom room)
        {
            room.AnchorCreatedEvent.RemoveListener(ReceiveAnchorCreatedEvent);
            room.AnchorRemovedEvent.RemoveListener(ReceiveAnchorRemovedCallback);
            room.AnchorUpdatedEvent.RemoveListener(ReceiveAnchorUpdatedCallback);
        }

        private void RegisterAnchorUpdates(MRUKRoom room)
        {
            room.AnchorCreatedEvent.AddListener(ReceiveAnchorCreatedEvent);
            room.AnchorRemovedEvent.AddListener(ReceiveAnchorRemovedCallback);
            room.AnchorUpdatedEvent.AddListener(ReceiveAnchorUpdatedCallback);
        }

        private void ReceiveAnchorUpdatedCallback(MRUKAnchor anchor)
        {
            UpdateAfterEvent();
        }

        private void ReceiveAnchorRemovedCallback(MRUKAnchor anchor)
        {
            UpdateAfterEvent();
        }

        private void ReceiveAnchorCreatedEvent(MRUKAnchor anchor)
        {
            UpdateAfterEvent();
        }

        public void ToggleMesh()
        {
            _meshVisibility = !_meshVisibility;
            EffectMesh.ToggleEffectMeshVisibility(_meshVisibility);
            EffectMesh_GlobalMesh.ToggleEffectMeshVisibility(_meshVisibility);
        }

        private void UpdateAfterEvent()
        {
            switch (currentDecoration)
            {
                case DecorationStyle.None:
                    ClearDecorations();
                    break;
                case DecorationStyle.Floor1:
                    DecorationFloor1();
                    break;
                case DecorationStyle.Floor2:
                    DecorationFloor2();
                    break;
                case DecorationStyle.Walls:
                    DecorationWalls();
                    break;
                case DecorationStyle.Desk:
                    DecorationDesk();
                    break;
                case DecorationStyle.Everything:
                    AllDecorations();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ClearDecorations()
        {
            Decorator.ClearDecorations();
            Decorator.sceneDecorations.Clear();
        }

        public void DecorationFloor1()
        {
            ClearDecorations();
            Decorator.sceneDecorations.Add(GetDecorationByStyle(DecorationStyle.Floor1));
            Decorator.DecorateScene();
        }

        public void DecorationFloor2()
        {
            ClearDecorations();
            Decorator.sceneDecorations.Add(GetDecorationByStyle(DecorationStyle.Floor2));
            Decorator.DecorateScene();
        }

        public void DecorationWalls()
        {
            ClearDecorations();
            Decorator.sceneDecorations.Add(GetDecorationByStyle(DecorationStyle.Walls));
            Decorator.DecorateScene();
        }

        public void DecorationDesk()
        {
            ClearDecorations();
            Decorator.sceneDecorations.Add(GetDecorationByStyle(DecorationStyle.Desk));
            Decorator.DecorateScene();
        }

        public void AllDecorations()
        {
            ClearDecorations();
            Decorator.sceneDecorations.Add(GetDecorationByStyle(DecorationStyle.Desk));
            Decorator.sceneDecorations.Add(GetDecorationByStyle(DecorationStyle.Walls));
            Decorator.sceneDecorations.Add(GetDecorationByStyle(DecorationStyle.Floor2));
            Decorator.sceneDecorations.Add(GetDecorationByStyle(DecorationStyle.Floor1));
            currentDecoration = DecorationStyle.Everything;
            Decorator.DecorateScene();
        }


        private SceneDecoration GetDecorationByStyle(DecorationStyle style)
        {
            currentDecoration = style;
            return (from e in Decorations where style == e.Style select e.Decoration).FirstOrDefault();
        }


        public async void RequestSpaceSetupManual()
        {
            await OVRScene.RequestSpaceSetup();
            await MRUK.Instance.LoadSceneFromDevice(false);
            SpaceMapGPU.StartSpaceMap(MRUK.RoomFilter.CurrentRoomOnly);
            ClearDecorations();
            currentDecoration = DecorationStyle.None;
        }
    }
}
