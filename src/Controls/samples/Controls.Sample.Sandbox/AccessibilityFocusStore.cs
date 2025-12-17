using Microsoft.Maui.Handlers;

#if ANDROID
using Android.Views;
#elif IOS
using Foundation;
using UIKit;
#endif

namespace Maui.Controls.Sample;

/// <summary>
/// Manages accessibility focus restoration across page navigation for screen readers (TalkBack, VoiceOver).
/// Tracks native view focus per page and restores it when returning to the same page.
/// </summary>
public static class AccessibilityFocusStore
{
	/// <summary>
	/// Initializes accessibility focus tracking on app startup.
	/// Must be called in MauiProgram before building the app.
	/// </summary>
	public static MauiAppBuilder EnableFocusTracking(this MauiAppBuilder mauiAppBuilder)
	{
#if ANDROID
		ViewHandler.ViewMapper.AppendToMapping("AccessibilityFocusTracking", (handler, view) =>
		{
			if (handler.PlatformView is Android.Views.View androidView)
				androidView.SetAccessibilityDelegate(new AndroidFocusTracker());
		});
#elif IOS
		var notificationName = new NSString("UIAccessibilityElementFocusedNotification");
		NSNotificationCenter.DefaultCenter.AddObserver(
			notificationName,
			notification =>
			{
				if (notification?.UserInfo?["UIAccessibilityFocusedElementKey"] is UIView focusedView)
				{
					RememberFocus(focusedView);
				}
			});
#endif
		return mauiAppBuilder;
	}

	/// <summary>
	/// Restores accessibility focus to the previously focused element on the given page.
	/// </summary>
	public static void RestoreFocus()
	{
#if ANDROID
		RestoreFocusAndroid();
#elif IOS
		RestoreFocusiOS();
#endif
	}

#if ANDROID
	private static Dictionary<int, WeakReference<Android.Views.View>> _focusByPageHashCode = new();

	private static void RestoreFocusAndroid()
	{
		int pageHash = GetCurrentPage().GetHashCode();
		if (!_focusByPageHashCode.TryGetValue(pageHash, out var weakRef) || !weakRef.TryGetTarget(out var view) || view == null)
		{
			return;
		}

		view.PostDelayed(() => view.SendAccessibilityEvent(Android.Views.Accessibility.EventTypes.ViewHoverEnter), 100);
	}

	private static void RememberFocus(Android.Views.View nativeView)
	{
		Page? currentPage = GetCurrentPage();

		if (nativeView is null || currentPage is null)
			return;

		_focusByPageHashCode[currentPage.GetHashCode()] = new WeakReference<Android.Views.View>(nativeView);
	}

	private class AndroidFocusTracker : Android.Views.View.AccessibilityDelegate
	{
		public override void SendAccessibilityEvent(Android.Views.View host, Android.Views.Accessibility.EventTypes eventType)
		{
			base.SendAccessibilityEvent(host, eventType);

			if (eventType == Android.Views.Accessibility.EventTypes.ViewAccessibilityFocused)
			{
				RememberFocus(host);
			}
		}
	}

#elif IOS
	private static Dictionary<int, WeakReference<UIView>> _focusByPageHashCode = new();

	private static void RestoreFocusiOS()
	{
		int pageHash = GetCurrentPage().GetHashCode();
		if (!_focusByPageHashCode.TryGetValue(pageHash, out var weakRef) || !weakRef.TryGetTarget(out var uiView) || uiView == null)
		{
			return;
		}
		
		MainThread.BeginInvokeOnMainThread(() => UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, uiView));
	}

	private static void RememberFocus(UIView nativeView)
	{
		Page? currentPage = GetCurrentPage();;

		if (nativeView is null || currentPage is null)
			return;

		_focusByPageHashCode[currentPage.GetHashCode()] = new WeakReference<UIView>(nativeView);
	}
#endif

	private static Page GetCurrentPage()
	{
		Page page =Shell.Current.CurrentPage

		return page switch
		{
			Shell shell => shell.CurrentPage,
			NavigationPage nav => nav.CurrentPage,
			TabbedPage tabbed => tabbed.CurrentPage,
			FlyoutPage flyout => flyout.Detail,
			_ => page
		};
	}

}

