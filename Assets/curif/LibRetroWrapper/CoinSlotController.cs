/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class CoinSlotController : MonoBehaviour
{
    int coins = 0;
    AudioSource caChin;

    public UnityEvent OnInsertCoin;

    [Tooltip("Set to false to suppress the coin-drop sound (e.g. for NES cores where coin-insert acts as select).")]
    public bool SoundEnabled = true;

    // Start is called before the first frame update
    void Start() {
        caChin = GetComponent<AudioSource>();
    }

    public void insertCoin() {
        coins++;
        if (SoundEnabled)
            caChin.Play();
        OnInsertCoin?.Invoke();

        ConfigManager.WriteConsole($"{gameObject.name} has {coins} coins in the bucket.");
    }

    public bool takeCoin() {
        if (coins > 0) {
            coins --;
            ConfigManager.WriteConsole($"takeCoin: coin taken from CoinSlot, remaining: {coins}");
            return true;
        }
        return false;
    }

    public bool hasCoins() {
        return coins > 0;
    }

    public void clean() {
        coins = 0;
    }

    public override string ToString() {
        return $"{coins} in the bucket";
    }

}
