using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Threading;

using robotManager.Helpful;
using robotManager.Products;

using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System.IO;

public class Main : wManager.Plugin.IPlugin
{
    const string FrameName = "GroupLootFrame";
    const string GreedButton = "GreedButton";
    const string RollButton = "RollButton";
    const string DisenchantButton = "DisenchantButton";

    private void log(string message)
    {
        Logging.WriteDebug("Roll/Dis/Greed => "+message);
    }

    public void Dispose()
    {
        try
        {
            log("dispose");
        }
        catch (Exception e)
        {

        }
    }

    public void Initialize()
    {
        log("initialize");
        EventsLua.AttachEventLua(LuaEventsId.START_LOOT_ROLL, OnConfirmLootRoll);
    }

    bool IsElementVisible(string element) 
    {
        try
        {
            return Lua.LuaDoString<bool>(String.Format("return {0}:IsVisible()", element));
        }
        catch (Exception ex)
        {
            log(ex.Message);
            return false;
        }
    }

    bool IsElementEnabled(string element) 
    {
        try
        {
            return Lua.LuaDoString<bool>(String.Format("return {0}:IsEnabled()", element));
        }
        catch (Exception ex)
        {
            log(ex.Message);
            return false;
        }
    }

    bool IsFrameVisible(int frameNumber)
    {
        string frame = String.Format("{0}{1}", FrameName, frameNumber);
        return IsElementVisible(frame);
    }

    bool IsButtonEnabled(string buttonName)
    {
        return IsElementEnabled(buttonName);
    }

    bool ClickButton(string buttonName)
    {
        try
        {
            log("Button: " + buttonName + " Click");
            return Lua.LuaDoString<bool>(String.Format("return {0}:Click()", buttonName));
        }
        catch (Exception ex)
        {
            log(ex.Message);
            return false;
        }
    }

    private void OnConfirmLootRoll(object context)
    {
        try
        {
            log("Loot roll started");

            for (int i = 1; i <= 5; i++)
            {
              if (IsFrameVisible(i))
              {
                log("Frame: " + i + " IsVisible");
                var roll = String.Format("{0}{1}{2}", FrameName, i, RollButton);
                var dis = String.Format("{0}{1}{2}", FrameName, i, DisenchantButton);
                var greed = String.Format("{0}{1}{2}", FrameName, i, GreedButton);
                if (IsButtonEnabled(roll))
                {
                  ClickButton(roll);
                } else if (IsButtonEnabled(dis))
                {
                  ClickButton(dis);
                } else {
                  ClickButton(greed);
                }
              }
            }
        }
        catch (Exception ex)
        {
            log(ex.Message);
        }
    }
    
    public void Settings()
    {
    }
}

