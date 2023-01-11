/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using CleverCrow.Fluid.BTs.TaskParents.Composites;
using Eflatun.SceneReference;

// https://github.com/ashblue/fluid-behavior-tree
// https://github.com/starikcetin/Eflatun.SceneReference


public class ExecuteAllSequence : CompositeBase
{
  protected override TaskStatus OnUpdate()
  {
    for (var i = ChildIndex; i < Children.Count; i++)
    {
      var child = Children[ChildIndex];

      if (child.Update() == TaskStatus.Continue)
        return TaskStatus.Continue;

      ChildIndex++;
    }

    return TaskStatus.Success;
  }
}

public static class BehaviorTreeBuilderExtensions
{
  public static BehaviorTreeBuilder ExecuteAllSequence(this BehaviorTreeBuilder builder, string name = "My Sequence")
  {
    return builder.ParentTask<ExecuteAllSequence>(name);
  }
}

[Serializable]
public class SceneGate
{
  public SceneReference SceneRef;
  [Tooltip("Objects that block the gate")]
  public GameObject[] Blockers;
}

public class GateController : MonoBehaviour
{

  [Tooltip("The minimal distance between the player and the gate to load/unload scenes.")]
  [SerializeField]
  public float MinimalDistance = 0.9f;

  [Header("Scene Load settings")]
  [Tooltip("Names of the scenes to load.")]
  [SerializeField]
  public SceneReference[] ScenesToLoad;

  [Header("Scene Unload settings")]
  [SerializeField]
  public SceneReference[] ScenesToUnload;

  [Header("Scene Blockers")]
  [SerializeField]
  public SceneGate[] SceneBlockers;

  [SerializeField]
  public BehaviorTree treeLoad, treeUnLoad, treeBlockers;

  private GameObject player;
  private bool playerIsOnTheGate = false;

  int loadSceneIdx = 0, unloadSceneIdx = 0, blockerIdx = 0;
  SceneReference controledSceneToLoad, controledSceneToUnLoad, controledSceneBlocker;
  AsyncOperation[] asyncLoad;

  void Start()
  {
    player = GameObject.Find("OVRPlayerControllerGalery");
    StartCoroutine(runBT());
  }

  IEnumerator runBT()
  {
    treeLoad = buildLoadScenesBT();
    treeUnLoad = buildUnLoadScenesBT();
    treeBlockers = buildBlockersBT();

    if (ScenesToLoad.Length > 0) {
      controledSceneToLoad = ScenesToLoad[loadSceneIdx];
      asyncLoad = new AsyncOperation[ScenesToLoad.Length];
    }

    if (ScenesToUnload.Length > 0)
      controledSceneToUnLoad = ScenesToUnload[unloadSceneIdx];
    
    if (SceneBlockers.Length > 0)
      controledSceneToUnLoad = SceneBlockers[unloadSceneIdx].SceneRef;

    while (true)
    {
      if (ScenesToLoad.Length > 0)
        treeLoad.Tick();

      if (ScenesToUnload.Length > 0)
        treeUnLoad.Tick();

      if (SceneBlockers.Length > 0)
        treeBlockers.Tick();

      yield return new WaitForSeconds(1f / 3f);
    }
  }
  void LockGate(SceneGate scn, bool block)
  {
    foreach (GameObject blocker in scn.Blockers)
    {
      if (blocker != null)
        blocker.SetActive(block);
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject == player)
    {
      playerIsOnTheGate = true; 
    }
  }

  private void OnTriggerExit(Collider other)
  {
      if (other.gameObject == player)
      {
        playerIsOnTheGate = false; 
      }
  }


  private BehaviorTree buildUnLoadScenesBT()
  {
    return new BehaviorTreeBuilder(gameObject)
    .Sequence()
      .Condition("Is the player on the gate?", () => playerIsOnTheGate)

      .ExecuteAllSequence()
        .Sequence()
          .Condition("Exists the scene?", () => controledSceneToUnLoad != null)
          .Condition("Is scene safe?", () => controledSceneToUnLoad.IsSafeToUse)
          .Condition("Is scene loaded?", () => SceneManager.GetSceneByName(controledSceneToUnLoad.Name).isLoaded)
          .Do("Unload Scene", () =>
          {
            ConfigManager.WriteConsole($"[GateController] UNLOAD SCENE: {controledSceneToUnLoad.Name} ******.");
            SceneManager.UnloadSceneAsync(controledSceneToUnLoad.Name);
            return TaskStatus.Success;
          })
        .End()
        .Do("Next Scene", () =>
        {
          unloadSceneIdx++;
          if (unloadSceneIdx >= ScenesToUnload.Length)
            unloadSceneIdx = 0;
          controledSceneToUnLoad = ScenesToUnload[unloadSceneIdx];
          return TaskStatus.Success;
        })
    .End()
    .Build();
  }

  private BehaviorTree buildLoadScenesBT()
  {
    return new BehaviorTreeBuilder(gameObject)
      .Sequence()
        .Condition("Is the player on the gate?", () => playerIsOnTheGate)
        .ExecuteAllSequence()
          .Sequence("load the scene if not loaded")
            .Condition("registered?", () => controledSceneToLoad != null)
            .Condition("Safe?", () => controledSceneToLoad.IsSafeToUse)
            .Condition("Is scene not loaded?", () => !SceneManager.GetSceneByName(controledSceneToLoad.Name).isLoaded)
            .Do("Load the scene (async)", () =>
            {
              asyncLoad[loadSceneIdx] = SceneManager.LoadSceneAsync(controledSceneToLoad.Name, LoadSceneMode.Additive);
              ConfigManager.WriteConsole($"[GateController] LOAD SCENE: {controledSceneToLoad.Name}");
              return TaskStatus.Success;
            })
            .RepeatUntilSuccess("Repeat until the scene is loaded")
              // .Condition("Scene is safe?", () => controledSceneToLoad.IsSafeToUse)
              .Condition("Scene loaded background?", () => asyncLoad[loadSceneIdx].isDone)
              .Condition("Is scene loaded?", () => SceneManager.GetSceneByName(controledSceneToLoad.Name).isLoaded)
            .End()
          .End()
          .Do("Next Scene to evaluate", () =>
          {
            loadSceneIdx++;
            if (loadSceneIdx >= ScenesToLoad.Length)
              loadSceneIdx = 0;
            controledSceneToLoad = ScenesToLoad[loadSceneIdx];
            return TaskStatus.Success;
          })
        .End()
      .End()
      .Build();
  }


  private BehaviorTree buildBlockersBT()
  {
    return new BehaviorTreeBuilder(gameObject)
      .ExecuteAllSequence()
        .Sequence("Control the gate")
          .Condition("Scene registered?", () => controledSceneBlocker != null)
          .Do("lock/unlock the gate", () =>
          {
            LockGate(SceneBlockers[blockerIdx], !SceneManager.GetSceneByName(controledSceneBlocker.Name).isLoaded);
            return TaskStatus.Success;
          })
        .End()
        .Do("Next Scene to evaluate", () =>
        {
          blockerIdx++;
          if (blockerIdx >= SceneBlockers.Length)
            blockerIdx = 0;
          controledSceneBlocker = SceneBlockers[blockerIdx].SceneRef;
          return TaskStatus.Success;
        })
        .End()
      .End()
      .Build();
  }
}
