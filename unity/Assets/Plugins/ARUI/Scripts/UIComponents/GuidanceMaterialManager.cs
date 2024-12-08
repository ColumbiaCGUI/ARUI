using System;
using System.Collections.Generic;
using UnityEngine;

public enum GuidanceMaterialType
{
    task = 0,
    overview = 1,
}

public class GuidanceMaterialManager : Singleton<GuidanceMaterialManager>
{

    private GuidanceImage _taskHelper = null;
    public GuidanceImage TaskImage { get { return _taskHelper; } }

    private GuidanceImage _overview = null;
    public GuidanceImage OverviewImage { get {  return _overview; } }    

    public void Start()
    {
        GameObject helperImage = Instantiate(Resources.Load(StringResources.HelperImage_path)) as GameObject;
        helperImage.gameObject.name = "***ARUI-HelperImage" + helperImage.GetInstanceID();
        _taskHelper = helperImage.AddComponent<GuidanceImage>();

        GameObject overviewImage = Instantiate(Resources.Load(StringResources.HelperImage_path)) as GameObject;
        overviewImage.gameObject.name = "***ARUI-OverviewImage" + overviewImage.GetInstanceID();
        _overview = overviewImage.AddComponent<GuidanceImage>();
    }

    public void UpdateImage(string base64String, GuidanceMaterialType type )
    {
        if (type == GuidanceMaterialType.task)
        {
            AngelARUI.Instance.DebugLogMessage("Update task material image.", true);
            _taskHelper.UpdateImage(base64String);
        } else
        {
            AngelARUI.Instance.DebugLogMessage("Update overview material image.", true);
            _overview.UpdateImage(base64String);
        }
    }

}