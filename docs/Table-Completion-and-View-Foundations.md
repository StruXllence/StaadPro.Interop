# Table completion and View foundation

**Current View follow-up:** ShowBack and ShowLeft are also available. View now has three orientation requests; see [current contracts and coverage](View-Orientations.md). The implementation checkpoint below records the original Table completion and ShowFront increment.

## Table signatures and differences

### SetCellTextColor(int, int, int, int, int, int, int)

```csharp
TableOperationResult SetCellTextColor(int reportId, int tableId, int rowNumber, int columnNumber, int red, int green, int blue);
```
Native target: `SetCellTextColor`. Unlike the packed ARGB, Color and text overloads, this accepts three separate RGB channels, each 0–255. It performs no alpha handling or color parsing; invalid channels are rejected before native work. HSL inputs belong in SetCellTextColorHsl. Exactly one native SetCellTextColor call receives Int32 RGB values.

### SetCellTextColor(int, int, int, int, Color)

```csharp
TableOperationResult SetCellTextColor(int reportId, int tableId, int rowNumber, int columnNumber, Color color);
```
Native target: `SetCellTextColor`. Unlike the string overload, this performs no parsing or malformed-text fallback. Uses the existing System.Windows.Media.Color type; its R/G/B byte properties become native Int32 channels. Alpha is ignored, including for transparent colors: no background blending or premultiplication is performed. The original RGB channels are retained.

### SetCellTextColor(int, int, int, int, string)

```csharp
TableOperationResult SetCellTextColor(int reportId, int tableId, int rowNumber, int columnNumber, string colorText);
```
### SetCellTextColor(int, int, int, int, int)

```csharp
TableOperationResult SetCellTextColor(int reportId, int tableId, int rowNumber, int columnNumber, int argbValue);
```
Native target: `SetCellTextColor`. Unlike the three-integer overload, this accepts one packed signed Int32. Extracts R/G/B from bits 16–23, 8–15 and 0–7. Alpha in the high byte is ignored, so a negative Int32 or zero alpha does not imply invalid or invisible text. No parsing or compositing occurs.

### SetCellTextColor(int, int, int, int, byte, byte, byte, byte)

```csharp
TableOperationResult SetCellTextColor(int reportId, int tableId, int rowNumber, int columnNumber, byte alpha, byte red, byte green, byte blue);
```
Native target: `SetCellTextColor`. Unlike the packed Int32 overload, channel bytes are supplied separately in alpha/red/green/blue order. The alpha argument is retained for source workflow parity but is ignored because native SetCellTextColor accepts RGB only. No premultiplication, transparency or background blending is applied. Native channel arguments are Int32 values.

### SetCellTextColorHsl(int, int, int, int, float, float, float)

```csharp
TableOperationResult SetCellTextColorHsl(int reportId, int tableId, int rowNumber, int columnNumber, float hue, float saturation, float lightness);
```
### SetCellTextHorizontalAlignment(int, int, int, int, TableHorizontalAlignment)

```csharp
TableOperationResult SetCellTextHorizontalAlignment(int reportId, int tableId, int rowNumber, int columnNumber, TableHorizontalAlignment alignment);
```
### SetCellTextVerticalAlignment(int, int, int, int, TableVerticalAlignment)

```csharp
TableOperationResult SetCellTextVerticalAlignment(int reportId, int tableId, int rowNumber, int columnNumber, TableVerticalAlignment alignment);
```
### SetCellTextSizeForTable(int, int, double)

```csharp
TableOperationResult SetCellTextSizeForTable(int reportId, int tableId, double fontSize);
```
Native target: `SetCellTextSizeAll`. Unlike SetCellTextSize, this targets the whole table and takes no row/column coordinates. Uses the same finite, positive native Microsoft Word-equivalent size convention, independent of model input length units. The Double is forwarded unchanged; no cell enumeration, string conversion, invented upper bound or implicit save occurs.

## Managed behavior and reuse

All Table methods retain the existing immutable `TableOperationResult`. New requests are native void calls: normal return means `InvokedWithoutAcknowledgement`, with `IsSuccessful == false` and `NativeReturnCode == null`. This is not a failure flag or independent effect/persistence evidence. Positive report/table identities and one-based cell coordinates use existing shared validation.

Reused the existing WPF `Color`, `ColorConverter`, Table results/statuses, validation, fixture and per-adapter gate. The main alignment enum values were ported as `TableHorizontalAlignment` (0/1/2) and `TableVerticalAlignment` (0/4/8), keeping domain meaning clear alongside WPF types. Conversion helpers live in `Services/Table`; no partial adapters, tuples, public ref/out or duplicate public color/HSL model was introduced.

## Example

With an acquired wrapper and real report/table IDs, on the established COM apartment:

```csharp
using StaadPro.Interop.Enums.Table;

var table = wrapper.Table;
int reportId = 11; // replace with an acquired report identity
int tableId = 7;   // replace with a table in that report
table.SetCellTextColor(reportId, tableId, 1, 2, 17, 34, 51);
table.SetCellTextColor(reportId, tableId, 1, 2, "#80112233"); // same RGB; alpha ignored
table.SetCellTextColorHsl(reportId, tableId, 1, 2, 210f, 0.5f, 0.25f);
table.SetCellTextHorizontalAlignment(reportId, tableId, 1, 2, TableHorizontalAlignment.Center);
table.SetCellTextVerticalAlignment(reportId, tableId, 1, 2, TableVerticalAlignment.Middle);
table.SetCellTextSizeForTable(reportId, tableId, 12.5);
var save = table.SaveTable(reportId, tableId); // explicit; still no acknowledgement
```
## View: ShowFront

```csharp
ViewOperationResult ShowFront();
```
`wrapper.View.ShowFront()` calls native `ShowFront` once on the captured RawView object. It requests the active graphical view's front orientation. It does not show a hidden STAAD application, create a window, transform geometry, run analysis or save. The native front convention belongs to STAAD.

## Versions, ownership and verification

The remaining four Table contracts and ShowFront match across installed 2024/2025/2026 help. Passive 2025/2026 metadata corroborates argument order/by-value slots. Common contracts share internal services; no artificial year-specific adapters were created. A 2024 vendor ABI was not inspected. Live COM, active-window/report context, licensing, persistence and every servicing build require native acceptance evidence.

All calls use captured interfaces on the caller's established COM apartment with per-adapter gates and binding checks before/after dispatch. External UI/raw-object edits and separate adapters are not locked out. Reacquire after wrapper refresh. No alternate STAAD root, automatic retry, rollback or implicit save is introduced. Native exceptions propagate unchanged; post-call failures may follow actual changes.

Passed **6,944 offline tests**, including **496 Table/View cases** (164 additional cases in this increment), and **1,282 installed-package XML examples**, including all 58 Table examples and two View examples. Managed fixture workflows cover the current Table routes and ShowFront; live native COM/UI/storage qualification remains separate.

Tests cover all overload signatures/packets, integer channel transport, color parsing/fallback, ignored alpha, HSL sector boundaries/clamping/nonfinite inputs, alignment code maps, font/coordinate validation, caller thread, stale/replaced/disposed bindings, original exceptions and no retry after a native FormatException. Interface and adapter XML match; packaged examples compile and execute with mutation guards, alongside managed native-signature workflow checks. No real model or report was mutated.
