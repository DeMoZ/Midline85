using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamAchievements : MonoBehaviour
{
    private bool _isStatsRecieved;
    private bool _isAchievementCleared;
    private bool _isAchievementStatusUpdated;
    private bool _isStatsStored;

    private string _achievementName = "USELESS_ACHIEVEMENT";
    // Start is called before the first frame update
    void Start()
    {
        RequestStats();
        ClearAchievement(_achievementName);
        SetAchievement(_achievementName);
    }

    private void RequestStats()
    {
        _isStatsRecieved = Steamworks.SteamUserStats.RequestCurrentStats();

        Debug.Log("Is Stat recieved:" + _isStatsRecieved);
    }

    private void ClearAchievement(string achName)
    {
        _isAchievementCleared = Steamworks.SteamUserStats.ClearAchievement(achName);

        Debug.Log("Is achievement " + achName + " cleared: " + _isAchievementCleared);

        StoreStats();
    }

    private void SetAchievement(string achName)
    {
        _isAchievementStatusUpdated = Steamworks.SteamUserStats.SetAchievement(achName);
        Debug.Log("Is achievement " + achName + " status updated: " + _isAchievementStatusUpdated);

        StoreStats();
    }

    private void StoreStats()
    {
        _isStatsStored = Steamworks.SteamUserStats.StoreStats();
        Debug.Log("Is Stat stored:" + _isStatsStored);
    }
}
