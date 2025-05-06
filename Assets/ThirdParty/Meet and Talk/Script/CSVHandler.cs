using System;
using System.Collections.Generic;
using System.IO;
using MeetAndTalk;
using MeetAndTalk.Localization;
using UnityEngine;

namespace MeetAndTalk
{
    public static class CSVHandler
    {
        public static void ImportCSV(string filePath, DialogueContainerSO SO)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("File does not exist at path: " + filePath);
                return;
            }

            try
            {
                using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.UTF8))
                {
                    string line;
                    bool headerSkipped = false;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!headerSkipped)
                        {
                            headerSkipped = true;
                            continue;
                        }

                        string[] fields = ParseCSVLine(line);
                        string nodeGuid = fields[0];

                        foreach (var node in SO.DialogueNodeDatas)
                        {
                            if (nodeGuid == node.NodeGuid)
                            {
                                for (int i = 0; i < fields.Length - 1; i++)
                                {
                                    node.TextType[i].LanguageGenericType = fields[i + 1];
                                }
                            }
                        }

                        foreach (var choiceNode in SO.DialogueChoiceNodeDatas)
                        {
                            if (nodeGuid == choiceNode.NodeGuid)
                            {
                                for (int i = 0; i < fields.Length - 1; i++)
                                {
                                    choiceNode.TextType[i].LanguageGenericType = fields[i + 1];
                                }
                            }

                            foreach (var port in choiceNode.DialogueNodePorts)
                            {
                                if (port.PortGuid == nodeGuid)
                                {
                                    for (int k = 0; k < fields.Length - 1; k++)
                                    {
                                        port.TextLanguage[k].LanguageGenericType = fields[k + 1];
                                    }
                                }
                            }
                        }

                        foreach (var timerNode in SO.TimerChoiceNodeDatas)
                        {
                            if (nodeGuid == timerNode.NodeGuid)
                            {
                                for (int i = 0; i < fields.Length - 1; i++)
                                {
                                    timerNode.TextType[i].LanguageGenericType = fields[i + 1];
                                }
                            }

                            foreach (var port in timerNode.DialogueNodePorts)
                            {
                                if (port.PortGuid == nodeGuid)
                                {
                                    for (int k = 0; k < fields.Length - 1; k++)
                                    {
                                        port.TextLanguage[k].LanguageGenericType = fields[k + 1];
                                    }
                                }
                            }
                        }

                        foreach (var choiceNode in SO.AdvancedChoiceNodeDatas)
                        {
                            foreach (var port in choiceNode.DialogueNodePorts)
                            {
                                if (port.PortGuid == nodeGuid)
                                {
                                    for (int k = 0; k < fields.Length - 1; k++)
                                    {
                                        port.TextLanguage[k].LanguageGenericType = fields[k + 1];
                                    }
                                }
                            }
                        }

                        foreach (var timerNode in SO.AdvancedTimeChoiceNodeDatas)
                        {
                            foreach (var port in timerNode.DialogueNodePorts)
                            {
                                if (port.PortGuid == nodeGuid)
                                {
                                    for (int k = 0; k < fields.Length - 1; k++)
                                    {
                                        port.TextLanguage[k].LanguageGenericType = fields[k + 1];
                                    }
                                }
                            }
                        }
                    }
                }

                Debug.Log("CSV imported successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError("Error while importing CSV: " + e.Message);
            }
        }

        public static void ExportCSV(string filePath, DialogueContainerSO SO)
        {
            List<string> csvContent = new List<string>();
            List<string> headers = new List<string> { "GUID", "English" };

            LocalizationManager _manager = (LocalizationManager)Resources.Load("Languages");
            for (int i = 0; i < _manager.lang.Count; i++)
            {
                if (_manager.lang[i].ToString() != "English")
                {
                    headers.Add(_manager.lang[i].ToString());
                }
            }
            csvContent.Add(string.Join(",", headers));

            int exportedNodes = 0;

            foreach (var node in SO.DialogueNodeDatas)
            {
                string line = "\"" + node.NodeGuid + "\"";
                foreach (var text in node.TextType)
                {
                    line += ",\"" + text.LanguageGenericType.Replace("\"", "\"\"") + "\"";
                }
                csvContent.Add(line);
                exportedNodes++;
            }

            foreach (var choiceNode in SO.DialogueChoiceNodeDatas)
            {
                string line = "\"" + choiceNode.NodeGuid + "\"";
                foreach (var text in choiceNode.TextType)
                {
                    line += ",\"" + text.LanguageGenericType.Replace("\"", "\"\"") + "\"";
                }
                csvContent.Add(line);
                exportedNodes++;

                foreach (var port in choiceNode.DialogueNodePorts)
                {
                    string portLine = "\"" + port.PortGuid + "\"";
                    foreach (var text in port.TextLanguage)
                    {
                        portLine += ",\"" + text.LanguageGenericType.Replace("\"", "\"\"") + "\"";
                    }
                    csvContent.Add(portLine);
                    exportedNodes++;
                }
            }

            foreach (var timerNode in SO.TimerChoiceNodeDatas)
            {
                string line = "\"" + timerNode.NodeGuid + "\"";
                foreach (var text in timerNode.TextType)
                {
                    line += ",\"" + text.LanguageGenericType.Replace("\"", "\"\"") + "\"";
                }
                csvContent.Add(line);
                exportedNodes++;

                foreach (var port in timerNode.DialogueNodePorts)
                {
                    string portLine = "\"" + port.PortGuid + "\"";
                    foreach (var text in port.TextLanguage)
                    {
                        portLine += ",\"" + text.LanguageGenericType.Replace("\"", "\"\"") + "\"";
                    }
                    csvContent.Add(portLine);
                    exportedNodes++;
                }
            }

            foreach (var choiceNode in SO.AdvancedChoiceNodeDatas)
            {
                foreach (var port in choiceNode.DialogueNodePorts)
                {
                    string portLine = "\"" + port.PortGuid + "\"";
                    foreach (var text in port.TextLanguage)
                    {
                        portLine += ",\"" + text.LanguageGenericType.Replace("\"", "\"\"") + "\"";
                    }
                    csvContent.Add(portLine);
                    exportedNodes++;
                }
            }

            foreach (var timerNode in SO.AdvancedTimeChoiceNodeDatas)
            {
                foreach (var port in timerNode.DialogueNodePorts)
                {
                    string portLine = "\"" + port.PortGuid + "\"";
                    foreach (var text in port.TextLanguage)
                    {
                        portLine += ",\"" + text.LanguageGenericType.Replace("\"", "\"\"") + "\"";
                    }
                    csvContent.Add(portLine);
                    exportedNodes++;
                }
            }

            try
            {
                File.WriteAllLines(filePath, csvContent, System.Text.Encoding.UTF8);
                Debug.Log($"CSV file generated at: {filePath}. Nodes exported: {exportedNodes}");
            }
            catch (IOException ex)
            {
                Debug.LogError("Failed to write CSV file. The file may be open in another program.\nError: " + ex.Message);
            }
        }

        private static string[] ParseCSVLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }

            fields.Add(currentField);

            return fields.ToArray();
        }
    }
}
