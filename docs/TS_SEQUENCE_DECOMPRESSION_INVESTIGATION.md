# Task-sequence XML decompression investigation

## Status

This branch is retained for future investigation. Do not submit it as a fix for a confirmed SCCM production issue without first obtaining a real compressed `TS_Sequence` payload.

## Why this branch exists

The original `get secrets` path decrypted the `TS_Sequence` property into `tsSequenceDoc`, then called `Helpers.DecompressXMLNodes` on `policyXmlDoc`. Those are different XML documents. If the decrypted task-sequence value is a `<PolicyXML Compression="zlib">` wrapper, the original code leaves that wrapper compressed and cannot find task-sequence variables inside it.

This branch moves decompression to `tsSequenceDoc` and extracts task-sequence variables through `ExtractTaskSequenceVariables`.

## Deterministic coverage

`UnitTests/HelpersTests.cs` adds a test fixture that creates a UTF-16 task-sequence XML document, compresses it with gzip, wraps it in `<PolicyXML Compression="zlib">`, and passes it to `ExtractTaskSequenceVariables`. The expected `OSDJoinPassword` value is then present in the result.

This proves that the changed code handles the compressed form. It does not prove that SCCM emits this form for task-sequence data in every environment.

## Lab investigation

The following tests used fresh PXE-derived client material and compared the baseline binary built from `main` with the binary built from this branch. Both binaries received the same secret-policy identifier in each comparison.

| Test | Result |
| --- | --- |
| Existing task sequence | Both binaries extracted the task-sequence variables. |
| `OSDRegisteredOrgName` with 64,000 characters | Both binaries extracted the value. |
| Task sequence with approximately 630 KB of harmless inline padding | Both binaries extracted `DECOMP_TEST_MARKER` and the padding. |
| Existing PXEHacker task-sequence policy artifacts | No artifact contained `Compression="zlib"`. |

The lab did not produce a compressed decrypted `TS_Sequence` payload. These results show that large variables and large inline task-sequence content do not reliably trigger that encoding in this environment.

## Conclusion

The source change corrects an apparent document-target mistake and is safe to keep for investigation. The compressed-payload test demonstrates its intended behavior. The lab did not reproduce a user-visible failure, and no supported SCCM setting or documented size threshold was found to force nested task-sequence compression.

If a future capture contains `Compression="zlib"` inside the decrypted `TS_Sequence` value, rerun the baseline and this branch against that capture. The expected difference is that the baseline misses variables inside the wrapper, while this branch extracts them.

PXEHacker currently decompresses other policy layers but does not decompress this same nested task-sequence wrapper. If a real payload is found, test PXEHacker separately before making a related change.
