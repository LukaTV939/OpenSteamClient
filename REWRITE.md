# TODO for huge rewrite
I did a lot of the stuff on this checklist, but then got burned out due to the huge scope. 
Now, do it more smartly and tackle these issues individually, or in SMALL groups, to avoid scope creep and burnout.
- Split DI into a separate package (along with API)
- Split logging into a separate package (along with API)
- OperationProgress, instead of IExtendedProgress
- Stop using concrete classes everywhere, and switch to interfaces.
- Use proper DI instead of relying on globals.
- Fix the app system.
- Make the app system async.
- And most importantly: Fix the app system.
- Combine OpenSteamClient and OpenSteamworks.Client, or just rewrite the entire thing from scratch with Reactivity in mind.
  - Rewrite the app system.
  - Rewrite the config system (for UI config).
  - Rewrite the bootstrapper.
- NativeAOT support:
  - OpenSteamworks
  - GUI
- Proper API:
  - Bootstrapper API
  - InstallManager API
  - Plugin system (not a priority)
    - LibraryArt API
	- AppManager API
	- ModManager API