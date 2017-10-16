using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Settings
{
    const string userRoot = "HKEY_CURRENT_USER\\SOFTWARE";
    const string subkey = "NoPayStationBrowser";
    const string keyName = userRoot + "\\" + subkey;

    public static Settings instance;

    public string downloadDir;
    public string pkgPath;
    public string pkgParams;
    public string GamesUri, DLCUri;

    public int records
    {
        get { return _records; }
        set
        {
            _records = value;
            Registry.SetValue(keyName, "records", value);
        }
    }

    int _records = 0;

    public Settings()
    {
        instance = this;
        //defaultRegion = Registry.GetValue(keyName, "region", "ALL")?.ToString();
        downloadDir = Registry.GetValue(keyName, "downloadDir", "")?.ToString();
        pkgPath = Registry.GetValue(keyName, "pkgPath", "")?.ToString();
        GamesUri = Registry.GetValue(keyName, "GamesUri", "")?.ToString();
        DLCUri = Registry.GetValue(keyName, "DLCUri", "")?.ToString();
        pkgParams = Registry.GetValue(keyName, "pkgParams", null)?.ToString();
        var rec = Registry.GetValue(keyName, "records", null)?.ToString();
        if (rec != null) int.TryParse(rec, out _records);

        if (pkgParams == null) pkgParams = "{pkgFile} --make-dirs=ux --license=\"{zRifKey}\"";


    }

    public void Store()
    {
        if (downloadDir != null)
            Registry.SetValue(keyName, "downloadDir", downloadDir);
        if (pkgPath != null)
            Registry.SetValue(keyName, "pkgPath", pkgPath);
        if (pkgParams != null)
            Registry.SetValue(keyName, "pkgParams", pkgParams);

        if (GamesUri != null)
            Registry.SetValue(keyName, "GamesUri", GamesUri);
        if (DLCUri != null)
            Registry.SetValue(keyName, "DLCUri", DLCUri);
    }





}
