# Report and table foundations

**Current coverage:** after the [lifecycle/typography increment](Table-Lifecycle-and-Formatting.md), twenty of 29 source overloads and twenty of 24 native names are implemented. Nine source overloads remain. The foundation checkpoint below records the original ten-method increment.

## Contracts and differences

Rows and columns are **one-based**: 1 is the first row/column. The main interface's zero-based XML wording conflicts with all three reviewed Bentley references; the implementation forwards coordinates directly and the new documentation follows the native contract. The source parameter called `cellId` actually identifies a table and is exposed as `tableId`. No public reference parameters, tuples, proprietary interop assemblies or partial adapters were added.

Counts and cell strings return data directly: these native functions supply no ordinary failure flag to retain. `GetCellValue` fills an internal reference output; an explicit empty string is valid, while an untouched/null/non-string output throws. This avoids treating an unchanged initial empty buffer as successful native data. Counts accept only nonnegative integral Int32-range responses; no string/Boolean/floating-point coercion occurs.

Creation and mutation methods return the shared immutable `TableOperationResult`. Positive creation IDs mean `Succeeded`; zero is `NativeFailure`, with no fabricated created ID. Negative/malformed IDs throw. Void edit/save calls return `InvokedWithoutAcknowledgement`, with `IsSuccessful == false` and `NativeReturnCode == null`. Normal return does not prove application effects or persistence.

## Example

The following changes a report in the acquired model. Run on the established STAAD COM apartment with the required native report context. Report/table creation may leave native state even if a later call fails.

```csharp
using StaadPro.Interop.Models;
using StaadPro.Interop.Enums.Table;

// With an already acquired OpenStaadWrapper named wrapper:
var table = wrapper.Table;
if (table == null) throw new System.InvalidOperationException("Table interface unavailable.");
var report = table.CreateReport("Member summary");
if (!report.IsSuccessful) return;
var created = table.AddTable(report.ReportId.Value, "Forces", 3, 2);
if (!created.IsSuccessful) return;
int reportId = report.ReportId.Value;
int tableId = created.TableId.Value;
table.SetColumnHeader(reportId, tableId, 2, "Axial force");
table.SetColumnUnitString(reportId, tableId, 2, "kN");
table.SetRowHeader(reportId, tableId, 1, "Member 17");
table.SetCellValue(reportId, tableId, 1, 2, "42.50");
string observed = table.GetCellValue(reportId, tableId, 1, 2);
var save = table.SaveReport(reportId);
System.Console.WriteLine(observed);
System.Console.WriteLine(save.Status); // InvokedWithoutAcknowledgement
```
The unit string is a display label. It neither changes model input units nor converts values. Native counts do not enumerate IDs; never infer IDs from a count. Names/text are forwarded unchanged: titles/table names must be nonblank, other text may be empty, all text must be non-null and contain no embedded NUL. Report/table IDs, dimensions and coordinates must be positive. No upper-bound, context or existence check is fabricated. Native errors propagate unchanged.

## Architecture and lifecycle

The ten reviewed contracts match across installed 2024/2025/2026 help. There is no evidence requiring a duplicate service per year. Passive 2025/2026 metadata corroborates argument order and the internal by-reference final `GetCellValue` slot; no installed 2024 ABI was inspected. All operations use the captured RawTable object and caller thread, serialize within an adapter, and check binding before/after dispatch. External UI edits/raw COM calls and other adapter instances are not locked out. Reacquire after refresh. No automatic model acquisition, thread-pool call, retry, rollback, conversion or save occurs.
