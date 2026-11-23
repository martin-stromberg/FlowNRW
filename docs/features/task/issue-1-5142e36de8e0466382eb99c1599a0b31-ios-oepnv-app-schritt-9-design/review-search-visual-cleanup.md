# Independent review: stop search and map text cleanup

Latest changes re-reviewed on 04.10.2026, limited to nearby failure visibility and compact result warnings. This was a read-only source/test review; tests were not executed.

## Result

The prior blocker is resolved. `ShowsNearbyStatus()` now uses the explicit `IsNearbyFailure` model state rather than partial text matching, and also keeps loading and successful-empty results visible. `StopMonitorViewModel` sets/clears this flag for the explicit nearby request lifecycle and notifies it with other search state. Thus denied, disabled, unsupported, timeout, unavailable, and general error descriptions all remain visible without relying on their wording.

The warning path is also coherent: `JourneyPresentation.CompactWarning()` preserves stale, fallback, and incomplete-result warnings while suppressing metadata for a current complete result. `StopSearchPage.SearchWarning()` reads provenance from whichever result is currently active; ordinary lookup changes clear the nearby result before the lookup result is displayed.

## Test review

- `LocationTests.NearbyLocationFailureAllowsRetry` is a theory covering denied, disabled, timeout, unsupported, unavailable, and error. It asserts a nearby failure sets `IsNearbyFailure` and a subsequent successful nearby request clears it.
- Existing Windows location journey coverage checks user-visible denied, timeout, and unavailable status labels. It does not check all six statuses in the actual nearby page; the core theory covers their model state through the shared failure path.
- `JourneyPresentationTests.CompactsStaleFallbackAndIncompleteWarnings` checks suppression for a normal response and preservation of all three warning categories together. It is formatter-level coverage; no new page-level assertion checks the stale/incomplete warning label in the native UI.
- The Windows nearby UI assertion continues to verify the fallback warning, and the candidate text cleanup assertions remain valid.

The latest changes address the earlier hidden-error finding. Remaining coverage gap: add native UI cases for disabled/unsupported/error nearby outcomes and for stale/incomplete result warning labels if the full warning presentation needs end-to-end proof. This is a coverage improvement, not a code review blocker for the changed semantics.

## Preserved behavior

Search and nearby requests retain their existing commands. Stop and map candidate actions retain their original selection commands and complete stop objects; only visible/accessibility text omits technical identities. Missing-position text, map attribution, and access to map provider information remain available.

**Verdict:** Reviewed changes are acceptable for the requested failure visibility and warning semantics. No tests were run during this re-review.
