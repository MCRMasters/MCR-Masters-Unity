using UnityEngine;
using Newtonsoft.Json.Linq;
using MCRGame.Net;
using MCRGame.Game;
using MCRGame.Common;
using MCRGame.UI;
using System;
using System.Collections.Generic;


public class HUHandTest : MonoBehaviour
{
    private bool showTestButton = true;

    void OnGUI()
    {
        if (showTestButton && GUI.Button(new Rect(10, 10, 200, 50), "Send Test HU_HAND"))
        {
            SendTestHuHandMessage();
        }
    }

    private void SendTestHuHandMessage()
    {
        var scoreResult = new ScoreResult
        {
            total_score = 24,
            yaku_score_list = new List<Tuple<int, int>>()
            {
                Tuple.Create(43, 8),  // (yakuId, score)
                Tuple.Create(42, 8),
                Tuple.Create(41, 8)
            }
        };
        
        // Create test data
        var testData = new JObject
        {
            ["hand"] = new JArray { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, // Example tile IDs
            ["call_blocks"] = new JArray
            {
                new JObject
                {
                    ["type"] = 0,
                    ["first_tile"] = 5,
                    ["source_seat"] = 1,
                    ["source_tile_index"] = 2
                },
                new JObject
                {
                    ["type"] = 1,
                    ["first_tile"] = 10,
                    ["source_seat"] = 2,
                    ["source_tile_index"] = 1
                }
            },
            ["score_result"] =  JObject.FromObject(scoreResult),
            ["player_seat"] = 0,
            ["current_player_seat"] = 0,
            ["flower_count"] = 3
        };

        // Create test message
        var testMessage = new GameWSMessage
        {
            Event = GameWSActionType.HU_HAND,
            Data = testData
        };

        // Send to mediator
        if (GameMessageMediator.Instance != null)
        {
            GameMessageMediator.Instance.EnqueueMessage(testMessage);
            Debug.Log("Test HU_HAND message sent!");
        }
        else
        {
            Debug.LogError("GameMessageMediator instance not found!");
        }
    }
}