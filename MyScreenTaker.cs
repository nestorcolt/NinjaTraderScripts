#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
/////////////////////////
using System.IO;
using System.Windows.Media.Imaging;
/////////////////////////
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class MyScreenTaker : Indicator
    {

        NinjaTrader.Gui.Chart.Chart chart;
        bool takeShot = true;
        BitmapFrame outputFrame;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Indicator here.";
                Name = "MyScreenTaker";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
            }
            else if (State == State.Configure)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    chart = Window.GetWindow(ChartControl) as Chart;
                }));
            }
        }

        protected override void OnBarUpdate()
        {
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (chart != null && takeShot == true)
                    {

                        RenderTargetBitmap screenCapture = chart.GetScreenshot(ShareScreenshotType.Chart);
                        outputFrame = BitmapFrame.Create(screenCapture);

                        if (screenCapture != null)
                        {
                            PngBitmapEncoder png = new PngBitmapEncoder();
                            png.Frames.Add(outputFrame);

                            using (Stream stream = File.Create(string.Format(@"{0}\{1}", Core.Globals.UserDataDir, "MyScreenshot.png")))
                                png.Save(stream);

                            Print("Screenshot saved to " + Core.Globals.UserDataDir);
                            takeShot = false;
                        }
                    }
                }));
        }
    }
		
}




#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MyScreenTaker[] cacheMyScreenTaker;
		public MyScreenTaker MyScreenTaker()
		{
			return MyScreenTaker(Input);
		}

		public MyScreenTaker MyScreenTaker(ISeries<double> input)
		{
			if (cacheMyScreenTaker != null)
				for (int idx = 0; idx < cacheMyScreenTaker.Length; idx++)
					if (cacheMyScreenTaker[idx] != null &&  cacheMyScreenTaker[idx].EqualsInput(input))
						return cacheMyScreenTaker[idx];
			return CacheIndicator<MyScreenTaker>(new MyScreenTaker(), input, ref cacheMyScreenTaker);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MyScreenTaker MyScreenTaker()
		{
			return indicator.MyScreenTaker(Input);
		}

		public Indicators.MyScreenTaker MyScreenTaker(ISeries<double> input )
		{
			return indicator.MyScreenTaker(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MyScreenTaker MyScreenTaker()
		{
			return indicator.MyScreenTaker(Input);
		}

		public Indicators.MyScreenTaker MyScreenTaker(ISeries<double> input )
		{
			return indicator.MyScreenTaker(input);
		}
	}
}

#endregion
