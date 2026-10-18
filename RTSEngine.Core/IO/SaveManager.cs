using RTSEngine.Core.Settings;
using RTSEngine.Core.State;

namespace RTSEngine.Core.IO;

public static class SaveManager
{
    /// <summary>
    /// Settings are mandatory: without them a save would lose
    /// PopulationCap, Speed, Mode, Victory and MapPath.
    /// Visibility still comes from the world.
    /// </summary>
    public static void Save(GameWorld world, GameSettings settings, string path)
    {
        var snapshot = GameSnapshot.Capture(world, settings);

        // Atomic: write to a temp file first, then swap it in. A crash
        // mid-write must never leave a half-written save at `path`.
        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, snapshot.Serialize());
        File.Move(tempPath, path, overwrite: true);
    }

    public static GameSnapshot Load(string path)
    {
        var json = File.ReadAllText(path);
        var snapshot = GameSnapshot.Deserialize(json)
            ?? throw new InvalidDataException($"Invalid save file: {path}");

        if (snapshot.FormatVersion != GameSnapshot.CurrentFormatVersion)
            throw new InvalidDataException(
                $"Unsupported save format version {snapshot.FormatVersion} " +
                $"(expected {GameSnapshot.CurrentFormatVersion}): {path}");

        return snapshot;
    }
}