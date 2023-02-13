/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using UnityEngine;
using System;

[Serializable]
public class PlaceInformation
{
  public GameObject Place;
  public int MaxTimeSpentThere = 120;
  public int MinTimeSpentThere = 1;
  public float MinimalDistanceToReachObject = 1.5f;
  public AgentScenePosition ScenePosition = null;
  [SerializeField]
  public PlaceType Type;

  public enum PlaceType
  {
    Generic=0, VendingMachine, ArcadeMachine, BoyPlay
  };


  public PlaceInformation(GameObject place, int maxTimeSpentThere, int minTimeSpentThere,
                          float minimalDistanceToReachObject, PlaceType type = PlaceType.Generic,
                          AgentScenePosition scenePosition = null)
  {
    this.Place = place;
    this.MaxTimeSpentThere = maxTimeSpentThere;
    this.MinTimeSpentThere = minTimeSpentThere;
    this.MinimalDistanceToReachObject = minimalDistanceToReachObject;
    this.Type = type;
    this.ScenePosition = scenePosition; 
  }

  public bool IsTaken
  {
      get { return ScenePosition != null && (ScenePosition.IsNPCPresent || ScenePosition.IsPlayerPresent);}
  }
  public DateTime getWaitingDateTime()
  {
    return DateTime.Now.AddSeconds(UnityEngine.Random.Range(MinTimeSpentThere, MaxTimeSpentThere));
  }
}
