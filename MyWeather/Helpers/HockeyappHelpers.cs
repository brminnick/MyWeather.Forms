using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using MyWeather.Services;

using Xamarin.Forms;

namespace MyWeather.Helpers
{
	public static class HockeyappHelpers
	{
		enum _pathType { Windows, Linux };

		public static void TrackEvent(string eventName)
		{
			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
				case Device.Android:
					HockeyApp.MetricsManager.TrackEvent(eventName);
					break;
				case Device.UWP:
					DependencyService.Get<IHockeyappTrackEventService>()?.TrackEvent(eventName);
					break;
			}
		}

		public static void TrackEvent(string eventName, Dictionary<string, string> properties, Dictionary<string, double> measurements)
		{
			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
				case Device.Android:
					HockeyApp.MetricsManager.TrackEvent(eventName, properties, measurements);
					break;
                case Device.UWP:
					DependencyService.Get<IHockeyappTrackEventService>()?.TrackEvent(eventName, properties, measurements);
					break;
			}
		}
		/// <summary>
		/// Reports a caught exception to Hockeyapp
		/// </summary>
		public static void Report(Exception exception, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string callerMembername = "")
		{
			var fileName = GetFileNameFromFilePath(filePath);

			var errorReport = new StringBuilder();

			errorReport.Append($"{exception.GetType()} ");
			errorReport.Append($"Error: {exception.Message} ");
			errorReport.Append($"Line Number: {lineNumber} ");
			errorReport.Append($"Caller Name: {callerMembername} ");
			errorReport.Append($"File Name: {fileName}");

			TrackEvent(errorReport.ToString());

			Debug.WriteLine(errorReport.ToString());
		}

		static string GetFileNameFromFilePath(string filePath)
		{
			string fileName;
			_pathType pathType;

			var directorySeparator = new Dictionary<_pathType, string>
			{
				{ _pathType.Linux, "/" },
				{ _pathType.Windows, @"\" }
			};

			pathType = filePath.Contains(directorySeparator[_pathType.Linux]) ? _pathType.Linux : _pathType.Windows;

			while (true)
			{
				if (!(filePath.Contains(directorySeparator[pathType])))
				{
					fileName = filePath;
					break;
				}

				var indexOfDirectorySeparator = filePath.IndexOf(directorySeparator[pathType], StringComparison.Ordinal);
				var newStringStartIndex = indexOfDirectorySeparator + 1;

				filePath = filePath.Substring(newStringStartIndex, filePath.Length - newStringStartIndex);
			}

			return fileName;
		}
	}
}
