using System.Windows;
using System.IO;
using Control = System.Windows.Forms.Control;
using System;

namespace SimpRef
{
    public partial class App : Application
    {
        App()
        {
            //get dpi ratio to adjust mouse position
            GloVar.DpiRatio = 96.0 / new Control().CreateGraphics().DpiX;
            GloVar.AutoHiding = true;

            if (File.Exists("setting.ini"))
            {
                StreamReader sr = new StreamReader("setting.ini");

                try
                {
                    while (sr.Peek() > -1)
                    {
                    String[] datas = sr.ReadLine().ToLower().Split('=');

                    
                        if (datas.Length >= 2)
                            if (datas[0].Contains("autofocus"))
                            {
                                if (datas[1].Contains("false")) GloVar.AutoFocus = false;
                                else if (datas[1].Contains("true")) GloVar.AutoFocus = true;
                            }
                            else if (datas[0].Contains("opacity"))
                            {
                                GloVar.Opacity_norm = Convert.ToDouble(datas[1]) / 100.0;
                            }
                            else if (datas[0].Contains("barheight"))
                            {
                                GloVar.BarHeight = Convert.ToDouble(datas[1]);
                            }
                    }
                }
                catch { new Msgbox("Setting Error!"); }
            }
            else
            {
                StreamWriter sw = new StreamWriter("setting.ini",false);
                sw.Write("AutoFocus = true\r\nOpacity = 100.0\r\nBarHeight=25.0");
                sw.Close();
                sw.Dispose();
            }
        }
    }
}
