# Speed Test Loggger

WinForms app on .NET Framework 4.8 (C#) that runs download speed tests on a schedule, graphs them and logs outages. It's distributed with ClickOnce from GitHub Pages and updates itself.

- **Repo:** `FifoTheHein/Speed-Test-Loggger`. It started as a fork of `A9T9/Speed-Test-Loggger` and was detached from it in October 2026, so it's a standalone repo now. PRs go into `master`.
- **License:** AGPL v3. Keep the original author's credit, "Copyright © 2015 a9t9", in the About box, `AssemblyInfo.cs` and the README's License section. The About box and `AssemblyCopyright` also carry "Copyright © 2026 Heinrich J Nel" for this project's changes, and `AssemblyCompany` is "Heinrich J Nel". Don't point anything at loggger.com: that site isn't part of this project.
- **Install and update URL:** `https://fifothehein.github.io/Speed-Test-Loggger/`. It's `fifothehein`, matching the owner name. A typo of it once broke this.

## Layout

| File | What's in it |
|---|---|
| `SpeedTest/Global.cs` | Entry point, settings defaults, CSV log load and save |
| `SpeedTest/FormMain.cs` | Logging loop: IP lookup, then a test download, then a log row |
| `SpeedTest/CustomWebClient.cs` | `WebClient` that times the test download (`StartTimedDownload`) |
| `SpeedTest/SpeedGraph.cs` | Graph control |
| `SpeedTest/FormSettings.cs` | Settings dialog |
| `SpeedTest/Theme.cs` | Colours, fonts, line icons, the flat toolbar/menu renderer (`FlatRenderer`) and settings sections (`SectionBox`) |
| `tools/make-icon.ps1` | Draws the app icon and writes `AppIcon.ico`, the window resources and the PNGs |
| `docs/images/` | README icon and screenshots |
| `.github/workflows/publish.yml` | Release workflow: builds, signs, deploys to Pages |
| `deploy/index.html` | Install page on the Pages site |

## Connection log

- Saved to `%AppData%\SpeedTest\SpeedTestLog2.csv`.
- 9 columns: `Index,Time(U),Time(S),Speed(S),IP,Message,Status,Filename,Speed(B)`.
- Each row is a `ListViewItem` in `Global.ConnectionLog`. Its SubItems are [0] Time, [1] speed text, [2] IP, [3] Message, [4] Status, [5] Filename, [6] speed in bytes.
  - Status holds "Logger Started", "Logger Stopped", "OK", "Offline" or "Error".
  - Message holds "-", "IP Changed", "Online", "Offline" or "Invalid URL".
  - Older rows in existing logs may use the old values, such as Status "Online".
- Never write an empty column: use "n/a". Don't split lines with `RemoveEmptyEntries`. An empty column once made the loader silently drop every row after it, and the next save erased them.
- Never write culture-formatted numbers to the CSV. On a PC set to South Africa, "512,34 kb/s" adds a column. `SaveConnectionLog` writes Speed(S) in the invariant culture with no grouping, and passes text columns through `CsvField`.
- The loader reads the first three columns from the front and the last five from the back. It rebuilds the speed text from Speed(B) and skips malformed rows instead of stopping.
  - This recovers a row an older version wrote with a comma in Speed(S), but only if that row was never reloaded.
  - Versions up to 1.4.1 reloaded and re-saved the log on every run, so on PCs with a comma decimal point, history from before then is already scrambled (speed 0, columns shifted). Don't promise users it's repaired.
- Saves go to a `.tmp` file and then `File.Replace`, never straight over the log. If the log couldn't be read at startup, the first save reads it again first. If rows were skipped, the first save keeps a dated `.bak` copy.
- In `FormMain`, add rows with `AddLogItem`. It shows the row, saves the log, and puts save errors in the status bar. Don't show dialogs from the logging loop.
- **The log is in time order, and code relies on that.** The graph, `UpdateLog` and the auto-download size check read it newest first and stop at the first row that's too old. Never scan the whole log on a timer: with a year of rows, the graph took 1.5 s per repaint until it stopped early (now 4 ms).
- **Time(U) is real Unix time.** `ToUnixTime` and `FromUnixTime` convert through UTC. The old code used a local 1970 epoch, which was wrong in daylight saving time. That made no difference in South Africa, which has no daylight saving.
- Older rows, in the original a9t9 format, have the two swapped: Message holds the result ("OK", "Error", "Offline") and Status holds the change. Anything that reads the change, like the graph's offline shading, must handle both. Use `SpeedGraph.GetConnectionChange` and `IsLoggerEvent`.

## Speed tests

- **A test runs in two steps:** `GetRemoteIP` looks up the public IP over HTTP, then always downloads the test file.
  - **Outcomes:** if the download works, the row is OK. If both steps fail, it's Offline. If only the download fails, it's Error, with the reason in Message ("HTTP 404", "Timed out", ...).
  - **No ping:** there is deliberately no ping. It froze the window and failed on networks that block ICMP.
- **One test at a time:** `p_ActiveClient` is the running test. `GetRemoteIP` skips a new test while one is active. Every completion path must call `FinishTest`, which clears it and disposes the client.
- **Timeouts:** 15 s for the IP lookup and 60 s for the download. `p_TestDeadline` is checked by the logger's once-a-second `p_TimerLog_Tick`, which calls `CancelAsync`.
  - Don't use a separate WinForms `Timer` for this. Stopping and restarting one quickly left it disabled, so hung downloads never timed out.
- **Speed measurement:** `CustomWebClient.StartTimedDownload` resets the counters, and only that download is timed.
  - **Timing:** speed is measured from the first chunk, which leaves out connecting and waiting for the server. If the file arrives in one chunk, the whole request time is used.
  - **No cache:** the cache is bypassed, so a cached file can't report disk speed.
- **Testing:** the networking tests used a local Python HTTP server, with routes for a fake IP, a captive portal page, a hang, a 404 and a throttled 1 MB file. They drove `FormMain` by reflection, and the harness called `p_TimerLog_Tick` once a second.
  - Build to a separate `OutDir` if Visual Studio is debugging, since that locks `bin\Debug`.
  - Closing a `FormMain` runs `OnFormClosing`, which saves settings and calls `UpdateAutoStart`. Point `LogFile` at a scratch file first.

## Settings and startup

- **Settings across updates:** user settings are stored per version. `Main` calls `Settings.Default.Upgrade()` once when `UpgradeRequired` is true, which it is by default in each new version, before `FormMain` is created. A new setting needs adding in three places: `Settings.settings`, `Settings.Designer.cs` and `App.config`.
- **Testing settings by reflection:** in a PowerShell test host, settings are saved under the PowerShell exe's own config, not the app's, so tests can't touch the user's real settings. They also can't exercise `Upgrade()`.

- **Settings dialog:** only OK applies changes. Cancel, Esc and the close button discard them. OK checks custom URLs with `Global.TryGetWebUri`, which requires an absolute http or https address. Code that uses a URL setting checks it again, because older settings may predate the check.
- **Auto-start:** `UpdateAutoStart` writes `Speed Test Loggger.cmd` to the Windows Startup folder.
  - For a ClickOnce install, the script starts the app through its Start Menu `.appref-ms` shortcut, so ClickOnce checks for updates first.
  - A `.appref-ms` can't pass `/start`, so the script first creates `%AppData%\SpeedTest\autostart.flag`. `Main` deletes the flag and starts logging if it's less than 10 minutes old.
  - Paths under AppData are written as `%APPDATA%`, because cmd reads scripts in the OEM code page.
  - Never launch the exe from ClickOnce's versioned `Apps\2.0` folder: that skips updates and breaks after one.
- **Testing auto-start by reflection:** `Application.ExecutablePath` is the PowerShell exe, so the fallback script names `powershell.exe`. That's expected.

## Look and feel

- **Theme:** the look is light and flat, in the style of Windows 11. Use `Theme`'s colours and fonts rather than new literals, and `Theme.StyleButton` for buttons, with only one accent (primary) button per window.
- **Icons:**
  - Toolbar, menu and list icons are glyphs from Segoe Fluent Icons or Segoe MDL2 Assets, drawn by `Theme.Icon`.
  - When neither font exists, `Theme.Icon` returns null. Callers fall back to the original bitmaps in `ImageListMain` and `ImageListLog`, so keep those.
- **Toolbar, status bar and tray menu:** these are `ToolStrip`, `StatusStrip` and `ContextMenuStrip` with `FlatRenderer`, not the old `ToolBar`, `StatusBar` and `ContextMenu`.
  - The toolbar toggles use `.Checked`.
  - Don't right-align toolbar items. In .NET Framework that hides them from UI Automation and screen readers. Settings, Help and About are left-aligned after `ToolBarSpacer`, a label that `ToolStripMain_Layout` widens to fill the gap.
  - Overflow is off (`CanOverflow = false`), so a button can never move into a hidden menu.
  - To check accessibility, list the toolbar's UI Automation children: every button should appear, and there should be no "ToolBar Options" item.
  - The status bar uses `HorizontalStackWithOverflow`, with `StatusMain` aligned right. `Spring` with the default Table layout pushed the label onto a hidden second row.
- **Graph (`SpeedGraph.OnPaint`):**
  - **Layout:** a summary row (last, average and fastest speeds, and outages), a plot with round axis steps (`NiceStep`), and a key.
  - **Bands:** outages are red and logger-off times grey (`DrawBands`).
  - **Empty state:** when nothing is on the timeline, the graph explains that instead of looking blank.
- **App icon:**
  - Run `powershell -ExecutionPolicy Bypass -File tools\make-icon.ps1 -UpdateResx` after changing the drawing.
  - It writes `SpeedTest\AppIcon.ico`, used for the exe and the ClickOnce shortcut, and the window and tray icons inside the `.resx` files.
  - It also writes `docs\images\icon.png` and `deploy\icon.png`.
- **README screenshots:**
  - Use sample data only: a documentation IP such as `203.0.113.7`, default settings and a generic log path. Never the developer's real log, IP or user name, because the repo is public.
  - Capture with `PrintWindow` and `PW_RENDERFULLCONTENT` to get the real window frame, then crop the 7–8 px invisible resize border, which comes out black.
  - `DrawToBitmap` draws an old-style frame, so use it only for quick checks.
- **Display scaling:** the app is per-monitor DPI-aware (PerMonitorV2).
  - **Setup:** this comes from `DpiAwareness` in `App.config`, and needs Windows 10 listed in `app.manifest`. Forms use `AutoScaleMode.Dpi` with a 96-DPI base. `SpeedGraph` inherits it.
  - **Your own pixel sizes:** anything drawn or sized in code (graph layout, icon sizes, padding) must go through `Theme.Scale(control, px)`, or `S()` in `SpeedGraph`. Write the 100% value and let it scale.
  - **List columns:** WinForms doesn't scale them. `FormMain` does, in `ScaleColumns`.
  - **Moving between monitors:** `FormMain.OnDpiChanged` rescales the columns and calls `ApplyThemeSizes` again for the new DPI.
- **Testing scaling:**
  - A reflection host (PowerShell) doesn't read the app's `App.config`, so WinForms reports 96 DPI there even when Windows scales the window. Check scaling by running the built exe itself and capturing its window from outside.
  - End that process with `Stop-Process -Force` rather than closing it, so `OnFormClosing` doesn't save anything. Ask the user first: the window opens on their screen.
  - Reflection tests that show `FormMain` should place it off-screen and `Dispose()` it, never `Close()` it.

## Building and testing

Build with MSBuild, found through vswhere:

```powershell
$msb = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
& $msb SpeedTest.sln /t:Build /p:Configuration=Debug /v:minimal /nologo
```

- **No automated tests.** Check changes by building. To check logic, load the built exe by reflection.
- **The exe is x86.** Load it from 32-bit PowerShell (`$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe`), or it fails with `BadImageFormatException`. Call `Add-Type -AssemblyName System.Windows.Forms` first.
- Set private statics by reflection where needed, such as `Global._gConnectionLog`, which `Main` normally sets.
- Git warns that LF will be replaced by CRLF. That's harmless.

## Signing

ClickOnce manifests are signed with a self-signed certificate:

- **Certificate:** `CN=Speed Test Loggger`, thumbprint `CB54C1B0BAAB7DF85D96396E9600ABDB8A2E8CB9`, valid until October 2031. The thumbprint is set in `SpeedTest.csproj`.
- **Local copy:** it's in the developer's CurrentUser certificate store. The local `SpeedTest/Speedtest_TemporaryKey.pfx` and its `.pfx.password.txt` file are git-ignored. Never commit them, show them, or print the password: the repo is public.
- **For CI:** the workflow reads the certificate from the `SIGNING_PFX_BASE64` and `SIGNING_PFX_PASSWORD` repository secrets.
- **Without the certificate:** build with `/p:SignManifests=false`.

## Making a release

1. **Bump the version on a branch with a PR.** It's in four places:
   - `ApplicationVersion` in `SpeedTest/SpeedTest.csproj`, for example `1.5.0.%2a`.
   - `AssemblyVersion` and `AssemblyFileVersion` in `SpeedTest/Properties/AssemblyInfo.cs`, for example `1.5.0.0`.
   - The About box in `SpeedTest/FormMain.cs`, in the form `V1.05` for 1.5.0, or `V1.04.1` for a patch release like 1.4.1.
2. **Check locally before the PR.** Run the workflow's tag check, then publish:
   ```powershell
   & $msb SpeedTest\SpeedTest.csproj /t:Clean,Publish /p:Configuration=Release /p:ApplicationVersion=1.5.0.0 /p:MinimumRequiredVersion=1.5.0.0 /p:UpdateRequired=true /v:minimal /nologo
   ```
   Then confirm `SpeedTest\bin\Release\app.publish\SpeedTestLoggger.application` has the new `minimumRequiredVersion` and the Pages `deploymentProvider`.
3. **After the user merges, publish the release on `master`:**
   ```bash
   gh release create v1.5.0 --repo FifoTheHein/Speed-Test-Loggger --target <master sha> --title "Speed Test Loggger 1.5.0" --latest --notes-file -
   ```
   - The notes cover what changed, with PR numbers, and how to install from the Pages link.
   - Publishing a release is public, so confirm with the user first.
4. **The "Publish release" workflow runs.**
   - It fails if the tag doesn't match `ApplicationVersion`.
   - It builds, signs and publishes with that version as the required minimum, so clients can't skip the update.
   - It deploys the installer, the `.application` file, that version's `Application Files` folder and `index.html` to Pages, and attaches the installer to the release.
   - MSBuild always names the installer `setup.exe`. The workflow renames it to `Logger_Setup.exe`, so look for that name on the site and the release.
   - Watch it with `gh run watch <id> --exit-status`.
5. **Verify the live site.** `curl` the Pages URL for `/`, `/Logger_Setup.exe`, `/SpeedTestLoggger.application` and `Application%20Files/SpeedTestLoggger_1_5_0_0/...`, and check the `.application` file's version.

- Clients on 1.3.0 or earlier were installed from disk with updates off. They must uninstall once and install from the Pages link.
- From 1.4.0 onwards, updates are automatic when the app starts.

## Git workflow

- **Branches and PRs:** never commit to `master` directly. Use a branch, a PR into `master`, and the user merges it. After the merge, update `master` locally. Delete merged branches when the user asks.
- **`.vs/` files:** some were committed by mistake, and Visual Studio rewrites them while it's open. Leave `.vs/` out of commits.
- **Renovate:** it opens PRs that upgrade the workflow's actions.
- **Repo settings:** GitHub Pages uses GitHub Actions as its source. The `github-pages` environment allows the `master` branch and `v*` tags to deploy.
