using Microsoft.Maui.Handlers;
#if ANDROID
using Android.Views;
#elif IOS
using Foundation;
using UIKit;
#endif

namespace Maui.Controls.Sample;

public static class AccessibilityFocusStore
{
	private static Page? _currentPage;
	

	public static MauiAppBuilder EnableFocusTracking(this MauiAppBuilder mauiAppBuilder)
	{
#if ANDROID
		ViewHandler.ViewMapper.AppendToMapping("Tracking", (handler, view) =>
		{
			if (handler.PlatformView is Android.Views.View androidView)
				androidView.SetAccessibilityDelegate(new TrackingAccessibilityDelegate());
		});
#elif IOS
		NSNotificationCenter.DefaultCenter.AddObserver(
			new NSString("UIAccessibilityElementFocusedNotification"),
			notification =>
			{
				if (notification?.UserInfo?["UIAccessibilityFocusedElementKey"] is UIView focusedView)
				{
					Console.WriteLine($"VoiceOver focused on: {focusedView}");
					Remember(focusedView);
				}
			});
#endif
		return mauiAppBuilder;
	}

	public static void RestoreFocus(Page currentPage)
	{
		_currentPage = currentPage;

#if ANDROID
		if (_lastFocusedViewOnPage.TryGetValue(_currentPage, out var view) && view is not null)
		{
			view.PostDelayed(() =>
			{
				Console.WriteLine("Restoring VoiceOver focus to: "+view);
				view.SendAccessibilityEvent(Android.Views.Accessibility.EventTypes.ViewHoverEnter);
			}, 300);
		}
#elif IOS
		if (_lastFocusedViewOnPage.TryGetValue(_currentPage, out var uiView) && uiView is not null)
		{
			Console.WriteLine("Restoring VoiceOver focus to: "+uiView);
			UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, uiView);
		}
#endif
	}

	public static void StopTracking()
	{
		_currentPage = null;
	}

#if ANDROID
	private static Dictionary<Page, Android.Views.View> _lastFocusedViewOnPage = new();

	private static void Remember(Android.Views.View nativeView)
	{
		if (nativeView is null || _currentPage is null)
			return;

		_lastFocusedViewOnPage[_currentPage] = nativeView;
	}

#elif IOS
	private static Dictionary<Page, UIView> _lastFocusedViewOnPage = new();

	private static void Remember(UIView nativeView)
	{
		if (nativeView is null || _currentPage is null)
			return;

		_lastFocusedViewOnPage[_currentPage] = nativeView;
	}
#endif

#if ANDROID
	class TrackingAccessibilityDelegate : Android.Views.View.AccessibilityDelegate
	{
		public override void SendAccessibilityEvent(Android.Views.View host, Android.Views.Accessibility.EventTypes eventType)
		{
			base.SendAccessibilityEvent(host, eventType);

			if (eventType == Android.Views.Accessibility.EventTypes.ViewHoverEnter ||
			 	eventType == Android.Views.Accessibility.EventTypes.ViewFocused)
				Remember(host);
		}
	}
#endif
}

