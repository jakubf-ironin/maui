namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
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

	protected override void OnAppearing()
	{
		base.OnAppearing();
		AccessibilityFocusStore.RestoreFocus();
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
		if (args.NavigationType == NavigationType.Pop)
		{
			AccessibilityFocusStore.RestoreFocus();
		}
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

		protected override void OnAppearing()
		{
			base.OnAppearing();
			AccessibilityFocusStore.RestoreFocus();
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);
			if (args.NavigationType == NavigationType.Pop)
			{
				AccessibilityFocusStore.RestoreFocus();
			}
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