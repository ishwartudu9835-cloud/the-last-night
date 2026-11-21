# Compile check (do NOT copy this into Unity's Assets folder)
How the code was verified outside Unity:
```
mcs -target:library -out:stubs.dll UnityStubs.cs
mcs -target:library -r:stubs.dll -d:ENABLE_INPUT_SYSTEM -d:UNITY_EDITOR -out:game.dll $(find ../Assets/TheLastNight/Scripts -name '*.cs')
mcs -target:library -r:stubs.dll -r:game.dll -out:editor.dll ../Assets/TheLastNight/Editor/*.cs
```
`apicheck.py` greps the real Unity 6 / URP 17 / AI Navigation / Input System sources for every API that the stubs assume.
