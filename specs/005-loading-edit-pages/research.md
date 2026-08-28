# Research: Loading Indicators for Edit Pages

**Date**: 2026-08-28  
**Feature**: specs/005-loading-edit-pages/spec.md

## Decisions

### Decision 1: Separate Loading Properties (`IsLoading` vs `IsSaving`)

**Chosen approach**: Two distinct observable properties in each ViewModel to control different loading states independently.

**Rationale**: 
- `IsLoading` controls full-screen overlay during data load operations (`LoadAsync`)
- `IsSaving` controls button-level indicator during save operations (`SaveAsync`)
- Independent lifecycle management prevents state conflicts (e.g., navigation cancellation affects only the active operation)
- Clear separation of concerns aligns with Constitution I (SOLID & Separation of Concerns)

**Alternatives considered**:
- **Single `IsLoading` property**: Ambiguous which operation is in progress; difficult to manage independent cancellation
- **Enum-based state**: Overly complex for boolean visibility bindings; requires additional converters

### Decision 2: Full-Screen Overlay Pattern for Data Loading

**Chosen approach**: Grid-based overlay within page content area, matching BookListPage pattern.

**Rationale**: 
- BookListPage already demonstrates a working overlay pattern using `Grid.Row` cell sharing
- The ActivityIndicator in Row 1 overlays the CollectionView when `IsLoading=true`
- This pattern is proven and consistent with existing codebase conventions
- No new dependencies required

**Alternatives considered**:
- **Modal Popup (MAUI Community Toolkit)**: Would require adding a new NuGet package; overkill for inline loading feedback
- **VisualStateManager**: More complex XAML state management; no current usage in the codebase
- **LoadingPage reuse**: LoadingPage is app-level initialization screen with error/retry logic — too heavy for page-level loading

**Implementation note**: The edit pages currently use `ScrollView` as root. To enable overlay, the layout must be restructured to use a `Grid` with two rows (`Auto, *`) where Row 0 contains toolbar/header elements and Row 1 contains both the form content AND the ActivityIndicator overlay. This matches BookListPage's structure.

### Decision 3: Button-Level Indicator During Save

**Chosen approach**: Replace save button with an inline ActivityIndicator during loading state; restore button on completion/error.

**Rationale**:
- Provides immediate visual feedback at the interaction point (save button)
- Button disabling prevents double-tap submissions
- Uses new `IsSaving` property toggled in ViewModel's `SaveAsync` method
- No new dependencies or complex state management required

**Alternatives considered**:
- **Button.IsBusy binding**: MAUI Button has no built-in `IsBusy` property; requires custom control or Grid replacement
- **VisualState trigger**: Requires VisualStateManager setup; adds XAML complexity without clear benefit
- **Toast notification during save**: Less immediate than button-level feedback; doesn't prevent double-taps

**Implementation note**: The current save buttons are simple `<Button>` elements. They will be replaced with a `Grid` containing both the Button and an ActivityIndicator, using `IsSaving` for visibility control via InvertedBoolConverter.

### Decision 4: CommunityToolkit.Maui Converters for Boolean Negation

**Chosen approach**: Use `InvertedBoolConverter` from CommunityToolkit.Maui to negate boolean values in XAML bindings.

**Rationale**:
- Eliminates need for computed negation properties (`IsNotLoading`) in ViewModels
- Keeps display logic in XAML where it belongs (MVVM principle)
- Already available via existing `CommunityToolkit.Maui` package reference
- Proven pattern: BookDetailPage already uses `IsNotNullConverter` from the same toolkit namespace

**XAML Namespace Declaration**:
```xml
xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"
```

**Resource Dictionary Setup**:
```xml
<ContentPage.Resources>
    <ResourceDictionary>
        <toolkit:InvertedBoolConverter x:Key="InvertedBoolConverter" />
    </ResourceDictionary>
</ContentPage.Resources>
```

**Binding Examples**:
- Form content visibility during load: `IsVisible="{Binding IsLoading, Converter={StaticResource InvertedBoolConverter}}"`
- Button visibility during save: `IsVisible="{Binding IsSaving, Converter={StaticResource InvertedBoolConverter}}"`
- Button enabled state: `IsEnabled="{Binding IsSaving, Converter={StaticResource InvertedBoolConverter}}"`

**Alternatives considered**:
- **Computed ViewModel properties**: Requires manual `OnPropertyChanged` notification; adds boilerplate to ViewModels
- **Custom XAML converters**: Unnecessary when CommunityToolkit provides a proven solution

### Decision 5: Blocking Overlay During Data Load

**Chosen approach**: Full-screen overlay that blocks all user input during data fetch (per clarification Q2).

**Rationale**:
- Prevents accidental form edits while stale data is displayed
- Ensures clean state transition when new data arrives
- Consistent with user expectation for "page loading" states

**Alternatives considered**:
- **Non-blocking overlay**: Allows scrolling but changes may be lost — confusing UX
- **Dimmed content without blocking**: Visual feedback only; doesn't prevent interaction

### Decision 6: Minimum Display Duration (2 seconds)

**Chosen approach**: 2-second minimum display duration for save indicator (per clarification Q3).

**Rationale**:
- User specified 2 seconds during local development context (no server, everything local)
- Prevents visual flicker on fast networks
- Provides clear feedback that action was received

**Alternatives considered**:
- **200ms industry standard**: Too short for user's preference; may still feel like flicker
- **No minimum**: Fast saves result in invisible indicator — defeats purpose

## Dependencies Analyzed

### MAUI ActivityIndicator Control

The `ActivityIndicator` is a built-in MAUI control with these relevant properties:
- `IsRunning`: Boolean to start/stop animation
- `IsVisible`: Boolean to show/hide element
- `Color`: Theming via AppThemeBinding (already styled in Styles.xaml)
- No minimum display duration property — must be managed in ViewModel

### CommunityToolkit.Mvvm ObservableProperty

The `[ObservableProperty]` attribute generates:
- A backing field with change notification (`OnPropertyChanged`)
- A partial getter/setter that triggers UI updates on the main thread
- Both `IsLoading` and new `IsSaving` properties will use this pattern

### CommunityToolkit.Maui Converters

The `InvertedBoolConverter` is available via the existing package:
- Inverts boolean values (`true` → `false`, `false` → `true`)
- Handles null input gracefully (returns `false`)
- Works seamlessly with XAML source generation for compile-time binding validation

### XAML Source Generation

With `<MauiXamlInflator>SourceGen</MauiXamlInflator>` enabled:
- Binding errors caught at compile time
- No runtime binding failures for new ActivityIndicator bindings
- Requires proper `x:DataType` declaration (already present in both edit pages)

## Patterns Confirmed

| Pattern | Source File | Usage |
|---------|-------------|-------|
| Grid-cell overlay | BookListPage.xaml (lines 116-121) | ActivityIndicator shares Row 1 with CollectionView |
| Inline indicator | BookDetailPage.xaml (lines 26-30) | ActivityIndicator as first child of VerticalStackLayout |
| Toolkit converters | BookDetailPage.xaml (lines 32, 74) | `IsNotNullConverter` for null-check visibility bindings |
| IsLoading toggle | All ViewModels | Set `true` before async work, reset in `finally` block |
| Cancellation handling | ShelflyViewModelBase.cs | `_lifetimeCts` manages navigation cancellation |

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Layout restructuring breaks existing form layout | Medium | Grid-based approach preserves ScrollView content; Row 1 contains all form elements |
| Button replacement changes visual appearance | Low | ActivityIndicator uses same theming (Primary color) via Styles.xaml global style |
| Minimum duration causes perceived delay on fast networks | Low | 2-second minimum only applies to save indicator; load overlay disappears immediately when data arrives |
| Separate properties increase ViewModel complexity | Low | Clear naming (`IsLoading` vs `IsSaving`) and independent lifecycle management reduce ambiguity |
