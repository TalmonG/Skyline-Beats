using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using TMPro;

[System.Serializable]
public class LeaderboardData
{
    public List<PlayerData> players = new List<PlayerData>();
}

[System.Serializable]
public class PlayerData
{
    public string name;
    public int score;
}

[System.Serializable]
public class LeaderboardDataWrapper
{
    public string jsonData;

    public LeaderboardData Parse() // fake leaderboard data lmao (only for now, will make online soon)
    {
        // remove any bom characters and normalize line endings
        jsonData = jsonData.Trim().Replace("\uFEFF", "");
        return JsonUtility.FromJson<LeaderboardData>(jsonData);
    }
}

public class LeaderboardManager : MonoBehaviour
{
    public float paddingValue = 10f;

    public GameObject leaderboardTemplate;
    public GameObject leaderboardContainer;
    

    void Start()
    {
        // verify container is set
        if (leaderboardContainer == null)
        {
            Debug.LogError("Leaderboard Container is not assigned! Please assign the Content object of the ScrollView in the inspector.");
            return;
        }
        LoadLeaderboard();
    }

    private void LoadLeaderboard()
    {
        try
        {
            //Debug.Log("Starting LoadLeaderboard");
            string directoryPath = Path.Combine(Application.dataPath, "LeaderboardData");
            string filePath = Path.Combine(directoryPath, "LeaderboardData.json");
            
            //Debug.Log($"Looking for file at: {filePath}");

            if (!File.Exists(filePath))
            {
                Debug.LogError($"File not found at {filePath}");
                return;
            }

            //Debug.Log("Reading JSON file");
            string jsonData = File.ReadAllText(filePath);
            
            // create wrapper and parse
            var wrapper = new LeaderboardDataWrapper { jsonData = jsonData };
            var leaderboardData = wrapper.Parse();

            if (leaderboardData == null || leaderboardData.players == null)
            {
                Debug.LogError("Failed to parse leaderboard data");
                return;
            }

            //Debug.Log($"Number of players loaded: {leaderboardData.players.Count}");

            // sort players by score
            var sortedPlayers = leaderboardData.players
                .OrderByDescending(player => player.score)
                .ToList();

           // Debug.Log($"Number of sorted players: {sortedPlayers.Count}");

            // verify components
            // if (leaderboardTemplate == null)
            // {
            //     Debug.LogError("Leaderboard template is null!");
            //     return;
            // }

            // if (leaderboardContainer == null)
            // {
            //     Debug.LogError("Leaderboard container is null!");
            //     return;
            // }

            // clear existing entries
            //Debug.Log("Clearing existing entries");
            foreach (Transform child in leaderboardContainer.transform) {
                Destroy(child.gameObject);
            }

            // create leaderboard entries
            //Debug.Log("Creating new entries");
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                var player = sortedPlayers[i];
                //Debug.Log($"Creating entry for player: {player.name}");
                
                GameObject leaderboardEntry = Instantiate(leaderboardTemplate, leaderboardContainer.transform);
                leaderboardEntry.SetActive(true);
                
                // find all TextMeshProUGUI components in the children's children
                TextMeshProUGUI rankText = null;
                TextMeshProUGUI nameText = null;
                TextMeshProUGUI scoreText = null;

                // loop child gameobject
                foreach (Transform child in leaderboardEntry.transform)
                {
                    // get TextMesh from childs child
                    var text = child.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        if (text.CompareTag("LeaderboardRank")) rankText = text;
                        else if (text.CompareTag("LeaderboardName")) nameText = text;
                        else if (text.CompareTag("LeaderboardScore")) scoreText = text;
                    }
                }

                if (rankText == null || nameText == null || scoreText == null)
                {
                    string hierarchy = "";
                    foreach (Transform child in leaderboardEntry.transform)
                    {
                        var text = child.GetComponentInChildren<TextMeshProUGUI>();
                        hierarchy += $"\n{child.name} -> {(text != null ? $"{text.name} (Tag: {text.tag})" : "No TextMeshProUGUI found")}";
                    }
                    
                    Debug.LogError($"Missing UI components on entry {i}. Hierarchy:" + hierarchy);
                    continue;
                }

                rankText.text = "#" + (i + 1).ToString();
                nameText.text = player.name;
                scoreText.text = player.score.ToString();
                
                //Debug.Log($"Successfully created entry {i + 1} for {player.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading leaderboard: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }

    // helper method list all tags in hierarchy
    private string[] GetChildTags(Transform parent)
    {
        List<string> tags = new List<string>();
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (!string.IsNullOrEmpty(child.tag) && child.tag != "Untagged")
            {
                tags.Add($"{child.name}: {child.tag}");
            }
        }
        return tags.ToArray();
    }
}
