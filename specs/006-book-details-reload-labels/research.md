# Research: Material Design Floating Label Text Field

**Date**: 2026-08-28 | **Branch**: `006-book-details-reload-labels`

## Decision: ContentView Composition with Focus-Driven Animation

A custom `ContentView` control (`FloatingLabelEntry`) will be created that composes an `Entry`, `Label`, and `Border` elements, using MAUI's animation APIs to animate the floating label effect.

## Rationale

1. **Consistency with existing patterns**: The project already uses `BookCardView` as a ContentView-based custom control at `Shelfly.App/Controls/BookCardView.xaml`
2. **Cross-platform compatibility**: Works on Android, iOS, Windows, and MacCatalyst without platform-specific code
3. **No external dependencies**: Pure MAUI approach aligns with the "no MaterialDesignThemes" requirement from the dependency policy (Constitution principle XII)
4. **Maintainability**: XAML defines structure; C# handles animation logic — clean separation of concerns
5. **Theming support**: Integrates naturally with existing `AppThemeBinding` color resources

## Alternatives Considered

| Alternative | Why Evaluated | Why Rejected |
|-------------|---------------|--------------|
| **Custom Handler (native TextInputLayout)** | True Material Design on Android | Requires platform-specific code; iOS/Windows need separate handlers; adds complexity per Constitution principle XII |
| **VisualStateManager alone** | Declarative state management | Limited animation control; VSM states don't natively support continuous position animation in MAUI |
| **Behavior-based approach (CommunityToolkit.Maui)** | Reactive behavior attachment | Adds dependency on CommunityToolkit behaviors; less intuitive than direct event handling |
| **Effect-based approach** | Legacy Xamarin.Forms pattern | Effects are being deprecated in favor of handlers in modern MAUI |

## Technical Details

### Animation Approach

MAUI provides multiple animation APIs suitable for the floating label effect:

- **`TranslateTo(x, y, duration, easing)`**: Simple position changes with duration and easing
- **`FadeTo(opacity, duration, easing)`**: Opacity transitions
- **`Easing.CubicInOut`**: Provides smooth Material Design-like feel

Recommended animation parameters:
- Duration: 200ms (matches Material Design guidelines)
- Easing: `Easing.CubicInOut` for smooth acceleration/deceleration
- Label Y translation: from 0 to -16 when floating

### Control Structure

The control will use a Grid layout with two rows:
- Row 0: Floating Label (initially hidden/opacity=0)
- Row 1: Entry control with bottom border

Bindable properties required:
- `LabelText` (string): The label text shown when focused or text present
- `Text` (string, TwoWay): Bound to the Entry's Text property

### State Transitions

| Trigger | Action |
|---------|--------|
| **Entry.Focused** | Animate label upward and fade in |
| **Entry.Unfocused + has text** | Keep label floating |
| **Entry.Unfocused + no text** | Animate label back to placeholder position, fade out |
| **Text changed (non-empty)** | Trigger label visibility update if not focused |

### Integration Points

The control will be used in:
- `BookEditPage.xaml` — replace Entry controls for Title, Author, Publisher, ISBN fields
- `BookmarkEditPage.xaml` — replace Entry/Editor controls for StartPage, EndPage, Note fields

### Localization

Per Constitution principle VIII, all label text must use `.resx` resource strings:
- New keys to be added to both `AppResources.resx` (en-US) and `AppResources.de.resx` (de-DE)
- Keys follow pattern: `{Feature}{Page/Control}{Purpose}`

## Code Approach Summary

1. **Create control**: `Shelfly.App/Controls/FloatingLabelEntry.xaml` + `.xaml.cs`
2. **XAML structure**: Grid with Label, Entry, and Border elements
3. **C# logic**: Bindable properties + focus/text event handlers triggering animation
4. **Animation**: Use `TranslateTo` for vertical movement + `FadeTo` for opacity
5. **Style integration**: Add styles to `Resources/Styles/Styles.xaml` with `AppThemeBinding` support
6. **Usage**: Replace Entry elements in edit pages with the new control

## Platform Considerations

| Platform | Implementation Notes |
|----------|---------------------|
| **Android** | ContentView approach provides consistent behavior; native TextInputLayout available as future enhancement |
| **iOS** | ContentView composition works well; UITextField placeholder behavior is naturally supported |
| **Windows** | ContentView composition with watermark support via TextBox built-in features |
| **MacCatalyst** | Same as macOS approach; ContentView composition fully compatible |
