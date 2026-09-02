# Research: FAB Edit & Save UI

**Date**: 2026-09-02 | **Feature**: [spec.md](./spec.md)

## Decisions

### Decision 1: FAB Implementation Pattern

**Decision**: Use the exact FAB pattern from BookListPage — Grid container with BoxView background circle (64x64, CornerRadius 32) and ImageButton overlay (48x48), positioned at bottom-right with Margin 16. Uses AppThemeBinding for light/dark theme colors.

**Rationale**: User explicitly requested to implement FABs like BookListPage does. This ensures visual consistency across the application and leverages an existing, proven pattern.

**Alternatives considered**:
- Custom reusable FAB control — deferred; inline implementation is simpler for 3 pages
- Third-party FAB library (e.g., Plugin.Maui.FAB) — requires new dependency approval per constitution Principle XI

### Decision 2: Delete Action Placement on BookDetailPage

**Decision**: Keep delete as a toolbar item; only edit action migrates to FAB.

**Rationale**: Reduces visual clutter from multiple FABs while preserving quick access to destructive actions. Follows Material Design convention of single primary FAB per page.

**Alternatives considered**:
- Expanded FAB menu — requires additional interaction (tap to expand)
- Two separate FABs — adds visual clutter and thumb reach complexity

### Decision 3: Add Bookmark Button on BookDetailPage

**Decision**: Keep Add Bookmark as inline image button next to Bookmarks section title.

**Rationale**: The FAB should focus on page-level navigation actions (edit current book). Section-level actions like adding bookmarks remain inline for contextual clarity — users associate the action with the specific section it affects.

**Alternatives considered**:
- Convert to second FAB — adds visual clutter and dilutes primary action prominence
- Remove entirely — loses quick-add capability from detail page

### Decision 4: Keyboard Behavior on Edit Pages

**Decision**: FAB stays fixed at bottom-right; users must dismiss keyboard to tap it.

**Rationale**: Simpler implementation with predictable behavior. The FAB is always visible (though potentially partially obscured by the keyboard), and dismissing the keyboard is a standard mobile interaction pattern.

**Alternatives considered**:
- FAB moves up above keyboard — requires keyboard visibility tracking and dynamic layout adjustment
- FAB hides when keyboard open — loses save accessibility until keyboard dismissed

### Decision 5: Loading State Visualization

**Decision**: Both spinner icon and reduced opacity during save operations.

**Rationale**: Provides dual visual feedback — the spinner indicates active operation while reduced opacity signals button unavailability. This follows standard mobile UI patterns for loading states.

**Alternatives considered**:
- Spinner only — may not clearly indicate disabled state
- Reduced opacity only — less noticeable on dark backgrounds

## Technical Notes

### FAB Structure (from BookListPage)

```xml
<Grid
    x:Name="FabContainer"
    Grid.Row="1"
    HorizontalOptions="End"
    VerticalOptions="End"
    Margin="16">

    <BoxView
        WidthRequest="64"
        HeightRequest="64"
        CornerRadius="32"
        BackgroundColor="{AppThemeBinding Light={StaticResource Primary}, Dark={StaticResource White}}" />

    <ImageButton
        WidthRequest="48"
        HeightRequest="48"
        HorizontalOptions="Center"
        VerticalOptions="Center"
        SemanticProperties.Description="{x:Static resx:AppResources.BookListPageAddNewBookDescription}"
        Source="add_icon.svg"
        Command="{Binding NavigateToAddBookCommand}">
        <ImageButton.Behaviors>
            <toolkit:IconTintColorBehavior TintColor="{AppThemeBinding Light={StaticResource Secondary}, Dark={StaticResource Primary}}" />
        </ImageButton.Behaviors>
    </ImageButton>
</Grid>
```

### Loading State Implementation (for edit pages)

The FAB Grid container will include an ActivityIndicator overlay that becomes visible during save operations:

```xml
<ActivityIndicator
    WidthRequest="24"
    HeightRequest="24"
    HorizontalOptions="Center"
    VerticalOptions="Center"
    IsRunning="{Binding IsSaving}"
    IsVisible="{Binding IsSaving}" />
```

The ImageButton will use reduced opacity during loading:

```xml
<ImageButton ...
    Opacity="{Binding IsSaving, Converter={toolkit:InvertedBoolConverter}}" />
```

### Icons to Use

- **Edit FAB**: `edit_icon.svg` (already used in toolbar items)
- **Save FAB**: `check_icon.svg` or `save_icon.svg` — verify availability in Resources/Fonts/Icons directory

## Open Items

None — all clarification questions answered during `/speckit.clarify`.
