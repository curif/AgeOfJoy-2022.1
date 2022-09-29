using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CoinSlotController : MonoBehaviour
{
    int coins = 0;
    AudioSource caChin;

    // Start is called before the first frame update
    void Start() {
        caChin = GetComponent<AudioSource>();
    }

    public void insertCoin() {
        coins++;
        caChin.Play();
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
