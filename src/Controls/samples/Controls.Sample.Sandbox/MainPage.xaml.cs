using Microsoft.Maui.Handlers;
#if ANDROID
using Android.Views;
#elif IOS
using Foundation;
using UIKit;
#endif

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		Console.WriteLine("asdasdasd");
	}

	async void OnNavigateToSubpage1(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Subpage1());
	}

	async void OnNavigateToSubpage2(object sender, EventArgs e)
	{
		var file = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
		{
			Title = "Take a photo",
		});

		if (file != null)
		{
			await DisplayAlertAsync(null, "Image Captured", "Ok");
		}
	}

	void OnItemTapped(object sender, TappedEventArgs e)
	{
		Navigation.PushAsync(new Subpage1());
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		AccessibilityFocusStore.RestoreFocus(this);
		// await Task.Delay(300);
		// AccessibilityFocusStore.RestoreFocus();

	}

	protected override async void OnDisappearing()
	{
		base.OnDisappearing();
	}

	class Subpage1 : ContentPage
	{
		public Subpage1()
		{
			Title = "Subpage 1";
			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "This is Subpage 1" },
					new Button
					{
						Text = "Go to Subpage 2",
						Command = new Command(async () =>
						{
							await Navigation.PushAsync(new Subpage2());
						})
					}
				}
			};
		}
		protected override async void OnAppearing()
		{
			base.OnAppearing();
			AccessibilityFocusStore.RestoreFocus(this);
		}

		class Subpage2 : ContentPage
		{
			public Subpage2()
			{
				Title = "Subpage 2";
				Content = new Label { Text = "This is Subpage 2" };
			}
		}
	}

}

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

