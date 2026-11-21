# Get the APK without Unity on your PC (GitHub cloud build)

Everything is already set up: the project files, the URP setup, the scene builder, the Android settings and the build workflow.
**There is only one thing left that only you can do:** connect your free Unity license to GitHub. Unity's rules require you to log in yourself. No one else can do it on your behalf, and your password must never go into the chat.

## Step 1: Unity account (2 minutes)
Make a free account at **id.unity.com** (Google login works too).
> If you signed in with Google, set a password as well (Account settings → Security). GitHub needs an email and password.

## Step 2: Get the license file (5–10 minutes, one time)
1. Install only **Unity Hub** from **unity.com/download**. (You do **not** need the Unity Editor. Hub is small.)
2. Open Hub → sign in → ⚙ **Preferences → Licenses → Add → Get a free personal license**.
3. A file is created on your PC:
   - Windows: `C:\ProgramData\Unity\Unity_lic.ulf`
   - Mac: `/Library/Application Support/Unity/Unity_lic.ulf`
   (`ProgramData` is a hidden folder. Paste the path straight into File Explorer's address bar.)
4. Open the file in **Notepad** and **copy everything** (Ctrl+A, Ctrl+C).

## Step 3: Put 3 secrets into GitHub (3 minutes)
github.com/ishwartudu9835-cloud/the-last-night → **Settings → Secrets and variables → Actions → New repository secret**

| Name | Value |
|---|---|
| `UNITY_EMAIL` | Your Unity account email |
| `UNITY_PASSWORD` | Your Unity password |
| `UNITY_LICENSE` | The full contents of the `.ulf` file you copied in step 2 |

GitHub keeps secrets encrypted and they never show up in the logs.

## Step 4: Build
Repo → **Actions → "Android APK (Unity)" → Run workflow**.
- The first build takes about **40–70 minutes**. Unity downloads and the whole project imports. Later builds are faster.
- When it is green ✅: repo home page → **Releases** → latest "The Last Night — build N" → download **TheLastNight.apk**. You can open this straight on your phone.
- Install on the phone: allow "Install unknown apps".

## If it goes red ❌
Tell me "build fail hua". I'll read the error and fix it, then push again. Every push automatically starts a new build.
