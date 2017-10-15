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

    //public string defaultRegion;
    public string downloadDir;
    public string pkgPath;
    public string pkgParams;

    public Settings()
    {
        instance = this;
        //defaultRegion = Registry.GetValue(keyName, "region", "ALL")?.ToString();
        downloadDir = Registry.GetValue(keyName, "downloadDir", "")?.ToString();
        pkgPath = Registry.GetValue(keyName, "pkgPath", "")?.ToString();
        pkgParams = Registry.GetValue(keyName, "pkgParams", null)?.ToString();

        if (pkgParams == null) pkgParams = "{pkgFile} --make-dirs=ux --license=\"{zRifKey}\"";
        //if (defaultRegion == null) defaultRegion = "ALL";


    }

    public void Store()
    {
        if (downloadDir != null)
            Registry.SetValue(keyName, "downloadDir", downloadDir);
        if (pkgPath != null)
            Registry.SetValue(keyName, "pkgPath", pkgPath);
        if (pkgParams != null)
            Registry.SetValue(keyName, "pkgParams", pkgParams);
    }

}
