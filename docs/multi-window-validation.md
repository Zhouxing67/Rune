# MSIX multi-window acceptance

The implementation uses Windows App SDK `AppInstance` for registration and
activation redirection, not a custom mutex or IPC protocol. All windows run on
the UI thread and share one application state. File identity checks normalize
paths and, on Windows, compare volume/file IDs to recognize hard links.

## Installed-package checks

Run these against an installed MSIX built from this revision (not an older
Store installation). Automated engine tests and compilation do not establish
that shell activation or window focus works on an installed package.

1. Launch Rune repeatedly, including rapid successive launches during startup.
   There should be one long-lived process and one initial window.
2. Create a second window with Ctrl+N. Open A in the first and B in the second.
   Both Home pages should show the same recent documents and removals.
3. Add an unsaved annotation to A. Open A from the other window, Explorer,
   and a hard-link path. Its original window/tab should activate without
   losing annotations or creating another editable session.
4. Activate the second window, then double-click a new PDF in Explorer.
   It should open there. Repeat while that window is minimized.
5. Drop a new PDF into the first window. It should open in the first window.
   Right-click an unopened recent PDF and choose Open in new window.
6. Save As or extract pages to B while B remains open. Rune should reject the
   replacement. Repeat during an ongoing export/open operation.
7. Open pickers and dialogs in both windows; verify ownership and independent
   dialog handling. Change the theme and confirm both windows update.
8. Close a dirty window: cancel, save, and discard in separate trials. Other
   windows should remain usable. Close the last window and restart; its tabs
   should restore. Explicitly closed earlier windows should not return.

Existing tabs cannot yet be moved between windows. External applications can
still modify files; cross-application conflict detection is outside this change.
