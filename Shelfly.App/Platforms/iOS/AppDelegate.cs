using Foundation;
using UIKit;

namespace Shelfly.App;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        bool result = base.FinishedLaunching(application, launchOptions);

        // Tint native DisplayAlert dialogs with the Dark Academia accent colors (oxblood in light mode, antique gold in dark mode).
        Window!.TintColor = UIColor.FromDynamicProvider(traits =>
            traits.UserInterfaceStyle == UIUserInterfaceStyle.Dark
                ? UIColor.FromRGB(0xC9, 0xA2, 0x27)
                : UIColor.FromRGB(0x6E, 0x2C, 0x34));

        return result;
    }
}
