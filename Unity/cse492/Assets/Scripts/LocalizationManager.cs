using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class LocalizationManager : MonoBehaviour
{
    public StringTable ui_en;
    public StringTable ui_tr;

    // Get the localized string based on the current locale and the provided key
    public string GetLocalizedString(string key)
    {
        // Get the current language
        var locale = LocalizationSettings.SelectedLocale;

        // Get the correct table based on the current language
        var table = locale.Identifier.Code == "en" ? ui_en : ui_tr;

        // Get the localized string from the table
        return GetLocalizedStringFromTable(table, key);
    }

    public static string GetLocalizedStringFromTable(StringTable table, string entryName)
    {
        var entry = table.GetEntry(entryName);

        if (entry == null)
        {
            Debug.Log($"Entry {entryName} not found in table {table.name}");
            return entryName;
        }

        return entry.GetLocalizedString(); // We can pass in optional arguments for Smart Format or String.Format here.
    }
}
