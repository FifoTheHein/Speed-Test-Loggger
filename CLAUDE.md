# Speed Test Loggger

WinForms app on .NET Framework 4.8 (C#) that runs download speed tests on a schedule, graphs them and logs outages. It's distributed with ClickOnce from GitHub Pages and updates itself.

- **Repo:** `FifoTheHein/Speed-Test-Loggger`, a fork of `A9T9/Speed-Test-Loggger`. Always target the fork. PRs go into `master`.
- **License:** AGPL v3. Keep the original author's credit, "Copyright © 2015 a9t9", in the About box and `AssemblyInfo.cs`. Don't point anything at loggger.com: that site isn't part of this fork.
- **Install and update URL:** `https://fifothehein.github.io/Speed-Test-Loggger/`. It's `fifothehein`, matching the owner name. A typo of it once broke this.

## Layout

| File | What's in it |
|---|---|
| `SpeedTest/Global.cs` | Entry point, settings defaults, CSV log load and save, `IsOnline`, which pings google.com |
| `SpeedTest/FormMain.cs` | Logging loop: IP lookup, then a test download, then a log row |
| `SpeedTest/CustomWebClient.cs` | `WebClient` that measures download speed |
| `SpeedTest/SpeedGraph.cs` | Graph control |
| `SpeedTest/FormSettings.cs` | Settings dialog |
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
- Older rows, in the original a9t9 format, have the two swapped: Message holds the result ("OK", "Error", "Offline") and Status holds the change. Anything that reads the change, like the graph's offline shading, must handle both. Use `SpeedGraph.GetConnectionChange` and `IsLoggerEvent`.

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
   - The About box in `SpeedTest/FormMain.cs`, in the form `V1.05` for 1.5.0.
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

- **Branches and PRs:** never commit to `master` directly. Use a branch, a PR into `master` on the fork, and the user merges it. After the merge, update `master` locally. Delete merged branches when the user asks.
- **`.vs/` files:** some were committed by mistake, and Visual Studio rewrites them while it's open. Leave `.vs/` out of commits.
- **Renovate:** it opens PRs that upgrade the workflow's actions.
- **Repo settings:** GitHub Pages uses GitHub Actions as its source. The `github-pages` environment allows the `master` branch and `v*` tags to deploy.
