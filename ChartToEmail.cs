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
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;

using System.Windows.Media.Imaging;
using System.Net.Mail;
using System.Net.Mime;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ChartToEmail : Indicator
	{
		
		//VARS
		
		NinjaTrader.Gui.Chart.Chart 	chart;
        BitmapFrame 					outputFrame;
		private bool ScreenShotSent 	= false;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Takes a screenshot of the chart and sends to email in PNG format.";
				Name								= "ChartToEmail";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive			= true;
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
			if (CurrentBar < Count -2) return; //dont do anything until last bar on chart
			
			if (!ScreenShotSent)
			{
				SendMailChart("Subject","This is the body of the email","FromEmail@gmail.com","ToEmail@gmail.com","smtp.gmail.com",587,"YourEmailUsername","YourEmailPassword");
				ScreenShotSent = true;
				
			}
			
		}
		
		private void SendMailChart(string Subject, string Body, string From, string To, string Host, int Port, string Username, string Password)
		{
			
			try	
			{	

				Dispatcher.BeginInvoke(new Action(() =>
				{
				
						if (chart != null)
				        {
							
							RenderTargetBitmap	screenCapture = chart.GetScreenshot(ShareScreenshotType.Chart);
		                    outputFrame = BitmapFrame.Create(screenCapture);
							
		                    if (screenCapture != null)
		                    {
		                       
								PngBitmapEncoder png = new PngBitmapEncoder();
		                        png.Frames.Add(outputFrame);
								System.IO.MemoryStream stream = new System.IO.MemoryStream();
								png.Save(stream);
								stream.Position = 0;
							
								MailMessage theMail = new MailMessage(From, To, Subject, Body);
								System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(stream, "image.png");
								theMail.Attachments.Add(attachment);
							
								SmtpClient smtp = new SmtpClient(Host, Port);
								smtp.EnableSsl = true;
								smtp.Credentials = new System.Net.NetworkCredential(Username, Password);
								string token = Instrument.MasterInstrument.Name + ToDay(Time[0]) + " " + ToTime(Time[0]) + CurrentBar.ToString();
								
								Print("Sending Mail!");
								smtp.SendAsync(theMail, token);
		                  
				            }
						}
			
			    
				}));

				
				
			}
			catch (Exception ex) {
				
				Print("Sending Chart email failed -  " + ex);
			
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ChartToEmail[] cacheChartToEmail;
		public ChartToEmail ChartToEmail()
		{
			return ChartToEmail(Input);
		}

		public ChartToEmail ChartToEmail(ISeries<double> input)
		{
			if (cacheChartToEmail != null)
				for (int idx = 0; idx < cacheChartToEmail.Length; idx++)
					if (cacheChartToEmail[idx] != null &&  cacheChartToEmail[idx].EqualsInput(input))
						return cacheChartToEmail[idx];
			return CacheIndicator<ChartToEmail>(new ChartToEmail(), input, ref cacheChartToEmail);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChartToEmail ChartToEmail()
		{
			return indicator.ChartToEmail(Input);
		}

		public Indicators.ChartToEmail ChartToEmail(ISeries<double> input )
		{
			return indicator.ChartToEmail(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChartToEmail ChartToEmail()
		{
			return indicator.ChartToEmail(Input);
		}

		public Indicators.ChartToEmail ChartToEmail(ISeries<double> input )
		{
			return indicator.ChartToEmail(input);
		}
	}
}

#endregion
