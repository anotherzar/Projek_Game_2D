using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class CardDataGenerator : EditorWindow
{
    [MenuItem("Tools/Memory Puzzle/Generate LVL2 Card Data")]
    public static void GenerateCardData()
    {
        string spriteFolder = "Assets/Project/Art/Cards/MemoryPuzzle_LVL2";
        string outputFolder = "Assets/Project/Art/UI/MemoryCards/LVL2";

        // Ensure output folder exists
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }

        // Get all png files in the sprite folder
        string[] spriteFiles = Directory.GetFiles(spriteFolder, "*.png");
        
        // Dictionary to group by card ID: ID -> (FixedSpritePath, DraggableSpritePath)
        Dictionary<int, (string fixedPath, string dragPath)> pairs = new Dictionary<int, (string, string)>();

        Regex regex = new Regex(@"Artboard\s+(\d+)-(\d+)\.png", RegexOptions.IgnoreCase);

        foreach (string file in spriteFiles)
        {
            string fileName = Path.GetFileName(file);
            Match match = regex.Match(fileName);
            if (match.Success)
            {
                int cardID = int.Parse(match.Groups[1].Value);
                int type = int.Parse(match.Groups[2].Value); // 1 = Fixed/Prompt, 2 = Draggable/Answer

                // Convert file path to Unity project-relative path
                string relativePath = file.Replace('\\', '/');

                if (!pairs.ContainsKey(cardID))
                {
                    pairs[cardID] = (null, null);
                }

                var current = pairs[cardID];
                if (type == 1)
                {
                    current.fixedPath = relativePath;
                }
                else if (type == 2)
                {
                    current.dragPath = relativePath;
                }
                pairs[cardID] = current;
            }
        }

        int count = 0;
        foreach (var kvp in pairs)
        {
            int cardID = kvp.Key;
            string fixedPath = kvp.Value.fixedPath;
            string dragPath = kvp.Value.dragPath;

            if (string.IsNullOrEmpty(fixedPath) || string.IsNullOrEmpty(dragPath))
            {
                Debug.LogWarning($"Skipped Card ID {cardID} because it does not have a complete pair (1-1 and 1-2).");
                continue;
            }

            // Load sprites
            Sprite fixedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fixedPath);
            Sprite dragSprite = AssetDatabase.LoadAssetAtPath<Sprite>(dragPath);

            if (fixedSprite == null || dragSprite == null)
            {
                Debug.LogWarning($"Could not load sprites for Card ID {cardID}. Make sure they are imported as Sprite (2D and UI).");
                continue;
            }

            // Create CardData Asset
            CardData asset = ScriptableObject.CreateInstance<CardData>();
            asset.cardID = cardID;
            asset.fixedSprite = fixedSprite;
            asset.cardSprite = dragSprite;

            string assetPath = $"{outputFolder}/Card_LVL2_{cardID}.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Successfully generated {count} Card Data assets in {outputFolder}!", "OK");
        Debug.Log($"Card Data generation complete. Created {count} assets.");
    }
}
