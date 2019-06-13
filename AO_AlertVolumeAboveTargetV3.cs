#region Using declarations
using System;
using System.Threading;
using System.Timers;
using System.Net.Mail;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

class StopWatch
{
    public Stopwatch stopwatch = new Stopwatch();
    private string elapsedTime = "00";
    //
    public StopWatch()
    {
        stopwatch.Start();
    }

    public string ElapsedTime
    {
        get { return stopwatch.Elapsed.ToString("ss"); }
        set { return; }
    }
}

// -----------------------------------------------------------------------------------------------------------------------------------------------------

class EmailSender
{
    SmtpClient client = new SmtpClient();
    private string username = "nestorcolt_charts@hotmail.com";
    private string password = "Nes123456789e!";
    //
    public EmailSender()
    {

        // Command line argument must the the SMTP host.     
        client.Port = 587;
        client.Host = "smtp-mail.outlook.com";
        client.EnableSsl = true;
        client.Timeout = 10000;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.UseDefaultCredentials = false;
        client.Credentials = new System.Net.NetworkCredential(username, password);
    }

    public void composeMessage(string from, string to, string Subject, string message, string imagePath = "")
    {

        string ImagePath = imagePath;
        MailMessage MyMessage = new MailMessage(from, to);
        MyMessage.Subject = Subject;
        MyMessage.BodyEncoding = UTF8Encoding.UTF8;
        MyMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
        MyMessage.IsBodyHtml = true;

        if (ImagePath.Length > 0)
        {
            // compose with image from path
            var inlineLogo = new LinkedResource(ImagePath, "image/png");
            inlineLogo.ContentId = Guid.NewGuid().ToString();

            string body = string.Format(@"
            <h2>Here your chart info:</h2>
            <p>{0}</p>
            <p></p>
            <img src=""cid:{1}"" />
            <p></p>
            <p></p>
            <p>Nestor Colt Informatic Trading Solutions 2018</p>
            ", message, inlineLogo.ContentId);

            var view = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
            view.LinkedResources.Add(inlineLogo);
            MyMessage.AlternateViews.Add(view);
        }
        else
        {
            string body = string.Format(@"
            <h2>Here your chart info:</h2>
            <p>{0}</p>
            <p></p>
            <p></p>
            <p>Nestor Colt Informatic Trading Solutions 2018</p>
            ", message);
            //
            MyMessage.Body = body;

        }

        // Send anyway
        client.Send(MyMessage);
        Console.WriteLine("Email Sent");

    }

}

// -----------------------------------------------------------------------------------------------------------------------------------------------------

namespace NinjaTrader.NinjaScript.Indicators
{
	public class AlertVolumenOver2000 : Indicator
	{
        NinjaTrader.Gui.Chart.Chart chart;
        bool takeShot = true;
        BitmapFrame outputFrame;
        //
        StopWatch MyStopW = new StopWatch();
        EmailSender Sender = new EmailSender();
        bool emailSent = false;
        int emailBar = -1;
        //
        static string userDocumentsPath = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents");
        string chartScreenShots = System.IO.Path.Combine(userDocumentsPath, "NT_ScreenShots");        

        // -----------------------------------------------------------------------------------------------------------------------------------------------------
        protected override void OnStateChange()
        
		{
            /// Create the folder at user documents
            System.IO.Directory.CreateDirectory(chartScreenShots);
            //
            if (State == State.SetDefaults)
			{
				Description									= @"Send and email when volumen is above a target in X time period chart";
				Name										= "A0_AlertVolumeAboveTarget";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;

				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				Email					= @"nestorcolt_charts@hotmail.com";
                volumenTarget = 2000;
                candleSize = 8;

            }
			else if (State == State.Configure)
			{
                AddDataSeries("NQ 12-18", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
                Dispatcher.BeginInvoke(new Action(() => {
                    chart = Window.GetWindow(ChartControl) as Chart;
                }));
            }
		}
        //
        public string pathAssembler(string barIndex)
        {
            DateTime Today = DateTime.Now;
            string DateString = Today.ToString(@"dd_MM_yyyy_HH_mm_ss");
            string filePath = System.IO.Path.Combine(chartScreenShots, DateString + "_Candle_" + barIndex + ".png");
            return filePath;
        }
        //
        public void SaveScreenShot(string filePath)
        {
            Dispatcher.BeginInvoke(new Action(() => {
                if (chart != null && takeShot == true)
                {
                    RenderTargetBitmap screenCapture = chart.GetScreenshot(ShareScreenshotType.Chart);
                    outputFrame = BitmapFrame.Create(screenCapture);

                    if (screenCapture != null)
                    {
                        PngBitmapEncoder png = new PngBitmapEncoder();
                        png.Frames.Add(outputFrame);

                        using (System.IO.Stream stream = System.IO.File.Create(filePath))
                            png.Save(stream);

                        Print("Screenshot saved to " + filePath);
                        takeShot = true;

                    }
                }
            }));
        }
        //
        protected override void OnBarUpdate()
		{

            double currentBarSize = (float)Math.Abs(Open[0] - Close[0]);

            if (CurrentBar > 2) {
                //
                if (Int32.Parse(MyStopW.ElapsedTime) >= 5)
                {
                    long volumeValue = Bars.GetVolume(CurrentBar);
                    //
                    if (volumeValue >= volumenTarget && currentBarSize >= candleSize)
                    {
                        //
                        if (emailSent == false && CurrentBar != emailBar)
                        {
                            emailSent = true;
                            emailBar = CurrentBar;
                            string filePath = pathAssembler(CurrentBar.ToString());
                            SaveScreenShot(filePath);
                            //
                            string message = string.Format("- Alert!!! The volume of the current bar is above target, current bar volumen of {0} and bar size of {1}",
                                volumeValue.ToString(), currentBarSize.ToString());
                            string subject = "Nestor Colt Informatic Trading Solutions";
                            // Send email :
                            Thread.Sleep(1000);
                            Sender.composeMessage("nestorcolt_charts@hotmail.com", Email, subject, message, filePath);
                            Print("Email Sent");

                        }
                    }
                    else if (volumeValue < 50 && CurrentBar != emailBar)
                    {
                        emailSent = false;
                    }
                }
            }
        }
        
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Email", Description="Company email", Order=1, GroupName="Parameters")]
		public string Email
		{ get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Volumen Target", Description = "volumen measure", Order = 2, GroupName = "Parameters")]
        public int volumenTarget
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Candle Size", Description = "Size of the candle in points", Order = 3, GroupName = "Parameters")]
        public int candleSize
        { get; set; }
        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlertVolumenOver2000[] cacheAlertVolumenOver2000;
		public AlertVolumenOver2000 AlertVolumenOver2000(string email, int volumenTarget, int candleSize)
		{
			return AlertVolumenOver2000(Input, email, volumenTarget, candleSize);
		}

		public AlertVolumenOver2000 AlertVolumenOver2000(ISeries<double> input, string email, int volumenTarget, int candleSize)
		{
			if (cacheAlertVolumenOver2000 != null)
				for (int idx = 0; idx < cacheAlertVolumenOver2000.Length; idx++)
					if (cacheAlertVolumenOver2000[idx] != null && cacheAlertVolumenOver2000[idx].Email == email && cacheAlertVolumenOver2000[idx].volumenTarget == volumenTarget && cacheAlertVolumenOver2000[idx].candleSize == candleSize && cacheAlertVolumenOver2000[idx].EqualsInput(input))
						return cacheAlertVolumenOver2000[idx];
			return CacheIndicator<AlertVolumenOver2000>(new AlertVolumenOver2000(){ Email = email, volumenTarget = volumenTarget, candleSize = candleSize }, input, ref cacheAlertVolumenOver2000);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlertVolumenOver2000 AlertVolumenOver2000(string email, int volumenTarget, int candleSize)
		{
			return indicator.AlertVolumenOver2000(Input, email, volumenTarget, candleSize);
		}

		public Indicators.AlertVolumenOver2000 AlertVolumenOver2000(ISeries<double> input , string email, int volumenTarget, int candleSize)
		{
			return indicator.AlertVolumenOver2000(input, email, volumenTarget, candleSize);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlertVolumenOver2000 AlertVolumenOver2000(string email, int volumenTarget, int candleSize)
		{
			return indicator.AlertVolumenOver2000(Input, email, volumenTarget, candleSize);
		}

		public Indicators.AlertVolumenOver2000 AlertVolumenOver2000(ISeries<double> input , string email, int volumenTarget, int candleSize)
		{
			return indicator.AlertVolumenOver2000(input, email, volumenTarget, candleSize);
		}
	}
}

#endregion
