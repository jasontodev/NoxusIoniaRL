using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using NoxusIoniaRL.Agents;

namespace NoxusIoniaRL.Events
{
    /// <summary>
    /// Logs game events to JSONL/Parquet format for analytics.
    /// </summary>
    public class EventLogger : MonoBehaviour
    {
        [Header("Logging Configuration")]
        public string logDirectory = "data/logs";
        public string logFileName = "events";
        public bool logToFile = true;
        public bool logToConsole = false;
        public float flushInterval = 5f; // seconds

        private List<string> eventBuffer = new List<string>();
        private StreamWriter fileWriter;
        private float lastFlushTime;
        private int tickCounter = 0;

        private void Start()
        {
            if (logToFile)
            {
                string directory = Path.Combine(Application.dataPath, "..", logDirectory);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string filePath = Path.Combine(directory, $"{logFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.jsonl");
                fileWriter = new StreamWriter(filePath, append: true);
            }

            lastFlushTime = Time.time;
        }

        private void Update()
        {
            tickCounter++;

            // Periodic flush
            if (Time.time - lastFlushTime > flushInterval)
            {
                FlushBuffer();
                lastFlushTime = Time.time;
            }
        }

        public void LogEvent(string eventType, int agentId, BaseAgent.TeamType team, Dictionary<string, object> data = null)
        {
            var eventData = new Dictionary<string, object>
            {
                { "tick", tickCounter },
                { "timestamp", DateTime.UtcNow.ToString("o") },
                { "agent_id", $"{team}_{agentId}" },
                { "team", team.ToString() },
                { "event_type", eventType }
            };

            if (data != null)
            {
                foreach (var kvp in data)
                {
                    eventData[kvp.Key] = kvp.Value;
                }
            }

            string json = JsonUtility.ToJson(new SerializableDictionary(eventData));
            eventBuffer.Add(json);

            if (logToConsole)
            {
                Debug.Log($"[EventLogger] {json}");
            }

            // Auto-flush if buffer is large
            if (eventBuffer.Count > 100)
            {
                FlushBuffer();
            }
        }

        private void FlushBuffer()
        {
            if (fileWriter != null && eventBuffer.Count > 0)
            {
                foreach (var json in eventBuffer)
                {
                    fileWriter.WriteLine(json);
                }
                fileWriter.Flush();
                eventBuffer.Clear();
            }
        }

        private void OnDestroy()
        {
            FlushBuffer();
            fileWriter?.Close();
        }

        private void OnApplicationQuit()
        {
            FlushBuffer();
            fileWriter?.Close();
        }

        // Helper class for JSON serialization
        [Serializable]
        private class SerializableDictionary
        {
            public Dictionary<string, object> data;

            public SerializableDictionary(Dictionary<string, object> dict)
            {
                data = dict;
            }
        }
    }
}

