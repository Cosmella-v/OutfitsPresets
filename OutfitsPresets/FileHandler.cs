using System;
using System.IO;
using UnityEngine;
namespace OutfitsPresets;
class FileUtils
{
    public static System.Collections.Generic.List<string> Files = new System.Collections.Generic.List<string>{""};
    public static System.Collections.Generic.List<string> GetPath()
    {
        string Outfits = Path.Combine(
          Application.persistentDataPath,
          "OutfitPresets"
      );
        Outfits = Path.GetFullPath(Outfits);
        if (!Directory.Exists(Outfits))
        {
            Directory.CreateDirectory(Outfits);
        } else
        {
            string[] jsonFiles = Directory.GetFiles(Outfits, "*.json");

          //   System.Diagnostics.Debug.WriteLine("JSON Files in Among Us folder:");
            Files.Clear();
            foreach (string file in jsonFiles)
            {
             //   System.Diagnostics.Debug.WriteLine(file);
                Files.Add(file);
            }
            return Files;
        }
        return Files;


    }

    public static string ReadJsonFile(string filePath)
    {

        if (File.Exists(filePath))
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error reading file: " + ex.Message);
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("File does not exist: " + filePath);
        }

        return null;
    }
    public static bool SaveFile(string fileName, string content)
    {
        try
        {
            string outfitsPath = Path.Combine(
                Application.persistentDataPath,
                "OutfitPresets"
            );
            outfitsPath = Path.GetFullPath(outfitsPath);

            if (!Directory.Exists(outfitsPath))
            {
                Directory.CreateDirectory(outfitsPath);
               // System.Diagnostics.Debug.WriteLine($"Created directory: {outfitsPath}");
            }

            string filePath = Path.Combine(outfitsPath, fileName);

            File.WriteAllText(filePath, content);

          //  System.Diagnostics.Debug.WriteLine($"File saved to: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save file: {ex.Message}");
            return false;
        }
    }
};


