// Haptics bridge for HapticsBridge.cs (iOS only): an impact of style 0 light, 1 medium or 2 heavy at intensity 0-1.
#import <UIKit/UIKit.h>

static UIImpactFeedbackGenerator *generators[3];

extern "C" void GameHapticsImpact(int style, float intensity)
{
    if (style < 0 || style > 2) return;
    if (generators[style] == nil)
    {
        UIImpactFeedbackStyle styles[3] = { UIImpactFeedbackStyleLight, UIImpactFeedbackStyleMedium, UIImpactFeedbackStyleHeavy };
        generators[style] = [[UIImpactFeedbackGenerator alloc] initWithStyle:styles[style]];
    }
    [generators[style] impactOccurredWithIntensity:intensity];
    [generators[style] prepare];
}
