# Property profile-name lookups and final documented cleanup

**Updated:** 26 October 2026

## Implemented contracts

### RemoveAllElementNodeReleaseSpec

~~~csharp
PropertyMutationResult RemoveAllElementNodeReleaseSpec();
~~~
When to use: remove all element-node release specifications in one explicit model-wide operation. RemoveElementNodeReleaseSpecFromPlate targets a particular plate and analytical node instead; DeleteMemberReleaseSpec concerns a member end. This operation does not remove member releases or element offsets. It can affect many assignments; native failure or a transport exception may follow partial changes. No implicit save, retry, rollback, analysis or geometry selection occurs.

A model-scoped acknowledgement: Succeeded for native 1/true, or NoSpecification for 0/false. NoSpecification means that no release was present and is not a fabricated successful mutation.

~~~csharp
bool allowNativeMutation = false;
if (allowNativeMutation)
{
    var result = property.RemoveAllElementNodeReleaseSpec();
    System.Console.WriteLine(result.Status);
}
~~~
### RemoveMemberFireProofingSpecFromBeam

~~~csharp
PropertyMutationResult RemoveMemberFireProofingSpecFromBeam(int memberId);
~~~
When to use: remove fireproofing from one member without deleting its section, material, release or offset specification. GetFireProofDataForBeam observes the existing assignment; GetFireProofingSpecDetails takes a zero-based specification index and does not identify a member. No specification index is derived from memberId. This mutation does not recompute design results or save the model, and partial native effects are not rolled back.

A member-scoped result retaining the requested ID and actual native acknowledgement. Native 1/true is Succeeded; 0/false is NativeFailure and does not prove that no specification existed.

| Parameter | Meaning |
| --- | --- |
| `memberId` | Positive analytical member ID, not a fireproofing specification index or specification definition ID. |

~~~csharp
bool allowNativeMutation = false;
if (allowNativeMutation)
{
    var result = property.RemoveMemberFireProofingSpecFromBeam(17);
    System.Console.WriteLine(result.Status);
}
~~~
### GetPublishedProfileName

~~~csharp
PropertyQueryResult<string> GetPublishedProfileName(string staadProfileName, Country country);
~~~
When to use: translate an existing STAAD catalog label to its published label for a specified country. GetStaadProfileName performs the reverse lookup. These mappings are not assumed to be one-to-one or reversible, and this query does not publish a section, create a property, search model assignments or change the database folder. Native text is preserved without trimming, case changes, shape inference or unit conversion.

The exact returned published name on Success. A null, DBNull or empty native name produces EmptyOrUnavailable without a data payload; non-string or embedded-NUL responses are rejected.

| Parameter | Meaning |
| --- | --- |
| `staadProfileName` | Exact nonblank STAAD catalog profile name without NUL characters; case and whitespace are preserved. This is not a model property ID or UPT section index. |
| `country` | Existing Country selector in the documented range 1 through 37. Country.Chilean (38) is not included in the reviewed table. |

~~~csharp
var result = property.GetPublishedProfileName("W12X26", Country.American);
if (result.IsSuccessful) System.Console.WriteLine(result.Data);
else System.Console.WriteLine(result.Status);
~~~
### GetStaadProfileName

~~~csharp
PropertyQueryResult<string> GetStaadProfileName(string publishedProfileName, Country country);
~~~
When to use: translate a published catalog label to the corresponding STAAD label. GetPublishedProfileName performs the opposite lookup. The translation uses the acquired native catalog and explicit country; it does not create an ASSIGN/UPT section, hydrate a SectionProperty object, alter current material or certify a round trip. Native names and country identity are preserved, and no model mutation or unit conversion occurs.

The exact returned STAAD profile name on Success. A null, DBNull or empty native name produces EmptyOrUnavailable without a data payload; malformed text or non-string returns are rejected.

| Parameter | Meaning |
| --- | --- |
| `publishedProfileName` | Exact nonblank published catalog profile name without NUL characters; case and whitespace are preserved. This is not a file path or a model section reference. |
| `country` | Existing Country selector in the documented range 1 through 37. Country.Chilean (38) is not included in the reviewed table. |

~~~csharp
var result = property.GetStaadProfileName("W12X26", Country.American);
if (result.IsSuccessful) System.Console.WriteLine(result.Data);
else System.Console.WriteLine(result.Status);
~~~
## State, transport and native evidence

All four operations run synchronously on the caller’s established COM apartment under the existing Property gate. Native calls are made once. Disposal or receiver replacement is checked before and after invocation; a mutation may already have taken effect when a post-call check fails. Other adapters, direct ComObject access, other wrappers and external edits remain outside this gate.

Native Boolean returns remain Boolean evidence; integral returns remain integral evidence. For these two removals only, documented 1/0 flags and their Boolean representations are accepted. Negative or other numeric codes are not silently treated as success. The model-wide removal maps zero to NoSpecification; the member fireproofing removal maps zero to NativeFailure. The meanings are deliberately different.

Country selectors are limited to the reviewed 1..37 table. Country.Chilean=38 remains available in the shared enum but is not accepted by these lookups. Input names preserve case and surrounding whitespace after nonblank/NUL validation. A string return is never converted to a number, a section shape or an existing property ID. Empty native names remain EmptyOrUnavailable rather than proving a missing catalog entry. Lookups do not change native units or database context.
