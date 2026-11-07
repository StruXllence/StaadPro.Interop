# Table lifecycle and cell typography

## Exact contracts and differences

All new functions return the existing immutable `TableOperationResult`. Each native function is void, so normal completion gives `Status == InvokedWithoutAcknowledgement`, `IsSuccessful == false`, and `NativeReturnCode == null`. This does not mean failure; it means STAAD provided no success flag. Post-call binding failure or a native exception can follow actual changes. No automatic save, retry, rollback, new STAAD instance, or inferred effect is added.

Report/table IDs are positive native identities, not collection indices. Cell rows and columns are **one-based**. Row/column dimensions are positive requested totals, not increments. Rename text is non-null/nonblank and rejects embedded NUL; otherwise exact text is preserved. Font sizes must be finite positive Double values using the native Word-equivalent convention, independent of model input length units. Native font and table limits remain authoritative; the wrapper does not invent an upper bound or guarantee table shrink support.

`SaveAllReports` deliberately records null ReportId and TableId and calls native `SaveReportAll` once. `SaveReport` retains one report ID; `SaveTable` retains report and table IDs. Deletion receipts retain requested IDs for diagnostics, not proof that the objects remain valid. Requery native data after destructive changes.

## Example

Use only against the acquired report context on the established COM apartment. The deletion calls are independently guarded so a formatting workflow does not automatically destroy its report.

```csharp
// With an acquired wrapper and report/table IDs obtained through creation:
var table = wrapper.Table;
if (table == null) throw new System.InvalidOperationException("Table interface unavailable.");
int reportId = 11; // replace with an actual native report ID
int tableId = 7;   // replace with an actual table ID in that report
table.RenameTable(reportId, tableId, "Design forces");
table.ResizeTable(reportId, tableId, 6, 4); // requested total dimensions; choose growth
table.SetCellTextBold(reportId, tableId, 1, 2);
table.SetCellTextItalic(reportId, tableId, 1, 2);
table.SetCellTextUnderline(reportId, tableId, 1, 2);
table.SetCellTextSize(reportId, tableId, 1, 2, 12.5);
var savedTable = table.SaveTable(reportId, tableId);
var savedAll = table.SaveAllReports();
System.Console.WriteLine(savedAll.Status); // InvokedWithoutAcknowledgement

bool removeReportData = false;
if (removeReportData)
{
    table.DeleteTable(reportId, tableId);
    table.DeleteReport(reportId);
}
```
Bold/italic/underline methods request one native style each. They expose no Boolean toggle or inverse because the inspected native functions provide none. They do not write body text or provide a formatting read-back. Save requests do not call Root model-save operations or enumerate other STAAD application instances.

## Reuse, versions and verification

Reused `TableOperationResult`, `TableOperationStatus`, `TableService`, positive-ID/cell/text validation, the existing captured-binding gate and permanent native-signature fixture. No new public data type, enum, service family, partial adapter, tuple or public ref/out parameter was needed. Source signatures, call sites, source hashes and all three native contracts are recorded in the [expanded contract index](contracts/TableFoundationContractIndex.json).

Installed 2024/2025/2026 help declarations match for all ten calls; passive 2025/2026 metadata corroborates argument order and by-value slots. The common implementation remains shared. A 2024 vendor ABI was not inspected, and source coverage does not certify native UI readiness, licensing, COM marshaling, storage persistence or every servicing build.

Passed **6,780 offline tests**, including **332 Table cases** (145 additional cases in this increment). Compiled and executed **1,262 installed-package XML examples**, including all **forty Table examples**, and verified an isolated lifecycle/formatting consumer workflow. All twenty Table methods have matching interface/concrete XML documentation and guarded examples. Regression checks exercise packets, one-based coordinates, scoped receipts, void evidence, disposal/replacement, caller thread and original exceptions; new font checks cover nonfinite/nonpositive inputs and culture-independent Double forwarding. Native model/report mutation was not performed by offline verification.
